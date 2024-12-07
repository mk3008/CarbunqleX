using Carbunqlex.ValueExpressions;
using System.Text;

namespace Carbunqlex.Clauses;

public class WhereClause : ISqlComponent
{
    public IValueExpression Condition { get; set; }

    public WhereClause(IValueExpression condition)
    {
        Condition = condition;
    }

    public string ToSql()
    {
        var sb = new StringBuilder();
        sb.Append("where ");
        sb.Append(Condition.ToSql());
        return sb.ToString();
    }

    public IEnumerable<Lexeme> GetLexemes()
    {
        var lexemes = new List<Lexeme> {
            new Lexeme(LexType.StartClause, "where", "where")
        };
        lexemes.AddRange(Condition.GetLexemes());
        lexemes.Add(new Lexeme(LexType.EndClause, string.Empty, "where"));
        return lexemes;
    }
}
