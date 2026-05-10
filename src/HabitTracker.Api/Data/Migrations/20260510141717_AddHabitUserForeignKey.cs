using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HabitTracker.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddHabitUserForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            ArgumentNullException.ThrowIfNull(migrationBuilder);

            migrationBuilder.CreateIndex(
                name: "IX_habits_UserId",
                schema: "habittracker",
                table: "habits",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_habits_users_UserId",
                schema: "habittracker",
                table: "habits",
                column: "UserId",
                principalSchema: "habittracker",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            ArgumentNullException.ThrowIfNull(migrationBuilder);

            migrationBuilder.DropForeignKey(
                name: "FK_habits_users_UserId",
                schema: "habittracker",
                table: "habits");

            migrationBuilder.DropIndex(
                name: "IX_habits_UserId",
                schema: "habittracker",
                table: "habits");
        }
    }
}
