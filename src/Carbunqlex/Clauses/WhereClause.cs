using Carbunqlex.ValueExpressions;
using System.Text;

namespace Carbunqlex.Clauses;

public class WhereClause : IWhereClause
{
    public IValueExpression Condition { get; set; }

    public WhereClause(IValueExpression condition)
    {
        Condition = condition;
    }

    public string ToSqlWithoutCte()
    {
        var sb = new StringBuilder();
        sb.Append("where ");
        sb.Append(Condition.ToSqlWithoutCte());
        return sb.ToString();
    }

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        var lexemes = new List<Lexeme> {
            new Lexeme(LexType.StartClause, "where", "where")
        };
        lexemes.AddRange(Condition.GenerateLexemesWithoutCte());
        lexemes.Add(new Lexeme(LexType.EndClause, string.Empty, "where"));
        return lexemes;
    }

    public IEnumerable<CommonTableClause> GetCommonTableClauses()
    {
        if (!Condition.MightHaveCommonTableClauses)
        {
            return Enumerable.Empty<CommonTableClause>();
        }
        return Condition.GetCommonTableClauses();
    }
}
