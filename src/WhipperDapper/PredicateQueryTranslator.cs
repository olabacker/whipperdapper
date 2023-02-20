using System.Linq.Expressions;
using System.Text;

namespace WhipperDapper;
internal class PredicateQueryTranslator : ExpressionVisitor
{
    private readonly StringBuilder _queryBuilder = new();
    public int? Skip { get; private set; } = null;

    public int? Take { get; private set; } = null;

    public string OrderBy { get; private set; } = string.Empty;

    public string WhereClause { get; private set; } = string.Empty;

    public string Translate(Expression expression)
    {
        Visit(expression);
        WhereClause = _queryBuilder.ToString();
        return WhereClause;
    }

    private static Expression StripQuotes(Expression e)
    {
        while (e.NodeType == ExpressionType.Quote)
        {
            e = ((UnaryExpression)e).Operand;
        }
        return e;
    }

    protected override Expression VisitMethodCall(MethodCallExpression expression)
    {
        if (expression.Method.DeclaringType == typeof(Queryable) && expression.Method.Name == "Where")
        {
            Visit(expression.Arguments[0]);
            var lambda = (LambdaExpression)StripQuotes(expression.Arguments[1]);
            Visit(lambda.Body);
            return expression;
        }
        
        return expression.Method.Name switch
        {
            "Take" => ParseTakeExpression(expression) ? Visit(expression.Arguments[0]) : expression,
            "Skip" => ParseSkipExpression(expression) ? Visit(expression.Arguments[0]) : expression,
            "OrderBy" => ParseOrderByExpression(expression, "ASC") ? Visit(expression.Arguments[0]) : expression,
            "OrderByDescending" => ParseOrderByExpression(expression, "DESC") ? Visit(expression.Arguments[0]) : expression,
            _ => throw new NotSupportedException($"The method '{expression.Method.Name}' is not supported")
        };
    }

    protected override Expression VisitUnary(UnaryExpression u)
    {
        switch (u.NodeType)
        {
            case ExpressionType.Not:
                _queryBuilder.Append(" NOT ");
                Visit(u.Operand);
                break;
            case ExpressionType.Convert:
                Visit(u.Operand);
                break;
            default:
                throw new NotSupportedException($"The unary operator '{u.NodeType}' is not supported");
        }
        return u;
    }


    protected override Expression VisitBinary(BinaryExpression expr)
    {
        _queryBuilder.Append('(');
        Visit(expr.Left);

        var toAppend = expr.NodeType switch
        {
            ExpressionType.And => " AND ",
            ExpressionType.AndAlso => " AND ",
            ExpressionType.Or => " OR ",
            ExpressionType.OrElse => " OR ",
            ExpressionType.Equal => IsNullConstant(expr.Right) ? " IS " : " = ",
            ExpressionType.NotEqual => IsNullConstant(expr.Right) ? " IS NOT " : " != ",
            ExpressionType.LessThan => " < ",
            ExpressionType.LessThanOrEqual => " <= ",
            ExpressionType.GreaterThan => " > ",
            ExpressionType.GreaterThanOrEqual => " >= ",
            _ => throw new NotSupportedException($"The binary operator '{expr.NodeType}' is not supported")
        };
        
        _queryBuilder.Append(toAppend);

        Visit(expr.Right);
        
        _queryBuilder.Append(')');
        return expr;
    }

    protected override Expression VisitConstant(ConstantExpression expr)
    {
        var q = expr.Value as IQueryable;

        if (q == null && expr.Value == null)
        {
            _queryBuilder.Append("NULL");
        }
        else if (q == null)
        {
            switch (Type.GetTypeCode(expr.Value.GetType()))
            {
                case TypeCode.Boolean:
                    _queryBuilder.Append(((bool)expr.Value) ? 1 : 0);
                    break;

                case TypeCode.String:
                    _queryBuilder
                        .Append('\'')
                        .Append(expr.Value)
                        .Append('\'');
                    break;

                case TypeCode.DateTime:
                    _queryBuilder.Append("'");
                    _queryBuilder.Append(expr.Value);
                    _queryBuilder.Append("'");
                    break;

                case TypeCode.Object:
                    throw new NotSupportedException($"The constant for '{expr.Value}' is not supported");

                default:
                    _queryBuilder.Append(expr.Value);
                    break;
            }
        }

        return expr;
    }

    protected override Expression VisitMember(MemberExpression expr)
    {
        if (expr.Expression is not {NodeType: ExpressionType.Parameter})
        {
            throw new NotSupportedException($"The member '{expr.Member.Name}' is not supported");
        }

        _queryBuilder.Append(expr.Member.Name);
        return expr;
    }

    protected static bool IsNullConstant(Expression exp)
    {
        return exp.NodeType == ExpressionType.Constant && ((ConstantExpression)exp).Value == null;
    }

    private bool ParseOrderByExpression(MethodCallExpression expression, string order)
    {
        var unary = (UnaryExpression)expression.Arguments[1];
        var lambdaExpression = (LambdaExpression)unary.Operand;

        if (lambdaExpression.Body is not MemberExpression body)
        {
            return false;
        }

        OrderBy = string.IsNullOrEmpty(OrderBy) ? $"{body.Member.Name} {order}" : $"{OrderBy}, {body.Member.Name} {order}";

        return true;
    }

    private bool ParseTakeExpression(MethodCallExpression expression)
    {
        var sizeExpression = (ConstantExpression)expression.Arguments[1];

        if (!int.TryParse(sizeExpression?.Value?.ToString(), out var size))
        {
            return false;
        }

        Take = size;
        return true;

    }

    private bool ParseSkipExpression(MethodCallExpression expression)
    {
        var sizeExpression = (ConstantExpression)expression.Arguments[1];

        if (!int.TryParse(sizeExpression?.Value?.ToString(), out var size))
        {
            return false;
        }

        Skip = size;
        return true;
    }
}
