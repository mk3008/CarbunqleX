
using Carbunqlex.DatasourceExpressions;
using Carbunqlex.Lexing;

namespace Carbunqlex.Clauses;

public class InsertClause : ISqlComponent
{
    public TableSource TableSource { get; set; }

    public List<string> ColumnNames { get; set; }

    public InsertClause(TableSource tableSource, List<string> columnNames)
    {
        TableSource = tableSource;
        ColumnNames = columnNames;
    }

    public InsertClause(TableSource tableSource)
    {
        TableSource = tableSource;
        ColumnNames = new List<string>();
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        yield return new Token(TokenType.Command, "insert into");
        yield return new Token(TokenType.Identifier, TableSource.TableFullName);
        if (TableSource.ColumnNames.Any())
        {
            yield return new Token(TokenType.OpenParen, "(");
            foreach (var columnName in TableSource.ColumnNames)
            {
                yield return new Token(TokenType.Identifier, columnName);
                yield return new Token(TokenType.Comma, ",");
            }
            yield return new Token(TokenType.CloseBracket, ")");
        }
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        yield break;
    }

    public string ToSqlWithoutCte()
    {
        if (ColumnNames.Any())
        {
            return $"insert into {TableSource.TableFullName}({string.Join(", ", ColumnNames)})";
        }
        return $"insert into {TableSource.TableFullName}";
    }
}
