using MCBank.WebApi.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MCBank.WebApi.Infrastructure.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Balance).HasPrecision(18, 2);
        builder.Property(a => a.Iban).IsRequired().HasMaxLength(34);
        builder.HasIndex(a => a.Iban).IsUnique();
        builder.HasQueryFilter(a => !a.IsDeleted);
        builder
            .HasOne(a => a.User)
            .WithMany(u => u.Accounts)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}