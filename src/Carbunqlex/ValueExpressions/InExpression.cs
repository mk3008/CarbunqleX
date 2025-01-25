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

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        foreach (var lexeme in Left.GenerateTokensWithoutCte())
        {
            yield return lexeme;
        }
        yield return new Token(TokenType.Operator, IsNegated ? "not in" : "in");
        yield return new Token(TokenType.OpenParen, "(");

        foreach (var lexeme in Right.GenerateTokensWithoutCte())
        {
            yield return lexeme;
        }

        yield return new Token(TokenType.CloseParen, ")");
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

public class ValueArguments : IArgumentExpression
{
    public List<IValueExpression> Values { get; }

    public OrderByClause? OrderByClause { get; set; }

    public ValueArguments(params IValueExpression[] values)
    {
        Values = values.ToList();
        OrderByClause = null;
    }

    public ValueArguments(IEnumerable<IValueExpression> values)
    {
        Values = values.ToList();
        OrderByClause = null;
    }

    public ValueArguments(List<IValueExpression> values)
    {
        Values = values;
        OrderByClause = null;
    }

    public ValueArguments(List<IValueExpression> values, OrderByClause orderBy)
    {
        Values = values;
        OrderByClause = orderBy;
    }

    public bool MightHaveQueries => Values.Any(v => v.MightHaveQueries);

    public string ToSqlWithoutCte()
    {
        var sb = new StringBuilder();
        sb.Append(string.Join(", ", Values.Select(v => v.ToSqlWithoutCte())));
        if (OrderByClause != null)
        {
            sb.Append(" ").Append(OrderByClause.ToSqlWithoutCte());
        }
        return sb.ToString();
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        foreach (var value in Values)
        {
            foreach (var lexeme in value.GenerateTokensWithoutCte())
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

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        var tokens = new List<Token>();
        tokens.AddRange(Query.GenerateTokensWithoutCte());
        return tokens;
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
