using Carbunqlex.Clauses;
using Carbunqlex.Lexing;

namespace Carbunqlex.Expressions;

/// <summary>
/// Represents the ORDER BY clause in the arguments of array_agg
/// </summary>
public class OrderByExpression : IValueExpression
{
    public OrderByClause OrderByClause { get; set; }

    public OrderByExpression(OrderByClause orderByClause)
    {
        OrderByClause = orderByClause;
    }

    public string DefaultName => string.Empty;

    public bool MightHaveQueries => OrderByClause.MightHaveQueries;

    public IEnumerable<ColumnExpression> ExtractColumnExpressions()
    {
        return OrderByClause.OrderByColumns.SelectMany(c => c.Column.ExtractColumnExpressions());
    }

    public string ToSqlWithoutCte()
    {
        return OrderByClause.ToSqlWithoutCte();
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        return OrderByClause.GenerateTokensWithoutCte();
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        return OrderByClause.GetQueries();
    }
}
