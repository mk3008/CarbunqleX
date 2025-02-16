using Carbunqlex.DatasourceExpressions;
using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Clauses;

public class UpdateClause : ISqlComponent
{
    public TableSource TableSource { get; set; }

    public string Alias { get; set; }

    public UpdateClause(TableSource tableSource)
    {
        TableSource = tableSource;
        Alias = tableSource.DefaultName;
    }

    public UpdateClause(TableSource tableSource, string alias)
    {
        TableSource = tableSource;
        Alias = alias;
    }

    public string ToSqlWithoutCte()
    {
        if (string.IsNullOrEmpty(Alias) || TableSource.DefaultName == Alias)
        {
            return $"update {TableSource.TableFullName}";
        }
        // Implement the method according to your requirements
        return $"update {TableSource.TableFullName} as {Alias}";
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        yield return new Token(TokenType.Command, "update");
        yield return new Token(TokenType.Identifier, TableSource.TableFullName);
        if (!string.IsNullOrEmpty(Alias) && TableSource.DefaultName != Alias)
        {
            yield return new Token(TokenType.Command, "as");
            yield return new Token(TokenType.Identifier, Alias);
        }
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        yield break;
    }
}

public class SetClause : ISqlComponent
{
    public List<SetExpression> SetExpressions { get; set; }

    public SetClause()
    {
        SetExpressions = new List<SetExpression>();
    }

    public SetClause(List<SetExpression> setExpressions)
    {
        SetExpressions = setExpressions;
    }

    public string ToSqlWithoutCte()
    {
        return "set " + string.Join(", ", SetExpressions.Select(se => se.ToSqlWithoutCte()));
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        yield return new Token(TokenType.Command, "set");
        foreach (var setExpression in SetExpressions)
        {
            foreach (var token in setExpression.GenerateTokensWithoutCte())
            {
                yield return token;
            }
        }
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        foreach (var setExpression in SetExpressions)
        {
            foreach (var query in setExpression.GetQueries())
            {
                yield return query;
            }
        }
    }

    public void Add(SetExpression setExpression)
    {
        SetExpressions.Add(setExpression);
    }
}

public class SetExpression : ISqlComponent
{
    public string ColumnName { get; set; }
    public IValueExpression Value { get; set; }

    public SetExpression(string columnName, IValueExpression value)
    {
        ColumnName = columnName;
        Value = value;
    }

    public string ToSqlWithoutCte()
    {
        return $"{ColumnName} = {Value.ToSqlWithoutCte()}";
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        yield return new Token(TokenType.Identifier, ColumnName);
        yield return new Token(TokenType.Operator, "=");
        foreach (var token in Value.GenerateTokensWithoutCte())
        {
            yield return token;
        }
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        return Value.GetQueries();
    }
}
