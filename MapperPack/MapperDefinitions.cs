using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace MapperPack;

public interface IMapper<TModel, TEntity>
{
    TModel Map(TEntity source);
    TEntity Map(TModel source);
    IEnumerable<TModel> Map(IEnumerable<TEntity> source);
    IEnumerable<TEntity> Map(IEnumerable<TModel> source);

    Task<TModel> MapAsync(TEntity source, CancellationToken cancellationToken = default);
    Task<TEntity> MapAsync(TModel source, CancellationToken cancellationToken = default);
    Task<IEnumerable<TModel>> MapAsync(IEnumerable<TEntity> source, CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> MapAsync(IEnumerable<TModel> source, CancellationToken cancellationToken = default);
}

public abstract class Mapper<TModel, TEntity> : MapperBase, IMapper<TModel, TEntity>
{
    public abstract void MapDefinition(TEntity source, TModel destination);

    public abstract void MapDefinition(TModel source, TEntity destination);

    public TModel Map(TEntity source)
        => GetMapper<TEntity, TModel>(MapDefinition)(source);

    public TEntity Map(TModel source)
        => GetMapper<TModel, TEntity>(MapDefinition)(source);

    public IEnumerable<TModel> Map(IEnumerable<TEntity> source)
        => source.Select(Map);

    public IEnumerable<TEntity> Map(IEnumerable<TModel> source)
        => source.Select(Map);

    public async Task<TModel> MapAsync(TEntity source, CancellationToken cancellationToken = default)
        => await Task.FromResult(Map(source));

    public async Task<TEntity> MapAsync(TModel source, CancellationToken cancellationToken = default)
        => await Task.FromResult(Map(source));

    public async Task<IEnumerable<TModel>> MapAsync(IEnumerable<TEntity> source, CancellationToken cancellationToken = default)
        => await Task.WhenAll(source.Select(async entity => await MapAsync(entity, cancellationToken)));

    public async Task<IEnumerable<TEntity>> MapAsync(IEnumerable<TModel> source, CancellationToken cancellationToken = default)
        => await Task.WhenAll(source.Select(async entity => await MapAsync(entity, cancellationToken)));
}
public abstract class MapperBase
{
    protected static Func<TSource, TDestination> GetMapper<TSource, TDestination>(Action<TSource, TDestination> definitions)
    {
        var sourceParameter = Expression.Parameter(typeof(TSource), "source");

        var memberBindings = new List<MemberBinding>();
        foreach (var sourceProperty in typeof(TSource).GetProperties())
        {
            var destinationProperty = typeof(TDestination).GetProperty(sourceProperty.Name);

            if (destinationProperty != null && destinationProperty.CanWrite)
            {
                var sourceValue = Expression.Property(sourceParameter, sourceProperty);
                var binding = Expression.Bind(destinationProperty, sourceValue);
                memberBindings.Add(binding);
            }
        }

        var newDestination = Expression.New(typeof(TDestination));
        var initialization = Expression.MemberInit(newDestination, memberBindings);

        var destinationParameter = Expression.Parameter(typeof(TDestination), "destination");

        Expression body;
        if (definitions != null)
        {
            var callCustomMappings = Expression.Call(
                Expression.Constant(definitions.Target),
                definitions.Method,
                sourceParameter,
                destinationParameter
            );

            body = Expression.Block(
                [destinationParameter],
                Expression.Assign(destinationParameter, initialization),
                callCustomMappings,
                destinationParameter
            );
        }
        else
        {
            body = Expression.Block(
                [destinationParameter],
                Expression.Assign(destinationParameter, initialization),
                destinationParameter
            );
        }

        var lambda = Expression.Lambda<Func<TSource, TDestination>>(body, sourceParameter);

        return lambda.Compile();
    }
}