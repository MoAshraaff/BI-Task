using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ProductService.Filters;

/// <summary>
/// The real OData Products controller (Controllers/OData/ProductsController.cs) is deliberately
/// hidden from Swashbuckle ([ApiExplorerSettings(IgnoreApi = true)]) because its overloaded
/// Get()/Get(int) actions crash Swashbuckle's route/schema generation. That leaves
/// $filter/$select/$orderby/etc completely invisible in Swagger UI even though the endpoints
/// work fine at runtime. This filter manually documents the two most useful OData GET routes so
/// they're discoverable and testable ("Try it out") from Swagger UI. Swagger UI just sends an
/// HTTP request to whatever path/parameters are declared here, so this is purely additive
/// documentation — it does not change routing, controllers, or how the real endpoints behave.
/// </summary>
public class ODataProductsDocumentationFilter : IDocumentFilter
{
    private const string TagName = "Products (OData)";

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var productDtoRef = new OpenApiSchema
        {
            Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = "ProductDto" }
        };

        var bearerSecurity = new List<OpenApiSecurityRequirement>
        {
            new()
            {
                {
                    new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
                    Array.Empty<string>()
                }
            }
        };

        swaggerDoc.Paths.Add("/odata/Products", new OpenApiPathItem
        {
            Operations = new Dictionary<OperationType, OpenApiOperation>
            {
                [OperationType.Get] = BuildCollectionOperation(productDtoRef, bearerSecurity)
            }
        });

        swaggerDoc.Paths.Add("/odata/Products({key})", new OpenApiPathItem
        {
            Operations = new Dictionary<OperationType, OpenApiOperation>
            {
                [OperationType.Get] = BuildByKeyOperation(productDtoRef, bearerSecurity)
            }
        });

        if (swaggerDoc.Tags.All(t => t.Name != TagName))
        {
            swaggerDoc.Tags.Add(new OpenApiTag { Name = TagName, Description = "OData v4 query endpoints (documented manually — see filter comments)" });
        }
    }

    private static OpenApiOperation BuildCollectionOperation(OpenApiSchema productDtoRef, List<OpenApiSecurityRequirement> security) => new()
    {
        OperationId = "ODataProducts_GetCollection",
        Tags = new List<OpenApiTag> { new() { Name = TagName } },
        Summary = "Query products with OData ($filter, $select, $orderby, $top)",
        Parameters = new List<OpenApiParameter>
        {
            ODataQueryParam("$filter", "e.g. Price gt 30 and Category eq 'Accessories'"),
            ODataQueryParam("$select", "e.g. Id,Name,Price"),
            ODataQueryParam("$orderby", "e.g. Price desc"),
            ODataQueryParam("$top", "Max number of results, e.g. 5")
        },
        Security = security,
        Responses = new OpenApiResponses
        {
            ["200"] = new OpenApiResponse
            {
                Description = "OK",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = new Dictionary<string, OpenApiSchema>
                            {
                                ["@odata.context"] = new OpenApiSchema { Type = "string" },
                                ["value"] = new OpenApiSchema { Type = "array", Items = productDtoRef }
                            }
                        }
                    }
                }
            },
            ["401"] = new OpenApiResponse { Description = "Unauthorized" }
        }
    };

    private static OpenApiOperation BuildByKeyOperation(OpenApiSchema productDtoRef, List<OpenApiSecurityRequirement> security) => new()
    {
        OperationId = "ODataProducts_GetByKey",
        Tags = new List<OpenApiTag> { new() { Name = TagName } },
        Summary = "Get a single product by key via OData",
        Parameters = new List<OpenApiParameter>
        {
            new()
            {
                Name = "key",
                In = ParameterLocation.Path,
                Required = true,
                Description = "Product Id",
                Schema = new OpenApiSchema { Type = "integer", Format = "int32" }
            }
        },
        Security = security,
        Responses = new OpenApiResponses
        {
            ["200"] = new OpenApiResponse
            {
                Description = "OK",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new OpenApiMediaType { Schema = productDtoRef }
                }
            },
            ["401"] = new OpenApiResponse { Description = "Unauthorized" },
            ["404"] = new OpenApiResponse { Description = "Not Found" }
        }
    };

    private static OpenApiParameter ODataQueryParam(string name, string description) => new()
    {
        Name = name,
        In = ParameterLocation.Query,
        Required = false,
        Description = description,
        Schema = new OpenApiSchema { Type = "string" }
    };
}
