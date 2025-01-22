using Carbunqlex.Clauses;
using System.Text;

namespace Carbunqlex.ValueExpressions;

public class FilterClause : ISqlComponent
{
    public IValueExpression Condition { get; set; }

    public FilterClause(IValueExpression condition)
    {
        Condition = condition;
    }

    public string ToSqlWithoutCte()
    {
        return $"filter (where {Condition.ToSqlWithoutCte()})";
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        return new[]
        {
            new Token(TokenType.StartClause, "filter", "filter"),
            new Token(TokenType.OpenParen, "("),
            new Token(TokenType.Command, "where"),
        }.Concat(Condition.GenerateTokensWithoutCte())
        .Append(new Token(TokenType.CloseParen, ")"))
        .Append(new Token(TokenType.EndClause, string.Empty, "filter"));
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        return Condition.MightHaveQueries ? Condition.GetQueries() : Enumerable.Empty<ISelectQuery>();
    }
}

public class WithinGroupClause : ISqlComponent
{
    public IOrderByClause OrderByClause { get; set; }

    public WithinGroupClause(IOrderByClause orderByClause)
    {
        OrderByClause = orderByClause;
    }

    public string ToSqlWithoutCte()
    {
        return $"within group ({OrderByClause.ToSqlWithoutCte()})";
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        return new[]
        {
            new Token(TokenType.StartClause, "within group", "within group"),
            new Token(TokenType.OpenParen, "("),
        }.Concat(OrderByClause.GenerateTokensWithoutCte())
        .Append(new Token(TokenType.CloseParen, ")"))
        .Append(new Token(TokenType.EndClause, string.Empty, "within group"));
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        return OrderByClause.MightHaveQueries ? OrderByClause.GetQueries() : Enumerable.Empty<ISelectQuery>();
    }
}

public class FunctionExpression : IValueExpression
{
    public IArgumentExpression Arguments { get; set; }
    public string FunctionName { get; set; }
    public OverClause? OverClause { get; set; }

    public FilterClause? FilterClause { get; set; }

    public WithinGroupClause? WithinGroupClause { get; set; }

    /// <summary>
    /// Prefix to be added to the function name.e.g. "distinct"
    /// </summary>
    public string PrefixModifier { get; set; }



    public FunctionExpression(string functionName, string prefixModifier, IArgumentExpression arguments, OverClause? overClause = null)
    {
        FunctionName = functionName;
        PrefixModifier = prefixModifier;
        Arguments = arguments;
        OverClause = overClause;
    }

    public string ToSqlWithoutCte()
    {
        var sb = new StringBuilder();
        sb.Append(FunctionName);
        sb.Append("(");
        sb.Append(Arguments.ToSqlWithoutCte());
        sb.Append(")");

        if (OverClause != null)
        {
            sb.Append(" ").Append(OverClause.ToSqlWithoutCte());
        }
        return sb.ToString();
    }

    public string DefaultName => string.Empty;

    public bool MightHaveQueries => Arguments.MightHaveQueries || (OverClause?.MightHaveCommonTableClauses ?? false);

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        yield return new Token(TokenType.Command, FunctionName);
        yield return new Token(TokenType.OpenParen, "(");
        foreach (var lexeme in Arguments.GenerateTokensWithoutCte())
        {
            yield return lexeme;
        }
        yield return new Token(TokenType.CloseParen, ")");

        if (OverClause != null)
        {
            foreach (var lexeme in OverClause.GenerateTokensWithoutCte())
            {
                yield return lexeme;
            }
        }
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        var queries = new List<ISelectQuery>();

        if (Arguments.MightHaveQueries)
        {
            queries.AddRange(Arguments.GetQueries());
        }

        if (OverClause?.MightHaveCommonTableClauses == true)
        {
            queries.AddRange(OverClause.GetQueries());
        }

        return queries;
    }

    public IEnumerable<ColumnExpression> ExtractColumnExpressions()
    {
        return Arguments.ExtractColumnExpressions();
    }
}
