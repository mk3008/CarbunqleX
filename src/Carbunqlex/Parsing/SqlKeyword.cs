using System.Text;

namespace Carbunqlex.Parsing;

public readonly struct SqlKeywordNode
{
    private static readonly IReadOnlyDictionary<string, SqlKeywordNode> EmptyDictionary = new Dictionary<string, SqlKeywordNode>();

    public string Keyword { get; init; }
    public IReadOnlyDictionary<string, SqlKeywordNode> Children { get; init; }
    public bool IsTerminal { get; init; }
    public SqlKeywordNode(string keyword, bool isTerminal, IEnumerable<SqlKeywordNode> children)
    {
        if (keyword == null) throw new ArgumentNullException(nameof(keyword));
        if (children == null) throw new ArgumentNullException(nameof(children));
        Keyword = keyword;
        Children = children.ToDictionary(x => x.Keyword);
        IsTerminal = isTerminal;
    }
    public SqlKeywordNode(string keyword)
    {
        if (keyword == null) throw new ArgumentNullException(nameof(keyword));
        Keyword = keyword;
        IsTerminal = true;
        Children = EmptyDictionary;
    }

    public string ToTreeString(int indent = 0)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"{new string(' ', indent)}{Keyword} {(IsTerminal ? "[terminal]" : "")}");
        foreach (var child in Children.Values)
        {
            sb.Append(child.ToTreeString(indent + 2));
        }
        return sb.ToString();
    }
}

public static class SqlKeyword
{
    public static IReadOnlyDictionary<string, SqlKeywordNode> CommandKeywords = GetCommandKeywords().ToDictionary(node => node.Keyword, node => node);

    public static IReadOnlyDictionary<string, SqlKeywordNode> ConstantValueKeywords = GetConstantValueKeywordNodes().ToDictionary(node => node.Keyword, node => node);

    private static List<SqlKeywordNode> GetConstantValueKeywordNodes()
    {
        return SqlKeywordBuilder.Build(new[] {
            "NULL",
            "TRUE",
            "FALSE",
            "UNKNOWN",
            "EPOCH",
            "INFINITY",
            "-INFINITY",
            "NOW",
            "TODAY",
            "YESTERDAY",
            "TOMORROW",
            "ALLBALLS",
            "CURRENT_DATE",
            "CURRENT_TIME",
            "CURRENT_TIMESTAMP",
            "LOCALTIME",
            "LOCALTIMESTAMP",
        }.Select(x => x.ToLowerInvariant()).ToHashSet());
    }

