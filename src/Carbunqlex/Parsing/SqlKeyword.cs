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
}

public static class SqlKeyword
{
    public static IReadOnlyDictionary<string, SqlKeywordNode> AllKeywords = GetKeywords();

    private static IReadOnlyDictionary<string, SqlKeywordNode> GetKeywords()
    {
        //GetSelectKeywordNode を辞書化して返却する
        return new[] {
            GetSelectKeywordNode(),
            GetInnerJoinKeywordNode()
        }.ToDictionary(node => node.Keyword, node => node);
    }

    private static SqlKeywordNode GetSelectKeywordNode()
    {
        return new SqlKeywordNode(
            keyword: "select",
            isTerminal: true,
            children: [
                new SqlKeywordNode("all"),
                new SqlKeywordNode("into"),
                new SqlKeywordNode(
                    keyword : "distinct",
                    isTerminal : true,
                    children:[
                        new SqlKeywordNode("on")
                    ])
            ]);
    }

    private static SqlKeywordNode GetInnerJoinKeywordNode()
    {
        return new SqlKeywordNode(
            keyword: "inner",
            isTerminal: false,
            children: [
                new SqlKeywordNode("join")
            ]);
    }
}

/// <summary>
/// 個々のSQLキーワード情報を表します。
/// </summary>
public readonly struct SqlKeywordInfo
{
    /// <summary>
    /// キーワードの正規化された文字列。
    /// </summary>
    public string Keyword { get; init; }

    /// <summary>
    /// キーワードを構成するセグメントの一覧。
    /// 例: "inner join" の場合、["inner", "join"] が格納される。
    /// </summary>
    public IReadOnlyList<string> Segments { get; init; }

    public SqlKeywordInfo(IEnumerable<string> segments, string normalizedKeyword)
    {
        if (normalizedKeyword == null) throw new ArgumentNullException(nameof(normalizedKeyword));
        if (segments == null) throw new ArgumentNullException(nameof(segments));

        Keyword = normalizedKeyword;
        Segments = segments.ToList().AsReadOnly();
    }
}

/// <summary>
/// SQLキーワードセットを表します。
/// </summary>
public readonly struct SqlKeywordSet
{
    /// <summary>
    /// キーワードの最初の単語。
    /// 例: "inner join" の場合、このプロパティには "inner" が格納される。
    /// </summary>
    public string FirstWord { get; init; }

    /// <summary>
    /// キーワードの一覧。正規化されたキーの重複を除外して格納。
    /// </summary>
    public IReadOnlyDictionary<IReadOnlyList<string>, SqlKeywordInfo> Keywords { get; init; }

    public SqlKeywordSet(string firstWord, IEnumerable<SqlKeywordInfo> keywords)
    {
        if (firstWord == null) throw new ArgumentNullException(nameof(firstWord));
        if (keywords == null) throw new ArgumentNullException(nameof(keywords));

        FirstWord = firstWord;

        Keywords = keywords.ToList().ToDictionary(
            x => x.Segments,
            x => x
        ).AsReadOnly();
    }
}



public static class SqlKeyword1
{
    public static IReadOnlyDictionary<string, IReadOnlyList<SqlKeywordInfo>> AllKeywords = GetKeywords();

