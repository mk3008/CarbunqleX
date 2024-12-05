using Carbunqlex.ValueExpressions;

namespace Carbunqlex.Clauses;

public class HavingClause : ISqlComponent
{
    public IValueExpression Condition { get; set; }

    public HavingClause(IValueExpression condition)
    {
        Condition = condition;
    }

    public string ToSql()
    {
        return $"having {Condition.ToSql()}";
    }

    public IEnumerable<Lexeme> GetLexemes()
    {
        var lexemes = new List<Lexeme> {
            new Lexeme(LexType.StartClause, "having", "having")
        };
        lexemes.AddRange(Condition.GetLexemes());
        lexemes.Add(new Lexeme(LexType.EndClause, string.Empty, "having"));
        return lexemes;
    }

}
