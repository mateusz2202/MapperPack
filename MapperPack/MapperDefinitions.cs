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

public abstract class Mapper<TModel, TEntity> : IMapper<TModel, TEntity>
{
    public abstract TModel Map(TEntity source);

    public abstract TEntity Map(TModel source);


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