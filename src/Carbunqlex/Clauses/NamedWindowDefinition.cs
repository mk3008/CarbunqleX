namespace Carbunqlex.Clauses;

public class NamedWindowDefinition : IWindowDefinition, ISqlComponent
{
    public string Name { get; }

    public NamedWindowDefinition(string name)
    {
        Name = name;
    }

    public string ToSqlWithoutCte()
    {
        return $" {Name}";
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        yield return new Token(TokenType.Identifier, Name);
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        return Enumerable.Empty<ISelectQuery>();
    }

    public bool MightHaveCommonTableClauses => false;
}
