namespace ShopNet.Contracts.Products;

public sealed record ProductResponse(
    int Id,
    string Name,
    string Description,
    decimal Price,
    int CategoryId,
    string CategoryName,
    bool IsActive);
