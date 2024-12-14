using System.Text;

namespace Carbunqlex.Clauses;

public class WithClause : ISqlComponent
{
    public List<CommonTableClause> CommonTableClauses { get; }
    public bool IsRecursive => CommonTableClauses.Any(cte => cte.IsRecursive);

    public WithClause(params CommonTableClause[] commonTableClauses)
    {
        CommonTableClauses = commonTableClauses.ToList();
    }

    public WithClause(IEnumerable<CommonTableClause> commonTableClauses)
    {
        CommonTableClauses = commonTableClauses.ToList();
    }

    public string ToSql()
    {
        if (CommonTableClauses.Count == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        sb.Append("with ");
        if (IsRecursive)
        {
            sb.Append("recursive ");
        }

        var orderedCtes = GetOrderedCtes();

        sb.Append(string.Join(", ", orderedCtes.Select(cte => cte.ToSqlWithoutCte())));
        return sb.ToString();
    }

    public string ToSqlWithoutCte()
    {
        return string.Empty;
    }

    public IEnumerable<Lexeme> GenerateLexemes()
    {
        if (CommonTableClauses.Count == 0)
        {
            return Enumerable.Empty<Lexeme>();
        }

        var lexemes = new List<Lexeme>();

        if (IsRecursive)
        {
            lexemes.Add(new Lexeme(LexType.StartClause, "with recursive", "with"));
        }
        else
        {
            lexemes.Add(new Lexeme(LexType.StartClause, "with", "with"));
        }

        var orderedCtes = GetOrderedCtes();

        foreach (var cte in orderedCtes)
        {
            lexemes.AddRange(cte.GenerateLexemesWithoutCte());
            lexemes.Add(Lexeme.Comma);
        }

        if (lexemes.Count > 1)
        {
            // Remove the last comma
            lexemes.RemoveAt(lexemes.Count - 1);
        }

        lexemes.Add(new Lexeme(LexType.EndClause, string.Empty, "with"));

        return lexemes;
    }

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        return Enumerable.Empty<Lexeme>();
    }

    public IEnumerable<CommonTableClause> GetCommonTableClauses()
    {
        return CommonTableClauses;
    }

    /// <summary>
    /// Orders the common table expressions (CTEs) such that recursive CTEs appear first,
    /// while maintaining the original order for non-recursive CTEs.
    /// Additionally, ensures that CTEs with the same name are not duplicated, with precedence given to the first occurrence.
    /// Note: This method does not raise an error for duplicate CTE names; it simply prioritizes the first occurrence.
    /// If you need to check for errors, please run the TryValidate method manually before calling this method.
    /// This method is necessary to ensure that recursive CTEs are prioritized in the SQL generation process,
    /// as required by SQL standards and query execution plans.
    /// </summary>
    /// <returns>An ordered list of common table clauses.</returns>
    private IEnumerable<CommonTableClause> GetOrderedCtes()
    {
        // Prioritize Recursive, maintain original order for others, and ensure unique names
        return CommonTableClauses
            .Select((cte, index) => new { Cte = cte, Index = index })
            .GroupBy(cte => cte.Cte.Alias)
            .Select(group => group.First())
            .OrderByDescending(cte => cte.Cte.IsRecursive)
            .ThenBy(cte => cte.Index)
            .Select(cte => cte.Cte);
    }

    /// <summary>
    /// Validates the state of the WITH clause.
    /// Checks for duplicate CTE names and multiple recursive CTEs.
    /// If duplicate CTE names are found, it checks if their SQL strings are identical.
    /// If they are identical, the duplicates are considered valid.
    /// </summary>
    /// <param name="errorMessages">An enumerable of error messages if validation fails.</param>
    /// <returns>True if the WITH clause is valid, otherwise false.</returns>
    public bool TryValidate(out IEnumerable<string> errorMessages)
    {
        var errors = new List<string>();

        var duplicateNames = CommonTableClauses
            .Select((cte, index) => new { cte.Alias, Index = index, Sql = cte.ToSqlWithoutCte() })
            .GroupBy(cte => cte.Alias)
            .Where(group => group.Count() > 1)
            .Select(group => new { Alias = group.Key, Indices = group.Select(g => g.Index).ToList(), Sqls = group.Select(g => g.Sql).Distinct().ToList() })
            .ToList();

        if (duplicateNames.Any())
        {
            foreach (var duplicate in duplicateNames)
            {
                if (duplicate.Sqls.Count > 1)
                {
                    errors.Add($"Duplicate CTE name '{duplicate.Alias}' found at indices: {string.Join(", ", duplicate.Indices)} with different SQL definitions.");
                }
            }
        }

        var recursiveCount = CommonTableClauses.Count(cte => cte.IsRecursive);
        if (recursiveCount > 1)
        {
            errors.Add("Multiple recursive CTEs found.");
        }

        errorMessages = errors;
        return !errors.Any();
    }
}