    private static IReadOnlyDictionary<string, IReadOnlyList<SqlKeywordInfo>> GetKeywords()
    {
        var groupedKeywords = GetDmlKeyword()
            .Concat(GetDdlKeyword())
            .Concat(GetJoinKeyword())
            .GroupBy(x => x.FirstWord)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<SqlKeywordInfo>)g.SelectMany(x => x.Keywords.Values).ToList().AsReadOnly()
            ).AsReadOnly();
        return groupedKeywords;
    }

    private static IEnumerable<SqlKeywordSet> GetDmlKeyword()
    {
        // SELECT句のキーワードセットを定義
        var selectKeywordSet = new SqlKeywordSet(
            firstWord: "select",
            keywords: [
                new SqlKeywordInfo(["select"], "select" ),
                new SqlKeywordInfo(["select", "all"], "select" ), //"select" として扱う
                new SqlKeywordInfo(["select", "distinct"], "select distinct"),
                new SqlKeywordInfo(["select", "distinct", "on"], "select distinct on"),
                new SqlKeywordInfo(["select", "top"] , "select top"),
                new SqlKeywordInfo(["select", "into"], "select into")
            ]
        );

        // INSERT句のキーワードセットを定義
        var insertKeywordSet = new SqlKeywordSet(
            firstWord: "insert",
            keywords: [
                new SqlKeywordInfo(["insert", "into"], "insert into"),
            ]
        );

        // UPDATE句のキーワードセットを定義
        var updateKeywordSet = new SqlKeywordSet(
            firstWord: "update",
            keywords: [
                new SqlKeywordInfo(["update"], "update"),
            ]
        );

        // DELETE句のキーワードセットを定義
        var deleteKeywordSet = new SqlKeywordSet(
            firstWord: "delete",
            keywords: [
                new SqlKeywordInfo(["delete", "from"], "delete from")
            ]
        );

        // すべてのDMLキーワードセットを返す
        return [
            selectKeywordSet,
            insertKeywordSet,
            updateKeywordSet,
            deleteKeywordSet
        ];
    }

    private static IEnumerable<SqlKeywordSet> GetDdlKeyword()
    {
        // CREATE句のキーワードセットを定義
        var createKeywordSet = new SqlKeywordSet(
            firstWord: "create",
            keywords: [
                new SqlKeywordInfo(["create", "table"], "create table"),
                new SqlKeywordInfo(["create", "temporary", "table"], "create temporary table"),
                new SqlKeywordInfo(["create", "view"], "create view"),
                new SqlKeywordInfo(["create", "temporary", "view"], "create temporary view"),
                new SqlKeywordInfo(["create", "index"], "create index"),
                new SqlKeywordInfo(["create", "schema"], "create schema"),
                new SqlKeywordInfo(["create", "database"], "create database"),
            ]
        );

        // ALTER句のキーワードセットを定義
        var alterKeywordSet = new SqlKeywordSet(
            firstWord: "alter",
            keywords: [
                new SqlKeywordInfo(["alter", "table"], "alter table"),
                new SqlKeywordInfo(["alter", "column"], "alter column"),
                new SqlKeywordInfo(["alter", "index"], "alter index"),
                new SqlKeywordInfo(["alter", "view"], "alter view"),
            ]
        );

        // DROP句のキーワードセットを定義
        var dropKeywordSet = new SqlKeywordSet(
            firstWord: "drop",
            keywords: [
                new SqlKeywordInfo(["drop", "table"], "drop table"),
                new SqlKeywordInfo(["drop", "index"], "drop index"),
                new SqlKeywordInfo(["drop", "view"], "drop view"),
                new SqlKeywordInfo(["drop", "schema"], "drop schema"),
                new SqlKeywordInfo(["drop", "database"], "drop database"),
            ]
        );

        // すべてのDDLキーワードセットを返す
        return [
            createKeywordSet,
            alterKeywordSet,
            dropKeywordSet
        ];
    }

    private static IEnumerable<SqlKeywordSet> GetJoinKeyword()
    {
        var joinKeywordSet = new SqlKeywordSet(
            firstWord: "join",
            keywords: [
                new SqlKeywordInfo(["join"], "inner join"), // inner join として記述を揃える
                new SqlKeywordInfo(["join", "lateral"], "cross join lateral"), // inner join として記述を揃える
            ]
        );

        // INNER JOIN句のキーワードセットを定義
        var innerJoinKeywordSet = new SqlKeywordSet(
            firstWord: "inner",
            keywords: [
                new SqlKeywordInfo(["inner", "join"], "inner join"),
            ]
        );

        // LEFT JOIN句のキーワードセットを定義
        var leftJoinKeywordSet = new SqlKeywordSet(
            firstWord: "left",
            keywords: [
                new SqlKeywordInfo(["left", "join"], "left join"),
                new SqlKeywordInfo(["left", "outer", "join"], "left join"), // "outer"を省略可能
                new SqlKeywordInfo(["left", "join", "lateral"], "left join lateral"),
            ]
        );

        // RIGHT JOIN句のキーワードセットを定義
        var rightJoinKeywordSet = new SqlKeywordSet(
            firstWord: "right",
            keywords: [
                new SqlKeywordInfo(["right", "join"], "right join"),
                new SqlKeywordInfo(["right", "outer", "join"], "right join") // "outer"を省略可能
            ]
        );

        // FULL OUTER JOIN句のキーワードセットを定義
        var fullOuterJoinKeywordSet = new SqlKeywordSet(
            firstWord: "full",
            keywords: [
                new SqlKeywordInfo(["full", "join"], "full join"),
                new SqlKeywordInfo(["full", "outer", "join"], "full join") // "outer"が省略可能
            ]
        );

        // CROSS JOIN句のキーワードセットを定義
        var crossJoinKeywordSet = new SqlKeywordSet(
            firstWord: "cross",
            keywords: [
                new SqlKeywordInfo(["cross", "join"], "cross join"),
                new SqlKeywordInfo(["cross", "join", "lateral"], "cross join lateral"),
            ]
        );

        var lateralJoinKeywordSet = new SqlKeywordSet(
            firstWord: "lateral",
            keywords: [
                new SqlKeywordInfo(["lateral"], "lateral"),
            ]
        );

        // すべてのJOINキーワードセットを返す
        return [
            joinKeywordSet,
            innerJoinKeywordSet,
            leftJoinKeywordSet,
            rightJoinKeywordSet,
            fullOuterJoinKeywordSet,
            crossJoinKeywordSet,
        ];
    }

    /// <summary>
    /// SQLのキーワードを管理する。
    /// 複数語で構成されているが、不可分で1単語のように扱うようなものも管理する。
    /// キーはキーワードが開始する初回の単語とする。
    /// </summary>
    //public static readonly Dictionary<string, Dictionary<string, (string NormalizedToken, string[] TokenSequence)>> Keywords = new()
    //{
    //    // 演算子
    //    {
    //        "and", new Dictionary<string, (string, string[])>
    //        {
    //            { "and", ("and", new[] { "and" }) }
    //        }
    //    },
    //    {
    //        "or", new Dictionary<string, (string, string[])>
    //        {
    //            { "or", ("or", new[] { "or" }) }
    //        }
    //    },
    //    {
    //        "not", new Dictionary<string, (string, string[])>
    //        {
    //            { "not", ("not", new[] { "not" }) }
    //        }
    //    },
    //    {
    //        "is", new Dictionary<string, (string, string[])>
    //        {
    //            { "is", ("is", new[] { "is" }) },
    //            { "is distinct from", ("is distinct from", new[] { "is", "distinct", "from" }) },
    //            { "is not distinct from", ("is not distinct from", new[] { "is", "not", "distinct", "from" }) }
    //        }
    //    },
    //    {
    //        "in", new Dictionary<string, (string, string[])>
    //        {
    //            { "in", ("in", new[] { "in" }) }
    //        }
    //    },
    //    {
    //        "like", new Dictionary<string, (string, string[])>
    //        {
    //            { "like", ("like", new[] { "like" }) }
    //        }
    //    },
    //    {
    //        "between", new Dictionary<string, (string, string[])>
    //        {
    //            { "between", ("between", new[] { "between" }) }
    //        }
    //    },
    //    {
    //        "exists", new Dictionary<string, (string, string[])>
    //        {
    //            { "exists", ("exists", new[] { "exists" }) }
    //        }
    //    },
    //    {
    //        "any", new Dictionary<string, (string, string[])>
    //        {
    //            { "any", ("any", new[] { "any" }) }
    //        }
    //    },
    //    {
    //        "nulls", new Dictionary<string, (string, string[])>
    //        {
    //            { "nulls first", ("nulls first", new[] { "nulls", "first" }) },
    //            { "nulls last", ("nulls last", new[] { "nulls", "last" }) }
    //        }
    //    },

    //    // DML
    //    {
    //        "select", new Dictionary<string, (string, string[])>
    //        {
    //            { "select", ("select", new[] { "select" }) },
    //            { "select all", ("select", new[] { "select", "all" }) },
    //            { "select into", ("select into", new[] { "select", "into" }) },
    //            { "select distinct", ("select distinct", new[] { "select", "distinct" }) },
    //            { "select distinct on", ("select distinct on", new[] { "select", "distinct", "on" }) }
    //        }
    //    },
    //    {
    //        "insert", new Dictionary<string, (string, string[])>
    //        {
    //            { "insert into", ("insert into", new[] { "insert", "into" }) }
    //        }
    //    },
    //    {
    //        "update", new Dictionary<string, (string, string[])>
    //        {
    //            { "update", ("update", new[] { "update" }) }
    //        }
    //    },
    //    {
    //        "delete", new Dictionary<string, (string, string[])>
    //        {
    //            { "delete from", ("delete from", new[] { "delete", "from" }) }
    //        }
    //    },
    //    {
    //        "truncate", new Dictionary<string, (string, string[])>
    //        {
    //            { "truncate table", ("truncate table", new[] { "truncate", "table" }) }
    //        }
    //    },
    //    {
    //        "merge", new Dictionary<string, (string, string[])>
    //        {
    //            { "merge into", ("merge into", new[] { "merge", "into" }) },
    //            { "merge match", ("merge match", new[] { "merge", "match" }) },
    //            { "merge using", ("merge using", new[] { "merge", "using" }) }
    //        }
    //    },

    //    // DDL
    //    {
    //        "create", new Dictionary<string, (string, string[])>
    //        {
    //            { "create table", ("create table", new[] { "create", "table" }) },
    //            { "create table if not exists", ("create table", new[] { "create", "table", "if", "not", "exists" }) },
    //            { "create temporary table", ("create temporary table", new[] { "create", "temporary", "table" }) },
    //            { "create temporary table if not exists", ("create temporary table", new[] { "create", "temporary", "table", "if", "not", "exists" }) },
    //            { "create view", ("create view", new[] { "create", "view" }) },
    //            { "create view if not exists", ("create view", new[] { "create", "view", "if", "not", "exists" }) },
    //            { "create index", ("create index", new[] { "create", "index" }) },
    //            { "create unique index", ("create unique index", new[] { "create", "unique", "index" }) },
    //            { "create index if not exists", ("create index", new[] { "create", "index", "if", "not", "exists" }) },
    //            { "create unique index if not exists", ("create unique index", new[] { "create", "unique", "index", "if", "not", "exists" }) },
    //            { "create schema", ("create schema", new[] { "create", "schema" }) },
    //            { "create database", ("create database", new[] { "create", "database" }) },
    //            { "create trigger", ("create trigger", new[] { "create", "trigger" }) },
    //            { "create function", ("create function", new[] { "create", "function" }) },
    //            { "create procedure", ("create procedure", new[] { "create", "procedure" }) },
    //            { "create sequence", ("create sequence", new[] { "create", "sequence" }) },
    //            { "create domain", ("create domain", new[] { "create", "domain" }) }
    //        }
    //    },
    //    {
    //        "alter", new Dictionary<string, (string, string[])>
    //        {
    //            { "alter table", ("alter table", new[] { "alter", "table" }) },
    //            { "alter index", ("alter index", new[] { "alter", "index" }) },
    //            { "alter schema", ("alter schema", new[] { "alter", "schema" }) },
    //            { "alter database", ("alter database", new[] { "alter", "database" }) },
    //            { "alter trigger", ("alter trigger", new[] { "alter", "trigger" }) },
    //            { "alter function", ("alter function", new[] { "alter", "function" }) },
    //            { "alter procedure", ("alter procedure", new[] { "alter", "procedure" }) },
    //            { "alter sequence", ("alter sequence", new[] { "alter", "sequence" }) },
    //            { "alter domain", ("alter domain", new[] { "alter", "domain" }) }
    //        }
    //    },
    //    {
    //        "drop", new Dictionary<string, (string, string[])>
    //        {
    //            { "drop table", ("drop table", new[] { "drop", "table" }) },
    //            { "drop table if exists", ("drop table", new[] { "drop", "table", "if", "exists" }) },
    //            { "drop index", ("drop index", new[] { "drop", "index" }) },
    //            { "drop index if exists", ("drop index", new[] { "drop", "index", "if", "exists" }) },
    //            { "drop schema", ("drop schema", new[] { "drop", "schema" }) },
    //            { "drop schema if exists", ("drop schema", new[] { "drop", "schema", "if", "exists" }) },
    //            { "drop database", ("drop database", new[] { "drop", "database" }) },
    //            { "drop database if exists", ("drop database", new[] { "drop", "database", "if", "exists" }) },
    //            { "drop trigger", ("drop trigger", new[] { "drop", "trigger" }) },
    //            { "drop trigger if exists", ("drop trigger", new[] { "drop", "trigger", "if", "exists" }) },
    //            { "drop function", ("drop function", new[] { "drop", "function" }) },
    //            { "drop function if exists", ("drop function", new[] { "drop", "function", "if", "exists" }) },
    //            { "drop procedure", ("drop procedure", new[] { "drop", "procedure" }) },
    //            { "drop procedure if exists", ("drop procedure", new[] { "drop", "procedure", "if", "exists" }) },
    //            { "drop sequence", ("drop sequence", new[] { "drop", "sequence" }) },
    //            { "drop sequence if exists", ("drop sequence", new[] { "drop", "sequence", "if", "exists" }) },
    //            { "drop domain", ("drop domain", new[] { "drop", "domain" }) },
    //            { "drop domain if exists", ("drop domain", new[] { "drop", "domain", "if", "exists" }) }
    //        }
    //    },

    //    // クエリ修飾
    //    {
    //        "with", new Dictionary<string, (string, string[])>
    //        {
    //            { "with", ("with", new[] { "with" }) },
    //            { "with recursive", ("with recursive", new[] { "with", "recursive" }) }
    //        }
    //    },
    //    {
    //        "order", new Dictionary<string, (string, string[])>
    //        {
    //            { "order by", ("order by", new[] { "order", "by" }) }
    //        }
    //    },
    //    {
    //        "group", new Dictionary<string, (string, string[])>
    //        {
    //            { "group by", ("group by", new[] { "group", "by" }) }
    //        }
    //    },
    //    {
    //        "having", new Dictionary<string, (string, string[])>
    //        {
    //            { "having", ("having", new[] { "having" }) }
    //        }
    //    },
    //    {
    //        "limit", new Dictionary<string, (string, string[])>
    //        {
    //            { "limit", ("limit", new[] { "limit" }) }
    //        }
    //    },
    //    {
    //        "offset", new Dictionary<string, (string, string[])>
    //        {
    //            { "offset", ("offset", new[] { "offset" }) }
    //        }
    //    }
    //};
}

