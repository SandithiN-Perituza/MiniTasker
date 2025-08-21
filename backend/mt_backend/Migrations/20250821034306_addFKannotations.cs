using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mt_backend.Migrations
{
    /// <inheritdoc />
    public partial class addFKannotations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 21, 3, 43, 6, 175, DateTimeKind.Utc).AddTicks(4041), new DateTime(2025, 8, 21, 3, 43, 6, 175, DateTimeKind.Utc).AddTicks(4042) });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "DueDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 21, 3, 43, 6, 175, DateTimeKind.Utc).AddTicks(4086), new DateTime(2025, 8, 28, 3, 43, 6, 175, DateTimeKind.Utc).AddTicks(4087), new DateTime(2025, 8, 21, 3, 43, 6, 175, DateTimeKind.Utc).AddTicks(4087) });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "DueDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 21, 3, 43, 6, 175, DateTimeKind.Utc).AddTicks(4095), new DateTime(2025, 8, 28, 3, 43, 6, 175, DateTimeKind.Utc).AddTicks(4096), new DateTime(2025, 8, 21, 3, 43, 6, 175, DateTimeKind.Utc).AddTicks(4095) });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "DueDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 21, 3, 43, 6, 175, DateTimeKind.Utc).AddTicks(4098), new DateTime(2025, 8, 28, 3, 43, 6, 175, DateTimeKind.Utc).AddTicks(4098), new DateTime(2025, 8, 21, 3, 43, 6, 175, DateTimeKind.Utc).AddTicks(4098) });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "DueDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 21, 3, 43, 6, 175, DateTimeKind.Utc).AddTicks(4100), new DateTime(2025, 8, 28, 3, 43, 6, 175, DateTimeKind.Utc).AddTicks(4101), new DateTime(2025, 8, 21, 3, 43, 6, 175, DateTimeKind.Utc).AddTicks(4100) });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "DueDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 21, 3, 43, 6, 175, DateTimeKind.Utc).AddTicks(4103), new DateTime(2025, 8, 28, 3, 43, 6, 175, DateTimeKind.Utc).AddTicks(4103), new DateTime(2025, 8, 21, 3, 43, 6, 175, DateTimeKind.Utc).AddTicks(4103) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 21, 3, 43, 6, 175, DateTimeKind.Utc).AddTicks(2229));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 21, 3, 43, 6, 175, DateTimeKind.Utc).AddTicks(2232));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 21, 3, 43, 6, 175, DateTimeKind.Utc).AddTicks(2233));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 21, 3, 43, 6, 175, DateTimeKind.Utc).AddTicks(2235));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 21, 3, 43, 6, 175, DateTimeKind.Utc).AddTicks(2236));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 20, 9, 52, 11, 638, DateTimeKind.Utc).AddTicks(6582), new DateTime(2025, 8, 20, 9, 52, 11, 638, DateTimeKind.Utc).AddTicks(6582) });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "DueDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 20, 9, 52, 11, 638, DateTimeKind.Utc).AddTicks(6584), new DateTime(2025, 8, 27, 9, 52, 11, 638, DateTimeKind.Utc).AddTicks(6585), new DateTime(2025, 8, 20, 9, 52, 11, 638, DateTimeKind.Utc).AddTicks(6585) });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "DueDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 20, 9, 52, 11, 638, DateTimeKind.Utc).AddTicks(6592), new DateTime(2025, 8, 27, 9, 52, 11, 638, DateTimeKind.Utc).AddTicks(6593), new DateTime(2025, 8, 20, 9, 52, 11, 638, DateTimeKind.Utc).AddTicks(6592) });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "DueDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 20, 9, 52, 11, 638, DateTimeKind.Utc).AddTicks(6594), new DateTime(2025, 8, 27, 9, 52, 11, 638, DateTimeKind.Utc).AddTicks(6595), new DateTime(2025, 8, 20, 9, 52, 11, 638, DateTimeKind.Utc).AddTicks(6595) });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "DueDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 20, 9, 52, 11, 638, DateTimeKind.Utc).AddTicks(6597), new DateTime(2025, 8, 27, 9, 52, 11, 638, DateTimeKind.Utc).AddTicks(6597), new DateTime(2025, 8, 20, 9, 52, 11, 638, DateTimeKind.Utc).AddTicks(6597) });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "DueDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 20, 9, 52, 11, 638, DateTimeKind.Utc).AddTicks(6599), new DateTime(2025, 8, 27, 9, 52, 11, 638, DateTimeKind.Utc).AddTicks(6600), new DateTime(2025, 8, 20, 9, 52, 11, 638, DateTimeKind.Utc).AddTicks(6600) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 20, 9, 52, 11, 638, DateTimeKind.Utc).AddTicks(4436));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 20, 9, 52, 11, 638, DateTimeKind.Utc).AddTicks(4438));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 20, 9, 52, 11, 638, DateTimeKind.Utc).AddTicks(4439));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 20, 9, 52, 11, 638, DateTimeKind.Utc).AddTicks(4441));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 20, 9, 52, 11, 638, DateTimeKind.Utc).AddTicks(4442));
        }
    }
}
