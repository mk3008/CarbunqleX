using Carbunqlex.Clauses;
using Carbunqlex.DatasourceExpressions;
using Carbunqlex.ValueExpressions;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.QueryTests;

public class QueryNodeFactoryTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

    [Fact]
    public void Test1()
    {
        // Arrange
        var query = SelectQueryFactory.CreateSelectQueryWithAllComponents();

        // Act
        var queryNode = QueryNodeFactory.Create(query);
        output.WriteLine(queryNode.ToTreeString());
        //// Assert
        //Assert.NotNull(queryNode);
        //Assert.Equal(query, queryNode.Query);
        //Assert.Equal(2, queryNode.Columns.Count);
        //Assert.Empty(queryNode.DatasourceNodes);
    }

    [Fact]
    public void TestWildCard()
    {
        // Arrange
        var query = CreateSelectQuery_WildCardDecode();

        // Act
        var queryNode = QueryNodeFactory.Create(query);
        output.WriteLine(queryNode.ToTreeString());
    }

    [Fact]
    public void TestSubQuery()
    {
        // Arrange
        var query = SelectQueryFactory.CreateSelectQueryWithSubQuery("table1", "t1", "sub1", "Column1", "Column2");

        // Act
        var queryNode = QueryNodeFactory.Create(query);
        output.WriteLine(queryNode.ToTreeString());
    }

    [Fact]
    public void TestJoinQuery()
    {
        // Arrange
        var query = CreateSelectQueryWithJoin();

        // Act
        var queryNode = QueryNodeFactory.Create(query);
        output.WriteLine(queryNode.ToTreeString());
    }

    [Fact]
    public void TestWithQuery()
    {
        // Arrange
        var query = SelectQueryFactory.CreateComplexSelectQuery();

        // Act
        var queryNode = QueryNodeFactory.Create(query);
        output.WriteLine(queryNode.ToTreeString());
    }

    [Fact]
    public void TestWithQuery_SubQuery()
    {
        // Arrange
        var query = CreateSelectQueryWithUnionAll_SubQuery();
        // Act
        var queryNode = QueryNodeFactory.Create(query);
        output.WriteLine(queryNode.ToTreeString());
    }

    [Fact]
    public void TestUnionQuery()
    {
        // Arrange
        var query = CreateSelectQueryWithUnionAll();

        // Act
        var queryNode = QueryNodeFactory.Create(query);
        output.WriteLine(queryNode.ToTreeString());
    }

    private static SelectQuery CreateSelectQueryWithJoin()
    {
        var selectClause = new SelectClause(
            new SelectExpression(new ColumnExpression("t1", "ColumnName1")),
            new SelectExpression(new ColumnExpression("t2", "ColumnName2"))
        );

        var fromClause = new FromClause(new TableSource("Table1", "t1"));

        var joinClause = new JoinClause(
            new TableSource("Table2", "t2"),
            JoinType.Inner,
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
                new ConstantExpression(100)
            )
        );

        var orderByClause = new OrderByClause(
            new OrderByColumn(new ColumnExpression("t1", "ColumnName1")),
            new OrderByColumn(new ColumnExpression("t2", "ColumnName2"), ascending: false)
        );

        var selectQuery = new SelectQuery(selectClause)
        {
            FromClause = fromClause,
            WhereClause = whereClause,
        };
        selectQuery.OrderByClause.OrderByColumns.AddRange(orderByClause.OrderByColumns);

        return selectQuery;
    }

    private static UnionQuery CreateSelectQueryWithUnionAll()
    {
        var firstQuery = SelectQueryFactory.CreateSelectQuery("Table1", "t1", "ColumnName1", "ColumnName2");
        var secondQuery = SelectQueryFactory.CreateSelectQuery("Table2", "t2", "ColumnName1", "ColumnName2");
        var unionAllQuery = new UnionQuery(UnionType.Union, firstQuery, secondQuery);

        return unionAllQuery;
    }

    private static UnionQuery CreateSelectQueryWithUnionAll_SubQuery()
    {
        var firstQuery = SelectQueryFactory.CreateSelectQuery("Table1", "t1", "ColumnName1", "ColumnName2");
        var secondQuery = SelectQueryFactory.CreateSelectQuery("Table2", "t2", "ColumnName1", "ColumnName2");
        var unionAllQuery = new UnionQuery(UnionType.Union, firstQuery, secondQuery);


        var thirdQuery = SelectQueryFactory.CreateSelectQuery("Table3", "t3", "ColumnName1", "ColumnName2");
        var subQuery = new SelectQuery(
            new SelectClause(
                new SelectExpression(new ColumnExpression("subquery", "ColumnName1")),
                new SelectExpression(new ColumnExpression("subquery", "ColumnName2"))
            ),
            fromClause: new FromClause(new SubQuerySource(thirdQuery, "subquery"))
        );

        var unionAllQueryWithSubQuery = new UnionQuery(UnionType.Union, unionAllQuery, subQuery);
        return unionAllQueryWithSubQuery;
    }

    private static SelectQuery CreateSelectQuery_WildCardDecode()
    {
        var firstQuery = SelectQueryFactory.CreateSelectQuery("Table1", "t1", "ColumnName1", "ColumnName2");

        var secondQuery = new SelectQuery(
           new SelectClause(
               new SelectExpression(new ColumnExpression("*"))
           ),
           fromClause: new FromClause(new SubQuerySource(firstQuery, "sub"))
        );

        var thirdQuery = new SelectQuery(
           new SelectClause(
               new SelectExpression(new ColumnExpression("*"))
           ),
           fromClause: new FromClause(new SubQuerySource(secondQuery, "sub"))
        );

        return thirdQuery;
    }
}
