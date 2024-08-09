using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MapperPack.Utils;
public static class LambdaFactory
{
    public static Func<TSource, TDestination> Create<TSource, TDestination>(Func<TSource, TDestination, ValueTask> definitions, object instance)
        => CreateLambda(definitions, instance);

    private static Func<TSource, TDestination> CreateLambda<TSource, TDestination>(Func<TSource, TDestination, ValueTask> definitions, object instance)
    {
        var sourceType = typeof(TSource);
        var sourceParameter = Expression.Parameter(sourceType, "source");

        var destinationType = typeof(TDestination);

        var destinationParameter = Expression.Parameter(destinationType, "destination");

        List<Expression> expressions =
        [
            Expression.Assign(
                destinationParameter,
                Expression.MemberInit(
                    destinationType.CreateType(sourceType, sourceParameter),
                    sourceType.GetMemberBindings(destinationType, sourceParameter).ToList()
                    )
                ),
            destinationParameter
        ];

        if (definitions is not null)
        {
            expressions.Insert(expressions.Count - 1, Expression.Call(
                Expression.Constant(instance),
                definitions.Method,
                sourceParameter,
                destinationParameter
            ));
        }

        var body = Expression.Block([destinationParameter], expressions);

        return Expression.Lambda<Func<TSource, TDestination>>(body, sourceParameter).Compile();
    }
}
