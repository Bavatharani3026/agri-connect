using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddHttpClient();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors();

// --- Load .env file if present ---
var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (!File.Exists(envPath))
{
    envPath = Path.Combine(AppContext.BaseDirectory, ".env");
}
if (File.Exists(envPath))
{
    foreach (var line in File.ReadAllLines(envPath))
    {
        var trimmed = line.Trim();
        if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('#')) continue;
        var parts = trimmed.Split('=', 2);
        if (parts.Length == 2)
        {
            Environment.SetEnvironmentVariable(parts[0].Trim(), parts[1].Trim());
        }
    }
}

// --- Config ---
var config = app.Configuration;
var supabaseUrl = Environment.GetEnvironmentVariable("NEXT_PUBLIC_SUPABASE_URL")
    ?? config["Supabase:Url"]
    ?? "https://wtyydxjnzqlitpufxnqu.supabase.co";
var anonKey = Environment.GetEnvironmentVariable("NEXT_PUBLIC_SUPABASE_ANON_KEY")
    ?? config["Supabase:AnonKey"]
    ?? "";
var serviceRoleKey = Environment.GetEnvironmentVariable("SUPABASE_SERVICE_ROLE_KEY")
    ?? config["Supabase:ServiceRoleKey"]
    ?? "";

Console.WriteLine($"[Admin Service] Supabase URL: {supabaseUrl}");
Console.WriteLine($"[Admin Service] Anon Key loaded: {!string.IsNullOrEmpty(anonKey)}");
Console.WriteLine($"[Admin Service] Service Role Key loaded: {!string.IsNullOrEmpty(serviceRoleKey)}");

var jsonOpts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

// ----------------------------------------------------------------
// Health endpoints
// ----------------------------------------------------------------
app.MapGet("/", () => Results.Ok(new { service = "Admin Service", status = "Healthy", port = 5001 }));
app.MapGet("/api/auth/health", () => Results.Ok(new { service = "Admin Service", status = "Healthy", port = 5001 }));
app.MapGet("/health", () => Results.Ok(new { service = "Admin Service", status = "Healthy", port = 5001 }));

// ----------------------------------------------------------------
// Login Handler
// ----------------------------------------------------------------
var handleLogin = async (HttpContext ctx, IHttpClientFactory factory) =>
{
    LoginRequest? req;
    try
    {
        req = await JsonSerializer.DeserializeAsync<LoginRequest>(ctx.Request.Body, jsonOpts);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Admin Auth] Error deserializing body: {ex.Message}");
        return Results.BadRequest(new { success = false, message = "Invalid request body" });
    }

    if (req is null || string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
    {
        return Results.BadRequest(new { success = false, message = "Email and password are required" });
    }

    Console.WriteLine($"[Admin Auth] Processing login attempt for: {req.Email}");

    var http = factory.CreateClient();

    // Step 1: Authenticate with Supabase Auth
    var authBody = JsonSerializer.Serialize(new { email = req.Email, password = req.Password });
    var authReq = new HttpRequestMessage(HttpMethod.Post, $"{supabaseUrl}/auth/v1/token?grant_type=password")
    {
        Content = new StringContent(authBody, Encoding.UTF8, "application/json")
    };
    authReq.Headers.Add("apikey", anonKey);

    HttpResponseMessage authResp;
    try
    {
        authResp = await http.SendAsync(authReq);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Admin Auth] Failed to contact Supabase: {ex.Message}");
        return Results.Problem($"Auth service connection error: {ex.Message}");
    }

    var authJson = await authResp.Content.ReadAsStringAsync();

    if (!authResp.IsSuccessStatusCode)
    {
        Console.WriteLine($"[Admin Auth] Supabase auth failed ({authResp.StatusCode}): {authJson}");
        string errMsg = "Invalid credentials";
        try
        {
            var errObj = JsonSerializer.Deserialize<SupabaseErrorResponse>(authJson, jsonOpts);
            if (!string.IsNullOrEmpty(errObj?.ErrorDescription)) errMsg = errObj.ErrorDescription;
            else if (!string.IsNullOrEmpty(errObj?.Message)) errMsg = errObj.Message;
            else if (!string.IsNullOrEmpty(errObj?.Msg)) errMsg = errObj.Msg;
        }
        catch { }

        return Results.Json(new { success = false, message = errMsg }, statusCode: (int)authResp.StatusCode);
    }

    var authData = JsonSerializer.Deserialize<SupabaseAuthResponse>(authJson, jsonOpts);
    if (authData is null || string.IsNullOrEmpty(authData.AccessToken) || authData.User is null)
    {
        return Results.Json(new { success = false, message = "Invalid credentials" }, statusCode: 401);
    }

    var supabaseUserId = authData.User.Id;
    var accessToken = authData.AccessToken;

    // Step 2: Query platform_users by auth_user_id
    var userReq = new HttpRequestMessage(HttpMethod.Get,
        $"{supabaseUrl}/rest/v1/platform_users?auth_user_id=eq.{supabaseUserId}&select=*");
    userReq.Headers.Add("apikey", serviceRoleKey);
    userReq.Headers.Add("Authorization", $"Bearer {serviceRoleKey}");

    HttpResponseMessage userResp;
    try
    {
        userResp = await http.SendAsync(userReq);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Admin Auth] Database query failed: {ex.Message}");
        return Results.Problem($"Database connection error: {ex.Message}");
    }

    if (!userResp.IsSuccessStatusCode)
    {
        var errText = await userResp.Content.ReadAsStringAsync();
        Console.WriteLine($"[Admin Auth] platform_users lookup error: {errText}");
        return Results.Json(new { success = false, message = "User profile not found in database" }, statusCode: 404);
    }

    var userJson = await userResp.Content.ReadAsStringAsync();
    var users = JsonSerializer.Deserialize<PlatformUser[]>(userJson, jsonOpts);

    if (users is null || users.Length == 0)
    {
        Console.WriteLine($"[Admin Auth] No platform_users record for auth_user_id: {supabaseUserId}");
        return Results.Json(new { success = false, message = "No assigned platform profile found for this user" }, statusCode: 403);
    }

    var platformUser = users[0];

    if (platformUser.Status != "active")
    {
        Console.WriteLine($"[Admin Auth] User {platformUser.Email} is not active (status: {platformUser.Status})");
        return Results.Json(new { success = false, message = $"Account is {platformUser.Status}" }, statusCode: 403);
    }

    Console.WriteLine($"[Admin Auth] Login successful for {platformUser.Email} (Role: {platformUser.Role})");

    return Results.Ok(new
    {
        success = true,
        role = platformUser.Role,
        token = accessToken,
        user = new
        {
            id = platformUser.Id,
            email = platformUser.Email,
            full_name = platformUser.FullName,
            role = platformUser.Role,
            status = platformUser.Status
        }
    });
};

