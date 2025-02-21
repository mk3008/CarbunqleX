﻿using Carbunqlex.Clauses;
using Carbunqlex.DatasourceExpressions;
using Carbunqlex.ValueExpressions;

namespace Carbunqlex;

public class FromEditor(ISelectQuery query, IReadOnlyDictionary<string, IValueExpression> keyValues)
{
    public readonly IReadOnlyDictionary<string, IValueExpression> ValueMap = keyValues;

    private readonly ISelectQuery Query = query;

    public DatasourceEditor Join(string joinType, DatasourceExpression datasource, IValueExpression? condition = null)
    {
        if (condition == null)
        {
            var joinClause = new JoinClause(datasource, joinType);
            Query.AddJoin(joinClause);
            return new DatasourceEditor(Query, datasource.Alias);
        }
        else
        {
            var joinClause = new JoinClause(datasource, joinType, condition);
            Query.AddJoin(joinClause);
            return new DatasourceEditor(Query, datasource.Alias);
        }
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

    public DatasourceEditor InnerJoin(string tableName, string alias)
    {
        var condition = BuildJoinCondition(alias);
        var datasource = new DatasourceExpression(new TableSource(tableName), alias);
        return Join("inner join", datasource, condition);
    }

    public DatasourceEditor LeftJoin(string tableName, string alias)
    {
        var condition = BuildJoinCondition(alias);
        var datasource = new DatasourceExpression(new TableSource(tableName), alias);
        return Join("left join", datasource, condition);
    }

    public DatasourceEditor RightJoin(string tableName, string alias)
    {
        var condition = BuildJoinCondition(alias);
        var datasource = new DatasourceExpression(new TableSource(tableName), alias);
        return Join("right join", datasource, condition);
    }

    public DatasourceEditor CrossJoin(string tableName, string alias)
    {
        var datasource = new DatasourceExpression(new TableSource(tableName), alias);
        return Join("cross join", datasource);
    }
}
