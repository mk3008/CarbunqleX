using System.Text;

namespace Carbunqlex.Clauses;

public class SelectClause : ISqlComponent
{
    public List<SelectExpression> Expressions { get; }
    public IDistinctClause DistinctClause { get; set; }

    public SelectClause(params SelectExpression[] selectExpressions)
    {
        DistinctClause = new EmptyDistinctClause();
        Expressions = selectExpressions.ToList();
    }

    public SelectClause(IDistinctClause distinctClause, params SelectExpression[] selectExpressions)
    {
        DistinctClause = distinctClause;
        Expressions = selectExpressions.ToList();
    }

    public string ToSql()
    {
        if (Expressions.Count == 0)
        {
            throw new InvalidOperationException("SelectClause must contain at least one expression.");
        }

        var sb = new StringBuilder();
        sb.Append("select ");

        var distinctSql = DistinctClause.ToSql();
        if (!string.IsNullOrEmpty(distinctSql))
        {
            sb.Append(distinctSql).Append(" ");
        }

        foreach (var column in Expressions)
        {
            sb.Append(column.ToSql()).Append(", ");
        }

        // Remove the trailing comma and space if there are any expressions
        if (Expressions.Count > 0)
        {
            sb.Length -= 2;
        }

        return sb.ToString();
    }

    public IEnumerable<Lexeme> GetLexemes()
    {
        if (Expressions.Count == 0)
        {
            throw new InvalidOperationException("SelectClause must contain at least one expression.");
        }

        // Estimate the initial capacity for the lexemes list.
        // Each SelectExpression can return up to 3 lexemes (column name, AS, alias).
        // Adding 2 for the "select" keyword and potential "distinct" clause.
        int initialCapacity = Expressions.Count * 3 + 2;
        var lexemes = new List<Lexeme>(initialCapacity)
            {
                new Lexeme(LexType.Keyword, "select")
            };

        lexemes.AddRange(DistinctClause.GetLexemes());

        foreach (var column in Expressions)
        {
            lexemes.AddRange(column.GetLexemes());
        }

        return lexemes;
    }
}
