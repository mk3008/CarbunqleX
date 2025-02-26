using Carbunqlex.Clauses;
using Carbunqlex.Lexing;

namespace Carbunqlex.Parsing.Clauses;

public class ForClauseParser
{
    public static ForClause Parse(SqlTokenizer tokenizer)
    {
        tokenizer.Read("for");
        var lockType = ParseLockType(tokenizer);
        return new ForClause(lockType);
    }

    public static string ParseLockType(SqlTokenizer tokenizer)
    {
        return tokenizer.Read(TokenType.Command).Value;
    }
}
