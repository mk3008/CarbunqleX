namespace Carbunqlex.Parsing;

public static class SqlKeyword
{
    static SqlKeyword()
    {
        NumericPrefixKeywords = new HashSet<string>
        {
            "0b",
            "0o",
            "0x",
        };

        EscapeLiteralKeywords = new HashSet<string>
        {
            // Postgres
            "E'",
            "U&'",
            "X'",
            "B'",
        };

        JoinCommandKeywords = new HashSet<string>
        {
            //join
            "join",
            "lateral",
            "inner join",
            "left join",
            //"left join lateral",
            "left outer join",
            //"left outer join lateral",
            "right join",
            //"right join lateral",
            "right outer join",
            //"right outer join lateral",
            "full join",
            "cross join",
            //"cross join lateral",
            "natural join",
            "natural inner join",
            "natural left join",
            "natural right join",
            "natural full join",
        };

        OperatorKeywords = new HashSet<string>
        {
            "is",
            "is not",
            "and",
            "or"
        };

        ConstantValueKeywords = new HashSet<string>
        {
            "null",
            "true",
            "false",
            "unknown",
            "epoch",
            "infinity",
            "-infinity",
            "now",
            "today",
            "yesterday",
            "tomorrow",
            "allballs",
            "current_date",
            "current_time",
            "current_timestamp",
            "localtime",
            "localtimestamp",
            "unbounded",
        };

        UnionCommandKeywords = new HashSet<string>
        {
            "union",
            "union all",
            "union distinct",
            "intersect",
            "intersect all",
            "intersect distinct",
            "except",
            "except all",
            "except distinct",
        };

        CommandKeywords = new HashSet<string>
        {
            //common
            "as",
            //with
            "with",
            "recursive",
            "only",
            "ordinality",
            "materialized",
            "not materialized",
            "search breadth first by",
            "search depth first by",
            //select
            "select",
            "select all",
            "distinct",
            "distinct on",
            //from
            "from",
            "from only",
            "from lateral",
            "from lateral rows from",
            "with ordinality",
            "table",
            "table only",
            "lateral",
            "on",
            "using",
            //where
            "where",
            "exists",
            "not exists",
            //group
            "group by",
            "group by all",
            "group by distinct",
            "cube",
            "rollup",
            "grouping sets",
            "having",
            "window",
            "rows",
            "range",
            "groups",
            "between", //rows between, range between, groups between
            "over",
            "filter",
            "within group",
            "preceding", //unbounded preceding, x preceding
            "following", //unbounded following, x following
            "current row",
            "partition by",
            //order by
            "order by",
            "asc",
            "desc",
            "using",
            "nulls first",
            "nulls last",
            //limit 
            "limit",
            "limit all",
            //offset
            "offset",
            "row",
            "rows",
            //fetch
            "fetch",
            "first",
            "next",
            "row",
            "rows",
            "rows only",
            "percent",
            "with ties",
            //for
            "for",
            "update",
            "no key update",
            "share",
            "key share",
            "update nowait",
            "nowait",
            "skip locked",
            "not",
            "is distinct from",
            "is not distinct from",
            "collate",
            "between",
            "not between",
            "in",
            "not in",
            "like",
            "not like",
            "ilike",
            "not ilike",
            "escape",
            "ilike",
            "similar to",
            "overlaps",
            "contains",
            "contained in",
            "any",
            "all",
            //date
            "at time zone",
            "interval",
            "year",
            "month",
            "day",
            "hour",
            "minute",
            "second",
            "year to month",
            "day to hour",
            "day to minute",
            "day to second",
            "hour to minute",
            "hour to second",
            "minute to second",
            // cast
            "cast",
            // case
            "case", // 'case when' is not a keyword. 'case' and 'when' are managed separately.
            "when",
            "then",
            "else",
            "end",
            // escape
            "escape",
            "uescape",
            //insert
            "insert into",
            "values",
            "default values",
            "returning",
            //update
            "update",
            "update only",
            "set",
            "where current of",
            //delete
            "delete from",
            "delete from only",
            //merge
            "merge into",
            "only",
            "using",
            "on",
            "matched",
            "not matched",
            "update set",
            "do nothing",
            "overriding system value",
            "overriding user value",
            "values",
            "default values",
            //truncate
            "truncate",
            "truncate table",
            "truncate only",
            "restart identity",
            "continue identity",
            "cascade",
            "restrict",
            //create
            "create table",
            "create table if not exists",
            "create temporary table",
            "create temporary table if not exists",
            "create temp table",
            "create temp table if not exists",
            "create global temporary table",
            "create global temporary table if not exists",
            "create local temporary table",
            "create local temporary table if not exists",
            "create unlogged table",
            "create unlogged table if not exists",
            "on commit preserve rows",
            "on commit delete rows",
            "on commit drop",
            "partition by range",
            "partition by list",
            "partition by hash",
            "primary key",
            "unique",
            "check",
            "foreign key",
            "exclude using",
            "tablespace",
            "using method",
            "using index tablespace",
            "including all",
            "excluding all",
            //drop
            "drop table",
            "drop table if exists",
            //alter
            "alter table",
            "alter table if exists",
            "alter table only",
            "add column",
            "add column if not exists",
            "drop column",
            "drop column if exists",
            "alter column",
            "set data type",
            "set default",
            "drop default",
            "set not null",
            "drop not null",
            "add generated always as identity",
            "add generated by default as identity",
            "set generated always",
            "set generated by default",
            "restart with",
            "drop identity",
            "set statistics",
            "reset",
            "set storage",
            "set compression",
            "add constraint",
            "add constraint if not exists",
            "drop constraint",
            "drop constraint if exists",
            "validate constraint",
            "alter constraint",
            "set not valid",
            "disable trigger",
            "enable trigger",
            "enable replica trigger",
            "enable always trigger",
            "disable rule",
            "enable rule",
            "enable replica rule",
            "enable always rule",
            "attach partition",
            "detach partition",
            "for values",
            "default",
            "rename column",
            "rename constraint",
            "rename to",
            "set schema",
            "set tablespace",
            "set logged",
            "set unlogged",
            "set access method",
            "inherit",
            "no inherit",
            "owner to",
            "replica identity",
            "cluster on",
            "set without cluster",
            "set without oids",
            "enable row level security",
            "disable row level security",
            "force row level security",
            "no force row level security",
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
        };

        // EscapeLiteralKeywords are treated as EscapedStringConstant, not Command,
        // and are not included in AllKeywords
        AllKeywords = OperatorKeywords.Concat(ConstantValueKeywords).Concat(CommandKeywords).Concat(JoinCommandKeywords).Concat(UnionCommandKeywords).ToHashSet();

        JoinCommandKeywordNodes = SqlKeywordBuilder.Build(JoinCommandKeywords).ToDictionary(node => node.Keyword, node => node);

        UnionCommandKeywordNodes = SqlKeywordBuilder.Build(UnionCommandKeywords).ToDictionary(node => node.Keyword, node => node);

        CommandKeywordNodes = SqlKeywordBuilder.Build(CommandKeywords).ToDictionary(node => node.Keyword, node => node);

        ConstantValueKeywordNodes = SqlKeywordBuilder.Build(ConstantValueKeywords).ToDictionary(node => node.Keyword, node => node);

        OperatorKeywordNodes = SqlKeywordBuilder.Build(OperatorKeywords).ToDictionary(node => node.Keyword, node => node);

        AllKeywordNodes = SqlKeywordBuilder.Build(AllKeywords).ToDictionary(node => node.Keyword, node => node);
    }

    public static IReadOnlyDictionary<string, SqlKeywordNode> CommandKeywordNodes { get; }

    public static IReadOnlyDictionary<string, SqlKeywordNode> UnionCommandKeywordNodes { get; }

    public static IReadOnlyDictionary<string, SqlKeywordNode> ConstantValueKeywordNodes { get; }

    public static IReadOnlyDictionary<string, SqlKeywordNode> OperatorKeywordNodes { get; }

    public static IReadOnlyDictionary<string, SqlKeywordNode> JoinCommandKeywordNodes { get; }

    public static IReadOnlyDictionary<string, SqlKeywordNode> AllKeywordNodes { get; }

    private static HashSet<string> AllKeywords { get; }

    public static HashSet<string> NumericPrefixKeywords { get; }

    public static HashSet<string> EscapeLiteralKeywords { get; }

    public static HashSet<string> OperatorKeywords { get; }

    public static HashSet<string> ConstantValueKeywords { get; }

    public static HashSet<string> JoinCommandKeywords { get; }

    public static HashSet<string> CommandKeywords { get; }

    public static HashSet<string> UnionCommandKeywords { get; }
}
