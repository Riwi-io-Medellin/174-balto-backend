var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// TODO: builder.Services.AddApplicationServices();
// TODO: builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Endpoints
// app.MapUserEndpoints();
// app.MapPetEndpoints();
// app.MapWalkerEndpoints();

app.Run();
