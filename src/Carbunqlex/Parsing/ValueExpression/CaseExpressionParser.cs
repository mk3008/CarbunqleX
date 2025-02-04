using Carbunqlex.Clauses;
using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Parsing.ValueExpressionParsing;

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

        var elseValue = elseOrEndToken.CommandOrOperatorText == "end"
            ? null
            : elseOrEndToken.CommandOrOperatorText == "else"
                   ? ValueExpressionParser.Parse(tokenizer)
                   : throw SqlParsingExceptionBuilder.UnexpectedToken(tokenizer, ["else", "end"], elseOrEndToken);

        if (caseValue != null)
        {
            return elseValue != null
                ? new CaseExpression(caseValue, whenClauses, elseValue)
                : new CaseExpression(caseValue, whenClauses);
        }
        else
        {
            return elseValue != null
                ? new CaseWhenExpression(whenClauses, elseValue)
                : new CaseWhenExpression(whenClauses);
        }
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
