using Carbunqlex.Lexing;
using Carbunqlex.Parsing.Expressions;
using Carbunqlex.QuerySources;

namespace Carbunqlex.Parsing.QuerySources;

internal static class TableSourceParser
{
    /// <summary>
    /// Parse table source
    /// </summary>
    /// <param name="tokenizer"></param>
    /// <returns></returns>
    public static TableSource Parse(SqlTokenizer tokenizer)
    {
        var items = new List<string>();

        while (true)
        {
            var identifier = tokenizer.Read(TokenType.Identifier).Value;
            items.Add(identifier);
            if (tokenizer.IsEnd)
            {
                break;
            }
            if (tokenizer.Peek().Type == TokenType.Dot)
            {
                tokenizer.CommitPeek();
                continue;
            }
            break;
        }

        if (items.Count == 0)
        {
            throw SqlParsingExceptionBuilder.UnexpectedTokenType(tokenizer, TokenType.Identifier, tokenizer.Peek());
        }
        else if (items.Count == 1)
        {
            return new TableSource(items[0]);
        }
        else
        {
            return new TableSource(items.Take(items.Count - 1).ToList(), items.Last());
        }
    }
}
