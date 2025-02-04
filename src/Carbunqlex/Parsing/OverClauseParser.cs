using Carbunqlex.Clauses;

namespace Carbunqlex.Parsing;

public class OverClauseParser
{
    public static OverClause Parse(SqlTokenizer tokenizer)
    {
        tokenizer.Read("over");

        var windowFunction = WindowFunctionParser.Parse(tokenizer);

        return new OverClause(windowFunction);
    }
}
