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
    public static IReadOnlyDictionary<string, SqlKeywordNode> CommandKeywords = SqlKeywordBuilder.Build(GetCommandKeywords()).ToDictionary(node => node.Keyword, node => node);

    public static IReadOnlyDictionary<string, SqlKeywordNode> ConstantValueKeywords = SqlKeywordBuilder.Build(GetConstantValueKeywords()).ToDictionary(node => node.Keyword, node => node);

    public static IReadOnlyDictionary<string, SqlKeywordNode> OperatorKeywordNodes = SqlKeywordBuilder.Build(GetOperatorKeywords()).ToDictionary(node => node.Keyword, node => node);

    public static readonly IReadOnlyDictionary<string, SqlKeywordNode> AllKeywordNodes = SqlKeywordBuilder.Build(AllKeywords).ToDictionary(node => node.Keyword, node => node);

    private static HashSet<string> AllKeywords => GetOperatorKeywords().Concat(GetConstantValueKeywords()).Concat(GetCommandKeywords()).ToHashSet();

    private static HashSet<string> GetOperatorKeywords()
    {
        return [
            "is",
            "is not",
            "and",
            "or"
            ];
    }

    private static HashSet<string> GetConstantValueKeywords()
    {
        return [
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
        ];
    }

    private static HashSet<string> GetCommandKeywords()
    {
        return [
            //common
            "as",
            //with
            "with",
            "with recursive",
            "with only",
            "with ordinality",
            "materialized",
            "not materialized",
            "search breadth first by",
            "search depth first by",
            //select
            "select",
            "select all",
            "select distinct",
            "select distinct on",
            //from
            "from",
            "from only",
            "from lateral",
            "from lateral rows from",
            "with ordinality",
            "table",
            "table only",
            //join
            "join",
            "lateral",
            "inner join",
            "left join",
            "left join lateral",
            "left outer join",
            "left outer join lateral",
            "right join",
            "right join lateral",
            "right outer join",
            "right outer join lateral",
            "full join",
            "cross join",
            "cross join lateral",
            "natural join",
            "natural inner join",
            "natural left join",
            "natural right join",
            "natural full join",
            "on",
            "using",
            //where
            "where",
            //group
            "group by",
            "group by all",
            "group by distinct",
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
            //set
            "union",
            "union all",
            "union distinct",
            "intersect",
            "intersect all",
            "intersect distinct",
            "except",
            "except all",
            "except distinct",
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
            "fetch first",
            "fetch next",
            "row only",
            "rows only",
            "row with ties",
            "rows with ties",
            //for
            "for update",
            "for no key update",
            "for share",
            "for key share",
            "for update nowait",
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
        ];
    }
}
