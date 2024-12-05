using Carbunqlex.ValueExpressions;

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
        return $"where {Condition.ToSql()}";
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
