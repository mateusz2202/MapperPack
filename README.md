# Get started:
1. Nuget
```powershell
dotnet add package MapperPack
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
### Deep mapper:
```c#
public class ProductInfoEntity
{

    public decimal X { get; set; }
    public decimal Y { get; set; }
}

public class ProductInfoModel
{

    public decimal Width { get; set; }
    public decimal Height { get; set; }
}

public sealed class ProductInfoMap : IMapDefinition<ProductInfoEntity, ProductInfoModel>
{
    public async ValueTask MapDefinition(ProductInfoEntity source, ProductInfoModel destination)
    {
        destination.Height = source.Y;
        destination.Width = source.X;
        await Task.CompletedTask;
    }

    public async ValueTask MapDefinition(ProductInfoModel source, ProductInfoEntity destination)
    {
        destination.X = source.Height;
        destination.Y = source.Width;
        await Task.CompletedTask;
    }
}
```
```c#
public class ProductEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public string Description { get; set; }
    public ProductInfoEntity ProductInfo { get; set; }
}

public class ProductModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public string Note { get; set; }

    public ProductInfoModel ProductInfo { get; set; }
}

public sealed class ProductMap : IMapDefinition<ProductEntity, ProductModel>
{
    private readonly IMapper<ProductInfoEntity, ProductInfoModel> _mapper;

    public ProductMap(IMapper<ProductInfoEntity, ProductInfoModel> mapper)
    {
        _mapper = mapper;
    }

    public async ValueTask MapDefinition(ProductEntity source, ProductModel destination)
    {
        destination.Note = source.Description;
        destination.ProductInfo = await _mapper.MapAsync(source.ProductInfo);
        await Task.CompletedTask;
    }

    public async ValueTask MapDefinition(ProductModel source, ProductEntity destination)
    {
        destination.Description = source.Note;
        destination.ProductInfo = await _mapper.MapAsync(source.ProductInfo);
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
        Description = $"bla bla ... {x}",
        ProductInfo = new ProductInfoEntity()
        {
            X = x * 5,
            Y = x * 10,
        }
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
        "note": "bla bla ... 1",
        "productInfo": {
            "width": 5,
            "height": 10
        }
    },
    {
        "id": 2,
        "name": "Name2",
        "code": "Code2",
        "note": "bla bla ... 2",
        "productInfo": {
            "width": 10,
            "height": 20
        }
    },
    {
        "id": 3,
        "name": "Name3",
        "code": "Code3",
        "note": "bla bla ... 3",
        "productInfo": {
            "width": 15,
            "height": 30
        }
    }
]
```