    private static List<SqlKeywordNode> GetCommandKeywords()
    {
        return SqlKeywordBuilder.Build(new[] {
            //common
            "AS",
            //with
            "WITH",
            "WITH RECURSIVE",
            "WITH ONLY",
            "WITH ORDINALITY",
            "MATERIALIZED",
            "NOT MATERIALIZED",
            "SEARCH BREADTH FIRST BY",
            "SEARCH DEPTH FIRST BY",
            //select
            "SELECT",
            "SELECT ALL",
            "SELECT DISTINCT",
            "SELECT DISTINCT ON",
            //from
            "FROM",
            "FROM ONLY",
            "FROM LATERAL",
            "FROM LATERAL ROWS FROM",
            "WITH ORDINALITY",
            "TABLE",
            "TABLE ONLY",
            //join
            "JOIN",
            "LATERAL",
            "INNER JOIN",
            "LEFT JOIN",
            "LEFT JOIN LATERAL",
            "LEFT OUTER JOIN",
            "LEFT OUTER JOIN LATERAL",
            "RIGHT JOIN",
            "RIGHT JOIN LATERAL",
            "RIGHT OUTER JOIN",
            "RIGHT OUTER JOIN LATERAL",
            "FULL JOIN",
            "CROSS JOIN",
            "CROSS JOIN LATERAL",
            "NATURAL JOIN",
            "NATURAL INNER JOIN",
            "NATURAL LEFT JOIN",
            "NATURAL RIGHT JOIN",
            "NATURAL FULL JOIN",
            "ON",
            "USING",
            //where
            "WHERE",
            //group
            "GROUP BY",
            "GROUP BY ALL",
            "GROUP BY DISTINCT",
            "HAVING",
            "WINDOW",
            //set
            "UNION",
            "UNION ALL",
            "UNION DISTINCT",
            "INTERSECT",
            "INTERSECT ALL",
            "INTERSECT DISTINCT",
            "EXCEPT",
            "EXCEPT ALL",
            "EXCEPT DISTINCT",
            //order by
            "ORDER BY",
            "ASC",
            "DESC",
            "USING",
            "NULLS FIRST",
            "NULLS LAST",
            //limit 
            "LIMIT",
            "LIMIT ALL",
            //offset
            "OFFSET",
            "ROW",
            "ROWS",
            //fetch
            "FETCH FIRST",
            "FETCH NEXT",
            "ROW ONLY",
            "ROWS ONLY",
            "ROW WITH TIES",
            "ROWS WITH TIES",
            //for
            "FOR UPDATE",
            "FOR NO KEY UPDATE",
            "FOR SHARE",
            "FOR KEY SHARE",
            "FOR UPDATE NOWAIT",
            "NOWAIT",
            "SKIP LOCKED",
            //operators
            "AND",
            "OR",
            "NOT",
            "IS",
            "IS NOT",
            "IS DISTINCT FROM",
            "IS NOT DISTINCT FROM",
            "COLLATE",
            "BETWEEN",
            "NOT BETWEEN",
            "IN",
            "NOT IN",
            "LIKE",
            "NOT LIKE",
            "ESCAPE",
            "ILIKE",
            "SIMILAR TO",
            "OVERLAPS",
            "CONTAINS",
            "CONTAINED IN",
            "ANY",
            "ALL",
            //date
            "AT TIME ZONE",
            "INTERVAL",
            "YEAR",
            "MONTH",
            "DAY",
            "HOUR",
            "MINUTE",
            "SECOND",
            "YEAR TO MONTH",
            "DAY TO HOUR",
            "DAY TO MINUTE",
            "DAY TO SECOND",
            "HOUR TO MINUTE",
            "HOUR TO SECOND",
            "MINUTE TO SECOND",
            // case
            "CASE",
            "CASE WHEN",
            "THEN",
            "ELSE",
            "END",
            //insert
            "INSERT INTO",
            "VALUES",
            "DEFAULT VALUES",
            "RETURNING",
            //update
            "UPDATE",
            "UPDATE ONLY",
            "SET",
            "WHERE CURRENT OF",
            //delete
            "DELETE FROM",
            "DELETE FROM ONLY",
            //merge
            "MERGE INTO",
            "ONLY",
            "USING",
            "ON",
            "MATCHED",
            "NOT MATCHED",
            "UPDATE SET",
            "DO NOTHING",
            "OVERRIDING SYSTEM VALUE",
            "OVERRIDING USER VALUE",
            "VALUES",
            "DEFAULT VALUES",
            //truncate
            "TRUNCATE",
            "TRUNCATE TABLE",
            "TRUNCATE ONLY",
            "RESTART IDENTITY",
            "CONTINUE IDENTITY",
            "CASCADE",
            "RESTRICT",
            //create
            "CREATE TABLE",
            "CREATE TABLE IF NOT EXISTS",
            "CREATE TEMPORARY TABLE",
            "CREATE TEMPORARY TABLE IF NOT EXISTS",
            "CREATE TEMP TABLE",
            "CREATE TEMP TABLE IF NOT EXISTS",
            "CREATE GLOBAL TEMPORARY TABLE",
            "CREATE GLOBAL TEMPORARY TABLE IF NOT EXISTS",
            "CREATE LOCAL TEMPORARY TABLE",
            "CREATE LOCAL TEMPORARY TABLE IF NOT EXISTS",
            "CREATE UNLOGGED TABLE",
            "CREATE UNLOGGED TABLE IF NOT EXISTS",
            "ON COMMIT PRESERVE ROWS",
            "ON COMMIT DELETE ROWS",
            "ON COMMIT DROP",
            "PARTITION BY RANGE",
            "PARTITION BY LIST",
            "PARTITION BY HASH",
            "PRIMARY KEY",
            "UNIQUE",
            "CHECK",
            "FOREIGN KEY",
            "EXCLUDE USING",
            "TABLESPACE",
            "USING METHOD",
            "USING INDEX TABLESPACE",
            "INCLUDING ALL",
            "EXCLUDING ALL",
            //drop
            "DROP TABLE",
            "DROP TABLE IF EXISTS",
            //alter
            "ALTER TABLE",
            "ALTER TABLE IF EXISTS",
            "ALTER TABLE ONLY",
            "ADD COLUMN",
            "ADD COLUMN IF NOT EXISTS",
            "DROP COLUMN",
            "DROP COLUMN IF EXISTS",
            "ALTER COLUMN",
            "SET DATA TYPE",
            "SET DEFAULT",
            "DROP DEFAULT",
            "SET NOT NULL",
            "DROP NOT NULL",
            "ADD GENERATED ALWAYS AS IDENTITY",
            "ADD GENERATED BY DEFAULT AS IDENTITY",
            "SET GENERATED ALWAYS",
            "SET GENERATED BY DEFAULT",
            "RESTART WITH",
            "DROP IDENTITY",
            "SET STATISTICS",
            "RESET",
            "SET STORAGE",
            "SET COMPRESSION",
            "ADD CONSTRAINT",
            "ADD CONSTRAINT IF NOT EXISTS",
            "DROP CONSTRAINT",
            "DROP CONSTRAINT IF EXISTS",
            "VALIDATE CONSTRAINT",
            "ALTER CONSTRAINT",
            "SET NOT VALID",
            "DISABLE TRIGGER",
            "ENABLE TRIGGER",
            "ENABLE REPLICA TRIGGER",
            "ENABLE ALWAYS TRIGGER",
            "DISABLE RULE",
            "ENABLE RULE",
            "ENABLE REPLICA RULE",
            "ENABLE ALWAYS RULE",
            "ATTACH PARTITION",
            "DETACH PARTITION",
            "FOR VALUES",
            "DEFAULT",
            "RENAME COLUMN",
            "RENAME CONSTRAINT",
            "RENAME TO",
            "SET SCHEMA",
            "SET TABLESPACE",
            "SET LOGGED",
            "SET UNLOGGED",
            "SET ACCESS METHOD",
            "INHERIT",
            "NO INHERIT",
            "OWNER TO",
            "REPLICA IDENTITY",
            "CLUSTER ON",
            "SET WITHOUT CLUSTER",
            "SET WITHOUT OIDS",
            "ENABLE ROW LEVEL SECURITY",
            "DISABLE ROW LEVEL SECURITY",
            "FORCE ROW LEVEL SECURITY",
            "NO FORCE ROW LEVEL SECURITY",
            //type
            "smallint",
            "integer",
            "bigint",
            "decimal",
            "numeric",
            "real",
            "double precision",
            "smallserial",
            "serial",
            "bigserial",
            "character varying",
            "varchar",
            "character",
            "char",
            "text",
            "date",
            "time",
            "time without time zone",
            "time with time zone",
            "timestamp",
            "timestamp without time zone",
            "timestamp with time zone",
            "interval",
            "boolean",
            "enum",
            "array",
            "json",
            "jsonb",
            "uuid",
            "xml",
            "bytea",
            "inet",
            "cidr",
            "macaddr"
        }.Select(x => x.ToLowerInvariant()).ToHashSet());
    }
}
