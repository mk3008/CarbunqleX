using Carbunqlex.Clauses;
using Carbunqlex.Parsing.ValueExpressionParsing;

namespace Carbunqlex.Parsing;

/// <summary>
/// Parses column alias clauses from SQL tokens.
/// e.g. (column1, column2)
/// </summary>
public class ColumnAliasClauseParser
{
    private static string ParserName => nameof(ColumnAliasClauseParser);
    public static ColumnAliasClause Parse(SqlTokenizer tokenizer)
    {
        tokenizer.Read(ParserName, TokenType.OpenParen);

        var next = tokenizer.Peek();

        var columns = new List<string>();
        while (true)
        {
            var column = tokenizer.Read(ParserName, TokenType.Identifier).Value;
            columns.Add(column);

            next = tokenizer.Peek();
            if (next.Type == TokenType.Comma)
            {
                tokenizer.Read();
                continue;
            }
            if (next.Type == TokenType.CloseParen)
            {
                tokenizer.Read();
                break;
            }
            throw SqlParsingExceptionBuilder.UnexpectedTokenType(ParserName, [TokenType.Comma, TokenType.CloseParen], tokenizer, next);
        }

        return new ColumnAliasClause(columns);
    }
}
