using EthereumT.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace EthereumT.DAL.Context
{
    public class AppDbContext : DbContext
    {
        public DbSet<Wallet> Wallets { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}