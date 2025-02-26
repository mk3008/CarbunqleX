using Carbunqlex.Expressions;
using Carbunqlex.Lexing;

namespace Carbunqlex.Expressions;

/// <summary>
/// Represents an IN clause value expression. 
/// </summary>
public interface IValueGroupExpression : IValueExpression
{
}

/// <summary>
/// Represents an IN clause value expression.
/// </summary>
public class InValueGroupExpression : IValueGroupExpression
{
    public List<IValueExpression> Arguments { get; }

    public InValueGroupExpression(List<IValueExpression> arguments)
    {
        Arguments = arguments;
    }

    public InValueGroupExpression(params IValueExpression[] arguments)
    {
        Arguments = arguments.ToList();
    }

    public InValueGroupExpression(IValueExpression argument)
    {
        Arguments = new List<IValueExpression> { argument };
    }

    public string DefaultName => string.Empty;

    public bool MightHaveQueries => Arguments.Any(arg => arg.MightHaveQueries);

    public IEnumerable<ColumnExpression> ExtractColumnExpressions()
    {
        foreach (var argument in Arguments)
        {
            foreach (var columnExpression in argument.ExtractColumnExpressions())
            {
                yield return columnExpression;
            }
        }
    }

    public string ToSqlWithoutCte()
    {
        if (Arguments.Count == 0)
        {
            throw new InvalidOperationException("IN clause must have at least one argument.");
        }
        else if (Arguments.Count == 1)
        {
            return Arguments[0].ToSqlWithoutCte();
        }
        else
        {
            return "(" + string.Join(", ", Arguments.Select(arg => arg.ToSqlWithoutCte())) + ")";
        }
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        if (Arguments.Count == 0)
        {
            throw new InvalidOperationException("IN clause must have at least one argument.");
        }
        else if (Arguments.Count == 1)
        {
            foreach (var token in Arguments[0].GenerateTokensWithoutCte())
            {
                yield return token;
            }
        }
        else
        {
            yield return new Token(TokenType.OpenParen, "(");
            foreach (var argument in Arguments)
            {
                foreach (var token in argument.GenerateTokensWithoutCte())
                {
                    yield return token;
                }
                yield return new Token(TokenType.Comma, ",");
            }
            yield return new Token(TokenType.CloseParen, ")");
        }
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        foreach (var argument in Arguments)
        {
            foreach (var query in argument.GetQueries())
            {
                yield return query;
            }
        }
    }
}

/// <summary>
/// Represents an IN clause value expression that is a subquery.
/// 
/// Similar classes:
/// ・ISelectQuery
/// Implements IArgumentExpression, so it can be used as an argument for the any function.
/// Does not hold parentheses.
/// Can be used generically.
/// ・SubQueryExpression
/// Implements IValueGroupExpression, so it can be used as an argument for the IN clause.
/// Holds parentheses.
/// </summary>
public class SubQueryExpression : IValueGroupExpression
{
    public ISelectQuery Query { get; }

    public SubQueryExpression(ISelectQuery query)
    {
        Query = query;
    }

    public string DefaultName => string.Empty;

    public bool MightHaveQueries => true;

    public IEnumerable<ColumnExpression> ExtractColumnExpressions()
    {
        yield break;
    }

    public string ToSqlWithoutCte()
    {
        return "(" + Query.ToSqlWithoutCte() + ")";
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        yield return new Token(TokenType.OpenParen, "(");
        foreach (var token in Query.GenerateTokensWithoutCte())
        {
            yield return token;
        }
        yield return new Token(TokenType.CloseParen, ")");
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        yield return Query;
    }
}
