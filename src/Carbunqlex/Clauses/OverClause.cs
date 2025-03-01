using Carbunqlex.Lexing;
using System.Text;

namespace Carbunqlex.Clauses;

public class OverClause : IFunctionModifier
{
    public IWindowDefinition? WindowFunction { get; set; }

    public bool MightHaveCommonTableClauses => WindowFunction?.MightHaveCommonTableClauses ?? false;

    public bool MightHaveQueries => WindowFunction != null;

    public OverClause(IWindowDefinition? windowFunction)
    {
        WindowFunction = windowFunction;
    }

    public OverClause() : this(null)
    {
    }

    public string ToSqlWithoutCte()
    {
        var sb = new StringBuilder();
        sb.Append("over");
        if (WindowFunction != null)
        {
            sb.Append(WindowFunction.ToSqlWithoutCte());
        }
        else
        {
            sb.Append("()");
        }
        return sb.ToString();
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        var tokens = new List<Token>
        {
            new Token(TokenType.StartClause, "over", "over"),
        };

        if (WindowFunction != null)
        {
            tokens.AddRange(WindowFunction.GenerateTokensWithoutCte());
        }
        else
        {
            tokens.Add(new Token(TokenType.OpenParen, "("));
            tokens.Add(new Token(TokenType.CloseParen, ")"));
        }

        tokens.Add(new Token(TokenType.EndClause, string.Empty, "over"));
        return tokens;
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        return WindowFunction?.GetQueries() ?? Enumerable.Empty<ISelectQuery>();
    }
}