app.MapPost("/api/auth/login", handleLogin);
app.MapPost("/login", handleLogin);

// ----------------------------------------------------------------
// Register Handler
// ----------------------------------------------------------------
var handleRegister = async (HttpContext ctx, IHttpClientFactory factory) =>
{
    RegisterRequest? req;
    try
    {
        req = await JsonSerializer.DeserializeAsync<RegisterRequest>(ctx.Request.Body, jsonOpts);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Admin Auth] Error deserializing register body: {ex.Message}");
        return Results.BadRequest(new { success = false, message = "Invalid request body" });
    }

    if (req is null || string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password)
        || string.IsNullOrWhiteSpace(req.FullName) || string.IsNullOrWhiteSpace(req.Role))
    {
        return Results.BadRequest(new { success = false, message = "Email, password, full_name, and role are required" });
    }

    var allowedRoles = new[] { "farmer", "supplier" };
    if (!Array.Exists(allowedRoles, r => r == req.Role.ToLowerInvariant()))
    {
        return Results.BadRequest(new { success = false, message = "Role must be 'farmer' or 'supplier'" });
    }

    Console.WriteLine($"[Admin Auth] Registering new {req.Role}: {req.Email} ({req.FullName})");

    var http = factory.CreateClient();

    // Step 1: Create user via Supabase Admin Auth API (auto-confirms email and bypasses SMTP rate limits)
    var signupBody = JsonSerializer.Serialize(new
    {
        email = req.Email,
        password = req.Password,
        email_confirm = true
    });
    var signupReq = new HttpRequestMessage(HttpMethod.Post, $"{supabaseUrl}/auth/v1/admin/users")
    {
        Content = new StringContent(signupBody, Encoding.UTF8, "application/json")
    };
    signupReq.Headers.Add("apikey", serviceRoleKey);
    signupReq.Headers.Add("Authorization", $"Bearer {serviceRoleKey}");

    HttpResponseMessage signupResp;
    try
    {
        signupResp = await http.SendAsync(signupReq);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Admin Auth] Supabase signup connection error: {ex.Message}");
        return Results.Problem($"Auth service error: {ex.Message}");
    }

    var signupJson = await signupResp.Content.ReadAsStringAsync();

    if (!signupResp.IsSuccessStatusCode)
    {
        Console.WriteLine($"[Admin Auth] Supabase signup failed ({signupResp.StatusCode}): {signupJson}");
        string message = "Registration failed";
        try
        {
            var errData = JsonSerializer.Deserialize<SupabaseErrorResponse>(signupJson, jsonOpts);
            if (!string.IsNullOrEmpty(errData?.Message)) message = errData.Message;
            else if (!string.IsNullOrEmpty(errData?.ErrorDescription)) message = errData.ErrorDescription;
            else if (!string.IsNullOrEmpty(errData?.Msg)) message = errData.Msg;
        }
        catch { }
        return Results.Json(new { success = false, message }, statusCode: (int)signupResp.StatusCode);
    }

    var signupData = JsonSerializer.Deserialize<SupabaseSignupResponse>(signupJson, jsonOpts);
    if (signupData is null || string.IsNullOrEmpty(signupData.Id))
    {
        return Results.Json(new { success = false, message = "Failed to retrieve new user ID from auth provider" }, statusCode: 400);
    }

    // Step 2: Insert into platform_users
    var insertBody = JsonSerializer.Serialize(new
    {
        auth_user_id = signupData.Id,
        full_name = req.FullName,
        email = req.Email,
        role = req.Role.ToLowerInvariant(),
        phone = req.Phone ?? "",
        status = "active"
    });

    var insertReq = new HttpRequestMessage(HttpMethod.Post, $"{supabaseUrl}/rest/v1/platform_users")
    {
        Content = new StringContent(insertBody, Encoding.UTF8, "application/json")
    };
    insertReq.Headers.Add("apikey", serviceRoleKey);
    insertReq.Headers.Add("Authorization", $"Bearer {serviceRoleKey}");
    insertReq.Headers.Add("Prefer", "return=minimal");

    HttpResponseMessage insertResp;
    try
    {
        insertResp = await http.SendAsync(insertReq);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Admin Auth] Failed to insert into platform_users: {ex.Message}");
        return Results.Problem($"Database error: {ex.Message}");
    }

    if (!insertResp.IsSuccessStatusCode)
    {
        var errText = await insertResp.Content.ReadAsStringAsync();
        Console.WriteLine($"[Admin Auth] platform_users insert failed ({insertResp.StatusCode}): {errText}");
        string message = "Failed to create user profile";
        try
        {
            var errData = JsonSerializer.Deserialize<SupabaseErrorResponse>(errText, jsonOpts);
            if (!string.IsNullOrEmpty(errData?.Message)) message = errData.Message;
        }
        catch { }
        return Results.Json(new { success = false, message }, statusCode: 400);
    }

    Console.WriteLine($"[Admin Auth] Registration completed for {req.Email}");

    return Results.Ok(new
    {
        success = true,
        message = "Registration successful",
        role = req.Role.ToLowerInvariant()
    });
};

