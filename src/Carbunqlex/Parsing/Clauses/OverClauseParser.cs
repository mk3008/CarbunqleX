using Carbunqlex.Clauses;
using Carbunqlex.Lexing;

namespace Carbunqlex.Parsing.Clauses;

public class OverClauseParser
{
    public static OverClause Parse(SqlTokenizer tokenizer)
    {
        tokenizer.Read("over");

        var windowFunction = WindowFunctionParser.Parse(tokenizer);

        return new OverClause(windowFunction);
    }
}
