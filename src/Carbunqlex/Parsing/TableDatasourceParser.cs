using Carbunqlex.DatasourceExpressions;
using Carbunqlex.Parsing.ValueExpressionParsing;

namespace Carbunqlex.Parsing;

/// <summary>
/// Parses table datasource expressions from SQL tokens.
/// e.g. [dbo].[table]
/// </summary>
public class TableDatasourceParser
{
    public static TableSource Parse(SqlTokenizer tokenizer)
    {
        var next = tokenizer.Peek();

        if (next.Type == TokenType.Identifier)
        {
            // TableSource
            var identifier = tokenizer.Read();
            if (tokenizer.IsEnd)
            {
                return new TableSource(identifier.Value);
            }

            var values = IdentifierValueParser.Parse(tokenizer, identifier).ToList();
            if (values.Count == 1)
            {
                return new TableSource(values[0]);
            }

            // Treat all but the last element as namespaces
            // Treat the last element as the table name
            var tableName = values[^1];
            var namespaces = values.GetRange(0, values.Count - 1);
            return new TableSource(namespaces, tableName);
        }

        throw SqlParsingExceptionBuilder.UnexpectedTokenType(tokenizer, TokenType.Identifier, next);
    }
}
