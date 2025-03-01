using Carbunqlex.Clauses;
using Carbunqlex.Expressions;
using Carbunqlex.Lexing;
using System.Text;

namespace Carbunqlex.Expressions;

public interface IArgumentExpression : ISqlComponent
{
    bool MightHaveQueries { get; }
    IEnumerable<ColumnExpression> ExtractColumnExpressions();
}

/// <summary>
/// Represents a list of values for a function or operator.
/// </summary>
public class ArgumentExpression : IArgumentExpression
{
    public List<IValueExpression> Values { get; }

    /// <summary>
    /// Optional ORDER BY clause for the arguments.
    /// e.g. array_agg(value order by sort_column)
    /// </summary>
    public OrderByClause? OrderByClause { get; set; }

    public ArgumentExpression(params IValueExpression[] values)
    {
        Values = values.ToList();
        OrderByClause = null;
    }

    public ArgumentExpression(IEnumerable<IValueExpression> values)
    {
        Values = values.ToList();
        OrderByClause = null;
    }

    public ArgumentExpression(List<IValueExpression> values)
    {
        Values = values;
        OrderByClause = null;
    }

    /// <summary>
    /// Constructor for ORDER BY clause.
    /// e.g. array_agg(value order by sort_column)
    /// </summary>
    /// <param name="values"></param>
    /// <param name="orderBy"></param>
    public ArgumentExpression(List<IValueExpression> values, OrderByClause orderBy)
    {
        Values = values;
        OrderByClause = orderBy;
    }

    public bool MightHaveQueries => Values.Any(v => v.MightHaveQueries);

    public string ToSqlWithoutCte()
    {
        var sb = new StringBuilder();
        sb.Append(string.Join(", ", Values.Select(v => v.ToSqlWithoutCte())));
        if (OrderByClause != null)
        {
            sb.Append(" ").Append(OrderByClause.ToSqlWithoutCte());
        }
        return sb.ToString();
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        foreach (var value in Values)
        {
            foreach (var lexeme in value.GenerateTokensWithoutCte())
            {
                yield return lexeme;
            }
        }
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        var queries = new List<ISelectQuery>();
        foreach (var value in Values)
        {
            if (value.MightHaveQueries)
            {
                queries.AddRange(value.GetQueries());
            }
        }
        return queries;
    }

    public IEnumerable<ColumnExpression> ExtractColumnExpressions()
    {
        var columns = new List<ColumnExpression>();
        foreach (var value in Values)
        {
            columns.AddRange(value.ExtractColumnExpressions());
        }
        return columns;
    }
}
