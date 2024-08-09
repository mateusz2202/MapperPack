using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace MapperPack;

public static partial class InsattlerMapper
{
    public static IServiceCollection AddMapper(this IServiceCollection services)
        => AppDomain.CurrentDomain
          .GetTypesFromAssembiles(IsMapperDefinition, "*.dll")
          .InstallMaps(services);

    public static IServiceCollection AddMapper(this IServiceCollection services, string searchPattern)
        => AppDomain.CurrentDomain
          .GetTypesFromAssembiles(IsMapperDefinition, searchPattern)
          .InstallMaps(services);

    public static IApplicationBuilder UseMapper(this IApplicationBuilder app)
        => UseMaps(app);

}
