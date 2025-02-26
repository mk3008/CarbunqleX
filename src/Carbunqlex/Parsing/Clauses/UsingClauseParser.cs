using Carbunqlex.Clauses;
using Carbunqlex.Lexing;
using Carbunqlex.Parsing.QuerySources;
using Carbunqlex.QuerySources;

namespace Carbunqlex.Parsing.Clauses;

public static class UsingClauseParser
{
    public static UsingClause Parse(SqlTokenizer tokenizer)
    {
        tokenizer.Read("using");
        var datasources = ParseDatasources(tokenizer);
        return new UsingClause(datasources);
    }

    public static List<DatasourceExpression> ParseDatasources(SqlTokenizer tokenizer)
    {
        var datasources = new List<DatasourceExpression>();
        while (true)
        {
            datasources.Add(DatasourceExpressionParser.Parse(tokenizer));
            if (tokenizer.IsEnd || tokenizer.Peek().Type != TokenType.Comma)
            {
                break;
            }
            tokenizer.Read(TokenType.Comma);
        }
        return datasources;
    }
}
