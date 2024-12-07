using Carbunqlex.ValueExpressions;
using System.Text;

namespace Carbunqlex.Clauses;

public class SelectClause : ISqlComponent
{
    public List<SelectExpression> Expressions { get; }
    public DistinctClause DistinctClause { get; set; }

    public SelectClause(params SelectExpression[] selectExpressions)
    {
        DistinctClause = new DistinctClause(false);
        Expressions = selectExpressions.ToList();
    }

    public SelectClause(DistinctClause distinctClause, params SelectExpression[] selectExpressions)
    {
        DistinctClause = distinctClause;
        Expressions = selectExpressions.ToList();
    }

    public string ToSql()
    {
        var sb = new StringBuilder();
        sb.Append("select ");
        if (DistinctClause.IsDistinct)
        {
            var distinctSql = DistinctClause.ToSql();
            if (!string.IsNullOrEmpty(distinctSql))
            {
                sb.Append(distinctSql).Append(" ");
            }
        }
        foreach (var column in Expressions)
        {
            sb.Append(column.ToSql()).Append(", ");
        }
        if (Expressions.Count > 0)
        {
            sb.Length -= 2;
        }
        return sb.ToString();
    }

    public IEnumerable<Lexeme> GetLexemes()
    {
        // Estimate the initial capacity for the lexemes list.
        // Each SelectExpression can return up to 3 lexemes (column name, AS, alias).
        // Adding 2 for the "select" keyword and potential "distinct" clause.
        int initialCapacity = Expressions.Count * 3 + 2;
        var lexemes = new List<Lexeme>(initialCapacity)
        {
            new Lexeme(LexType.Keyword, "select")
        };

        if (DistinctClause.IsDistinct)
        {
            lexemes.AddRange(DistinctClause.GetLexemes());
        }

        foreach (var column in Expressions)
        {
            lexemes.AddRange(column.GetLexemes());
        }

        return lexemes;
    }
}

public class SelectExpression : ISqlComponent
{
    public IValueExpression Expression { get; }
    public string Alias { get; }

    public SelectExpression(IValueExpression expression)
    {
        Expression = expression;
        Alias = string.Empty;
    }

    public SelectExpression(IValueExpression expression, string alias)
    {
        Expression = expression;
        Alias = alias;
    }

    public string ToSql()
    {
        var sql = Expression.ToSql();
        if (string.IsNullOrEmpty(Alias) || sql == Alias)
        {
            return sql;
        }
        return $"{sql} as {Alias}";
    }

    public IEnumerable<Lexeme> GetLexemes()
    {
        var lexemes = new List<Lexeme>(Expression.GetLexemes());
        if (!string.IsNullOrEmpty(Alias) && Alias != Expression.DefaultName)
        {
            lexemes.Add(new Lexeme(LexType.Keyword, "as"));
            lexemes.Add(new Lexeme(LexType.Identifier, Alias));
        }
        return lexemes;
    }
}

public class DistinctClause : ISqlComponent
{
    public bool IsDistinct { get; }
    public List<IValueExpression> DistinctOnColumns { get; }

    public DistinctClause(bool isDistinct)
    {
        IsDistinct = isDistinct;
        DistinctOnColumns = new();
    }

    public DistinctClause(params IValueExpression[] distinctOnColumns)
    {
        IsDistinct = true;
        DistinctOnColumns = distinctOnColumns.ToList();
    }

    public string ToSql()
    {
        if (!IsDistinct)
        {
            return string.Empty;
        }

        if (DistinctOnColumns.Count == 0)
        {
            return "distinct";
        }

        return $"distinct on ({string.Join(", ", DistinctOnColumns.Select(c => c.ToSql()))})";
    }

    public IEnumerable<Lexeme> GetLexemes()
    {
        if (DistinctOnColumns.Count == 0)
        {
            yield return new Lexeme(LexType.Keyword, "distinct");
            yield break;
        }

        yield return new Lexeme(LexType.Keyword, "distinct");
        yield return new Lexeme(LexType.Keyword, "on");
        yield return new Lexeme(LexType.OpenParen, "(");

        foreach (var lexeme in DistinctOnColumns.SelectMany(c => c.GetLexemes()))
        {
            yield return lexeme;
        }

        yield return new Lexeme(LexType.CloseParen, ")");
    }
}
