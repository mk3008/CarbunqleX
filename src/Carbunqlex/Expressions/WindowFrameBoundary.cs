using Carbunqlex.Lexing;
using System.Text;

namespace Carbunqlex.Expressions;

public interface IWindowFrameBoundary : ISqlComponent
{
    //bool MightHaveQueries { get; }

    IEnumerable<IWindowFrameBoundaryExpression> WindowFrameBoundaries { get; }
}

public class BetweenWindowFrameBoundary : IWindowFrameBoundary
{
    public IWindowFrameBoundaryExpression Start { get; set; }
    public IWindowFrameBoundaryExpression End { get; set; }

    public bool MightHaveQueries => Start.MightHaveQueries || End.MightHaveQueries;

    public IEnumerable<IWindowFrameBoundaryExpression> WindowFrameBoundaries => [Start, End];

    public BetweenWindowFrameBoundary(IWindowFrameBoundaryExpression start, IWindowFrameBoundaryExpression end)
    {
        Start = start;
        End = end;
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        var tokens = new List<Token>
        {
            new Token(TokenType.Command, "between"),
        };
        tokens.AddRange(Start.GenerateTokensWithoutCte());
        tokens.Add(new Token(TokenType.Command, "and"));
        tokens.AddRange(End.GenerateTokensWithoutCte());
        return tokens;
    }

    public string ToSqlWithoutCte()
    {
        var sb = new StringBuilder();
        sb.Append("between ");
        sb.Append(Start.ToSqlWithoutCte());
        sb.Append(" and ");
        sb.Append(End.ToSqlWithoutCte());
        return sb.ToString();
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        var queries = new List<ISelectQuery>();
        queries.AddRange(Start.GetQueries());
        queries.AddRange(End.GetQueries());
        return queries;
    }
}

public class WindowFrameBoundary : IWindowFrameBoundary
{
    public IWindowFrameBoundaryExpression Boundary { get; }

    public IEnumerable<IWindowFrameBoundaryExpression> WindowFrameBoundaries => [Boundary];

    public WindowFrameBoundary(IWindowFrameBoundaryExpression boundary)
    {
        Boundary = boundary;
    }

    public bool MightHaveQueries => Boundary.MightHaveQueries;

    public string ToSqlWithoutCte()
    {
        return Boundary.ToSqlWithoutCte();
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        return Boundary.GenerateTokensWithoutCte();
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        return Boundary.GetQueries();
    }
}
