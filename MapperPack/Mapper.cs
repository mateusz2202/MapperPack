using MapperPack.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
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

public class Mapper<TModel, TEntity>(MapperCache mapperCache) : MapperBase(mapperCache), IMapper<TModel, TEntity>
{

    public TModel Map(TEntity source)
        => GetMapper<TEntity, TModel>()(source);

    public TEntity Map(TModel source)
        => GetMapper<TModel, TEntity>()(source);

    public IEnumerable<TModel> Map(IEnumerable<TEntity> source)
        => GetMapperCollection<TModel, TEntity>(source);

    public IEnumerable<TEntity> Map(IEnumerable<TModel> source)
        => GetMapperCollection<TEntity, TModel>(source);

    public async Task<TModel> MapAsync(TEntity source, CancellationToken cancellationToken = default)
        => await Task.Run(() => Map(source), cancellationToken);

    public async Task<TEntity> MapAsync(TModel source, CancellationToken cancellationToken = default)
        => await Task.Run(() => Map(source), cancellationToken);

    public async Task<IEnumerable<TModel>> MapAsync(IEnumerable<TEntity> source, CancellationToken cancellationToken = default)
        => await Task.Run(() => Map(source), cancellationToken);

    public async Task<IEnumerable<TEntity>> MapAsync(IEnumerable<TModel> source, CancellationToken cancellationToken = default)
        => await Task.Run(() => Map(source), cancellationToken);
}

public class MapperBase
{
    private static MapperCache _mapperCache;
    protected MapperBase(MapperCache mapperCache)
    {
        _mapperCache = mapperCache;
    }

    protected static IEnumerable<TDestination> GetMapperCollection<TDestination, TSource>(IEnumerable<TSource> source)
    {
        var map = GetMapper<TSource, TDestination>();

        return source.Select(map);
    }

    protected static Func<TSource, TDestination> GetMapper<TSource, TDestination>()
        => _mapperCache.Get<TSource, TDestination>();

}