using Carbunqlex.Clauses;
using System.Text;

namespace Carbunqlex;

public enum UnionType : byte
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

    /// <summary>
    /// Generates the SQL string for the union query, including the WITH clause if necessary.
    /// If there are duplicate CTEs, the one defined in the Left query takes precedence.
    /// </summary>
    /// <returns>The SQL string representation of the union query.</returns>
    public string ToSql()
    {
        var sb = new StringBuilder();

        // Generate merged WITH clause
        var withClause = new WithClause(GetCommonTableClauses()).ToSql();
        if (!string.IsNullOrEmpty(withClause))
        {
            sb.Append(withClause);
            sb.Append(" ");
        }

        // Combine Left and Right queries, excluding the WITH clause
        sb.Append(Left.ToSqlWithoutCte());
        sb.Append(" ");
        sb.Append(UnionType.ToSqlString());
        sb.Append(" ");
        sb.Append(Right.ToSqlWithoutCte());

        return sb.ToString();
    }

    public IEnumerable<Lexeme> GenerateLexemes()
    {
        var leftLexemes = Left.GenerateLexemes().ToList();
        var rightLexemes = Right.GenerateLexemes().ToList();

        // Initial capacity is set to accommodate the lexemes from Left, Right, and the UnionType keyword.
        var lexemes = new List<Lexeme>(leftLexemes.Count + rightLexemes.Count + 1);
        lexemes.AddRange(leftLexemes);
        lexemes.Add(new Lexeme(LexType.Keyword, UnionType.ToSqlString()));
        lexemes.AddRange(rightLexemes);
        return lexemes;
    }

    public string ToSqlWithoutCte()
    {
        return $"{Left.ToSqlWithoutCte()} {UnionType.ToSqlString()} {Right.ToSqlWithoutCte()}";
    }

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        var leftLexemes = Left.GenerateLexemesWithoutCte().ToList();
        var rightLexemes = Right.GenerateLexemesWithoutCte().ToList();

        // Initial capacity is set to accommodate the lexemes from Left, Right, and the UnionType keyword.
        var lexemes = new List<Lexeme>(leftLexemes.Count + rightLexemes.Count + 1);
        lexemes.AddRange(leftLexemes);
        lexemes.Add(new Lexeme(LexType.Keyword, UnionType.ToSqlString()));
        lexemes.AddRange(rightLexemes);
        return lexemes;
    }

    public IEnumerable<CommonTableClause> GetCommonTableClauses()
    {
        var queries = Left.GetCommonTableClauses().Union(Right.GetCommonTableClauses());

        var commonTables = new List<(CommonTableClause Cte, int Index)>();
        commonTables.AddRange(queries.Select((cte, index) => (cte, index + commonTables.Count)));

        return commonTables
            .GroupBy(ct => ct.Cte.Alias)
            .Select(group => group.First())
            .OrderByDescending(cte => cte.Cte.IsRecursive)
            .ThenBy(ct => ct.Index)
            .Select(ct => ct.Cte);
    }

    public IEnumerable<IQuery> GetQueries()
    {
        var queries = new List<IQuery>
        {
            this
        };
        queries.AddRange(Left.GetQueries());
        queries.AddRange(Right.GetQueries());

        return queries;
    }

    public Dictionary<string, object?> Parameters { get; } = new();

    public IDictionary<string, object?> GetParameters()
    {
        var parameters = new Dictionary<string, object?>();

        // Add own parameters first
        foreach (var parameter in Parameters)
        {
            parameters[parameter.Key] = parameter.Value;
        }

        // Add internal parameters, excluding duplicates
        foreach (var parameter in GetQueries().Where(q => q != this).SelectMany(q => q.GetParameters()))
        {
            if (!parameters.ContainsKey(parameter.Key))
            {
                parameters[parameter.Key] = parameter.Value;
            }
        }

        return parameters;
    }
}
