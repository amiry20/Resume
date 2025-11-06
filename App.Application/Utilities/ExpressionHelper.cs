using System.Linq.Expressions;
using System.Reflection;
using App.Application.Bases;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace App.Application.Utilities
{
    public static class ExpressionHelper
    {
        public static Expression<Func<TEntity, bool>> ToExpressionFilter<TEntity>(string propertyName, Type propertyType, eFilterType? filterType, object propertyValue)
        {
            // x
            var parameter = Expression.Parameter(typeof(TEntity), "x");
            // x.IsEnable
            var propertyExpression = Expression.PropertyOrField(parameter, propertyName);
            //  true
            var constant = Expression.Constant(propertyValue, propertyType);

            var equalExpression = Expression.Equal(propertyExpression, constant);
            switch (filterType)
            {
                case eFilterType.Equal: equalExpression = Expression.Equal(propertyExpression, constant); break;
                case eFilterType.NotEqual: equalExpression = Expression.NotEqual(propertyExpression, constant); break;
                case eFilterType.GreaterThan: equalExpression = Expression.GreaterThan(propertyExpression, constant); break;
                case eFilterType.LessThan: equalExpression = Expression.LessThan(propertyExpression, constant); break;
                case eFilterType.GreaterThanOrEqual: equalExpression = Expression.GreaterThanOrEqual(propertyExpression, constant); break;
                case eFilterType.LessThanOrEqual: equalExpression = Expression.LessThanOrEqual(propertyExpression, constant); break;
                //case eFilterType.Contains:   equalExpression = Expression.Contains(propertyExpression, constant); break;
                case eFilterType.IsNull: equalExpression = Expression.Equal(propertyExpression, constant); break;
                case eFilterType.NotNull: equalExpression = Expression.NotEqual(propertyExpression, constant); break;

                default:
                    break;
            }
            // x.IsEnable == true 



            // x => x.IsEnable == true 
            return Expression.Lambda<Func<TEntity, bool>>(equalExpression, parameter);

        }
        public static Expression<Func<TEntity, bool>> ToExpressionWhere<TEntity>(Type propertyType, object propertyValue, string propertyName)
        {
            // x
            var parameter = Expression.Parameter(typeof(TEntity), "x");
            // x.IsEnable
            var propertyExpression = Expression.PropertyOrField(parameter, propertyName);
            //  true
            var constant = Expression.Constant(propertyValue, propertyType);
            // x.IsEnable == true 
            var equalExpression = Expression.Equal(propertyExpression, constant);
            // x => x.IsEnable == true 
            return Expression.Lambda<Func<TEntity, bool>>(equalExpression, parameter);

        }
        public static Expression Simplify(this Expression expression)
        {
            var searcher = new ParameterlessExpressionSearcher();
            searcher.Visit(expression);
            return new ParameterlessExpressionEvaluator(searcher.ParameterlessExpressions).Visit(expression);
        }
        public static Expression<Func<T, bool>>? AndAlsoExpression<T>(Expression<Func<T, bool>> expression, Expression<Func<T, bool>> andex)
        {
            return Expression.Lambda<Func<T, bool>>(
                 Expression.AndAlso(new SwapVisitor(expression.Parameters[0], andex.Parameters[0]).Visit(expression.Body), andex.Body), andex.Parameters);
        }
        public static Expression<Func<T, bool>>? OrExpression<T>(Expression<Func<T, bool>> expression, Expression<Func<T, bool>> andex)
        {
            return Expression.Lambda<Func<T, bool>>(
                 Expression.OrElse(new SwapVisitor(expression.Parameters[0], andex.Parameters[0]).Visit(expression.Body), andex.Body), andex.Parameters);
        }


        public async static Task<Expression<Func<T, bool>>?> ToExpressionAsync<T>(this string value, CancellationToken cancellationToken)
        {
            if (value.IsNullEmpty()) return null;
            var options = ScriptOptions.Default
             .WithImports("System", "System.Collections.Generic", "System.Linq").AddReferences(typeof(T).Assembly);
            return await CSharpScript.EvaluateAsync<Expression<Func<T, bool>>>(value, options, cancellationToken);
        }
        public async static Task<Expression<Func<T, object>>?> ToOrderExpression<T>(this string order)
        {
            if (order.IsNullEmpty()) return null;
            var options = ScriptOptions.Default
             .WithImports("System", "System.Collections.Generic", "System.Linq").AddReferences(typeof(T).Assembly);
            return await CSharpScript.EvaluateAsync<Expression<Func<T, object>>>(order, options);

        }
        public static IQueryable<T> OrderByField<T>(this IQueryable<T> q, string SortField, bool Ascending)
        {
            //var param0 = Expression.Parameter(typeof(T), "p"); 
            //var prop = Expression.Property(param0, SortField);
            //var  exp = Expression.Lambda(prop, param0);

            var exp = GetPropertyLambda<T>(SortField);
            string method = Ascending ? "OrderBy" : "OrderByDescending";
            Type[] types = new Type[] { q.ElementType, exp.Body.Type };
            var mce = Expression.Call(typeof(Queryable), method, types, q.Expression, exp);
            return q.Provider.CreateQuery<T>(mce);
        }

        public static Expression<Func<T, object>> GetPropertyLambda<T>(string propertyName)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var propertyNames = propertyName.Split('.');

            Expression propertyAccess = parameter;
            foreach (var prop in propertyNames)
            {
                propertyAccess = Expression.PropertyOrField(propertyAccess, prop);
            }

            var propertyExpr = Expression.Convert(propertyAccess, typeof(object));
            return Expression.Lambda<Func<T, object>>(propertyExpr, parameter);
        }

        public static Expression<Func<TTo, bool>>? ToWhereEntityExpression<TFrom, TTo>(Expression<Func<TFrom, bool>>? where)
        {
            if (where == null) return null;
            return (Expression<Func<TTo, bool>>)new WhereReplacerVisitor<TFrom, TTo>().Visit(where);
        }
        //Unable to cast object of type
        //'System.Linq.Expressions.Expression1`1[System.Func`2[App.Domain.DB.Model.RoleSection,App.Application.DTOs.RoleSectionDTO]]'
        //to type
        //'System.Linq.Expressions.Expression`1[System.Func`2[App.Domain.DB.Model.RoleSection,System.Object]]'.'

        public static Expression<Func<TTo, object>>? ToSelectEntityExpression<TFrom, TTo>(Expression<Func<TFrom, object>>? select)
        {
            if (select == null) return null;
            return (Expression<Func<TTo, object>>)new WhereReplacerVisitor<TFrom, TTo>().Visit(select);
        }
        public static List<Expression<Func<TTo, object>>>? ToIncludsEntityExpression<TFrom, TTo>(List<Expression<Func<TFrom, object>>>? includs)
        {
            if (includs == null || includs.Count == 0) return null;
            var result = new List<Expression<Func<TTo, object>>>();


            var visitor = new TypeReplacer();

            foreach (var item in includs)
            {
                var resu1 = visitor.Visit(item);
                var resu2 = (Expression<Func<TTo, object>>)resu1;

                var d1 = new WhereReplacerVisitor<TFrom, TTo>().Visit(item);
                var resu = (Expression<Func<TTo, object>>)d1;
                result.Add(resu);
            }
            return result;
        }
        public static BaseParameterModel<TTo>? ToEntityParameter<TFrom, TTo>(this BaseParameterModel<TFrom>? parameterModel)
        {
            if (parameterModel == null) return null;

            return new BaseParameterModel<TTo>()
            {
                Paging = parameterModel.Paging,
                Includs = parameterModel.Includs,
                Includs1 = ToIncludsEntityExpression<TFrom, TTo>(parameterModel?.Includs1),
                Select = ToSelectEntityExpression<TFrom, TTo>(parameterModel?.Select),
                Where = ToWhereEntityExpression<TFrom, TTo>(parameterModel?.Where),
            };
        }

    }
    public class WhereReplacerVisitor<TFrom, TTo> : ExpressionVisitor
    {
        private readonly ParameterExpression _parameter = Expression.Parameter(typeof(TTo), "c");

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            // replace parameter here
            return Expression.Lambda(Visit(node.Body), _parameter);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            // replace parameter member access with new type
            if (node.Member.DeclaringType == typeof(TFrom) && node.Expression is ParameterExpression)
            {
                return Expression.PropertyOrField(_parameter, node.Member.Name);
            }
            else
            {
                try
                {

                    var f = Expression.PropertyOrField(_parameter, node.Member.Name);
                }
                catch (Exception e)
                {
                    string d = e.Message;
                }

            }
            return base.VisitMember(node);
        }
    }
    public class TypeReplacer : ExpressionVisitor
    {
        public readonly Dictionary<Type, Type> Conversions = new Dictionary<Type, Type>();
        private readonly Dictionary<Expression, Expression> ParameterConversions = new Dictionary<Expression, Expression>();

        protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
        {
            Type to;

            if (Conversions.TryGetValue(node.Member.DeclaringType, out to))
            {
                var member = ConvertMember(node.Member, to);
                node = Expression.Bind(member, node.Expression);
            }

            return base.VisitMemberAssignment(node);
        }

        public override Expression Visit(Expression node)
        {
            if (node != null && node.NodeType == ExpressionType.Lambda)
            {
                var lambda = (LambdaExpression)node;

                var parameters = lambda.Parameters.ToArray();

                Type to;
                for (int i = 0; i < parameters.Length; i++)
                {
                    ParameterExpression parameter = parameters[i];

                    to = null;
                    if (Conversions.TryGetValue(parameter.Type, out to))
                    {
                        var oldParameter = parameter;
                        parameter = Expression.Parameter(to, parameter.Name);
                        ParameterConversions.Add(oldParameter, parameter);
                    }
                }
                Expression body = base.Visit(lambda.Body);



                for (int i = 0; i < parameters.Length; i++)
                {
                    var parameter = (ParameterExpression)base.Visit(parameters[i]);
                    parameters[i] = parameter;
                }

                // Handling of the delegate type
                var arguments = node.Type.GetGenericArguments();


                for (int i = 0; i < arguments.Length; i++)
                {
                    to = null;
                    if (Conversions.TryGetValue(arguments[i], out to))
                    {
                        arguments[i] = to;
                    }
                }


                var delegateType = node.Type.GetGenericTypeDefinition().MakeGenericType(arguments);

                var node2 = Expression.Lambda(delegateType, body, parameters);
                return node2;
            }
            else if (node != null && node.NodeType == ExpressionType.MemberAccess)
            {

            }

            return base.Visit(node);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            Type to;

            if (Conversions.TryGetValue(node.Type, out to))
            {
                node = Expression.Constant(node.Value, to);
            }

            return base.VisitConstant(node);
        }

        protected override Expression VisitNew(NewExpression node)
        {
            Type to;

            if (Conversions.TryGetValue(node.Type, out to))
            {
                var constructor = node.Constructor;

                BindingFlags bf = (constructor.IsPublic ? BindingFlags.Public : BindingFlags.NonPublic) |
                    BindingFlags.Instance;

                var parameters = constructor.GetParameters();
                var types = Array.ConvertAll(parameters, x => x.ParameterType);

                var constructor2 = to.GetConstructor(bf, null, types, null);

                if (node.Members != null)
                {
                    // Shouldn't happen. node.Members != null with anonymous types
                    IEnumerable<MemberInfo> members = node.Members.Select(x => ConvertMember(x, to));
                    node = Expression.New(constructor2, node.Arguments, members);
                }
                else
                {
                    node = Expression.New(constructor2, node.Arguments);
                }
            }

            return base.VisitNew(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            Type to = null;

            Expression expression = null;

            if (node.Expression != null)
            {
                if (ParameterConversions.TryGetValue(node.Expression, out expression))
                {
                    to = expression.Type;
                }
            }

            if (to != null || (node.Expression == null && Conversions.TryGetValue(node.Member.DeclaringType, out to)))
            {
                //System.Reflection.RuntimePropertyInfo)node.Member).PropertyType

                //if (((System.Reflection.PropertyInfo)node.Member).PropertyType.IsGenericType)
                //{
                //    to = ((System.Reflection.PropertyInfo)node.Member).DeclaringType;
                //    //to = ((System.Reflection.PropertyInfo)node.Member).PropertyType.GetGenericArguments()[0];

                //}
                MemberInfo member = ConvertMember(node.Member, to);

                if (member != null)
                {
                    var node1 = Expression.MakeMemberAccess(expression, member);
                    return node1;
                }
            }
            return base.VisitMember(node);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            Expression to;

            if (ParameterConversions.TryGetValue(node, out to))
            {
                node = (ParameterExpression)to;
                return node;
            }

            return base.VisitParameter(node);
        }

        protected override Expression VisitLabel(LabelExpression node)
        {
            var d = 0;
            if (d > 1)
                return null;
            return base.VisitLabel(node);
        }

        protected override Expression VisitLoop(LoopExpression node)
        {
            var d = 0;
            if (d > 1)
                return null;
            return base.VisitLoop(node);
        }
        protected override Expression VisitListInit(ListInitExpression node)
        {
            if (node != null)
                return base.VisitListInit(node);
            else
                return null;
        }

        protected override Expression VisitConditional(ConditionalExpression node)
        {
            if (node != null)
                return base.VisitConditional(node);
            else
                return null;
        }
        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            if (node != null)
                return base.VisitMemberInit(node);
            else
                return null;
        }
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node != null)
                return base.VisitMethodCall(node);
            else
                return null;
        }
        protected override Expression VisitBlock(BlockExpression node)
        {
            if (node != null)
                return base.VisitBlock(node);
            else
                return null;
        }
        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (node != null)
                return base.VisitBinary(node);
            else
                return null;
        }
        protected override Expression VisitDebugInfo(DebugInfoExpression node)
        {
            if (node != null)
                return base.VisitDebugInfo(node);
            else
                return null;
        }
        protected override Expression VisitDefault(DefaultExpression node)
        {
            if (node != null)
                return base.VisitDefault(node);
            else
                return null;
        }
        protected override Expression VisitDynamic(DynamicExpression node)
        {
            if (node != null)
                return base.VisitDynamic(node);
            else
                return null;
        }
        protected override Expression VisitGoto(GotoExpression node)
        {
            if (node != null)
                return base.VisitGoto(node);
            else
                return null;
        }
        protected override Expression VisitIndex(IndexExpression node)
        {
            if (node != null)
                return base.VisitIndex(node);
            else
                return null;
        }
        protected override Expression VisitInvocation(InvocationExpression node)
        {
            if (node != null)
                return base.VisitInvocation(node);
            else
                return null;
        }
        protected override Expression VisitRuntimeVariables(RuntimeVariablesExpression node)
        {
            if (node != null)
                return base.VisitRuntimeVariables(node);
            else
                return null;
        }
        protected override Expression VisitSwitch(SwitchExpression node)
        {
            if (node != null)
                return base.VisitSwitch(node);
            else
                return null;
        }
        protected override Expression VisitTypeBinary(TypeBinaryExpression node)
        {
            if (node != null)
                return base.VisitTypeBinary(node);
            else
                return null;
        }
        protected override Expression VisitUnary(UnaryExpression node)
        {
            if (node != null)
                return base.VisitUnary(node);
            else
                return null;
        }

        protected override Expression VisitNewArray(NewArrayExpression node)
        {
            if (node != null)
                return base.VisitNewArray(node);
            else
                return null;
        }

        protected override Expression VisitTry(TryExpression node)
        {
            if (node != null)
                return base.VisitTry(node);
            else
                return null;
        }
        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            if (node != null)
                return base.VisitLambda(node);
            else
                return null;
        }

        protected override Expression VisitExtension(Expression node)
        {
            if (node != null)
                return base.VisitExtension(node);
            else
                return null;
        }

        // Conversion of method/property/field accessor (supported indexers)
        private MemberInfo ConvertMember(MemberInfo member, Type to)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    {
                        var field = (FieldInfo)member;

                        BindingFlags bf = (field.IsPublic ? BindingFlags.Public : BindingFlags.NonPublic) |
                            (field.IsStatic ? BindingFlags.Static : BindingFlags.Instance);

                        var field2 = to.GetField(member.Name, bf);

                        return field2;
                    }

                case MemberTypes.Property:
                    {

                        var p = to.GetProperties().Where(x => x.Name == member.Name).FirstOrDefault();
                        //var property2 = to.GetProperty(member.Name,  propType, types);

                        return p;
                    }

                case MemberTypes.Method:
                    {
                        var method = (MethodInfo)member;

                        BindingFlags bf = (method.IsPublic ? BindingFlags.Public : BindingFlags.NonPublic) |
                            (method.IsStatic ? BindingFlags.Static : BindingFlags.Instance);

                        var parameters = method.GetParameters();
                        var types = Array.ConvertAll(parameters, x => x.ParameterType);

                        var method2 = to.GetMethod(member.Name, bf, null, types, null);

                        return method2;
                    }

                default:
                    throw new NotSupportedException(member.MemberType.ToString());
            }
        }


    }

    public class SwapVisitor : ExpressionVisitor
    {
        private readonly Expression from, to;
        public SwapVisitor(Expression from, Expression to)
        {
            this.from = from;
            this.to = to;
        }
        public override Expression Visit(Expression node)
        {
            return node == from ? to : base.Visit(node);
        }
    }

    public class ParameterlessExpressionSearcher : ExpressionVisitor
    {
        public HashSet<Expression> ParameterlessExpressions { get; } = new HashSet<Expression>();
        private bool containsParameter = false;

        public override Expression Visit(Expression node)
        {
            bool originalContainsParameter = containsParameter;
            containsParameter = false;
            base.Visit(node);
            if (!containsParameter)
            {
                if (node?.NodeType == ExpressionType.Parameter)
                    containsParameter = true;
                else
                    ParameterlessExpressions.Add(node);
            }
            containsParameter |= originalContainsParameter;

            return node;
        }
    }

    public class ParameterlessExpressionEvaluator : ExpressionVisitor
    {
        private HashSet<Expression> parameterlessExpressions;
        public ParameterlessExpressionEvaluator(HashSet<Expression> parameterlessExpressions)
        {
            this.parameterlessExpressions = parameterlessExpressions;
        }
        public override Expression Visit(Expression node)
        {
            if (parameterlessExpressions.Contains(node))
                return Evaluate(node);
            else
                return base.Visit(node);
        }

        private Expression Evaluate(Expression node)
        {
            if (node.NodeType == ExpressionType.Constant)
            {
                return node;
            }
            object value = Expression.Lambda(node).Compile().DynamicInvoke();
            return Expression.Constant(value, node.Type);
        }
    }

}
