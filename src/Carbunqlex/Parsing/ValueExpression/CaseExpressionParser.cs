using Carbunqlex.Clauses;
using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Parsing.ValueExpression;

public static class CaseExpressionParser
{
    public static ICaseExpression Parse(SqlTokenizer tokenizer)
    {
        tokenizer.Read("case");

        var caseValue = tokenizer.Peek(token =>
        {
            return token.CommandOrOperatorText == "when" ? null : ValueExpressionParser.Parse(tokenizer);
        }, null);

        var whenClauses = ParseWhenThenPair(tokenizer).ToList();

        if (whenClauses.Count == 0)
        {
            var errorToken = tokenizer.Read();
            throw SqlParsingExceptionBuilder.UnexpectedToken(tokenizer, ["between", "not between"], errorToken);
        }

        var elseOrEndToken = tokenizer.Read(TokenType.Command);

        if (elseOrEndToken.CommandOrOperatorText == "else")
        {
            var elseValue = ValueExpressionParser.Parse(tokenizer);
            tokenizer.Read("end");

            if (caseValue != null)
            {
                return new CaseExpression(caseValue, whenClauses, elseValue);
            }
            else
            {
                return new CaseWhenExpression(whenClauses, elseValue);
            }
        }
        else if (elseOrEndToken.CommandOrOperatorText == "end")
        {
            if (caseValue != null)
            {
                return new CaseExpression(caseValue, whenClauses);
            }
            else
            {
                return new CaseWhenExpression(whenClauses);
            }
        }
        throw SqlParsingExceptionBuilder.UnexpectedToken(tokenizer, ["else", "end"], elseOrEndToken);
    }

    private static IEnumerable<WhenClause> ParseWhenThenPair(SqlTokenizer tokenizer)
    {
        while (tokenizer.TryPeek(out var token) && token.CommandOrOperatorText == "when")
        {
            tokenizer.CommitPeek();
            var whenValue = ValueExpressionParser.Parse(tokenizer);
            tokenizer.Read("then");
            var thenValue = ValueExpressionParser.Parse(tokenizer);

            yield return new WhenClause(whenValue, thenValue);
        }
    }
}
