using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Clauses;

public class SelectExpression : ISqlComponent
{
    public IValueExpression Expression { get; }
    public string Alias { get; }

    public SelectExpression(IValueExpression expression)
    {
        Expression = expression;
        Alias = expression.DefaultName;
    }

    public SelectExpression(IValueExpression expression, string alias)
    {
        Expression = expression;
        Alias = alias;
    }

    public string ToSqlWithoutCte()
    {
        var sql = Expression.ToSqlWithoutCte();
        if (string.IsNullOrEmpty(Alias) || Expression.DefaultName == Alias)
        {
            return sql;
        }
        return $"{sql} as {Alias}";
    }

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        var lexemes = new List<Lexeme>(Expression.GenerateLexemesWithoutCte());
        if (!string.IsNullOrEmpty(Alias) && Alias != Expression.DefaultName)
        {
            lexemes.Add(new Lexeme(LexType.Keyword, "as"));
            lexemes.Add(new Lexeme(LexType.Identifier, Alias));
        }
        return lexemes;
    }

    public IEnumerable<IQuery> GetQueries()
    {
        if (Expression.MightHaveQueries)
        {
            return Expression.GetQueries();
        }
        return Enumerable.Empty<IQuery>();
    }
}
