using EthereumT.DAL.Context;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

string connectionString = builder.Configuration.GetConnectionString("ApiConntection")
     ?? throw new InvalidOperationException("Connection string 'ApiConntection' not found.");

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString, m => m.MigrationsAssembly("EthereumT.PgSQL")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();