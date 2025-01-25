using Carbunqlex.Clauses;
using Carbunqlex.Parsing.ValueExpressionParsing;
using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Parsing;

/// <summary>
/// e.g. partition by a, b
/// </summary>
public class PartitionByClauseParser
{
    private static string ParserName => nameof(PartitionByClauseParser);

    public static PartitionByClause Parse(SqlTokenizer tokenizer)
    {
        tokenizer.Read(ParserName, "partition by");

        var args = ParseArgs(tokenizer);

        return new PartitionByClause(args);
    }

    private static List<IValueExpression> ParseArgs(SqlTokenizer tokenizer)
    {
        var args = new List<IValueExpression>();
        while (true)
        {
            var expression = ValueExpressionParser.Parse(tokenizer);
            args.Add(expression);
            if (tokenizer.TryPeek(out var comma) && comma.Type == TokenType.Comma)
            {
                tokenizer.Read();
                continue;
            }
            break;
        }
        return args;
    }
}
