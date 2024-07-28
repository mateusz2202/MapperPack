using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace MapperPack;
public static class Extensions
{
    public static IEnumerable<Expression> GetArguments(this ConstructorInfo constructor, Type sourceType, ParameterExpression sourceParameter)
    {
        foreach (var param in constructor.GetParameters())
            if (sourceType.GetProperty(param.Name == "FullName" ? "Name" : param.Name) is PropertyInfo sourceProperty)
                yield return Expression.Property(sourceParameter, sourceProperty);

    }

    public static ConstructorInfo GetDefaultConstructor(this Type type)
        => type.GetConstructors().First();

    public static NewExpression CreateType(this Type type, Type sourceType, ParameterExpression sourceParameter)
    {
        var constructor = type.GetDefaultConstructor();

        return Expression.New(constructor, constructor.GetArguments(sourceType, sourceParameter).ToList());
    }

    public static IEnumerable<MemberBinding> GetMemberBindings(this Type sourceType, Type destinationType, ParameterExpression sourceParameter)
    {
        foreach (var sourceProperty in sourceType.GetProperties())
            if (destinationType.GetProperty(sourceProperty.Name) is PropertyInfo destinationProperty && destinationProperty.CanWrite)
                yield return Expression.Bind(destinationProperty, Expression.Property(sourceParameter, sourceProperty));

    }
}
