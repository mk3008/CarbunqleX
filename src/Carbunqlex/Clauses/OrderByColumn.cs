using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Clauses;

public class OrderByColumn : ISqlComponent
{
    public IValueExpression Column { get; }
    public bool Ascending { get; }

    public OrderByColumn(IValueExpression column, bool ascending = true)
    {
        Column = column;
        Ascending = ascending;
    }

    public string ToSql()
    {
        if (Ascending)
        {
            return Column.ToSql();
        }
        return $"{Column.ToSql()} desc";
    }

    public IEnumerable<Lexeme> GetLexemes()
    {
        var lexemes = new List<Lexeme>();
        lexemes.AddRange(Column.GetLexemes());
        if (!Ascending)
        {
            lexemes.Add(new Lexeme(LexType.Keyword, "desc"));
        }
        return lexemes;
    }
}
