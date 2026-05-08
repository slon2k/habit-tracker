using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HabitTracker.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTagsAndHabitTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            ArgumentNullException.ThrowIfNull(migrationBuilder);

            migrationBuilder.CreateTable(
                name: "tags",
                schema: "habittracker",
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
                });

            migrationBuilder.CreateTable(
                name: "habit_tags",
                schema: "habittracker",
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
                        principalSchema: "habittracker",
                        principalTable: "habits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_habit_tags_tags_TagId",
                        column: x => x.TagId,
                        principalSchema: "habittracker",
                        principalTable: "tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_habit_tags_TagId",
                schema: "habittracker",
                table: "habit_tags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_tags_UserId_Name",
                schema: "habittracker",
                table: "tags",
#pragma warning disable CA1861
                columns: new[] { "UserId", "Name" },
#pragma warning restore CA1861
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            ArgumentNullException.ThrowIfNull(migrationBuilder);
            migrationBuilder.DropTable(
                name: "habit_tags",
                schema: "habittracker");

            migrationBuilder.DropTable(
                name: "tags",
                schema: "habittracker");
        }
    }
}