//public static class SqlKeyword
//{
//    /// <summary>
//    /// SQLのキーワードを管理する。
//    /// 複数語で構成されているが、不可分で1単語のように扱うようなものも管理する。
//    /// キーはキーワードが開始する初回の単語とする。
//    /// </summary>
//    public static readonly Dictionary<string, List<string[]>> Keywords = new()
//    {
//        // 関数はユーザー定義が存在するため、管理しない
//        // 型はユーザー定義が存在するため、管理しない（複数語のみ管理）

//        // 演算子
//        { "and", new List<string[]> {
//            new[] { "and" }
//        } },
//        { "or", new List<string[]> {
//            new[] { "or" }
//        } },
//        { "not", new List<string[]> {
//            new[] { "not" }
//        } },
//        { "is", new List<string[]> {
//            new[] { "is" },
//            new[] { "is", "distinct", "from" },
//            new[] { "is", "not", "distinct", "from" }
//        } },
//        { "in", new List<string[]> {
//            new[] { "in" }
//        } },
//        { "like", new List<string[]> {
//            new[] { "like" }
//        } },
//        { "between", new List<string[]> {
//            new[] { "between" }
//        } },
//        { "exists", new List<string[]> {
//            new[] { "exists" }
//        } },
//        { "any", new List<string[]> {
//            new[] { "any" }
//        } },
//        { "nulls", new List<string[]> {
//            new[] { "nulls", "first" },
//            new[] { "nulls", "last" }
//        } },

