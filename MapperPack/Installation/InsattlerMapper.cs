using MapperPack.Cache;
using MapperPack.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace MapperPack;
public static partial class InsattlerMapper
{
    private static Assembly[] GetMapperAssembiles(this AppDomain domain, string searchPattern)
        => Directory
            .GetFiles(domain.BaseDirectory, searchPattern)
            .Where(file => !Path.GetFileName(file).Contains("microsoft", StringComparison.CurrentCultureIgnoreCase))
            .Select(Assembly.LoadFrom)
            .ToArray();

    private static List<Type> GetTypesFromAssembiles(this AppDomain domain, Func<Type, bool> condition, string searchPattern)
        => domain.GetMapperAssembiles(searchPattern)
            .SelectMany(x => x.GetTypes())
            .Where(condition)
            .ToList();


    private static bool IsMapperDefinition(this Type type)
        => type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMapDefinition<,>));


    private static IServiceCollection InstallMaps(this List<Type> maps, IServiceCollection services)
    {
        maps.ForEach(x => services.AddTransient(typeof(IMapDefinition), x));

        return services
                .AddScoped(typeof(IMapper<,>), typeof(Mapper<,>))
                .AddSingleton<MapperCache>();
    }

    private static IApplicationBuilder UseMaps(this IApplicationBuilder app)
    {
        var mapperCache = app.ApplicationServices.GetRequiredService<MapperCache>();

        var maps = app.ApplicationServices.GetServices(typeof(IMapDefinition));

        foreach (var instance in maps)
        {
            var typeMap = instance.GetType();

            var implementedInterfaces = typeMap.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMapDefinition<,>));

            var genericArguments = implementedInterfaces.GetGenericArguments();

            mapperCache
                .Add(genericArguments[0], genericArguments[1], CreateFunc(typeMap, genericArguments[0], genericArguments[1], instance))
                .Add(genericArguments[1], genericArguments[0], CreateFunc(typeMap, genericArguments[1], genericArguments[0], instance));
        }

        return app;
    }

    private static Delegate CreateFunc(Type map, Type sourceType, Type destinationType, object instance)
    {

        var mapMethod = map.GetMethod("MapDefinition", [sourceType, destinationType]);
        var funcType = typeof(Func<,,>).MakeGenericType(sourceType, destinationType, typeof(ValueTask));
        var @delegate = Delegate.CreateDelegate(funcType, null, mapMethod);
        var createMethod = typeof(LambdaFactory).GetMethod("Create", BindingFlags.Static | BindingFlags.Public).MakeGenericMethod(sourceType, destinationType);
        return (Delegate)createMethod.Invoke(null, [@delegate, instance]);
    }
}
