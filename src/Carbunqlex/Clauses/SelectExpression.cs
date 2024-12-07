using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Clauses;

public class SelectExpression : ISqlComponent
{
    public IValueExpression Expression { get; }
    public string Alias { get; }

    public SelectExpression(IValueExpression expression)
    {
        Expression = expression;
        Alias = string.Empty;
    }

    public SelectExpression(IValueExpression expression, string alias)
    {
        Expression = expression;
        Alias = alias;
    }

    public string ToSql()
    {
        var sql = Expression.ToSql();
        if (string.IsNullOrEmpty(Alias) || sql == Alias)
        {
            return sql;
        }
        return $"{sql} as {Alias}";
    }

    public IEnumerable<Lexeme> GetLexemes()
    {
        var lexemes = new List<Lexeme>(Expression.GetLexemes());
        if (!string.IsNullOrEmpty(Alias) && Alias != Expression.DefaultName)
        {
            lexemes.Add(new Lexeme(LexType.Keyword, "as"));
            lexemes.Add(new Lexeme(LexType.Identifier, Alias));
        }
        return lexemes;
    }
}
