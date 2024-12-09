using System.Text;

namespace Carbunqlex.Clauses;

public enum UnionType
{
    Union,
    UnionAll,
    Intersect,
    Except
}

internal static class UnionTypeExtensions
{
    public static string ToSqlString(this UnionType unionType)
    {
        return unionType switch
        {
            UnionType.Union => "union",
            UnionType.UnionAll => "union all",
            UnionType.Intersect => "intersect",
            UnionType.Except => "except",
            _ => throw new ArgumentOutOfRangeException(nameof(unionType), unionType, null)
        };
    }
}

public class UnionQuery : IQuery
{
    public IQuery Left { get; }
    public IQuery Right { get; }
    public UnionType UnionType { get; }

    public UnionQuery(IQuery left, IQuery right, UnionType unionType)
    {
        Left = left;
        Right = right;
        UnionType = unionType;
    }

    public string ToSqlWithoutCte()
    {
        var sb = new StringBuilder();

        if (includeWithClause)
        {
            // In case of name conflicts, the first occurrence takes precedence
            var uniqueCommonTableClauses = new List<CommonTableClause>();
            var seenAliases = new HashSet<string>();

            foreach (var cte in GetCommonTableClauses())
            {
                if (seenAliases.Add(cte.Alias))
                {
                    uniqueCommonTableClauses.Add(cte);
                }
            }

            // Append the combined WithClause if any
            if (uniqueCommonTableClauses.Any())
            {
                var combinedWithClause = new WithClause(uniqueCommonTableClauses.ToArray());
                sb.Append(combinedWithClause.ToSqlWithoutCte()).Append(" ");
            }
        }

        sb.Append(Left.ToSqlWithoutCte());
        sb.Append(" ");
        sb.Append(UnionType.ToSqlString());
        sb.Append(" ");
        sb.Append(Right.ToSqlWithoutCte());

        return sb.ToString();
    }

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        var lexemes = new List<Lexeme>();

        if (includeWithClause)
        {
            // In case of name conflicts, the first occurrence takes precedence
            var uniqueCommonTableClauses = new List<CommonTableClause>();
            var seenAliases = new HashSet<string>();

            foreach (var cte in GetCommonTableClauses())
            {
                if (seenAliases.Add(cte.Alias))
                {
                    uniqueCommonTableClauses.Add(cte);
                }
            }

            // Append the combined WithClause lexemes if any
            if (uniqueCommonTableClauses.Any())
            {
                var combinedWithClause = new WithClause(uniqueCommonTableClauses.ToArray());
                lexemes.AddRange(combinedWithClause.GetLexemes());
            }
        }

        lexemes.AddRange(Left.GenerateLexemesWithoutCte());
        lexemes.Add(new Lexeme(LexType.Keyword, UnionType.ToSqlString()));
        lexemes.AddRange(Right.GenerateLexemesWithoutCte());

        return lexemes;
    }

    public IEnumerable<CommonTableClause> GetCommonTableClauses()
    {
        var commonTableClauses = new List<CommonTableClause>();
        commonTableClauses.AddRange(Left.GetCommonTableClauses());
        commonTableClauses.AddRange(Right.GetCommonTableClauses());
        return commonTableClauses;
    }
}
