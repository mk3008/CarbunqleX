using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Parsing.ValueExpressionParsing;

public static class ColumnExpressionParser
{
    public static ColumnExpression Parse(SqlTokenizer tokenizer, Token identifier)
    {
        var values = IdentifierValueParser.Parse(tokenizer, identifier).ToList();

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
