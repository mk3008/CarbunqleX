using Carbunqlex.ValueExpressions;
using System.Text;

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

    public string ToSqlWithoutCte()
    {
        var sb = new StringBuilder();
        sb.Append(Column.ToSqlWithoutCte());
        if (!Ascending)
        {
            sb.Append(" desc");
        }
        return sb.ToString();
    }

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        var lexemes = new List<Lexeme>();
        lexemes.AddRange(Column.GenerateLexemesWithoutCte());
        if (!Ascending)
        {
            lexemes.Add(new Lexeme(LexType.Keyword, "desc"));
        }
        return lexemes;
    }

    public IEnumerable<CommonTableClause> GetCommonTableClauses()
    {
        if (Column.MightHaveCommonTableClauses)
        {
            return Column.GetCommonTableClauses();
        }
        return Enumerable.Empty<CommonTableClause>();
    }
}
