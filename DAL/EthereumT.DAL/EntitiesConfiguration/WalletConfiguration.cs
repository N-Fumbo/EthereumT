using EthereumT.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EthereumT.DAL.EntitiesConfiguration
{
    internal class WalletConfiguration : IEntityTypeConfiguration<Wallet>
    {
        public void Configure(EntityTypeBuilder<Wallet> builder)
        {
            builder.ToTable("Wallets");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Address)
                .HasMaxLength(42)
                .IsRequired();
        }
    }
}