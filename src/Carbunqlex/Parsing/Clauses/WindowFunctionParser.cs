using Carbunqlex.Clauses;
using Carbunqlex.Lexing;

namespace Carbunqlex.Parsing.Clauses;

public class WindowFunctionParser
{
    public static IWindowDefinition Parse(SqlTokenizer tokenizer)
    {
        var next = tokenizer.Peek();
        if (next.Type == TokenType.Identifier)
        {
            var name = tokenizer.Read().Value;
            return new NamedWindowDefinition(name);
        }

        tokenizer.Read(TokenType.OpenParen);

        var partitionBy = tokenizer.Peek(static (r, next) =>
        {
            return next.CommandOrOperatorText == "partition by"
                ? PartitionByClauseParser.Parse(r)
                : null;
        });

        var orderBy = tokenizer.Peek(static (r, next) =>
        {
            return next.CommandOrOperatorText == "order by"
                ? OrderByClauseParser.Parse(r)
                : null;
        });

        var windowFrame = tokenizer.Peek(static (r, next) =>
        {
            return next.CommandOrOperatorText == "rows" || next.CommandOrOperatorText == "range" || next.CommandOrOperatorText == "groups"
                ? WindowFrameParser.Parse(r)
                : null;
        });

        tokenizer.Read(TokenType.CloseParen);

        return new NamelessWindowDefinition(partitionBy, orderBy, windowFrame);
    }
}
