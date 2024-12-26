using Carbunqlex.Clauses;
using Carbunqlex.DatasourceExpressions;
using Carbunqlex.ValueExpressions;
using System.Text;

namespace Carbunqlex;

public class ValuesQuery : IQuery
{
    public readonly List<ValuesRow> Rows = new();
    private int? columnCount;

    public void AddRow(IEnumerable<IValueExpression> columns)
    {
        var row = columns.ToList();
        if (columnCount == null)
        {
            columnCount = row.Count;
        }
        else if (row.Count != columnCount)
        {
            throw new ArgumentException($"All rows must have the same number of columns. Expected {columnCount}, but got {row.Count}.");
        }

        Rows.Add(new ValuesRow(row));
    }

    public string ToSql()
    {
        var sb = new StringBuilder("values ");
        for (int i = 0; i < Rows.Count; i++)
        {
            sb.Append(Rows[i].ToSqlWithoutCte());
            if (i < Rows.Count - 1)
            {
                sb.Append(", ");
            }
        }
        return sb.ToString();
    }

    public string ToSqlWithoutCte()
    {
        return ToSql();
    }

    public IEnumerable<Lexeme> GenerateLexemes()
    {
        int capacity = Rows.Sum(row => row.Capacity) + (Rows.Count - 1) + 1;

        var lexemes = new List<Lexeme>(capacity) { new Lexeme(LexType.Keyword, "VALUES") };

        for (int i = 0; i < Rows.Count; i++)
        {
            lexemes.AddRange(Rows[i].GenerateLexemesWithoutCte());
            if (i < Rows.Count - 1)
            {
                lexemes.Add(new Lexeme(LexType.Comma, ","));
            }
        }
        return lexemes;
    }

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        return GenerateLexemes();
    }

    public IEnumerable<CommonTableClause> GetCommonTableClauses()
    {
        var commonTableClauses = new List<CommonTableClause>();

        foreach (var query in GetQueries())
        {
            commonTableClauses.AddRange(query.GetCommonTableClauses());
        }

        return commonTableClauses
            .GroupBy(cte => cte.Alias)
            .Select(group => group.First())
            .OrderByDescending(cte => cte.IsRecursive);
    }

    public IEnumerable<IQuery> GetQueries()
    {
        var queries = new List<IQuery>();

        queries.Add(this);
        foreach (var row in Rows)
        {
            queries.AddRange(row.GetQueries());
        }

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

    public IEnumerable<SelectExpression> GetSelectExpressions()
    {
        return Enumerable.Empty<SelectExpression>();
    }

    public IEnumerable<IDatasource> GetDatasources()
    {
        return Enumerable.Empty<IDatasource>();
    }
}
