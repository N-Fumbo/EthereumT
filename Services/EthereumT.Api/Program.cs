using EthereumT.Api.Middlewares;
using EthereumT.Api.Services;
using EthereumT.DAL.Context;
using EthereumT.DAL.Repositories.Base;
using EthereumT.Domain.Entities;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

string connectionString = builder.Configuration.GetConnectionString("ApiConntection")
     ?? throw new InvalidOperationException("Connection string 'ApiConntection' not found.");

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString, m => m.MigrationsAssembly("EthereumT.PgSQL")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton(builder.Configuration);

builder.Services.AddMemoryCache();

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddScoped<RepositoryAsync<Wallet>>();

builder.Services.AddHostedService<ObserverETHWallets>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();