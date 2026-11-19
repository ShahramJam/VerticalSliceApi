using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using System.Reflection;
using VerticalSliceApi.Features.Common;
using VerticalSliceApi.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

var assembly = Assembly.GetExecutingAssembly();

// EF Core 10 + SQL Server
builder.Services.AddDbContext<SchoolDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SchoolDbConnectionString")).UseAsyncSeeding(DbSeeder.SeedAsync));



// MediatR + Validation Pipeline
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});
builder.Services.AddValidatorsFromAssembly(assembly);

// Auto-discover all IEndpoint slices
var endpointTypes = assembly.GetTypes()
    .Where(t => t is { IsAbstract: false, IsInterface: false } && t.IsAssignableTo(typeof(IEndpoint)));

foreach (var type in endpointTypes)
{
    builder.Services.AddTransient(typeof(IEndpoint), type);
}

builder.Services.AddOpenApi();

var app = builder.Build();

// Return 400 Bad Request on FluentValidation errors
app.UseExceptionHandler(exceptionApp =>
{
    exceptionApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        if (exception is ValidationException validationException)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new
            {
                Title = "Validation Failed",
                Errors = validationException.Errors.Select(e => new { e.PropertyName, e.ErrorMessage })
            });
        }
    });
});

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<SchoolDbContext>();
    await db.Database.MigrateAsync();   

    app.MapOpenApi();  
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

// Map all Vertical Slice endpoints
foreach (var endpoint in app.Services.GetServices<IEndpoint>())
{
    endpoint.MapEndpoint(app);
}

app.Run();
