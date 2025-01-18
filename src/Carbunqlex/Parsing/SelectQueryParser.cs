//namespace Carbunqlex.Parsing;

//public class SelectQueryParser
//{
//    public static ISelectQuery Parse(string sql)
//    {
//        var lexer = new QueryLexer(sql);
//        var parser = new SelectQueryParser(lexer);
//        return parser.Parse();
//    }

//    private readonly QueryLexer _lexer;
//    private Token _currentToken;
//    public SelectQueryParser(QueryLexer lexer)
//    {
//        _lexer = lexer;
//        _currentToken = _lexer.GetNextToken();
//    }
//    private void Eat(TokenType type)
//    {
//        if (_currentToken.Type == type)
//        {
//            _currentToken = _lexer.GetNextToken();
//        }
//        else
//        {
//            throw new Exception($"Expected token type {type} but got {_currentToken.Type}");
//        }
//    }
//    private ISelectQuery Parse()
//    {
//        var query = new SelectQuery();
//        Eat(TokenType.SELECT);
//        var columns = ParseColumns();
//        query.Columns = columns;
//        Eat(TokenType.FROM);
//        var from = ParseFrom();
//        query.From = from;
//        if (_currentToken.Type == TokenType.WHERE)
//        {
//            Eat(TokenType.WHERE);
//            var where = ParseWhere();
//            query.Where = where;
//        }
//        return query;
//    }
//    private List<string> ParseColumns()
//    {
//        var columns = new List<string>();
//        while (_currentToken.Type != TokenType.FROM)
//        {
//            columns.Add(_currentToken.Value);
//            Eat(TokenType.ID);
//        }
//        return columns;
//    }
//    private string ParseFrom()
//    {
//        Eat(TokenType.ID);
//        return _currentToken.Value;
//    }
//    private string ParseWhere()
//    {
//        Eat(TokenType.ID);
//        return _currentToken.Value;
//    }
//}
