using Carbunqlex.Clauses;

namespace Carbunqlex.Parsing;

public class OverClauseParser
{
    private static string ParserName => nameof(OverClauseParser);

    public static OverClause Parse(SqlTokenizer tokenizer)
    {
        tokenizer.Read(ParserName, "over");

        var windowFunction = WindowFunctionParser.Parse(tokenizer);

        return new OverClause(windowFunction);
    }
}
