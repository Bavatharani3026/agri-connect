var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();

app.MapGet("/", () => Results.Ok(new 
{ 
    service = "Farmer Service", 
    status = "Healthy", 
    port = 5002,
    timestamp = DateTime.UtcNow 
}));

app.MapGet("/api/farmers/health", () => Results.Ok(new 
{ 
    service = "Farmer Service", 
    status = "Healthy" 
}));

app.MapGet("/api/farmers", () => Results.Ok(new[] 
{
    new { id = 1, name = "John Farmer", location = "Northern Province", crops = new[] { "Rice", "Corn" } },
    new { id = 2, name = "Sarah Jenkins", location = "Central Province", crops = new[] { "Tea", "Vegetables" } }
}));

app.Run();
