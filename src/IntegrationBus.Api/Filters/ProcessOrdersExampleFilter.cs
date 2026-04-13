using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace IntegrationBus.Api.Filters;

public class ProcessOrdersExampleFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type != typeof(Core.Models.ProcessOrdersRequest))
            return;

        schema.Example = new OpenApiObject
        {
            ["filters"] = new OpenApiObject
            {
                ["Status"] = new OpenApiString("Active"),
                ["MinAmount"] = new OpenApiString("500")
            },
            ["orders"] = new OpenApiArray
            {
                new OpenApiObject
                {
                    ["id"] = new OpenApiString("001"),
                    ["status"] = new OpenApiString("Active"),
                    ["amount"] = new OpenApiInteger(1500),
                    ["product"] = new OpenApiString("Widget"),
                    ["createdAt"] = new OpenApiString("2026-01-01T00:00:00")
                },
                new OpenApiObject
                {
                    ["id"] = new OpenApiString("002"),
                    ["status"] = new OpenApiString("Cancelled"),
                    ["amount"] = new OpenApiInteger(200),
                    ["product"] = new OpenApiString("Gadget"),
                    ["createdAt"] = new OpenApiString("2026-01-02T00:00:00")
                }
            }
        };
    }
}
