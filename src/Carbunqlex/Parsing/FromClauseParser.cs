using Carbunqlex.Clauses;

namespace Carbunqlex.Parsing;

/// <summary>
/// Parses FROM clauses from SQL tokens.
/// e.g. FROM [dbo].[table] AS t1 INNER JOIN [dbo].[table] AS t2 ON t1.[column] = t2.[column]
/// </summary>
public class FromClauseParser
{
    private static string ParserName => nameof(FromClauseParser);

    public static FromClause Parse(SqlTokenizer tokenizer)
    {
        tokenizer.Read(ParserName, "from");

        var datasource = DatasourceExpressionParser.Parse(tokenizer);

        if (tokenizer.IsEnd)
        {
            return new FromClause(datasource);
        }

        var next = tokenizer.Peek();
        var joins = new List<JoinClause>();

        while (next.Type == TokenType.Command || next.Type == TokenType.Comma)
        {
            if (next.Type == TokenType.Comma || SqlKeyword.JoinKeywords.Contains(next.CommandOrOperatorText))
            {
                joins.Add(JoinClauseParser.Parse(tokenizer));
            }
            else
            {
                break;
            }

            if (tokenizer.IsEnd)
            {
                break;
            }

            next = tokenizer.Peek();
        }

        return new FromClause(datasource, joins);
    }
}
