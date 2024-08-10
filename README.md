# Get started:
1. Get Nuget
```powershell
dotnet add package MapperPack --version 1.0.12
```
2. Add to program
```c#
builder.Services.AddMapper();

var app = builder.Build();

app.UseMapper();
```
3. Example usage
```c#
public class ProductEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public string Description { get; set; }
}

public class ProductModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public string Note { get; set; }
}

public sealed class ProductMap : IMapDefinition<ProductEntity, ProductModel>
{
    public async ValueTask MapDefinition(ProductEntity source, ProductModel destination)
    {
        destination.Note = source.Description;
        await Task.CompletedTask;
    }

    public async ValueTask MapDefinition(ProductModel source, ProductEntity destination)
    {
        destination.Description = source.Note;
        await Task.CompletedTask;
    }
}
```
```c#
//In controller file
[ApiController]
[Route("[controller]")]
public class ProductController(IMapper<ProductEntity, ProductModel> mapper) : ControllerBase
{
    private static List<ProductEntity> _products = Enumerable.Range(1, 3).Select(x => new ProductEntity()
    {
        Id = x,
        Name = $"Name{x}",
        Code = $"Code{x}",
        Description = $"bla bla ... {x}"
    }).ToList();

    [HttpGet]
    public async Task<IEnumerable<ProductModel>> Get()
    {
        return await mapper.MapAsync(_products);
    }
}
```
result:
```json
[
  {
    "id": 1,
    "name": "Name1",
    "code": "Code1",
    "note": "bla bla ... 1"
  },
  {
    "id": 2,
    "name": "Name2",
    "code": "Code2",
    "note": "bla bla ... 2"
  },
  {
    "id": 3,
    "name": "Name3",
    "code": "Code3",
    "note": "bla bla ... 3"
  }
]
```
