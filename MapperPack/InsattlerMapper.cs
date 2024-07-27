using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MapperPack;

public static class InsattlerMapper
{
    public static IServiceCollection AddMapper(this IServiceCollection services)
    {
        AppDomain.CurrentDomain
           .GetTypesFromAssembiles(IsMapper, "*.dll")
           .ForEach(x => services.AddScoped(x));

        return services;
    }

    public static IServiceCollection AddMapper(this IServiceCollection services, string searchPattern)
    {
        AppDomain.CurrentDomain
           .GetTypesFromAssembiles(IsMapper, searchPattern)
           .ForEach(x => services.AddScoped(x));

        return services;
    }

    private static Assembly[] GetMapperAssembiles(this AppDomain domain, string searchPattern)
        => Directory
            .GetFiles(domain.BaseDirectory, searchPattern)
            .Select(Assembly.LoadFrom)
            .ToArray();

    private static List<Type> GetTypesFromAssembiles(this AppDomain domain, Func<Type, bool> condition, string searchPattern)
        => domain.GetMapperAssembiles(searchPattern)
            .SelectMany(x => x.GetTypes())
            .Where(condition)
            .ToList();
    private static bool IsMapper(this Type t) => !t.IsAbstract && t.BaseType != null &&
              t.BaseType.IsGenericType &&
              t.BaseType.GetGenericTypeDefinition() == typeof(Mapper<,>);
}
