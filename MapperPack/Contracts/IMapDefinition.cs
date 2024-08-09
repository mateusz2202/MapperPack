using System.Threading.Tasks;

namespace MapperPack;

public interface IMapDefinition { }

public interface IMapDefinition<TModel, TEntity> : IMapDefinition
    where TModel : class, new()
    where TEntity : class, new()
{
    ValueTask MapDefinition(TModel source, TEntity destination);

    ValueTask MapDefinition(TEntity source, TModel destination);
}
