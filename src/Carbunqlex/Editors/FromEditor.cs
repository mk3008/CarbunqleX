using Carbunqlex.Clauses;
using Carbunqlex.Expressions;
using Carbunqlex.QuerySources;

namespace Carbunqlex.Editors;

public class FromEditor(QueryNode node, IReadOnlyDictionary<string, IValueExpression> keyValues)
{
    public readonly IReadOnlyDictionary<string, IValueExpression> ValueMap = keyValues;

    private readonly ISelectQuery Query = node.Query;

    private readonly QueryNode Node = node;

    public FromEditor EditQuery(Action<QueryNode> action)
    {
        action(Node);
        return this;
    }

    private FromEditor Join(string joinType, DatasourceExpression datasource, IValueExpression? condition = null)
    {
        if (condition == null)
        {
            var joinClause = new JoinClause(datasource, joinType);
            Query.AddJoin(joinClause);
        }
        else
        {
            var joinClause = new JoinClause(datasource, joinType, condition);
            Query.AddJoin(joinClause);
        }
        return this;
    }

    public FromEditor Join(string joinType, DatasourceExpression datasource, Func<IReadOnlyDictionary<string, IValueExpression>, DatasourceExpression, IValueExpression> on)
    {
        return Join(joinType, datasource, on(ValueMap, datasource));
    }

    private IValueExpression? BuildJoinCondition(string alias)
    {
        IValueExpression? condition = null;
        foreach (var (key, value) in ValueMap)
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

    public FromEditor InnerJoin(string tableName, string alias)
    {
        var condition = BuildJoinCondition(alias);
        var datasource = new DatasourceExpression(new TableSource(tableName), alias);
        return Join("inner join", datasource, condition);
    }

    public FromEditor LeftJoin(string tableName, string alias)
    {
        var condition = BuildJoinCondition(alias);
        var datasource = new DatasourceExpression(new TableSource(tableName), alias);
        return Join("left join", datasource, condition);
    }

    public FromEditor RightJoin(string tableName, string alias)
    {
        var condition = BuildJoinCondition(alias);
        var datasource = new DatasourceExpression(new TableSource(tableName), alias);
        return Join("right join", datasource, condition);
    }

    public FromEditor CrossJoin(string tableName, string alias)
    {
        var datasource = new DatasourceExpression(new TableSource(tableName), alias);
        return Join("cross join", datasource);
    }
}
