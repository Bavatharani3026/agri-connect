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
    service = "Officer Service", 
    status = "Healthy", 
    port = 5003,
    timestamp = DateTime.UtcNow 
}));

app.MapGet("/api/officers/health", () => Results.Ok(new 
{ 
    service = "Officer Service", 
    status = "Healthy" 
}));

app.MapGet("/api/officers", () => Results.Ok(new[] 
{
    new { id = 1, name = "Officer Silva", district = "District 1", jurisdiction = "Agricultural Extension" },
    new { id = 2, name = "Officer Perera", district = "District 2", jurisdiction = "Crop Inspection" }
}));

app.Run();
