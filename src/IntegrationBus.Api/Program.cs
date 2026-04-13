using System.Reflection;
using IntegrationBus.Api.Filters;
using IntegrationBus.Api.Middleware;
using IntegrationBus.Core.Interfaces;
using IntegrationBus.Services;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.OperationFilter<TenantHeaderOperationFilter>();
    c.SchemaFilter<ProcessOrdersExampleFilter>();
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);
});

// DI — Part 1
builder.Services.AddScoped<ITenantContext, TenantContext>();
builder.Services.AddScoped<ITenantService, TenantService>();

// DI — Part 2
builder.Services.AddScoped<IValidationHook, DefaultValidationHook>();
builder.Services.AddScoped<IOrderProcessor, OrderProcessor>();
builder.Services.AddScoped<IDynamicFilterService, DynamicFilterService>();
builder.Services.AddHttpClient<IExternalDispatcher, ExternalDispatcher>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ExternalApi:BaseUrl"] ?? "https://external-api.example.com");
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseMiddleware<TenantMiddleware>();
app.MapControllers();

app.Run();
