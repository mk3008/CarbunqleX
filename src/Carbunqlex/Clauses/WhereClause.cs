using Carbunqlex.ValueExpressions;
using System.Text;

namespace Carbunqlex.Clauses;

public class WhereClause : ISqlComponent
{
    public IValueExpression? Condition { get; set; }

    public WhereClause()
    {
        Condition = null;
    }

    public WhereClause(IValueExpression condition)
    {
        Condition = condition;
    }

    public string ToSqlWithoutCte()
    {
        if (Condition == null)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        sb.Append("where ");
        sb.Append(Condition.ToSqlWithoutCte());
        return sb.ToString();
    }

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        if (Condition == null)
        {
            return Enumerable.Empty<Lexeme>();
        }

        var lexemes = new List<Lexeme> {
            new Lexeme(LexType.StartClause, "where", "where")
        };
        lexemes.AddRange(Condition.GenerateLexemesWithoutCte());
        lexemes.Add(new Lexeme(LexType.EndClause, string.Empty, "where"));
        return lexemes;
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        if (Condition == null)
        {
            return Enumerable.Empty<ISelectQuery>();
        }

        if (Condition.MightHaveQueries)
        {
            return Condition.GetQueries();
        }
        return Enumerable.Empty<ISelectQuery>();
    }

    public IEnumerable<ColumnExpression> ExtractColumnExpressions()
    {
        if (Condition == null)
        {
            return Enumerable.Empty<ColumnExpression>();
        }

        return Condition.ExtractColumnExpressions();
    }

    public void AddCondition(string @operator, IValueExpression expression)
    {
        if (Condition == null)
        {
            Condition = expression;
        }
        else
        {
            Condition = new BinaryExpression(@operator, Condition, expression);
        }
    }

    public void And(IValueExpression expression)
    {
        AddCondition("and", expression);
    }
}