//        // DML
//        { "select", new List<string[]> {
//            new[] { "select" },
//            new[] { "select", "into" },
//            new[] { "select", "distinct" },
//            new[] { "select", "distinct", "on" }
//        } },
//        { "insert", new List<string[]> {
//            new[] { "insert", "into" }
//        } },
//        { "update", new List<string[]> {
//            new[] { "update" }
//        } },
//        { "delete", new List<string[]> {
//            new[] { "delete", "from" }
//        } },
//        { "truncate", new List<string[]> {
//            new[] { "truncate", "table" }
//        } },
//        { "merge", new List<string[]> {
//            new[] { "merge", "into" },
//            new[] { "merge", "match" },
//            new[] { "merge", "using" }
//        } },

//        // DDL
//        { "create", new List<string[]> {
//            new[] { "create", "table" },
//            new[] { "create", "table", "if", "not", "exists" },
//            new[] { "create", "temporary", "table" },
//            new[] { "create", "temporary", "table", "if", "not", "exists" },
//            new[] { "create", "view" },
//            new[] { "create", "view", "if", "not", "exists" },
//            new[] { "create", "index" },
//            new[] { "create", "unique", "index" },
//            new[] { "create", "index", "if", "not", "exists" },
//            new[] { "create", "unique", "index", "if", "not", "exists" },
//            new[] { "create", "schema" },
//            new[] { "create", "database" },
//            new[] { "create", "trigger" },
//            new[] { "create", "function" },
//            new[] { "create", "procedure" },
//            new[] { "create", "sequence" },
//            new[] { "create", "domain" }
//        } },
//        { "alter", new List<string[]> {
//            new[] { "alter", "table" },
//            new[] { "alter", "index" },
//            new[] { "alter", "schema" },
//            new[] { "alter", "database" },
//            new[] { "alter", "trigger" },
//            new[] { "alter", "function" },
//            new[] { "alter", "procedure" },
//            new[] { "alter", "sequence" },
//            new[] { "alter", "domain" }
//        } },
//        { "drop", new List<string[]> {
//            new[] { "drop", "table" },
//            new[] { "drop", "table", "if", "exists" },
//            new[] { "drop", "index" },
//            new[] { "drop", "index", "if", "exists" },
//            new[] { "drop", "schema" },
//            new[] { "drop", "schema", "if", "exists" },
//            new[] { "drop", "database" },
//            new[] { "drop", "database", "if", "exists" },
//            new[] { "drop", "trigger" },
//            new[] { "drop", "trigger", "if", "exists" },
//            new[] { "drop", "function" },
//            new[] { "drop", "function", "if", "exists" },
//            new[] { "drop", "procedure" },
//            new[] { "drop", "procedure", "if", "exists" },
//            new[] { "drop", "sequence" },
//            new[] { "drop", "sequence", "if", "exists" },
//            new[] { "drop", "domain" },
//            new[] { "drop", "domain", "if", "exists" }
//        } },

