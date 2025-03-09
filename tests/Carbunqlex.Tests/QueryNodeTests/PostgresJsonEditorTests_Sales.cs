using Xunit.Abstractions;

namespace Carbunqlex.Tests.QueryNodeTests;

/*
-- 既存テーブルの削除
--DROP TABLE IF EXISTS sales_detail;
--DROP TABLE IF EXISTS sales_invoice;
--DROP TABLE IF EXISTS payment;
--DROP TABLE IF EXISTS product;
--DROP TABLE IF EXISTS category;
--DROP TABLE IF EXISTS customers;

-- テーブル作成
CREATE TABLE customers (
    customer_id SERIAL PRIMARY KEY,
    name TEXT NOT NULL
);

CREATE TABLE category (
    category_id SERIAL PRIMARY KEY,
    name TEXT NOT NULL
);

CREATE TABLE product (
    product_id SERIAL PRIMARY KEY,
    category_id INT NOT NULL,
    name TEXT NOT NULL,
    price INT NOT NULL
);

CREATE TABLE sales_invoice (
    sales_invoice_id SERIAL PRIMARY KEY,
    customer_id INT NOT NULL,
    total_amount INT NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE sales_detail (
    sales_detail_id SERIAL PRIMARY KEY,
    sales_invoice_id INT NOT NULL,
    product_id INT NOT NULL,
    quantity INT NOT NULL,
    unit_price INT NOT NULL
);

CREATE TABLE payment (
    payment_id SERIAL PRIMARY KEY,
    sales_invoice_id INT NOT NULL,
    amount_paid INT NOT NULL,
    payment_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 顧客データ（5件）
INSERT INTO customers (customer_id, name) VALUES
(1, 'Alice'),
(2, 'Bob'),
(3, 'Charlie'),
(4, 'David'),
(5, 'Emma');

-- カテゴリデータ（3件）
INSERT INTO category (category_id, name) VALUES
(1, 'Electronics'),
(2, 'Clothing'),
(3, 'Books');

-- 商品データ（各カテゴリ2件ずつ）
INSERT INTO product (product_id, category_id, name, price) VALUES
(1, 1, 'Laptop', 150000),
(2, 1, 'Smartphone', 80000),
(3, 2, 'T-Shirt', 2000),
(4, 2, 'Jeans', 5000),
(5, 3, 'Novel', 1500),
(6, 3, 'Textbook', 3000);

-- 売上データ（各顧客3～5件ずつ）
INSERT INTO sales_invoice (sales_invoice_id, customer_id, total_amount, created_at) VALUES
(1, 1, 20000, '2024-03-01'),
(2, 1, 45000, '2024-03-02'),
(3, 1, 30000, '2024-03-05'),
(4, 2, 15000, '2024-03-03'),
(5, 2, 50000, '2024-03-07'),
(6, 2, 35000, '2024-03-08'),
(7, 3, 10000, '2024-03-04'),
(8, 3, 25000, '2024-03-06'),
(9, 3, 40000, '2024-03-09'),
(10, 4, 28000, '2024-03-02'),
(11, 4, 32000, '2024-03-05'),
(12, 5, 22000, '2024-03-03'),
(13, 5, 33000, '2024-03-07'),
(14, 5, 15000, '2024-03-09');

-- 売上明細データ（固定で3～10件ずつ）
INSERT INTO sales_detail (sales_detail_id, sales_invoice_id, product_id, quantity, unit_price) VALUES
(1, 1, 1, 1, 150000),
(2, 1, 3, 2, 2000),
(3, 2, 2, 1, 80000),
(4, 2, 5, 1, 1500),
(5, 3, 4, 3, 5000),
(6, 3, 6, 2, 3000),
(7, 4, 3, 5, 2000),
(8, 4, 5, 1, 1500),
(9, 5, 1, 1, 150000),
(10, 5, 6, 1, 3000),
(11, 6, 2, 1, 80000),
(12, 6, 4, 2, 5000),
(13, 7, 5, 3, 1500),
(14, 7, 3, 2, 2000),
(15, 8, 6, 4, 3000),
(16, 9, 1, 1, 150000),
(17, 9, 2, 1, 80000),
(18, 10, 3, 3, 2000),
(19, 10, 5, 2, 1500),
(20, 11, 6, 1, 3000),
(21, 12, 1, 1, 150000),
(22, 12, 4, 1, 5000),
(23, 13, 2, 1, 80000),
(24, 14, 3, 1, 2000);

-- 支払いデータ（50%の売上に固定値で）
INSERT INTO payment (payment_id, sales_invoice_id, amount_paid, payment_date) VALUES
(1, 1, 20000, '2024-03-02'),
(2, 3, 30000, '2024-03-06'),
(3, 5, 50000, '2024-03-08'),
(4, 7, 10000, '2024-03-05'),
(5, 9, 40000, '2024-03-10'),
(6, 11, 32000, '2024-03-06'),
(7, 13, 33000, '2024-03-08');
*/
public class PostgresJsonEditorTests_Sales(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

    private string QueryCommandText = """
        SELECT 
            sd.sales_detail_id, 
            sd.quantity, 
            sd.unit_price, 
            (sd.quantity * sd.unit_price) AS sd__subtotal,
            p.product_id, 
            p.name AS product_name,
            c.category_id,
            c.name AS category_name,
            si.sales_invoice_id, 
            si.created_at AS invoice_date, 
            si.total_amount, 
            cust.customer_id, 
            cust.name AS customer_name
        FROM 
            sales_detail sd
            INNER JOIN product p ON sd.product_id = p.product_id 
            INNER JOIN category c ON p.category_id = c.category_id
            INNER JOIN sales_invoice si ON sd.sales_invoice_id = si.sales_invoice_id 
            INNER JOIN customers cust ON si.customer_id = cust.customer_id
        """;

    [Fact]
    public void JsonSerializeRootBySalesDetails()
    {
        // Arrange
        var query = QueryAstParser.Parse(QueryCommandText);
        output.WriteLine(query.Query.ToSql());

        // Act
        query = query.Where("sales_invoice_id", static w => w.Equal(1))
            .NormalizeSelectClause()
            .ToJsonQuery(jsonKeyFormatter: StringExtensions.ToPascalCase, builder: static e =>
            {
                return e.SerializeArray(datasource: "sd", jsonKey: "SalesDetails", parent: static e =>
                {
                    return e.Serialize(datasource: "si", jsonKey: "SalesInvoice", parent: static e =>
                    {
                        return e.Serialize(datasource: "cust", jsonKey: "Customer")
                            .Serialize(datasource: "p", jsonKey: "Product", parent: static e =>
                            {
                                return e.Serialize(datasource: "c", jsonKey: "Category");
                            });
                    });
                });
            });
        var actual = query.ToSql();

        var expected = "with __json as (select sd.sales_detail_id as sd__sales_detail_id, sd.quantity as sd__quantity, sd.unit_price as sd__unit_price, (sd.quantity * sd.unit_price) as sd__subtotal, p.product_id as p__product_id, p.name as p__product_name, c.category_id as c__category_id, c.name as c__category_name, si.sales_invoice_id as si__sales_invoice_id, si.created_at as si__invoice_date, si.total_amount as si__total_amount, cust.customer_id as cust__customer_id, cust.name as cust__customer_name from sales_detail as sd inner join product as p on sd.product_id = p.product_id inner join category as c on p.category_id = c.category_id inner join sales_invoice as si on sd.sales_invoice_id = si.sales_invoice_id inner join customers as cust on si.customer_id = cust.customer_id where si.sales_invoice_id = 1), __json_SalesDetails as (select json_agg(json_build_object('SalesDetailId', __json.sd__sales_detail_id, 'Quantity', __json.sd__quantity, 'UnitPrice', __json.sd__unit_price, 'Subtotal', __json.sd__subtotal, 'Salesinvoice', json_build_object('SalesInvoiceId', __json.si__sales_invoice_id, 'InvoiceDate', __json.si__invoice_date, 'TotalAmount', __json.si__total_amount, 'Customer', json_build_object('CustomerId', __json.cust__customer_id, 'CustomerName', __json.cust__customer_name), 'Product', json_build_object('ProductId', __json.p__product_id, 'ProductName', __json.p__product_name, 'Category', json_build_object('CategoryId', __json.c__category_id, 'CategoryName', __json.c__category_name))))) as SalesDetails from __json) select row_to_json(d) from (select __json_SalesDetails.SalesDetails as \"SalesDetails\" from __json_SalesDetails) as d limit 1";

        output.WriteLine($"/*expected*/ {expected}");
        output.WriteLine($"/*actual  */ {actual}");

        // Assert
        Assert.Equal(expected, actual);

        /* JSON sample
{
  "SalesDetails": [
    {
      "SalesDetailId": 1,
      "Quantity": 1,
      "UnitPrice": 150000,
      "Subtotal": 150000,
      "Salesinvoice": {
        "SalesInvoiceId": 1,
        "InvoiceDate": "2024-03-01T00:00:00",
        "TotalAmount": 20000,
        "Customer": {
          "CustomerId": 1,
          "CustomerName": "Alice"
        },
        "Product": {
          "ProductId": 1,
          "ProductName": "Laptop",
          "Category": {
            "CategoryId": 1,
            "CategoryName": "Electronics"
          }
        }
      }
    },
    {
      "SalesDetailId": 2,
      "Quantity": 2,
      "UnitPrice": 2000,
      "Subtotal": 4000,
      "Salesinvoice": {
        "SalesInvoiceId": 1,
        "InvoiceDate": "2024-03-01T00:00:00",
        "TotalAmount": 20000,
        "Customer": {
          "CustomerId": 1,
          "CustomerName": "Alice"
        },
        "Product": {
          "ProductId": 3,
          "ProductName": "T-Shirt",
          "Category": {
            "CategoryId": 2,
            "CategoryName": "Clothing"
          }
        }
      }
    }
  ]
}
         */
    }

    [Fact]
    public void JsonSerializeRootBySalesDetails_WithLocalFunctions()
    {
        // Arrange
        var query = QueryAstParser.Parse(QueryCommandText);
        output.WriteLine(query.Query.ToSql());

        // Define JSON hierarchy structure with local functions
        static PostgresJsonEditor buildSalesDetail(PostgresJsonEditor e) =>
            e.SerializeArray(datasource: "sd", jsonKey: "SalesDetails", parent: buildSalesDetailParent);

        static PostgresJsonEditor buildSalesDetailParent(PostgresJsonEditor e) =>
            e.Serialize(datasource: "si", jsonKey: "SalesInvoice", parent: buildSaleInvoiceParent);

        static PostgresJsonEditor buildSaleInvoiceParent(PostgresJsonEditor e) =>
            e.Serialize(datasource: "cust", jsonKey: "Customer")
             .Serialize(datasource: "p", jsonKey: "Product", parent: buildProductParent);

        static PostgresJsonEditor buildProductParent(PostgresJsonEditor e) =>
            e.Serialize(datasource: "c", jsonKey: "Category");

        // Act
        query = query.Where("sales_invoice_id", static w => w.Equal(1))
            .NormalizeSelectClause()
            .ToJsonQuery(jsonKeyFormatter: StringExtensions.ToPascalCase, builder: buildSalesDetail);

        var actual = query.ToSql();

        var expected = "with __json as (select sd.sales_detail_id as sd__sales_detail_id, sd.quantity as sd__quantity, sd.unit_price as sd__unit_price, (sd.quantity * sd.unit_price) as sd__subtotal, p.product_id as p__product_id, p.name as p__product_name, c.category_id as c__category_id, c.name as c__category_name, si.sales_invoice_id as si__sales_invoice_id, si.created_at as si__invoice_date, si.total_amount as si__total_amount, cust.customer_id as cust__customer_id, cust.name as cust__customer_name from sales_detail as sd inner join product as p on sd.product_id = p.product_id inner join category as c on p.category_id = c.category_id inner join sales_invoice as si on sd.sales_invoice_id = si.sales_invoice_id inner join customers as cust on si.customer_id = cust.customer_id where si.sales_invoice_id = 1), __json_SalesDetails as (select json_agg(json_build_object('SalesDetailId', __json.sd__sales_detail_id, 'Quantity', __json.sd__quantity, 'UnitPrice', __json.sd__unit_price, 'Subtotal', __json.sd__subtotal, 'Salesinvoice', json_build_object('SalesInvoiceId', __json.si__sales_invoice_id, 'InvoiceDate', __json.si__invoice_date, 'TotalAmount', __json.si__total_amount, 'Customer', json_build_object('CustomerId', __json.cust__customer_id, 'CustomerName', __json.cust__customer_name), 'Product', json_build_object('ProductId', __json.p__product_id, 'ProductName', __json.p__product_name, 'Category', json_build_object('CategoryId', __json.c__category_id, 'CategoryName', __json.c__category_name))))) as SalesDetails from __json) select row_to_json(d) from (select __json_SalesDetails.SalesDetails as \"SalesDetails\" from __json_SalesDetails) as d limit 1";

        output.WriteLine($"/*expected*/ {expected}");
        output.WriteLine($"/*actual  */ {actual}");

        // Assert
        Assert.Equal(expected, actual);
    }

}
