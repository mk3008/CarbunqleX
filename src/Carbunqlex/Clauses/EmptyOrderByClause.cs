﻿namespace Carbunqlex.Clauses;

public class EmptyOrderByClause : IOrderByClause
{
    public static readonly EmptyOrderByClause Instance = new EmptyOrderByClause();
    private EmptyOrderByClause() { }
    public string ToSqlWithoutCte() => string.Empty;
    public IEnumerable<Lexeme> GenerateLexemesWithoutCte() => new List<Lexeme>();
    public IEnumerable<CommonTableClause> GetCommonTableClauses() => new List<CommonTableClause>();
    public bool MightHaveCommonTableClauses => false;
}
