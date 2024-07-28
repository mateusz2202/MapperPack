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
                Expression.Constant(definitions.Target),
                definitions.Method,
                sourceParameter,
                destinationParameter
            ));
        }

        var body = Expression.Block([destinationParameter], expressions);

        return Expression.Lambda<Func<TSource, TDestination>>(body, sourceParameter).Compile();
    }
}