//        // DCL
//        //{ "grant", new List<string[]> {
//        //    new[] { "grant" }
//        //} },
//        //{ "revoke", new List<string[]> {
//        //    new[] { "revoke" }
//        //} },

//        // 制約
//        //{ "default", new List<string[]> {
//        //    new[] { "default" }
//        //} },
//        //{ "check", new List<string[]> {
//        //    new[] { "check" }
//        //} },
//        //{ "primary", new List<string[]> {
//        //    new[] { "primary", "key" }
//        //} },
//        //{ "foreign", new List<string[]> {
//        //    new[] { "foreign", "key" }
//        //} },
//        //{ "unique", new List<string[]> {
//        //    new[] { "unique" }
//        //} },
//        //{ "key", new List<string[]> {
//        //    new[] { "key" }
//        //} },

//        // クエリ修飾
//        { "with", new List<string[]> {
//            new[] { "with" }
//        } },
//        { "limit", new List<string[]> {
//            new[] { "limit" }
//        } },
//        { "offset", new List<string[]> {
//            new[] { "offset" }
//        } },

//        // セット操作
//        { "union", new List<string[]> {
//            new[] { "union" },
//            new[] { "union", "all" }
//        } },
//        { "intersect", new List<string[]> {
//            new[] { "intersect" },
//            new[] { "intersect", "all" }
//        } },
//        { "except", new List<string[]> {
//            new[] { "except" },
//            new[] { "except", "all" }
//        } },

