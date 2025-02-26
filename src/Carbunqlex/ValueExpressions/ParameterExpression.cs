using Carbunqlex.Lexing;

namespace Carbunqlex.ValueExpressions;

public class ParameterExpression : IValueExpression, IArgumentExpression
{
    public string Name { get; }
    public bool MightHaveQueries => false;

    public ParameterExpression(string name)
    {
        Name = name;
    }

    public string ToSqlWithoutCte()
    {
        return Name;
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        return new List<Token>
        {
            new Token(TokenType.Parameter, Name)
        };
    }

    public string DefaultName => string.Empty;

    public IEnumerable<ISelectQuery> GetQueries()
    {
        // ParameterExpression does not directly use queries, so return an empty list
        return Enumerable.Empty<ISelectQuery>();
    }

    public IEnumerable<ColumnExpression> ExtractColumnExpressions()
    {
        // ParameterExpression does not directly use columns, so return an empty list
        return Enumerable.Empty<ColumnExpression>();
    }
}
