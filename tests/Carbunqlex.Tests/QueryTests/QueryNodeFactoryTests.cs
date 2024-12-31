using Carbunqlex.Clauses;
using Carbunqlex.DatasourceExpressions;
using Carbunqlex.ValueExpressions;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.QueryTests;

public class QueryNodeFactoryTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

    [Fact]
    public void QueryNodeTest_Filter()
    {
        // Arrange
        var query = SelectQueryFactory.CreateSelectQuery("table_a", "a", "table_a_id", "value");

        // Act
        var queryNode = QueryNodeFactory.Create(query);
        output.WriteLine(queryNode.ToTreeString());

        queryNode.When("table_a_id", r =>
        {
            r.Equal(100)
                .NotEqual(-100)
                .GreaterThan(50)
                .GreaterThanOrEqual(30)
                .LessThan(200)
                .LessThanOrEqual(300)
                .In(1, 2, 3)
                .In(SelectQueryFactory.CreateSelectQuery("table_b", "b", "table_a_id"))
                .NotIn(4, 5, 6)
                .NotIn(SelectQueryFactory.CreateSelectQuery("table_c", "c", "table_a_id"))
                .Like("%a%")
                .NotLike("%b%")
                .Any(1, 2, 3)
                .Any(r.AddParameter(":prm", new int[] { 1, 2, 3 }));
        });

        output.WriteLine(queryNode.Query.ToSql());
    }

    [Fact]
    public void TestAllComponents()
    {
        // Arrange
        var query = SelectQueryFactory.CreateSelectQueryWithAllComponents();

        // Act
        var queryNode = QueryNodeFactory.Create(query);
        output.WriteLine(queryNode.ToTreeString());

        // when(column, static q => q.EqualTo(100));
        queryNode.When("ColumnName1", accessor =>
        {
            output.WriteLine(accessor.ToString());
            accessor.Equal(100)
                .In(1, 2, 3)
                .In(SelectQueryFactory.CreateSelectQuery("Table1", "t1", "ColumnName1"));
            //.NotIn(4, 5, 6)
            //.NotIn(SelectQueryFactory.CreateSelectQuery("Table2", "t2", "ColumnName1", "ColumnName2"));

        });

        output.WriteLine(queryNode.Query.ToSql());
    }

    [Fact]
    public void TestWildCard()
    {
        // Arrange
        var query = CreateSelectQuery_WildCard();

        // Act
        var queryNode = QueryNodeFactory.Create(query);
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
        var queryNode = QueryNodeFactory.Create(query);
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

    private static SelectQuery CreateSelectQuery_WildCard()
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

    private static SelectQuery CreateSelectQuery_WildCard_TableAlias()
    {
        var firstQuery = SelectQueryFactory.CreateSelectQuery("Table1", "t1", "ColumnName1", "ColumnName2");

        var secondQuery = new SelectQuery(
           new SelectClause(
               new SelectExpression(new ColumnExpression("sub", "ColumnName1"), "ColumnNameX"),
               new SelectExpression(new ColumnExpression("sub", "*"))
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
