using MediatR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddControllers();
builder.Services.AddAuthorization();

// Source: https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Source: https://github.com/LuckyPennySoftware/MediatR
builder.Services.AddMediatR(c => c.RegisterServicesFromAssembly(typeof(Program).Assembly));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();
app.UseAuthorization();

app.Run();