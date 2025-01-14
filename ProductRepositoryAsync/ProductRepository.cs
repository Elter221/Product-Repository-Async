using System.Globalization;

namespace ProductRepositoryAsync;

/// <summary>
/// Represents a product storage service and provides a set of methods for managing the list of products.
/// </summary>
public class ProductRepository : IProductRepository
{
    private readonly string productCollectionName;
    private readonly IDatabase database;

    public ProductRepository(string productCollectionName, IDatabase database)
    {
        this.productCollectionName = productCollectionName;
        this.database = database;
    }

    public async Task<Product> GetProductAsync(int productId)
    {
        OperationResult result = await this.database.IsCollectionExistAsync(this.productCollectionName, out bool collectionExists);

        if (result == OperationResult.ConnectionIssue)
        {
            throw new DatabaseConnectionException();
        }
        else if (result != OperationResult.Success)
        {
            throw new RepositoryException();
        }

        if (!collectionExists)
        {
            throw new CollectionNotFoundException();
        }

        result = await this.database.IsCollectionElementExistAsync(this.productCollectionName, productId, out bool collectionElementExists);

        if (result == OperationResult.ConnectionIssue)
        {
            throw new DatabaseConnectionException();
        }
        else if (result != OperationResult.Success)
        {
            throw new RepositoryException();
        }

        if (!collectionElementExists)
        {
            throw new ProductNotFoundException();
        }

        result = await this.database.GetCollectionElementAsync(this.productCollectionName, productId, out IDictionary<string, string> data);

        if (result == OperationResult.ConnectionIssue)
        {
            throw new DatabaseConnectionException();
        }
        else if (result != OperationResult.Success)
        {
            throw new RepositoryException();
        }

        return new Product
        {
            Id = productId,
            Name = data["name"],
            Category = data["category"],
            UnitPrice = decimal.Parse(data["price"], CultureInfo.InvariantCulture),
            UnitsInStock = int.Parse(data["in-stock"], CultureInfo.InvariantCulture),
            Discontinued = bool.Parse(data["discontinued"]),
        };
    }

    public async Task<int> AddProductAsync(Product product)
    {
        if (string.IsNullOrWhiteSpace(product.Name) || string.IsNullOrEmpty(product.Name))
        {
            throw new ArgumentException("Product name can't be Null or Empty or WhiteSpace", nameof(product));
        }

        if (string.IsNullOrWhiteSpace(product.Category) || string.IsNullOrEmpty(product.Category))
        {
            throw new ArgumentException("Category name can't be Null or Empty or WhiteSpace", nameof(product));
        }

        if (product.UnitPrice <= 0)
        {
            throw new ArgumentException("Price can't be less than 0", nameof(product));
        }

        if (product.UnitsInStock <= 0)
        {
            throw new ArgumentException("Unit in stock can't be less than 0", nameof(product));
        }

        return await this.AddProductInternalAsunc(product);
    }

    public async Task RemoveProductAsync(int productId)
    {
        var result = await this.database.IsCollectionExistAsync(this.productCollectionName, out bool collectionExists);

        if (result == OperationResult.ConnectionIssue)
        {
            throw new DatabaseConnectionException();
        }
        else if (result != OperationResult.Success)
        {
            throw new RepositoryException();
        }

        if (!collectionExists)
        {
            throw new CollectionNotFoundException();
        }

        result = await this.database.IsCollectionElementExistAsync(this.productCollectionName, productId, out bool collectionElementExists);

        if (result == OperationResult.ConnectionIssue)
        {
            throw new DatabaseConnectionException();
        }
        else if (result != OperationResult.Success)
        {
            throw new RepositoryException();
        }

        if (!collectionElementExists)
        {
            throw new ProductNotFoundException();
        }

        result = await this.database.DeleteCollectionElementAsync(this.productCollectionName, productId);

        if (result == OperationResult.ConnectionIssue)
        {
            throw new DatabaseConnectionException();
        }
        else if (result != OperationResult.Success)
        {
            throw new RepositoryException();
        }
    }

