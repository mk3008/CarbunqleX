using Carbunqlex.Parsing;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.QueryNodeTests;

public class SelectEditorTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

    [Fact]
    public void GreatestAndLeastTest()
    {
        // Arrange
        var query = SelectQueryParser.Parse("select a.table_a_id, a.value from table_a as a");

        // Act
        var root = QueryAstParser.Parse(query);
        output.WriteLine(root.Query.ToSql());

        root.ModifyColumn("value", static value => value.Greatest(1).Least(10));

        var actual = root.Query.ToSql();
        output.WriteLine(actual);

        var expected = "select a.table_a_id, least(greatest(a.value, 1), 10) as value from table_a as a";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CoalesceTest()
    {
        // Arrange
        var query = SelectQueryParser.Parse("select a.table_a_id, a.value from table_a as a");

        // Act
        var root = QueryAstParser.Parse(query);
        output.WriteLine(root.Query.ToSql());

        root.ModifyColumn("value", static value => value.Coalesce(1, 2, 3));

        var actual = root.Query.ToSql();
        output.WriteLine(actual);

        var expected = "select a.table_a_id, coalesce(a.value, 1, 2, 3) as value from table_a as a";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ExcludeColumnTest()
    {
        // Arrange
        var query = SelectQueryParser.Parse("select a.table_a_id, a.value from table_a as a");

        // Act
        var root = QueryAstParser.Parse(query);
        output.WriteLine(root.Query.ToSql());

        root.RemoveColumn("value");

        var actual = root.Query.ToSql();
        output.WriteLine(actual);

        var expected = "select a.table_a_id from table_a as a";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ExcludeColumnAliasTest()
    {
        // Arrange
        var query = SelectQueryParser.Parse("select a.table_a_id, a.value as val from table_a as a");

        // Act
        var root = QueryAstParser.Parse(query);
        output.WriteLine(root.Query.ToSql());

        root.RemoveColumn("val");

        var actual = root.Query.ToSql();
        output.WriteLine(actual);

        var expected = "select a.table_a_id from table_a as a";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void SelectColumnTest()
    {
        // Arrange
        var query = SelectQueryParser.Parse("select a.table_a_id from table_a as a");

        // Act
        var root = QueryAstParser.Parse(query);
        output.WriteLine(root.Query.ToSql());

        root.AddColumn("a.value");

        var actual = root.Query.ToSql();
        output.WriteLine(actual);

        var expected = "select a.table_a_id, a.value from table_a as a";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void SelectValueTest()
    {
        // Arrange
        var query = SelectQueryParser.Parse("select a.table_a_id from table_a as a");

        // Act
        var root = QueryAstParser.Parse(query);
        output.WriteLine(root.Query.ToSql());

        root.AddColumn("current_timestamp", "created_at");

        var actual = root.Query.ToSql();
        output.WriteLine(actual);

        var expected = "select a.table_a_id, current_timestamp as created_at from table_a as a";
        Assert.Equal(expected, actual);
    }
}
