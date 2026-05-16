namespace HabitTracker.Api.Data.Configurations;

using HabitTracker.Api.Entities;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RoleConfiguration : IEntityTypeConfiguration<IdentityRole>
{
    private const string MemberRoleId = "4f4f4f86-2db8-4d07-9616-8a3d9f4cc5e0";
    private const string AdminRoleId = "9f88c785-8e57-4da2-b78c-5f87c3629ec8";
    private const string MemberRoleConcurrencyStamp = "ad1a8249-8359-462d-836f-1439454fadba";
    private const string AdminRoleConcurrencyStamp = "0e551eba-9151-4ad4-9b24-921f80d97969";

    public void Configure(EntityTypeBuilder<IdentityRole> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.HasData(
            new IdentityRole
            {
                Id = MemberRoleId,
                Name = nameof(AppRole.Member),
                NormalizedName = nameof(AppRole.Member).ToUpperInvariant(),
                ConcurrencyStamp = MemberRoleConcurrencyStamp,
            },
            new IdentityRole
            {
                Id = AdminRoleId,
                Name = nameof(AppRole.Admin),
                NormalizedName = nameof(AppRole.Admin).ToUpperInvariant(),
                ConcurrencyStamp = AdminRoleConcurrencyStamp,
            }
        );
    }
}