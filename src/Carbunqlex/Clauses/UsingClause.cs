using Carbunqlex.Lexing;
using Carbunqlex.QuerySources;

namespace Carbunqlex.Clauses;

public class UsingClause : ISqlComponent
{
    public List<DatasourceExpression> Datasources { get; }

    public UsingClause(List<DatasourceExpression> datasources)
    {
        Datasources = datasources;
    }

    public string ToSqlWithoutCte()
    {
        // Implement the method based on your requirements
        return $"using {string.Join(", ", Datasources.Select(d => d.ToSqlWithoutCte()))}";
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        // Implement the method based on your requirements
        var tokens = new List<Token>();
        tokens.Add(new Token(TokenType.Command, "using"));
        foreach (var datasource in Datasources)
        {
            tokens.AddRange(datasource.GenerateTokensWithoutCte());
        }
        return tokens;
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        // Implement the method based on your requirements
        return Datasources.SelectMany(d => d.GetQueries());
    }
}
