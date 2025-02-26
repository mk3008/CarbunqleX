using Carbunqlex.Lexing;

namespace Carbunqlex.Expressions;

/// <summary>
/// Represents a SQL modifier expression, such as INTERVAL '1 day' or DATE '2024-12-25'.
public class ModifierExpression : IValueExpression
{
    public string Modifier { get; }
    public IValueExpression Value { get; }

    public ModifierExpression(string modifier, IValueExpression value)
    {
        Modifier = modifier;
        Value = value;
    }

    public string DefaultName => string.Empty;

    public bool MightHaveQueries => Value.MightHaveQueries;

    public string ToSqlWithoutCte()
    {
        return $"{Modifier} {Value.ToSqlWithoutCte()}";
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        yield return new Token(TokenType.Command, Modifier);
        foreach (var lexeme in Value.GenerateTokensWithoutCte())
        {
            yield return lexeme;
        }
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        return Value.GetQueries();
    }

    public IEnumerable<ColumnExpression> ExtractColumnExpressions()
    {
        return Value.ExtractColumnExpressions();
    }
}