    public async Task UpdateProductAsync(Product product)
    {
        if (string.IsNullOrWhiteSpace(product.Name) || string.IsNullOrEmpty(product.Name))
        {
            throw new ArgumentException("Product name can't be Null or Empty or WhiteSpace", nameof(product));
        }

        if (string.IsNullOrWhiteSpace(product.Category) || string.IsNullOrEmpty(product.Category))
        {
            throw new ArgumentException("Category name can't be Null or Empty or WhiteSpace", nameof(product));
        }

        if (product.UnitPrice <= 0)
        {
            throw new ArgumentException("Price can't be less than 0", nameof(product));
        }

        if (product.UnitsInStock <= 0)
        {
            throw new ArgumentException("Unit in stock can't be less than 0", nameof(product));
        }

        await this.UpdateProductInternalAsync(product);
    }

    private async Task UpdateProductInternalAsync(Product product)
    {
        var result = await this.database.IsCollectionExistAsync(this.productCollectionName, out bool collectionExists);

        if (result == OperationResult.ConnectionIssue)
        {
            throw new DatabaseConnectionException();
        }
        else if (result != OperationResult.Success)
        {
            throw new RepositoryException();
        }

        if (!collectionExists)
        {
            throw new CollectionNotFoundException();
        }

        result = await this.database.IsCollectionElementExistAsync(this.productCollectionName, product.Id, out bool collectionElementExists);

        if (result == OperationResult.ConnectionIssue)
        {
            throw new DatabaseConnectionException();
        }
        else if (result != OperationResult.Success)
        {
            throw new RepositoryException();
        }

        if (!collectionElementExists)
        {
            throw new ProductNotFoundException();
        }

        var productDictionary = new Dictionary<string, string>
        {
             { "name", product.Name },
             { "category", product.Category },
             { "price", product.UnitPrice.ToString(CultureInfo.InvariantCulture) },
             { "in-stock", product.UnitsInStock.ToString(CultureInfo.InvariantCulture) },
             { "discontinued", product.Discontinued.ToString() },
        };

        result = await this.database.UpdateCollectionElementAsync(this.productCollectionName, product.Id, productDictionary);

        if (result == OperationResult.ConnectionIssue)
        {
            throw new DatabaseConnectionException();
        }
        else if (result != OperationResult.Success)
        {
            throw new RepositoryException();
        }
    }

    private async Task<int> AddProductInternalAsunc(Product product)
    {
        var result = await this.database.IsCollectionExistAsync(this.productCollectionName, out bool collectionExists);

        if (result == OperationResult.ConnectionIssue)
        {
            throw new DatabaseConnectionException();
        }
        else if (result != OperationResult.Success)
        {
            throw new RepositoryException();
        }

        if (!collectionExists)
        {
            result = await this.database.CreateCollectionAsync(this.productCollectionName);

            if (result == OperationResult.ConnectionIssue)
            {
                throw new DatabaseConnectionException();
            }
            else if (result != OperationResult.Success)
            {
                throw new RepositoryException();
            }
        }

        result = await this.database.GenerateIdAsync(this.productCollectionName, out int id);

        if (result == OperationResult.ConnectionIssue)
        {
            throw new DatabaseConnectionException();
        }
        else if (result != OperationResult.Success)
        {
            throw new RepositoryException();
        }

        var productDictionary = new Dictionary<string, string>
        {
             { "name", product.Name },
             { "category", product.Category },
             { "price", product.UnitPrice.ToString(CultureInfo.InvariantCulture) },
             { "in-stock", product.UnitsInStock.ToString(CultureInfo.InvariantCulture) },
             { "discontinued", product.Discontinued.ToString() },
        };

        result = await this.database.InsertCollectionElementAsync(this.productCollectionName, id, productDictionary);

        if (result == OperationResult.ConnectionIssue)
        {
            throw new DatabaseConnectionException();
        }
        else if (result != OperationResult.Success)
        {
            throw new RepositoryException();
        }

        return id;
    }
}
