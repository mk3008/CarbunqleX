namespace Carbunqlex.Parsing;


public class CreateTableAsQueryParser
{
    public static CreateTableAsQuery Parse(string sql)
    {
        var tokenizer = new SqlTokenizer(sql);
        return Parse(tokenizer);
    }

    public static CreateTableAsQuery Parse(SqlTokenizer tokenizer)
    {
        var command = tokenizer.Read(["create table", "create temporary table"]);
        var isTemporary = command.CommandOrOperatorText == "create temporary table";

        var tableSource = TableSourceParser.Parse(tokenizer);

        tokenizer.Read("as");

        var selectQuery = SelectQueryParser.Parse(tokenizer);

        return new CreateTableAsQuery(tableSource, selectQuery, isTemporary);
    }
}
