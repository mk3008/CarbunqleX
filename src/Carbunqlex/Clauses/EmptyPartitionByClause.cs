﻿namespace Carbunqlex.Clauses;

public class EmptyPartitionByClause : IPartitionByClause
{
    public static readonly EmptyPartitionByClause Instance = new EmptyPartitionByClause();
    private EmptyPartitionByClause() { }
    public string ToSqlWithoutCte() => string.Empty;
    public IEnumerable<Lexeme> GenerateLexemesWithoutCte() => new List<Lexeme>();
    public IEnumerable<ISelectQuery> GetQueries() => new List<ISelectQuery>();
    public bool MightHaveQueries => false;
}
