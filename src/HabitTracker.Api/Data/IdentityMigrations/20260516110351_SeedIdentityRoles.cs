using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HabitTracker.Api.Data.IdentityMigrations
{
    /// <inheritdoc />
    public partial class SeedIdentityRoles : Migration
    {
        private static readonly string[] AspNetRoleColumns = ["Id", "ConcurrencyStamp", "Name", "NormalizedName"];
        private static readonly string[] TagIndexColumns = ["UserId", "Name"];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            ArgumentNullException.ThrowIfNull(migrationBuilder);

            migrationBuilder.CreateTable(
                name: "users",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    identity_id = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "habits",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    frequency_type = table.Column<int>(type: "integer", nullable: false),
                    frequency_times_per_period = table.Column<int>(type: "integer", nullable: false),
                    target_value = table.Column<int>(type: "integer", nullable: true),
                    target_unit = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    milestone_target = table.Column<int>(type: "integer", nullable: false),
                    milestone_current = table.Column<int>(type: "integer", nullable: false),
                    reminder_time = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    current_streak = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    longest_streak = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    last_streak_broken_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    archived_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastCompletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_habits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_habits_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tags",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tags_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "habit_tags",
                schema: "identity",
                columns: table => new
                {
                    HabitId = table.Column<Guid>(type: "uuid", nullable: false),
                    TagId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_habit_tags", x => new { x.HabitId, x.TagId });
                    table.ForeignKey(
                        name: "FK_habit_tags_habits_HabitId",
                        column: x => x.HabitId,
                        principalSchema: "identity",
                        principalTable: "habits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_habit_tags_tags_TagId",
                        column: x => x.TagId,
                        principalSchema: "identity",
                        principalTable: "tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "identity",
                table: "asp_net_roles",
                columns: AspNetRoleColumns,
                values: new object[,]
                {
                    { "4f4f4f86-2db8-4d07-9616-8a3d9f4cc5e0", "ad1a8249-8359-462d-836f-1439454fadba", "Member", "MEMBER" },
                    { "9f88c785-8e57-4da2-b78c-5f87c3629ec8", "0e551eba-9151-4ad4-9b24-921f80d97969", "Admin", "ADMIN" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_habit_tags_TagId",
                schema: "identity",
                table: "habit_tags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_habits_UserId",
                schema: "identity",
                table: "habits",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_tags_UserId_Name",
                schema: "identity",
                table: "tags",
                columns: TagIndexColumns,
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                schema: "identity",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_identity_id",
                schema: "identity",
                table: "users",
                column: "identity_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            ArgumentNullException.ThrowIfNull(migrationBuilder);

            migrationBuilder.DropTable(
                name: "habit_tags",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "habits",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "tags",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "users",
                schema: "identity");

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "asp_net_roles",
                keyColumn: "Id",
                keyValue: "4f4f4f86-2db8-4d07-9616-8a3d9f4cc5e0");

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "asp_net_roles",
                keyColumn: "Id",
                keyValue: "9f88c785-8e57-4da2-b78c-5f87c3629ec8");
        }
    }
}
