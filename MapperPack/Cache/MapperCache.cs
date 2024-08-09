using System;
using System.Collections.Concurrent;

namespace MapperPack.Cache;

public record struct MapperKey(Type Source, Type Destination);

public class MapperCache
{
    private readonly ConcurrentDictionary<MapperKey, Delegate> _cache;

    public MapperCache()
    {
        _cache = new();
    }

    public MapperCache Add(Type t1, Type t2, Delegate func)
    {
        _cache[GetKey(t1, t2)] = func;
        return this;
    }

    public Func<TSource, TDestination> Get<TSource, TDestination>()
    {
        _cache.TryGetValue(GetKey<TSource, TDestination>(), out var func);
        return (Func<TSource, TDestination>)func;
    }

    private static MapperKey GetKey(Type t1, Type t2) => new(t1, t2);

    private static MapperKey GetKey<TSource, TDestination>() => new(typeof(TSource), typeof(TDestination));
}
