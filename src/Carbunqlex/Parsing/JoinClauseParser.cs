﻿using Carbunqlex.Clauses;
using Carbunqlex.Parsing.ValueExpressionParsing;

namespace Carbunqlex.Parsing;

/// <summary>
/// Parses join clauses from SQL tokens.
/// e.g. INNER JOIN [dbo].[table] AS t2 ON t1.[column] = t2.[column]
/// </summary>
public static class JoinClauseParser
{
    private static string ParserName => nameof(JoinClauseParser);

    private static string ParseJoinKeyword(SqlTokenizer tokenizer)
    {
        // comma
        if (tokenizer.Peek().Type == TokenType.Comma)
        {
            tokenizer.CommitPeek();
            return "cross join";
        }

        var joinKeyword = tokenizer.Read(ParserName, TokenType.Command);
        if (!SqlKeyword.JoinKeywords.Contains(joinKeyword.Identifier))
        {
            throw SqlParsingExceptionBuilder.UnexpectedToken(ParserName, SqlKeyword.JoinKeywords.ToArray(), tokenizer, joinKeyword);
        }
        return joinKeyword.Value;
    }

    public static JoinClause Parse(SqlTokenizer tokenizer)
    {
        var joinKeyword = ParseJoinKeyword(tokenizer);

        bool isLateral = false;
        var next = tokenizer.Peek();
        if (next.Identifier == "lateral")
        {
            tokenizer.CommitPeek();
            isLateral = true;
        }

        var datasource = DatasourceExpressionParser.Parse(tokenizer);

        if (tokenizer.IsEnd)
        {
            return new JoinClause(datasource, joinKeyword) { IsLateral = isLateral };
        }

        next = tokenizer.Peek();
        if (next.Identifier == "on")
        {
            tokenizer.Read();
            var condition = ValueExpressionParser.Parse(tokenizer);
            return new JoinClause(datasource, joinKeyword, condition) { IsLateral = isLateral };
        }

        // no condition
        return new JoinClause(datasource, joinKeyword) { IsLateral = isLateral };
    }
}
