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
    service = "Supplier Service", 
    status = "Healthy", 
    port = 5004,
    timestamp = DateTime.UtcNow 
}));

app.MapGet("/api/suppliers/health", () => Results.Ok(new 
{ 
    service = "Supplier Service", 
    status = "Healthy" 
}));

app.MapGet("/api/suppliers", () => Results.Ok(new[] 
{
    new { id = 1, name = "Agro Seeds & Fertilizers Co.", category = "Seeds & Fertilizer", contact = "sales@agroseeds.com" },
    new { id = 2, name = "Green Equipment Machinery", category = "Equipment & Machinery", contact = "support@greenequip.com" }
}));

app.Run();
