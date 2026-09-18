using CardiacMonitor.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CardiacMonitor.Data.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    // Configures refresh-token uniqueness, expiry lookup, and user ownership.
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.Property(refreshToken => refreshToken.Token).IsRequired().HasMaxLength(256);
        builder.Property(refreshToken => refreshToken.JwtId).IsRequired().HasMaxLength(100);
        builder.Property(refreshToken => refreshToken.UserId).IsRequired().HasMaxLength(450);

        builder.HasIndex(refreshToken => refreshToken.Token)
            .IsUnique()
            .HasDatabaseName("UX_RefreshTokens_Token");
        builder.HasIndex(refreshToken => refreshToken.ExpiryDate)
            .HasDatabaseName("IX_RefreshTokens_ExpiryDate");

        builder.HasOne(refreshToken => refreshToken.User)
            .WithMany()
            .HasForeignKey(refreshToken => refreshToken.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
