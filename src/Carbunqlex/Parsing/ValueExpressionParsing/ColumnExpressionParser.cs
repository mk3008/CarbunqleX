using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Parsing.ValueExpressionParsing;

public static class ColumnExpressionParser
{
    private static string Name => nameof(ColumnExpressionParser);

    public static ColumnExpression Parse(SqlTokenizer tokenizer)
    {
        var values = new List<string>();

        while (true)
        {
            if (!tokenizer.TryPeek(out var token))
            {
                throw SqlParsingExceptionBuilder.EndOfInput(Name, tokenizer);
            }

            if (token.Type != TokenType.Identifier)
            {
                throw SqlParsingExceptionBuilder.UnexpectedTokenType(Name, TokenType.Identifier, tokenizer, token);
            }

            values.Add(token.Value);
            tokenizer.CommitPosition();

            if (!tokenizer.TryPeek(out var nextToken) || nextToken.Type != TokenType.Dot)
            {
                break;
            }

            tokenizer.CommitPosition();
        }

        if (values.Count == 1)
        {
            return new ColumnExpression(values[0]);
        }

        // Treat all but the last element as namespaces
        // Treat the last element as the column name
        var columnName = values[^1];
        var namespaces = values.GetRange(0, values.Count - 1);
        return new ColumnExpression(namespaces, columnName);
    }
}
