using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace mt_backend.Migrations
{
    /// <inheritdoc />
    public partial class SeedErrorLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ErrorLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Timestamp = table.Column<DateTime>(type: "datetime", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    StackTrace = table.Column<string>(type: "text", nullable: false),
                    Source = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErrorLogs", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.InsertData(
                table: "ErrorLogs",
                columns: new[] { "Id", "Message", "Source", "StackTrace", "Timestamp" },
                values: new object[,]
                {
                    { 1, "Initial error log entry", "System", "Stack trace goes here", new DateTime(2025, 10, 15, 9, 45, 49, 857, DateTimeKind.Utc).AddTicks(746) },
                    { 2, "Another error log entry", "Application", "Another stack trace", new DateTime(2025, 10, 15, 9, 45, 49, 857, DateTimeKind.Utc).AddTicks(748) }
                });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "DueDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 9, 45, 49, 857, DateTimeKind.Utc).AddTicks(2577), new DateTime(2025, 10, 22, 9, 45, 49, 857, DateTimeKind.Utc).AddTicks(2577), new DateTime(2025, 10, 15, 9, 45, 49, 857, DateTimeKind.Utc).AddTicks(2577) });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "DueDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 9, 45, 49, 857, DateTimeKind.Utc).AddTicks(2585), new DateTime(2025, 10, 22, 9, 45, 49, 857, DateTimeKind.Utc).AddTicks(2585), new DateTime(2025, 10, 15, 9, 45, 49, 857, DateTimeKind.Utc).AddTicks(2585) });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "DueDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 9, 45, 49, 857, DateTimeKind.Utc).AddTicks(2587), new DateTime(2025, 10, 22, 9, 45, 49, 857, DateTimeKind.Utc).AddTicks(2588), new DateTime(2025, 10, 15, 9, 45, 49, 857, DateTimeKind.Utc).AddTicks(2587) });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "DueDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 9, 45, 49, 857, DateTimeKind.Utc).AddTicks(2590), new DateTime(2025, 10, 22, 9, 45, 49, 857, DateTimeKind.Utc).AddTicks(2590), new DateTime(2025, 10, 15, 9, 45, 49, 857, DateTimeKind.Utc).AddTicks(2590) });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "DueDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 9, 45, 49, 857, DateTimeKind.Utc).AddTicks(2592), new DateTime(2025, 10, 22, 9, 45, 49, 857, DateTimeKind.Utc).AddTicks(2593), new DateTime(2025, 10, 15, 9, 45, 49, 857, DateTimeKind.Utc).AddTicks(2592) });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "DueDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 10, 15, 9, 45, 49, 857, DateTimeKind.Utc).AddTicks(2595), new DateTime(2025, 10, 22, 9, 45, 49, 857, DateTimeKind.Utc).AddTicks(2595), new DateTime(2025, 10, 15, 9, 45, 49, 857, DateTimeKind.Utc).AddTicks(2595) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 9, 45, 49, 857, DateTimeKind.Utc).AddTicks(619));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 9, 45, 49, 857, DateTimeKind.Utc).AddTicks(620));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 9, 45, 49, 857, DateTimeKind.Utc).AddTicks(622));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 9, 45, 49, 857, DateTimeKind.Utc).AddTicks(623));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 15, 9, 45, 49, 857, DateTimeKind.Utc).AddTicks(624));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ErrorLogs");

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "DueDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 9, 4, 4, 45, 1, 212, DateTimeKind.Utc).AddTicks(3674), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 9, 4, 4, 45, 1, 212, DateTimeKind.Utc).AddTicks(3675) });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "DueDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 9, 4, 4, 45, 1, 212, DateTimeKind.Utc).AddTicks(3679), new DateTime(2025, 9, 11, 4, 45, 1, 212, DateTimeKind.Utc).AddTicks(3680), new DateTime(2025, 9, 4, 4, 45, 1, 212, DateTimeKind.Utc).AddTicks(3680) });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "DueDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 9, 4, 4, 45, 1, 212, DateTimeKind.Utc).AddTicks(3689), new DateTime(2025, 9, 11, 4, 45, 1, 212, DateTimeKind.Utc).AddTicks(3691), new DateTime(2025, 9, 4, 4, 45, 1, 212, DateTimeKind.Utc).AddTicks(3690) });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "DueDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 9, 4, 4, 45, 1, 212, DateTimeKind.Utc).AddTicks(3693), new DateTime(2025, 9, 11, 4, 45, 1, 212, DateTimeKind.Utc).AddTicks(3694), new DateTime(2025, 9, 4, 4, 45, 1, 212, DateTimeKind.Utc).AddTicks(3694) });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "DueDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 9, 4, 4, 45, 1, 212, DateTimeKind.Utc).AddTicks(3697), new DateTime(2025, 9, 11, 4, 45, 1, 212, DateTimeKind.Utc).AddTicks(3698), new DateTime(2025, 9, 4, 4, 45, 1, 212, DateTimeKind.Utc).AddTicks(3697) });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "DueDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 9, 4, 4, 45, 1, 212, DateTimeKind.Utc).AddTicks(3700), new DateTime(2025, 9, 11, 4, 45, 1, 212, DateTimeKind.Utc).AddTicks(3701), new DateTime(2025, 9, 4, 4, 45, 1, 212, DateTimeKind.Utc).AddTicks(3701) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 4, 4, 45, 1, 212, DateTimeKind.Utc).AddTicks(1108));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 4, 4, 45, 1, 212, DateTimeKind.Utc).AddTicks(1111));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 4, 4, 45, 1, 212, DateTimeKind.Utc).AddTicks(1113));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 4, 4, 45, 1, 212, DateTimeKind.Utc).AddTicks(1115));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 4, 4, 45, 1, 212, DateTimeKind.Utc).AddTicks(1117));
        }
    }
}
