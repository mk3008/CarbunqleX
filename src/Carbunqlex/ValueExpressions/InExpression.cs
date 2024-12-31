using Carbunqlex.Clauses;
using System.Text;

namespace Carbunqlex.ValueExpressions;

public class InExpression : IValueExpression
{
    public IValueExpression Left { get; set; }
    public IArgumentExpression Right { get; set; }
    public bool IsNegated { get; set; }

    public InExpression(bool isNegated, IValueExpression left, IArgumentExpression right)
    {
        IsNegated = isNegated;
        Left = left;
        Right = right;
    }

    public string DefaultName => Left.DefaultName;

    public bool MightHaveQueries => Left.MightHaveQueries || Right.MightHaveQueries;

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        foreach (var lexeme in Left.GenerateLexemesWithoutCte())
        {
            yield return lexeme;
        }
        yield return new Lexeme(LexType.Operator, IsNegated ? "not in" : "in");
        yield return new Lexeme(LexType.OpenParen, "(");

        foreach (var lexeme in Right.GenerateLexemesWithoutCte())
        {
            yield return lexeme;
        }

        yield return new Lexeme(LexType.CloseParen, ")");
    }

    public string ToSqlWithoutCte()
    {
        var sb = new StringBuilder();
        sb.Append(Left.ToSqlWithoutCte());
        sb.Append(IsNegated ? " not in (" : " in (");
        sb.Append(Right.ToSqlWithoutCte());
        sb.Append(")");
        return sb.ToString();
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        var queries = new List<ISelectQuery>();

        if (Left.MightHaveQueries)
        {
            queries.AddRange(Left.GetQueries());
        }
        queries.AddRange(Right.GetQueries());

        return queries;
    }

    public IEnumerable<ColumnExpression> ExtractColumnExpressions()
    {
        var columns = new List<ColumnExpression>();

        columns.AddRange(Left.ExtractColumnExpressions());
        columns.AddRange(Right.ExtractColumnExpressions());

        return columns;
    }
}

public interface IArgumentExpression : ISqlComponent
{
    bool MightHaveQueries { get; }
    IEnumerable<ColumnExpression> ExtractColumnExpressions();
}

public class ValueSet : IArgumentExpression
{
    public List<IValueExpression> Values { get; set; }

    public ValueSet(params IValueExpression[] values)
    {
        Values = values.ToList();
    }

    public ValueSet(IEnumerable<IValueExpression> values)
    {
        Values = values.ToList();
    }

    public bool MightHaveQueries => Values.Any(v => v.MightHaveQueries);

    public string ToSqlWithoutCte()
    {
        return string.Join(", ", Values.Select(v => v.ToSqlWithoutCte()));
    }

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        foreach (var value in Values)
        {
            foreach (var lexeme in value.GenerateLexemesWithoutCte())
            {
                yield return lexeme;
            }
        }
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        var queries = new List<ISelectQuery>();
        foreach (var value in Values)
        {
            if (value.MightHaveQueries)
            {
                queries.AddRange(value.GetQueries());
            }
        }
        return queries;
    }

    public IEnumerable<ColumnExpression> ExtractColumnExpressions()
    {
        var columns = new List<ColumnExpression>();
        foreach (var value in Values)
        {
            columns.AddRange(value.ExtractColumnExpressions());
        }
        return columns;
    }
}

public class ScalarSubquery : IArgumentExpression
{
    public ISelectQuery Query { get; }

    public ScalarSubquery(ISelectQuery query)
    {
        Query = query;
    }

    public bool MightHaveQueries => true;

    public string ToSqlWithoutCte()
    {
        return Query.ToSqlWithoutCte();
    }

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        var lexemes = new List<Lexeme>();
        lexemes.AddRange(Query.GenerateLexemesWithoutCte());
        return lexemes;
    }

    public IEnumerable<CommonTableClause> GetCommonTableClauses()
    {
        return Query.GetCommonTableClauses();
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        return new List<ISelectQuery> { Query };
    }

    public IEnumerable<ColumnExpression> ExtractColumnExpressions()
    {
        return Enumerable.Empty<ColumnExpression>();
    }
}
