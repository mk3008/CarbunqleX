﻿using Carbunqlex.Parsing;
using Xunit.Abstractions;

namespace Carbunqlex.Tests.ParsingTests;

public class SelectQueryParserTests
{
    public SelectQueryParserTests(ITestOutputHelper output)
    {
        Output = output;
    }

    private ITestOutputHelper Output { get; }

    [Theory]
    [InlineData("from less", "select 1")]
    [InlineData("argument less", "select now()")]
    [InlineData("basic select", "select id, name from users")]
    [InlineData("distinct", "select distinct name from users")]
    [InlineData("distinct on", "select distinct on (department) id, name from users order by department, id")]
    [InlineData("where condition", "select id, name from users where age > 18")]
    [InlineData("where condition multiple", "select id, name from users where age > 18 and status = 'active'")]
    [InlineData("where in", "select id, name from users where id in (1, 2, 3)")]
    [InlineData("where between", "select id, name from users where age between 18 and 30")]
    [InlineData("where like", "select id, name from users where name like 'A%'")]
    [InlineData("where like escape", "select id, name from users where name like 'A!%' escape '!'")]
    [InlineData("where is null", "select id, name from users where name is null")]
    [InlineData("order by asc", "select id, name from users order by name")]
    [InlineData("order by desc", "select id, name from users order by age desc")]
    [InlineData("limit", "select id, name from users limit 10")]
    [InlineData("limit offset", "select id, name from users limit 10 offset 5")]
    [InlineData("fetch first 5 rows", "select id, name from users fetch first 5")]
    [InlineData("fetch next 10 rows", "select id, name from users fetch next 10")]
    [InlineData("fetch next 1 row", "select id, name from users fetch next 1")]
    [InlineData("fetch with offset", "select id, name from users offset 5 fetch next 10")]
    [InlineData("fetch percent", "select id, name from users fetch first 10 percent")]
    [InlineData("fetch percent with offset", "select id, name from users offset 5 fetch next 20")]
    [InlineData("group by", "select department, count(*) from users group by department")]
    [InlineData("group by having", "select department, count(*) from users group by department having count(*) > 10")]
    [InlineData("distinct count", "select count(distinct age) from users")]
    [InlineData("filter", "select count(*) filter(where age > 18) from users")]
    [InlineData("within group", "select department, string_agg(name, ', ') within group(order by name) from employees group by department")]
    [InlineData("cube", "select department, age, count(*) from employees group by cube(department, age)")]
    [InlineData("rollup", "select department, age, count(*) from employees group by rollup(department, age)")]
    [InlineData("inner join", "select u.id, u.name, o.total from users as u join orders as o on u.id = o.user_id")]
    [InlineData("join multiple", "select u.id, u.name, o.total, p.name from users as u join orders as o on u.id = o.user_id join products as p on o.product_id = p.id")]
    [InlineData("left join", "select u.id, u.name, o.total from users as u left join orders as o on u.id = o.user_id")]
    [InlineData("right join", "select u.id, u.name, o.total from users as u right join orders as o on u.id = o.user_id")]
    [InlineData("full join", "select u.id, u.name, o.total from users as u full join orders as o on u.id = o.user_id")]
    [InlineData("cross join", "select u.id, o.id from users as u cross join orders as o")]
    [InlineData("self join", "select a.id, b.id from users as a join users as b on a.manager_id = b.id")]
    [InlineData("left join lateral", "select u.id, u.name, o.total from users as u left join lateral (select * from orders where user_id = u.id) as o on true")]
    [InlineData("union subquery", "select id, name from (select id, name from users union select id, name from users) as u")]
    [InlineData("exists", "select id, name from users where exists (select 1 from orders where orders.user_id = users.id)")]
    [InlineData("not exists", "select id, name from users where not exists (select 1 from orders where orders.user_id = users.id)")]
    [InlineData("union", "select id, name from users union select id, name from admins")]
    [InlineData("union all", "select id, name from users union all select id, name from admins")]
    [InlineData("intersect", "select id, name from users intersect select id, name from admins")]
    [InlineData("except", "select id, name from users except select id, name from admins")]
    [InlineData("subquery in select", "select id, (select count(*) from orders where orders.user_id = users.id) as order_count from users")]
    [InlineData("subquery in from", "select id, name from (select id, name from users) as u")]
    [InlineData("subquery in where", "select id, name from users where id in (select user_id from orders where total > 1000)")]
    [InlineData("with", "with cte as (select id, name from users where age > 18) select * from cte")]
    [InlineData("with materialized", "with cte as materialized (select id, name from users where age > 18) select * from cte")]
    [InlineData("with not materialized", "with cte as not materialized (select id, name from users where age > 18) select * from cte")]
    [InlineData("with multiple", "with cte1 as (select id, name from users where age > 18), cte2 as (select id, name from admins) select * from cte1 union select * from cte2")]
    [InlineData("with recursive", "with recursive cte as (select 1 as n union all select n + 1 from cte where n < 10) select * from cte")]
    [InlineData("window function rank", "select id, name, rank() over(partition by department order by age desc) from users")]
    [InlineData("window function dense_rank", "select id, name, dense_rank() over(partition by department order by age desc) from users")]
    [InlineData("window function row_number", "select id, name, row_number() over(partition by department order by age desc) from users")]
    [InlineData("window function lead", "select id, name, lead(name) over(order by age) from users")]
    [InlineData("window function lag", "select id, name, lag(name) over(order by age) from users")]
    [InlineData("window function sum", "select id, department, sum(salary) over(partition by department) from users")]
    [InlineData("window definition", "select id, name, rank() over w from users window w as (partition by department order by age desc)")]
    [InlineData("case when", "select id, name, case when age >= 18 then 'adult' else 'minor' end as category from users")]
    [InlineData("case x when", "select id, name, case age when 18 then 'just adult' when 21 then 'drinking age' else 'other' end as category from users")]
    [InlineData("tablesample system", "select id, name from users as u tablesample system(10)")]
    [InlineData("tablesample bernoulli", "select id, name from users tablesample bernoulli(10)")]
    [InlineData("column alias tablesample", "select * from temp_users as t(id, name2) tablesample system(50)")]
    [InlineData("jsonb extract", "select id, name, info ->> 'age' as age from users")]
    [InlineData("jsonb exists", "select id, name from users where info ? 'hobbies'")]
    [InlineData("jsonb contains", "select id, name from users where info @> '{\"age\": 30}'")]
    [InlineData("jsonb object keys", "select jsonb_object_keys(info) from users")]
    [InlineData("generate_series", "select generate_series(1, 10)")]
    [InlineData("random order", "select id, name from users order by random()")]
    [InlineData("regexp match", "select id, name from users where name ~ '^A'")]
    [InlineData("ilike", "select id, name from users where name ilike 'alice%'")]
    [InlineData("coalesce", "select id, name, coalesce(nickname, 'unknown') from users")]
    [InlineData("nullif", "select id, name from users where nullif(age, 0) is not null")]
    [InlineData("extract year", "select id, name from users where extract(year from birthdate) = 2000")]
    [InlineData("date_part", "select id, name from users where date_part('year', birthdate) = 2000")]
    [InlineData("interval", "select id, name from users where birthdate + interval '1 year' < now()")]
    [InlineData("cast symbol", "select '1'::integer")]
    [InlineData("cast symbol double", "select id, name from users where age::double precision > 18.5")]
    [InlineData("cast symbol array", "select array[1, 2, 3]::integer[]")]
    [InlineData("cast as", "select cast('1' as integer)")]
    [InlineData("cast as double", "select cast('1' as double precision)")]
    [InlineData("cast as array", "select cast(array[1, 2, 3] as integer[])")]
    [InlineData("uuid generate", "select gen_random_uuid()")]
    [InlineData("current timestamp", "select current_timestamp")]
    [InlineData("string concatenation", "select id, name || ' (' || age || ')' from users")]
    [InlineData("array_agg", "select id, array_agg(name) from users group by id")]
    [InlineData("json_agg", "select json_agg(users) from users")]
    [InlineData("tsvector search", "select id, name from users where to_tsvector(name) @@ to_tsquery('alice')")]
    [InlineData("plainto_tsquery", "select id, name from users where plainto_tsquery(name) @@ to_tsvector('alice')")]
    [InlineData("inner join with tablesample", "select u.id, u.name, o.total from users as u tablesample system(10) join orders as o on u.id = o.user_id")]
    [InlineData("named parameter Postgres", "select id, name from users where id = :id")]
    [InlineData("named parameters Postgres", "select id, name from users where id = :id and age > :age")]
    [InlineData("named parameter MySQL", "select id, name from users where id = @id")]
    [InlineData("positional parameter Postgres", "select id, name from users where id = $1")]
    [InlineData("positional parameters Postgres", "select id, name from users where id = $1 and age > $2")]
    [InlineData("positional parameter MySQL", "select id, name from users where id = ?")]
    [InlineData("Excessive parentheses in subqueries", "select id from ((select id from users) union (select id from users)) as d")]
    [InlineData("identifier escape symbol Postgres", "select \"columnName\" from \"tableName\"")]
    [InlineData("identifier escape symbol MySQL", "select `columnName` from `tableName`")]
    [InlineData("identifier escape symbol SQL Server", "select [columnName] from [tableName]")]
    public void Parse(string caption, string query)
    {
        Output.WriteLine($"Caption: {caption}\n{query}");
        // Arrange
        var tokenizer = new SqlTokenizer(query);

        // Act
        var result = SelectQueryParser.Parse(tokenizer);
        var actual = result.ToSql();
        Output.WriteLine(actual);

        // Assert
        Assert.Equal(query, actual);
    }
}
