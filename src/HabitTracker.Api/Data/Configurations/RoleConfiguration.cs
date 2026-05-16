namespace HabitTracker.Api.Data.Configurations;

using HabitTracker.Api.Entities;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RoleConfiguration : IEntityTypeConfiguration<IdentityRole>
{
    private const string MemberRoleId = "4f4f4f86-2db8-4d07-9616-8a3d9f4cc5e0";
    private const string AdminRoleId = "9f88c785-8e57-4da2-b78c-5f87c3629ec8";

    public void Configure(EntityTypeBuilder<IdentityRole> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.HasData(
            new IdentityRole { Id = MemberRoleId, Name = Roles.Member, NormalizedName = Roles.Member.ToUpperInvariant() },
            new IdentityRole { Id = AdminRoleId, Name = Roles.Admin, NormalizedName = Roles.Admin.ToUpperInvariant() }
        );
    }
}