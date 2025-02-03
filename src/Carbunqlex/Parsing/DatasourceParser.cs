using Carbunqlex.DatasourceExpressions;
using Carbunqlex.Parsing.ValueExpressionParsing;

namespace Carbunqlex.Parsing;

/// <summary>
/// Parses datasource expressions from SQL tokens.
/// e.g. [dbo].[table]
/// </summary>
public class DatasourceParser
{
    private static string ParserName => nameof(DatasourceParser);
    public static IDatasource Parse(SqlTokenizer tokenizer)
    {
        var next = tokenizer.Peek();

        // TableSource or FunctionSource
        if (next.Type == TokenType.Identifier)
        {
            var identifier = tokenizer.Read();
            if (tokenizer.IsEnd)
            {
                return new TableSource(identifier.Value);
            }

            next = tokenizer.Peek();

            if (next.CommandOrOperatorText == "as")
            {
                return new TableSource(identifier.Value);
            }

            if (next.Type == TokenType.OpenParen)
            {
                return FunctionDatasourceParser.Parse(tokenizer, functionName: identifier.Value);
            }

            // Without AS keyword alias
            return new TableSource(identifier.Value);
        }

        // SubQuerySource or UnionQuerySource
        if (next.Type == TokenType.OpenParen)
        {
            throw new NotImplementedException();
        }

        throw SqlParsingExceptionBuilder.UnexpectedTokenType(ParserName, new[] { TokenType.Identifier, TokenType.OpenParen }, tokenizer, next);
    }
}