//        // 結合
//        { "join", new List<string[]> {
//            new[] { "join" }
//        } },
//        { "inner", new List<string[]> {
//            new[] { "inner", "join" }
//        } },
//        { "left", new List<string[]> {
//            new[] { "left", "join" },
//            new[] { "left", "outer", "join" }
//        } },
//        { "right", new List<string[]> {
//            new[] { "right", "join" },
//            new[] { "right", "outer", "join" }
//        } },
//        { "lateral", new List<string[]> {
//            new[] { "lateral" },
//            new[] { "lateral", "join" }
//        } },
//        { "full", new List<string[]> {
//            new[] { "full", "join" },
//            new[] { "full", "outer", "join" }
//        } },
//        { "natural", new List<string[]> {
//            new[] { "natural", "join" }
//        } },
//        { "cross", new List<string[]> {
//            new[] { "cross", "join" }
//        } },

//        // 定数
//        { "null", new List<string[]> {
//            new[] { "null" }
//        } },
//        { "true", new List<string[]> {
//            new[] { "true" }
//        } },
//        { "false", new List<string[]> {
//            new[] { "false" }
//        } },

//        // 型（複数語のみ）
//        { "double", new List<string[]> {
//            new[] { "double" },
//            new[] { "double", "precision" }
//        } },
//        { "timestamp", new List<string[]> {
//            new[] { "timestamp" },
//            new[] { "timestamp", "with", "time", "zone" },
//            new[] { "timestamp", "without", "time", "zone"}
//        } },

