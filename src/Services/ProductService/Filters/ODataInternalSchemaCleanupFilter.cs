using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ProductService.Filters;

/// <summary>
/// The auto-registered OData service-document and $metadata routes return types from
/// Microsoft.AspNetCore.OData's own model (ODataServiceDocument, IEdmModel, EdmTypeKind, ...).
/// Swashbuckle walks those types recursively and dumps dozens of unrelated OData/EDM internal
/// types into the Swagger "Schemas" section — none of them are part of this API's actual REST
/// contract. Strip them so Schemas only lists this service's real request/response models.
/// </summary>
public class ODataInternalSchemaCleanupFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var leakedSchemaKeys = swaggerDoc.Components.Schemas.Keys
            .Where(key => key.StartsWith("Edm", StringComparison.Ordinal)
                       || key.StartsWith("IEdm", StringComparison.Ordinal)
                       || key.StartsWith("OData", StringComparison.Ordinal))
            .ToList();

        foreach (var key in leakedSchemaKeys)
        {
            swaggerDoc.Components.Schemas.Remove(key);
        }

        // Replace any now-dangling $ref left on the /odata* operations with a plain object
        // schema so the document stays valid instead of pointing at a removed component.
        foreach (var path in swaggerDoc.Paths.Where(p => p.Key.StartsWith("/odata", StringComparison.Ordinal)))
        {
            foreach (var operation in path.Value.Operations.Values)
            {
                foreach (var response in operation.Responses.Values)
                {
                    foreach (var content in response.Content.Values)
                    {
                        if (content.Schema?.Reference is not null && leakedSchemaKeys.Contains(content.Schema.Reference.Id))
                        {
                            content.Schema = new OpenApiSchema { Type = "object" };
                        }
                    }
                }
            }
        }
    }
}
