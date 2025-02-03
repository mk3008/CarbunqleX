using Carbunqlex.Clauses;
using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Parsing.ValueExpressionParsing;

public static class CaseExpressionParser
{
    private static string ParserName => nameof(CaseExpressionParser);

    public static ICaseExpression Parse(SqlTokenizer tokenizer)
    {
        tokenizer.Read(ParserName, "case");

        var caseValue = tokenizer.Peek(token =>
        {
            return token.CommandOrOperatorText == "when" ? null : ValueExpressionParser.Parse(tokenizer);
        }, null);

        var whenClauses = ParseWhenThenPair(tokenizer).ToList();

        if (whenClauses.Count == 0)
        {
            var errorToken = tokenizer.Read();
            throw SqlParsingExceptionBuilder.UnexpectedTokenIdentifier(ParserName, "when", tokenizer, errorToken);
        }

        var elseOrEndToken = tokenizer.Read(ParserName, TokenType.Command);

        var elseValue = elseOrEndToken.CommandOrOperatorText == "end"
            ? null
            : elseOrEndToken.CommandOrOperatorText == "else"
                   ? ValueExpressionParser.Parse(tokenizer)
                   : throw SqlParsingExceptionBuilder.UnexpectedTokenIdentifier(ParserName, "'else' or 'end'", tokenizer, elseOrEndToken);

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
            tokenizer.Read(ParserName, "then");
            var thenValue = ValueExpressionParser.Parse(tokenizer);

            yield return new WhenClause(whenValue, thenValue);
        }
    }
}