//        // その他
//        { "all", new List<string[]> {
//            new[] { "all" }
//        } },
//        { "as", new List<string[]> {
//            new[] { "as" }
//        } },
//        { "case", new List<string[]> {
//            new[] { "case" },
//            new[] { "case", "when" }
//        } },
//        { "distinct", new List<string[]> {
//            new[] { "distinct" },
//            new[] { "distinct", "on" }
//        } },
//        { "else", new List<string[]> {
//            new[] { "else" }
//        } },
//        { "end", new List<string[]> {
//            new[] { "end" }
//        } },
//        { "from", new List<string[]> {
//            new[] { "from" }
//        } },
//        { "group", new List<string[]> {
//            new[] { "group", "by" }
//        } },
//        { "having", new List<string[]> {
//            new[] { "having" }
//        } },
//        { "on", new List<string[]> {
//            new[] { "on" }
//        } },
//        { "order", new List<string[]> {
//            new[] { "order", "by" }
//        } },
//        { "set", new List<string[]> {
//            new[] { "set" }
//        } },
//        { "then", new List<string[]> {
//            new[] { "then" }
//        } },
//        { "using", new List<string[]> {
//            new[] { "using" }
//        } },
//        { "values", new List<string[]> {
//            new[] { "values" }
//        } },
//        { "when", new List<string[]> {
//            new[] { "when" },
//            new[] { "when", "matched", "then" },
//            new[] { "when", "not", "matched" }
//        } },
//        { "where", new List<string[]> {
//            new[] { "where" }
//        } },
//        { "window", new List<string[]> {
//            new[] { "window" }
//        } },
//        { "partition", new List<string[]> {
//            new[] { "partition", "by" }
//        } },
//        { "escape", new List<string[]> {
//            new[] { "escape" }
//        } },
//    };
//}
