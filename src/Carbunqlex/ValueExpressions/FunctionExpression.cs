﻿using Carbunqlex.Clauses;
using System.Text;

namespace Carbunqlex.ValueExpressions;

public class FunctionExpression : IValueExpression
{
    public IArgumentExpression Arguments { get; set; }
    public string FunctionName { get; set; }
    public OverClause? OverClause { get; set; }

    public FunctionExpression(string functionName, IArgumentExpression arguments, OverClause? overClause = null)
    {
        FunctionName = functionName;
        Arguments = arguments;
        OverClause = overClause;
    }

    public string ToSqlWithoutCte()
    {
        var sb = new StringBuilder();
        sb.Append(FunctionName);
        sb.Append("(");
        sb.Append(Arguments.ToSqlWithoutCte());
        sb.Append(")");

        if (OverClause != null)
        {
            sb.Append(" ").Append(OverClause.ToSqlWithoutCte());
        }
        return sb.ToString();
    }

    public string DefaultName => string.Empty;

    public bool MightHaveQueries => Arguments.MightHaveQueries || (OverClause?.MightHaveCommonTableClauses ?? false);

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        yield return new Lexeme(LexType.Keyword, FunctionName);
        yield return new Lexeme(LexType.OpenParen, "(");
        foreach (var lexeme in Arguments.GenerateLexemesWithoutCte())
        {
            yield return lexeme;
        }
        yield return new Lexeme(LexType.CloseParen, ")");

        if (OverClause != null)
        {
            foreach (var lexeme in OverClause.GenerateLexemesWithoutCte())
            {
                yield return lexeme;
            }
        }
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        var queries = new List<ISelectQuery>();

        if (Arguments.MightHaveQueries)
        {
            queries.AddRange(Arguments.GetQueries());
        }

        if (OverClause?.MightHaveCommonTableClauses == true)
        {
            queries.AddRange(OverClause.GetQueries());
        }

        return queries;
    }

    public IEnumerable<ColumnExpression> ExtractColumnExpressions()
    {
        return Arguments.ExtractColumnExpressions();
    }
}
