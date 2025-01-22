using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Parsing.ValueExpressionParsing;

public static class ColumnExpressionParser
{
    private static string ParserName => nameof(ColumnExpressionParser);

    public static ColumnExpression Parse(SqlTokenizer tokenizer, Token identifier)
    {
        var values = new List<string>() { identifier.Value };
        values.AddRange(ReadValues(tokenizer));

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

    private static IEnumerable<string> ReadValues(SqlTokenizer tokenizer)
    {
        while (tokenizer.TryPeek(out var nextToken) && nextToken.Type == TokenType.Dot)
        {
            tokenizer.CommitPeek();
            var token = tokenizer.Read(ParserName, TokenType.Identifier);
            yield return token.Value;
        }
    }
}
