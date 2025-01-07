using Carbunqlex.Clauses;
using Carbunqlex.DatasourceExpressions;
using Carbunqlex.ValueExpressions;

namespace Carbunqlex;

public class ColumnModifier(ISelectQuery query, IValueExpression expression, SelectExpression? selectExpression = null)
{
    public ISelectQuery Query { get; } = query;

    public IValueExpression Value { get; internal set; } = expression;

    private SelectExpression? _selectExpression = selectExpression;
    public SelectExpression? SelectExpression => _selectExpression ??= Query.GetSelectExpressions().Where(e => e.Value == Value).FirstOrDefault();

    private SelectModifier? _selectModifier;
    public SelectModifier SelectModifier => _selectModifier ??= new(this);

    private WhereModifier? _whereModifier;
    public WhereModifier WhereModifier => _whereModifier ??= new(this);

    private JoinModifier? _fromModifier;
    public JoinModifier FromModifier => _fromModifier ??= new(Query, new Dictionary<string, IValueExpression>() { { Value.DefaultName.ToLowerInvariant(), Value } });

    public override string ToString()
    {
        try
        {
            return $"{Value.ToSqlWithoutCte()} : {Query.ToSqlWithoutCte()}";
        }
        catch (Exception ex)
        {
            return ex.ToString();
        }
    }

    //internal void AddCondition(string operatorSymbol, IValueExpression rightValue)
    //{
    //    var condition = new BinaryExpression(operatorSymbol, Value, rightValue);
    //    AddCondition(condition);
    //}

    public ParameterExpression AddParameter(string name, object value)
    {
        return Query.AddParameter(name, value);
    }

    public SelectModifier AddColumn(SelectExpression expr)
    {
        Query.AddColumn(expr);
        return new SelectModifier(new ColumnModifier(Query, expr.Value, expr));
    }

    public SelectModifier AddColumn(IValueExpression value)
    {
        return AddColumn(value, value.DefaultName);
    }

    public SelectModifier AddColumn(IValueExpression value, string alias)
    {
        var expr = new SelectExpression(value, alias);
        Query.AddColumn(expr);
        return new SelectModifier(new ColumnModifier(Query, expr.Value, expr));
    }

    internal static IValueExpression ToValueExpression(object value)
    {
        return value is IValueExpression expr
            ? expr
            : ValueBuilder.Constant(value);
    }
}

public class DatasourceModifier
{
    private readonly ISelectQuery Query;

    private readonly string DatasourceAlias;

    public DatasourceModifier(ISelectQuery query, string datasourceAlias)
    {
        Query = query;
        DatasourceAlias = datasourceAlias;
    }

    public SelectModifier AddColumn(string columnName)
    {
        return AddColumn(columnName, columnName);
    }

    public SelectModifier AddColumn(string columnName, string columnAlias)
    {
        var expr = new SelectExpression(new ColumnExpression(DatasourceAlias, columnName), columnAlias);
        Query.AddColumn(expr);
        return new SelectModifier(new ColumnModifier(Query, expr.Value, expr));
    }

    public WhereModifier Filter(string columnName)
    {
        return new WhereModifier(new ColumnModifier(Query, new ColumnExpression(DatasourceAlias, columnName)));
    }
}

public class JoinModifier(ISelectQuery query, IReadOnlyDictionary<string, IValueExpression> keyValues)
{
    public readonly IReadOnlyDictionary<string, IValueExpression> Values = keyValues;

    private readonly ISelectQuery Query = query;

    public DatasourceModifier Join(JoinType joinType, IDatasource datasource, IValueExpression? condition = null)
    {
        if (condition == null)
        {
            var joinClause = new JoinClause(datasource, joinType);
            Query.AddJoin(joinClause);
            return new DatasourceModifier(Query, datasource.Alias);
        }
        else
        {
            var joinClause = new JoinClause(datasource, joinType, condition);
            Query.AddJoin(joinClause);
            return new DatasourceModifier(Query, datasource.Alias);
        }
    }

    private IValueExpression? BuildJoinCondition(string alias)
    {
        IValueExpression? condition = null;
        foreach (var (key, value) in Values)
        {
            if (condition == null)
            {
                condition = value;
                condition = condition.Equal(new ColumnExpression(alias, value.DefaultName ?? key));
            }
            else
            {
                condition = condition.And(value.Equal(new ColumnExpression(alias, value.DefaultName ?? key)));
            }
        }
        return condition;
    }

    public DatasourceModifier InnerJoin(string tableName, string alias)
    {
        var condition = BuildJoinCondition(alias);
        var datasource = new TableSource(tableName, alias);
        return Join(JoinType.Inner, datasource, condition);
    }

    public DatasourceModifier LeftJoin(string tableName, string alias)
    {
        var condition = BuildJoinCondition(alias);
        var datasource = new TableSource(tableName, alias);
        return Join(JoinType.Left, datasource, condition);
    }

    public DatasourceModifier RightJoin(string tableName, string alias)
    {
        var condition = BuildJoinCondition(alias);
        var datasource = new TableSource(tableName, alias);
        return Join(JoinType.Right, datasource, condition);
    }

    public DatasourceModifier CrossJoin(string tableName, string alias)
    {
        var datasource = new TableSource(tableName, alias);
        return Join(JoinType.Cross, datasource);
    }
}
