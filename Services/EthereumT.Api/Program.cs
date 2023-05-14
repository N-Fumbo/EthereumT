using EthereumT.DAL.Context;
using EthereumT.DAL.Repositories.Base;
using EthereumT.Domain.Entities;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

string connectionString = builder.Configuration.GetConnectionString("ApiConntection")
     ?? throw new InvalidOperationException("Connection string 'ApiConntection' not found.");

string infuraApiKey = builder.Configuration.GetValue<string>("InfuraApiKey");

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString, m => m.MigrationsAssembly("EthereumT.PgSQL")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<RepositoryAsync<Wallet>>();

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