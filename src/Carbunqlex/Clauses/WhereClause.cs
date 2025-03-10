﻿using Carbunqlex.Expressions;
using Carbunqlex.Lexing;
using Carbunqlex.QuerySources;
using System.Text;

namespace Carbunqlex.Clauses;

public class WhereClause : ISqlComponent
{
    public IValueExpression? Condition { get; set; }

    public WhereClause()
    {
        Condition = null;
    }

    public WhereClause(IValueExpression condition)
    {
        Condition = condition;
    }

    public string ToSqlWithoutCte()
    {
        if (Condition == null)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        sb.Append("where ");
        sb.Append(Condition.ToSqlWithoutCte());
        return sb.ToString();
    }

    public IEnumerable<Token> GenerateTokensWithoutCte()
    {
        if (Condition == null)
        {
            return Enumerable.Empty<Token>();
        }

        var tokens = new List<Token> {
            new Token(TokenType.StartClause, "where")
        };
        tokens.AddRange(Condition.GenerateTokensWithoutCte());
        tokens.Add(new Token(TokenType.EndClause, string.Empty, "where"));
        return tokens;
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        if (Condition == null)
        {
            return Enumerable.Empty<ISelectQuery>();
        }

        if (Condition.MightHaveQueries)
        {
            return Condition.GetQueries();
        }
        return Enumerable.Empty<ISelectQuery>();
    }

    public IEnumerable<ColumnExpression> ExtractColumnExpressions()
    {
        if (Condition == null)
        {
            return Enumerable.Empty<ColumnExpression>();
        }

        return Condition.ExtractColumnExpressions();
    }

    public void Add(string @operator, IValueExpression expression)
    {
        if (Condition == null)
        {
            Condition = expression;
        }
        else
        {
            Condition = new BinaryExpression(@operator, Condition, expression);
        }
    }

    public void Add(IValueExpression expression)
    {
        Add("and", expression);
    }

    public IEnumerable<DatasourceExpression> GetDatasources()
    {
        if (Condition == null)
        {
            return Enumerable.Empty<DatasourceExpression>();
        }
        return Condition.GetQueries().SelectMany(query => query.GetDatasources());
    }
}