app.MapPost("/api/auth/register", handleRegister);
app.MapPost("/register", handleRegister);

app.Run();

// ----------------------------------------------------------------
// DTOs
// ----------------------------------------------------------------
record LoginRequest(
    [property: JsonPropertyName("email")] string? Email,
    [property: JsonPropertyName("password")] string? Password
);

record RegisterRequest(
    [property: JsonPropertyName("email")] string? Email,
    [property: JsonPropertyName("password")] string? Password,
    [property: JsonPropertyName("full_name")] string? FullName,
    [property: JsonPropertyName("role")] string? Role,
    [property: JsonPropertyName("phone")] string? Phone
);

class SupabaseAuthResponse
{
    [JsonPropertyName("access_token")] public string? AccessToken { get; set; }
    [JsonPropertyName("user")] public SupabaseAuthUser? User { get; set; }
}

class SupabaseAuthUser
{
    [JsonPropertyName("id")] public string? Id { get; set; }
}

class SupabaseSignupResponse
{
    [JsonPropertyName("id")] public string? Id { get; set; }
}

class PlatformUser
{
    [JsonPropertyName("id")] public string? Id { get; set; }
    [JsonPropertyName("auth_user_id")] public string? AuthUserId { get; set; }
    [JsonPropertyName("full_name")] public string? FullName { get; set; }
    [JsonPropertyName("email")] public string? Email { get; set; }
    [JsonPropertyName("role")] public string? Role { get; set; }
    [JsonPropertyName("status")] public string? Status { get; set; }
    [JsonPropertyName("phone")] public string? Phone { get; set; }
}

class SupabaseErrorResponse
{
    [JsonPropertyName("message")] public string? Message { get; set; }
    [JsonPropertyName("msg")] public string? Msg { get; set; }
    [JsonPropertyName("error_description")] public string? ErrorDescription { get; set; }
}
