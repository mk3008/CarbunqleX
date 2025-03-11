using Carbunqlex.Lexing;
using Carbunqlex.Parsing.Expressions;
using Carbunqlex.QuerySources;

namespace Carbunqlex.Parsing.QuerySources;

/// <summary>
/// Parses datasource expressions from SQL tokens.
/// e.g. [dbo].[table]
/// </summary>
public class DatasourceParser
{
    public static IDatasource Parse(SqlTokenizer tokenizer)
    {
        var next = tokenizer.Peek();

        // TableSource or FunctionSource
        if (next.Type == TokenType.Identifier)
        {
            // If it is an identifier, dot, identifier, ..., continue reading
            var tokens = new List<Token>();
            while (true)
            {
                tokens.Add(tokenizer.Read(TokenType.Identifier));
                if (tokenizer.IsEnd)
                {
                    break;
                }
                if (tokenizer.Peek().Type == TokenType.Dot)
                {
                    // Do not cache Dot
                    tokenizer.CommitPeek();
                    continue;
                }
                break;
            }

            if (tokenizer.IsEnd)
            {
                return ParseAsTableSource(tokens);
            }

            next = tokenizer.Peek();

            if (next.Type == TokenType.OpenParen)
            {
                // Treat as a function table.
                // Combine tokens into a single identifier
                var functionFullName = tokens.Select(t => t.Value).Aggregate((a, b) => $"{a}.{b}");
                return FunctionDatasourceParser.Parse(tokenizer, functionName: functionFullName);
            }

            return ParseAsTableSource(tokens);
        }

        // SubQuerySource or UnionQuerySource
        if (next.Type == TokenType.OpenParen)
        {
            var query = SelectQueryParser.ParseSubQuery(tokenizer);
            return new SubQuerySource(query);
        }

        throw SqlParsingExceptionBuilder.UnexpectedTokenType(tokenizer, [TokenType.Identifier, TokenType.OpenParen], next);
    }

    private static TableSource ParseAsTableSource(List<Token> tokens)
    {
        // If there is only one element, it is a table with a schema name omitted
        if (tokens.Count == 1)
        {
            return new TableSource(tokens[0].Value);
        }
        // If there is a schema name, the last of tokens is the table name
        // Otherwise, it is a schema name (correct the dot)
        var schema = tokens.Take(tokens.Count - 1).Select(t => t.Value);
        var table = tokens.Last().Value;
        return new TableSource(schema, table);
    }
}
