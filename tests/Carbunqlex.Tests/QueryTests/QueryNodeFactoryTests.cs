﻿using Carbunqlex.Clauses;
using Carbunqlex.Expressions;
using Carbunqlex.QuerySources;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.QueryTests;

public class QueryNodeFactoryTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

    [Fact]
    public void TestAllComponents()
    {
        // Arrange
        var query = SelectQueryFactory.CreateSelectQueryWithAllComponents();

        // Act
        var queryNode = QueryAstParser.Parse(query);
        output.WriteLine(queryNode.ToTreeString());
    }

    [Fact]
    public void TestWildCard()
    {
        // Arrange
        var query = CreateSelectQuery_WildCard();

        // Act
        var queryNode = QueryAstParser.Parse(query);
        output.WriteLine(queryNode.ToTreeString());

        //queryNode.When(node => node.)

        //var queries = queryNode.FindDeepestQueriesWithColumn("columnname1").ToList();
        //foreach (var q in queries)
        //{
        //    output.WriteLine(q.ToSqlWithoutCte());
        //}
    }

    [Fact]
    public void TestWildCard_TableAlias()
    {
        // TODO: * があったら、のぞくのではなく、そのテーブルの全てのカラムを取得する

        // Arrange
        var query = CreateSelectQuery_WildCard_TableAlias();

        // Act
        var queryNode = QueryAstParser.Parse(query);
        output.WriteLine(queryNode.ToTreeString());

        //var queries = queryNode.FindDeepestQueriesWithColumn("columnname1").ToList();
        //foreach (var q in queries)
        //{
        //    output.WriteLine(q.ToSqlWithoutCte());
        //}
    }

    [Fact]
    public void TestSubQuery()
    {
        // Arrange
        var query = SelectQueryFactory.CreateSelectQueryWithSubQuery("table1", "t1", "sub1", "Column1", "Column2");

        // Act
        var queryNode = QueryAstParser.Parse(query);
        output.WriteLine(queryNode.ToTreeString());
    }

    [Fact]
    public void TestJoinQuery()
    {
        // Arrange
        var query = CreateSelectQueryWithJoin();

        // Act
        var queryNode = QueryAstParser.Parse(query);
        output.WriteLine(queryNode.ToTreeString());
    }

    [Fact]
    public void TestWithQuery()
    {
        // Arrange
        var query = SelectQueryFactory.CreateComplexSelectQuery();

        // Act
        var queryNode = QueryAstParser.Parse(query);
        output.WriteLine(queryNode.ToTreeString());
    }

    [Fact]
    public void TestWithQuery_SubQuery()
    {
        // Arrange
        var query = CreateSelectQueryWithUnionAll_SubQuery();
        // Act
        var queryNode = QueryAstParser.Parse(query);
        output.WriteLine(queryNode.ToTreeString());
    }

    [Fact]
    public void TestUnionQuery()
    {
        // Arrange
        var query = CreateSelectQueryWithUnionAll();

        // Act
        var queryNode = QueryAstParser.Parse(query);
        output.WriteLine(queryNode.ToTreeString());
    }

    private static SelectQuery CreateSelectQueryWithJoin()
    {
        var selectClause = new SelectClause(
            new SelectExpression(new ColumnExpression("t1", "ColumnName1")),
            new SelectExpression(new ColumnExpression("t2", "ColumnName2"))
        );

        var fromClause = new FromClause(new DatasourceExpression(new TableSource("Table1"), "t1"));

        var joinClause = new JoinClause(
            new DatasourceExpression(new TableSource("Table2"), "Table2"),
            "inner join",
            new BinaryExpression(
                "=",
                new ColumnExpression("t1", "JoinColumn"),
                new ColumnExpression("t2", "JoinColumn")
            )
        );

        fromClause.JoinClauses.Add(joinClause);

        var whereClause = new WhereClause(
            new BinaryExpression(
                ">",
                new ColumnExpression("t1", "ColumnName1"),
                new LiteralExpression(100)
            )
        );

        var orderByClause = new OrderByClause(
            new OrderByColumn(new ColumnExpression("t1", "ColumnName1")),
            new OrderByColumn(new ColumnExpression("t2", "ColumnName2"), ascending: false)
        );

        var selectQuery = new SelectQuery(selectClause)
        {
            FromClause = fromClause,
        };
        selectQuery.WhereClause.Add(whereClause.Condition!);
        selectQuery.OrderByClause.OrderByColumns.AddRange(orderByClause.OrderByColumns);

        return selectQuery;
    }

    private static UnionQuery CreateSelectQueryWithUnionAll()
    {
        var firstQuery = SelectQueryFactory.CreateSelectQuery("Table1", "t1", "ColumnName1", "ColumnName2");
        var secondQuery = SelectQueryFactory.CreateSelectQuery("Table2", "t2", "ColumnName1", "ColumnName2");
        var unionAllQuery = new UnionQuery("union", firstQuery, secondQuery);

        return unionAllQuery;
    }

    private static UnionQuery CreateSelectQueryWithUnionAll_SubQuery()
    {
        var firstQuery = SelectQueryFactory.CreateSelectQuery("Table1", "t1", "ColumnName1", "ColumnName2");
        var secondQuery = SelectQueryFactory.CreateSelectQuery("Table2", "t2", "ColumnName1", "ColumnName2");
        var unionAllQuery = new UnionQuery("union", firstQuery, secondQuery);


        var thirdQuery = SelectQueryFactory.CreateSelectQuery("Table3", "t3", "ColumnName1", "ColumnName2");
        var subQuery = new SelectQuery(
            new SelectClause(
                new SelectExpression(new ColumnExpression("subquery", "ColumnName1")),
                new SelectExpression(new ColumnExpression("subquery", "ColumnName2"))
            ),
            fromClause: new FromClause(new DatasourceExpression(new SubQuerySource(thirdQuery), "subquery"))
        );

        var unionAllQueryWithSubQuery = new UnionQuery("union", unionAllQuery, subQuery);
        return unionAllQueryWithSubQuery;
    }

    private static SelectQuery CreateSelectQuery_WildCard()
    {
        var firstQuery = SelectQueryFactory.CreateSelectQuery("Table1", "t1", "ColumnName1", "ColumnName2");

        var secondQuery = new SelectQuery(
           new SelectClause(
               new SelectExpression(new ColumnExpression("*"))
           ),
           fromClause: new FromClause(new DatasourceExpression(new SubQuerySource(firstQuery), "sub"))
        );

        var thirdQuery = new SelectQuery(
           new SelectClause(
               new SelectExpression(new ColumnExpression("*"))
           ),
           fromClause: new FromClause(new DatasourceExpression(new SubQuerySource(secondQuery), "sub"))
        );

        return thirdQuery;
    }

    private static SelectQuery CreateSelectQuery_WildCard_TableAlias()
    {
        var firstQuery = SelectQueryFactory.CreateSelectQuery("Table1", "t1", "ColumnName1", "ColumnName2");

        var secondQuery = new SelectQuery(
           new SelectClause(
               new SelectExpression(new ColumnExpression("sub", "ColumnName1"), "ColumnNameX"),
               new SelectExpression(new ColumnExpression("sub", "*"))
           ),
           fromClause: new FromClause(new DatasourceExpression(new SubQuerySource(firstQuery), "sub"))
        );

        var thirdQuery = new SelectQuery(
           new SelectClause(
               new SelectExpression(new ColumnExpression("*"))
           ),
           fromClause: new FromClause(new DatasourceExpression(new SubQuerySource(secondQuery), "sub"))
        );

        return thirdQuery;
    }
}
