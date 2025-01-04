using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Clauses;

public class SelectExpression : ISqlComponent
{
    public IValueExpression Value { get; internal set; }
    public string Alias { get; }

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

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        var lexemes = new List<Lexeme>(Value.GenerateLexemesWithoutCte());
        if (!string.IsNullOrEmpty(Alias) && Alias != Value.DefaultName)
        {
            lexemes.Add(new Lexeme(LexType.Keyword, "as"));
            lexemes.Add(new Lexeme(LexType.Identifier, Alias));
        }
        return lexemes;
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
