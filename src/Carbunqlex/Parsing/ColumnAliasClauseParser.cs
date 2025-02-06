using Carbunqlex.Clauses;
using Carbunqlex.Parsing.ValueExpression;

namespace Carbunqlex.Parsing;

/// <summary>
/// Parses column alias clauses from SQL tokens.
/// e.g. (column1, column2)
/// </summary>
public class ColumnAliasClauseParser
{
    public static ColumnAliasClause Parse(SqlTokenizer tokenizer)
    {
        tokenizer.Read(TokenType.OpenParen);

        var next = tokenizer.Peek();

        var columns = new List<string>();
        while (true)
        {
            var column = tokenizer.Read(TokenType.Identifier).Value;
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
            throw SqlParsingExceptionBuilder.UnexpectedTokenType(tokenizer, [TokenType.Comma, TokenType.CloseParen], next);
        }

        return new ColumnAliasClause(columns);
    }
}
