using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mt_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordToUserModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

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
                columns: new[] { "CreatedAt", "Password" },
                values: new object[] { new DateTime(2025, 7, 31, 3, 51, 22, 362, DateTimeKind.Utc).AddTicks(5114), "hashed-password-1" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "Password" },
                values: new object[] { new DateTime(2025, 7, 31, 3, 51, 22, 362, DateTimeKind.Utc).AddTicks(5116), "hashed-password-1" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "Password" },
                values: new object[] { new DateTime(2025, 7, 31, 3, 51, 22, 362, DateTimeKind.Utc).AddTicks(5117), "hashed-password-1" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "Password" },
                values: new object[] { new DateTime(2025, 7, 31, 3, 51, 22, 362, DateTimeKind.Utc).AddTicks(5119), "hashed-password-1" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "Password" },
                values: new object[] { new DateTime(2025, 7, 31, 3, 51, 22, 362, DateTimeKind.Utc).AddTicks(5120), "hashed-password-1" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Password",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 30, 11, 0, 34, 507, DateTimeKind.Utc).AddTicks(5954), new DateTime(2025, 7, 30, 11, 0, 34, 507, DateTimeKind.Utc).AddTicks(5954) });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 30, 11, 0, 34, 507, DateTimeKind.Utc).AddTicks(5957), new DateTime(2025, 7, 30, 11, 0, 34, 507, DateTimeKind.Utc).AddTicks(5957) });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 30, 11, 0, 34, 507, DateTimeKind.Utc).AddTicks(5960), new DateTime(2025, 7, 30, 11, 0, 34, 507, DateTimeKind.Utc).AddTicks(5960) });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 30, 11, 0, 34, 507, DateTimeKind.Utc).AddTicks(5962), new DateTime(2025, 7, 30, 11, 0, 34, 507, DateTimeKind.Utc).AddTicks(5962) });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 30, 11, 0, 34, 507, DateTimeKind.Utc).AddTicks(5964), new DateTime(2025, 7, 30, 11, 0, 34, 507, DateTimeKind.Utc).AddTicks(5964) });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 30, 11, 0, 34, 507, DateTimeKind.Utc).AddTicks(5966), new DateTime(2025, 7, 30, 11, 0, 34, 507, DateTimeKind.Utc).AddTicks(5966) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 7, 30, 11, 0, 34, 507, DateTimeKind.Utc).AddTicks(5849));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 7, 30, 11, 0, 34, 507, DateTimeKind.Utc).AddTicks(5851));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 7, 30, 11, 0, 34, 507, DateTimeKind.Utc).AddTicks(5853));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 7, 30, 11, 0, 34, 507, DateTimeKind.Utc).AddTicks(5854));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 7, 30, 11, 0, 34, 507, DateTimeKind.Utc).AddTicks(5855));
        }
    }
}
