using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1062, S4581 // Suppress code analysis warnings for auto-generated migration code

namespace HabitTracker.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class ImprovedHabitModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsArchived",
                schema: "habittracker",
                table: "habits",
                newName: "is_archived");

            migrationBuilder.RenameColumn(
                name: "EndDate",
                schema: "habittracker",
                table: "habits",
                newName: "end_date");

            migrationBuilder.RenameColumn(
                name: "ArchivedAt",
                schema: "habittracker",
                table: "habits",
                newName: "archived_at");

            migrationBuilder.AlterColumn<int>(
                name: "milestone_target",
                schema: "habittracker",
                table: "habits",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "milestone_current",
                schema: "habittracker",
                table: "habits",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "is_archived",
                schema: "habittracker",
                table: "habits",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                schema: "habittracker",
                table: "habits",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "current_streak",
                schema: "habittracker",
                table: "habits",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "last_streak_broken_at",
                schema: "habittracker",
                table: "habits",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "longest_streak",
                schema: "habittracker",
                table: "habits",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "reminder_time",
                schema: "habittracker",
                table: "habits",
                type: "time without time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "habittracker",
                table: "habits");

            migrationBuilder.DropColumn(
                name: "current_streak",
                schema: "habittracker",
                table: "habits");

            migrationBuilder.DropColumn(
                name: "last_streak_broken_at",
                schema: "habittracker",
                table: "habits");

            migrationBuilder.DropColumn(
                name: "longest_streak",
                schema: "habittracker",
                table: "habits");

            migrationBuilder.DropColumn(
                name: "reminder_time",
                schema: "habittracker",
                table: "habits");

            migrationBuilder.RenameColumn(
                name: "is_archived",
                schema: "habittracker",
                table: "habits",
                newName: "IsArchived");

            migrationBuilder.RenameColumn(
                name: "end_date",
                schema: "habittracker",
                table: "habits",
                newName: "EndDate");

            migrationBuilder.RenameColumn(
                name: "archived_at",
                schema: "habittracker",
                table: "habits",
                newName: "ArchivedAt");

            migrationBuilder.AlterColumn<int>(
                name: "milestone_target",
                schema: "habittracker",
                table: "habits",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "milestone_current",
                schema: "habittracker",
                table: "habits",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<bool>(
                name: "IsArchived",
                schema: "habittracker",
                table: "habits",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);
        }
    }
}

#pragma warning restore CA1062, S4581
