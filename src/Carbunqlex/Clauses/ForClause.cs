﻿namespace Carbunqlex.Clauses;

public enum LockType
{
    Update,
    Share,
    NoKeyUpdate,
    KeyShare
}

internal static class LockTypeExtensions
{
    /// <summary>
    /// Converts the LockType enum to its corresponding SQL string representation.
    /// This method uses a switch statement for optimal performance.
    /// </summary>
    /// <param name="lockType">The LockType enum value.</param>
    /// <returns>The SQL string representation of the LockType.</returns>
    public static string ToSqlString(this LockType lockType)
    {
        return lockType switch
        {
            LockType.Update => "update",
            LockType.Share => "share",
            LockType.NoKeyUpdate => "no key update",
            LockType.KeyShare => "key share",
            _ => throw new ArgumentOutOfRangeException(nameof(lockType), lockType, null)
        };
    }
}

public class ForClause : IForClause
{
    public LockType LockType { get; }

    public ForClause(LockType lockType)
    {
        LockType = lockType;
    }

    public string ToSqlWithoutCte()
    {
        return $"for {LockType.ToSqlString()}";
    }

    public IEnumerable<Lexeme> GenerateLexemesWithoutCte()
    {
        return new List<Lexeme>
            {
                new Lexeme(LexType.StartClause, "for", "for"),
                new Lexeme(LexType.Keyword, LockType.ToSqlString()),
                new Lexeme(LexType.EndClause, string.Empty, "for")
            };
    }

    public IEnumerable<ISelectQuery> GetQueries()
    {
        // ForClause does not directly use queries, so return an empty list
        return Enumerable.Empty<ISelectQuery>();
    }
}
