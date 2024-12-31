using Carbunqlex.Clauses;

namespace Carbunqlex.ValueExpressions;

public class InlineQuery : IValueExpression
{
    public ISelectQuery Query { get; }
    public string DefaultName => string.Empty;
    public bool MightHaveCommonTableClauses => true;
    public bool MightHaveQueries => true;

    public InlineQuery(ISelectQuery query)
    {
        Query = query;
    }

    public string ToSqlWithoutCte()
    {
        return $"({Query.ToSqlWithoutCte()})";
    }

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        var lexemes = new List<Lexeme>
        {
            new Lexeme(LexType.OpenParen, "(", "inline_query")
        };

        lexemes.AddRange(Query.GenerateLexemesWithoutCte());

        lexemes.Add(new Lexeme(LexType.CloseParen, ")", "inline_query"));

        return lexemes;
    }

    public IEnumerable<CommonTableClause> GetCommonTableClauses()
    {
        return Query.GetCommonTableClauses();
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        return new List<ISelectQuery> { Query };
    }

    public IEnumerable<ColumnExpression> ExtractColumnExpressions()
    {
        return Enumerable.Empty<ColumnExpression>();
    }
}
