using Carbunqlex.Clauses;

namespace Carbunqlex.Parsing;

public class WindowFunctionParser
{
    private static string ParserName => nameof(WindowFunctionParser);

    public static IWindowDefinition Parse(SqlTokenizer tokenizer)
    {
        var next = tokenizer.Peek();
        if (next.Type == TokenType.Identifier)
        {
            var name = tokenizer.Read().Value;
            return new NamedWindowDefinition(name);
        }

        tokenizer.Read(ParserName, TokenType.OpenParen);

        var partitionBy = tokenizer.Peek(static (r, next) =>
        {
            return next.Identifier == "partition by"
                ? PartitionByClauseParser.Parse(r)
                : null;
        });

        var orderBy = tokenizer.Peek(static (r, next) =>
        {
            return next.Identifier == "order by"
                ? OrderByClauseParser.Parse(r)
                : null;
        });

        var windowFrame = tokenizer.Peek(static (r, next) =>
        {
            return next.Identifier == "rows" || next.Identifier == "range" || next.Identifier == "groups"
                ? WindowFrameParser.Parse(r)
                : null;
        });

        tokenizer.Read(ParserName, TokenType.CloseParen);

        return new NamelessWindowDefinition(partitionBy, orderBy, windowFrame);
    }
}
