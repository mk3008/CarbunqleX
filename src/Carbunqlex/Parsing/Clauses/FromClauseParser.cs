﻿using Carbunqlex.Clauses;
using Carbunqlex.Lexing;
using Carbunqlex.Parsing.QuerySources;

namespace Carbunqlex.Parsing.Clauses;

/// <summary>
/// Parses FROM clauses from SQL tokens.
/// e.g. FROM [dbo].[table] AS t1 INNER JOIN [dbo].[table] AS t2 ON t1.[column] = t2.[column]
/// </summary>
public class FromClauseParser
{
    public static FromClause Parse(SqlTokenizer tokenizer)
    {
        tokenizer.Read("from");

        var datasource = DatasourceExpressionParser.Parse(tokenizer);

        if (tokenizer.IsEnd)
        {
            return new FromClause(datasource);
        }

        var next = tokenizer.Peek();
        var joins = new List<JoinClause>();

        while (next.Type == TokenType.Command || next.Type == TokenType.Comma)
        {
            if (next.Type == TokenType.Comma || SqlKeyword.JoinCommandKeywords.Contains(next.CommandOrOperatorText))
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
