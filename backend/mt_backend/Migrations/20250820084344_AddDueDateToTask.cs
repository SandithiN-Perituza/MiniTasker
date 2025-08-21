using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mt_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddDueDateToTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "Tasks",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "DueDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 20, 8, 43, 43, 634, DateTimeKind.Utc).AddTicks(8548), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 8, 20, 8, 43, 43, 634, DateTimeKind.Utc).AddTicks(8549) });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "DueDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 20, 8, 43, 43, 634, DateTimeKind.Utc).AddTicks(8551), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 8, 20, 8, 43, 43, 634, DateTimeKind.Utc).AddTicks(8551) });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "DueDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 20, 8, 43, 43, 634, DateTimeKind.Utc).AddTicks(8553), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 8, 20, 8, 43, 43, 634, DateTimeKind.Utc).AddTicks(8553) });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "DueDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 20, 8, 43, 43, 634, DateTimeKind.Utc).AddTicks(8554), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 8, 20, 8, 43, 43, 634, DateTimeKind.Utc).AddTicks(8555) });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "DueDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 20, 8, 43, 43, 634, DateTimeKind.Utc).AddTicks(8556), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 8, 20, 8, 43, 43, 634, DateTimeKind.Utc).AddTicks(8556) });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "DueDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 20, 8, 43, 43, 634, DateTimeKind.Utc).AddTicks(8558), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 8, 20, 8, 43, 43, 634, DateTimeKind.Utc).AddTicks(8558) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 20, 8, 43, 43, 634, DateTimeKind.Utc).AddTicks(8457));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 20, 8, 43, 43, 634, DateTimeKind.Utc).AddTicks(8459));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 20, 8, 43, 43, 634, DateTimeKind.Utc).AddTicks(8460));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 20, 8, 43, 43, 634, DateTimeKind.Utc).AddTicks(8461));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 20, 8, 43, 43, 634, DateTimeKind.Utc).AddTicks(8463));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "Tasks");

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 31, 3, 51, 22, 362, DateTimeKind.Utc).AddTicks(5220), new DateTime(2025, 7, 31, 3, 51, 22, 362, DateTimeKind.Utc).AddTicks(5221) });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 31, 3, 51, 22, 362, DateTimeKind.Utc).AddTicks(5223), new DateTime(2025, 7, 31, 3, 51, 22, 362, DateTimeKind.Utc).AddTicks(5223) });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 31, 3, 51, 22, 362, DateTimeKind.Utc).AddTicks(5225), new DateTime(2025, 7, 31, 3, 51, 22, 362, DateTimeKind.Utc).AddTicks(5226) });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 31, 3, 51, 22, 362, DateTimeKind.Utc).AddTicks(5227), new DateTime(2025, 7, 31, 3, 51, 22, 362, DateTimeKind.Utc).AddTicks(5228) });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 31, 3, 51, 22, 362, DateTimeKind.Utc).AddTicks(5229), new DateTime(2025, 7, 31, 3, 51, 22, 362, DateTimeKind.Utc).AddTicks(5230) });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 31, 3, 51, 22, 362, DateTimeKind.Utc).AddTicks(5231), new DateTime(2025, 7, 31, 3, 51, 22, 362, DateTimeKind.Utc).AddTicks(5232) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 7, 31, 3, 51, 22, 362, DateTimeKind.Utc).AddTicks(5114));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 7, 31, 3, 51, 22, 362, DateTimeKind.Utc).AddTicks(5116));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 7, 31, 3, 51, 22, 362, DateTimeKind.Utc).AddTicks(5117));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 7, 31, 3, 51, 22, 362, DateTimeKind.Utc).AddTicks(5119));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 7, 31, 3, 51, 22, 362, DateTimeKind.Utc).AddTicks(5120));
        }
    }
}
