using System.Collections.Generic;

namespace Carbunqlex.Clauses;

public class EmptyDistinctClause : IDistinctClause
{
    public string ToSql()
    {
        return string.Empty;
    }

    public IEnumerable<Lexeme> GetLexemes()
    {
        return Enumerable.Empty<Lexeme>();
    }
}
