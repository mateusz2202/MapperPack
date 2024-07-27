using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MapperPack;

public interface IMapper<TSource, TResult>
{
    Task<TResult> Map(TSource source);
    Task<TSource> Map(TResult source);
}


public abstract class Mapper<TSource, TResult> : IMapper<TSource, TResult>
{
    public abstract Task<TResult> Map(TSource source);

    public abstract Task<TSource> Map(TResult source);

    public async Task<IEnumerable<TResult>> Map(IEnumerable<TSource> source)
        => await Task.WhenAll(source.Select(Map));

    public async Task<IEnumerable<TSource>> Map(IEnumerable<TResult> source)
        => await Task.WhenAll(source.Select(Map));
}