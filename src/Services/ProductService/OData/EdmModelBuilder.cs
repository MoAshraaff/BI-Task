using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using ProductService.Models;

namespace ProductService.OData;

public static class EdmModelBuilder
{
    public static IEdmModel GetEdmModel()
    {
        var builder = new ODataConventionModelBuilder();
        builder.EntitySet<Product>("Products");
        return builder.GetEdmModel();
    }
}
