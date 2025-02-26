using Carbunqlex.Clauses;
using Carbunqlex.DatasourceExpressions;
using Carbunqlex.Lexing;

namespace Carbunqlex.Parsing;

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
