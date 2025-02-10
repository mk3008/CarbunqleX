using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Clauses;

public class SelectExpression : ISqlComponent
{
    public IValueExpression Value { get; internal set; }
    public string Alias { get; set; }

    public SelectExpression(IValueExpression expression)
    {
        Value = expression;
        Alias = expression.DefaultName;
    }

    public SelectExpression(IValueExpression expression, string alias)
    {
        Value = expression;
        Alias = alias;
    }

    public string ToSqlWithoutCte()
    {
        var sql = Value.ToSqlWithoutCte();
        if (string.IsNullOrEmpty(Alias) || Value.DefaultName == Alias)
        {
            return sql;
        }
        return $"{sql} as {Alias}";
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        var tokens = new List<Token>(Value.GenerateTokensWithoutCte());
        if (!string.IsNullOrEmpty(Alias) && Alias != Value.DefaultName)
        {
            tokens.Add(new Token(TokenType.Command, "as"));
            tokens.Add(new Token(TokenType.Identifier, Alias));
        }
        return tokens;
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        if (Value.MightHaveQueries)
        {
            return Value.GetQueries();
        }
        return Enumerable.Empty<ISelectQuery>();
    }
}
