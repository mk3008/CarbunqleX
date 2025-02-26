using Carbunqlex.Expressions;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.QueryTests;

public class ValuesQueryTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

    [Fact]
    public void AddRow_ShouldAddRow_WhenColumnsAreValid()
    {
        // Arrange
        var query = new ValuesQuery();
        var columns = new List<IValueExpression>
        {
            ValueBuilder.Constant(1),
            ValueBuilder.Constant("test")
        };

        // Act
        query.AddRow(columns);

        // Assert
        Assert.Single(query.Rows);
        Assert.Equal(2, query.Rows[0].Columns.Count);
    }

    [Fact]
    public void ToSql_ShouldReturnCorrectSql()
    {
        // Arrange
        var query = new ValuesQuery();
        var columns1 = new List<IValueExpression>
        {
            ValueBuilder.Constant(1),
            ValueBuilder.Constant("test"),
            ValueBuilder.Null
        };
        var columns2 = new List<IValueExpression>
        {
            ValueBuilder.Constant(2),
            ValueBuilder.Constant("example"),
            ValueBuilder.Constant(new DateTime(2001,2,3))
        };
        var columns3 = new List<IValueExpression>
        {
            ValueBuilder.Constant(3),
            ValueBuilder.Constant("O'Reilly"),
            ValueBuilder.Null
        };
        query.AddRow(columns1);
        query.AddRow(columns2);
        query.AddRow(columns3);

        // Act
        var sql = query.ToSql();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("values (1, 'test', null), (2, 'example', '2001-02-03 00:00:00'), (3, 'O''Reilly', null)", sql);
    }

    [Fact]
    public void AddRow_ShouldAddRow_WhenColumnsContainDouble()
    {
        // Arrange
        var query = new ValuesQuery();
        var columns = new List<IValueExpression>
        {
            ValueBuilder.Constant(1.23),
            ValueBuilder.Constant("double test")
        };
        query.AddRow(columns);

        // Act
        var sql = query.ToSql();
        output.WriteLine(sql);

        // Assert
        Assert.Equal("values (1.23, 'double test')", sql);
    }
}
