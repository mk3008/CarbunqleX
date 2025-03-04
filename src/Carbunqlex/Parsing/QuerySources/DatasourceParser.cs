﻿using Carbunqlex.Lexing;
using Carbunqlex.Parsing.Expressions;
using Carbunqlex.QuerySources;

namespace Carbunqlex.Parsing.QuerySources;

/// <summary>
/// Parses datasource expressions from SQL tokens.
/// e.g. [dbo].[table]
/// </summary>
public class DatasourceParser
{
    public static IDatasource Parse(SqlTokenizer tokenizer)
    {
        var next = tokenizer.Peek();

        // TableSource or FunctionSource
        if (next.Type == TokenType.Identifier)
        {
            var identifier = tokenizer.Read();
            if (tokenizer.IsEnd)
            {
                return new TableSource(identifier.Value);
            }

            next = tokenizer.Peek();

            if (next.CommandOrOperatorText == "as")
            {
                return new TableSource(identifier.Value);
            }

            if (next.Type == TokenType.OpenParen)
            {
                return FunctionDatasourceParser.Parse(tokenizer, functionName: identifier.Value);
            }

            // Without AS keyword alias
            return new TableSource(identifier.Value);
        }

        // SubQuerySource or UnionQuerySource
        if (next.Type == TokenType.OpenParen)
        {
            var query = SelectQueryParser.ParseSubQuery(tokenizer);
            return new SubQuerySource(query);
        }

        throw SqlParsingExceptionBuilder.UnexpectedTokenType(tokenizer, [TokenType.Identifier, TokenType.OpenParen], next);
    }
}
