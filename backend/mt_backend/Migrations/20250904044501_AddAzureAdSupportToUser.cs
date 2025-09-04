using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mt_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddAzureAdSupportToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AzureAdId",
                table: "Users",
                type: "varchar(255)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 9, 4, 4, 45, 1, 212, DateTimeKind.Utc).AddTicks(3674), new DateTime(2025, 9, 4, 4, 45, 1, 212, DateTimeKind.Utc).AddTicks(3675) });

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
                columns: new[] { "AzureAdId", "CreatedAt" },
                values: new object[] { null, new DateTime(2025, 9, 4, 4, 45, 1, 212, DateTimeKind.Utc).AddTicks(1108) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "AzureAdId", "CreatedAt" },
                values: new object[] { null, new DateTime(2025, 9, 4, 4, 45, 1, 212, DateTimeKind.Utc).AddTicks(1111) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "AzureAdId", "CreatedAt" },
                values: new object[] { null, new DateTime(2025, 9, 4, 4, 45, 1, 212, DateTimeKind.Utc).AddTicks(1113) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "AzureAdId", "CreatedAt" },
                values: new object[] { null, new DateTime(2025, 9, 4, 4, 45, 1, 212, DateTimeKind.Utc).AddTicks(1115) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "AzureAdId", "CreatedAt" },
                values: new object[] { null, new DateTime(2025, 9, 4, 4, 45, 1, 212, DateTimeKind.Utc).AddTicks(1117) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AzureAdId",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 29, 11, 55, 38, 428, DateTimeKind.Utc).AddTicks(4555), new DateTime(2025, 8, 29, 11, 55, 38, 428, DateTimeKind.Utc).AddTicks(4556) });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "DueDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 29, 11, 55, 38, 428, DateTimeKind.Utc).AddTicks(4561), new DateTime(2025, 9, 5, 11, 55, 38, 428, DateTimeKind.Utc).AddTicks(4562), new DateTime(2025, 8, 29, 11, 55, 38, 428, DateTimeKind.Utc).AddTicks(4562) });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "DueDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 29, 11, 55, 38, 428, DateTimeKind.Utc).AddTicks(4574), new DateTime(2025, 9, 5, 11, 55, 38, 428, DateTimeKind.Utc).AddTicks(4576), new DateTime(2025, 8, 29, 11, 55, 38, 428, DateTimeKind.Utc).AddTicks(4575) });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "DueDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 29, 11, 55, 38, 428, DateTimeKind.Utc).AddTicks(4580), new DateTime(2025, 9, 5, 11, 55, 38, 428, DateTimeKind.Utc).AddTicks(4582), new DateTime(2025, 8, 29, 11, 55, 38, 428, DateTimeKind.Utc).AddTicks(4581) });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "DueDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 29, 11, 55, 38, 428, DateTimeKind.Utc).AddTicks(4586), new DateTime(2025, 9, 5, 11, 55, 38, 428, DateTimeKind.Utc).AddTicks(4587), new DateTime(2025, 8, 29, 11, 55, 38, 428, DateTimeKind.Utc).AddTicks(4587) });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "DueDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 29, 11, 55, 38, 428, DateTimeKind.Utc).AddTicks(4591), new DateTime(2025, 9, 5, 11, 55, 38, 428, DateTimeKind.Utc).AddTicks(4593), new DateTime(2025, 8, 29, 11, 55, 38, 428, DateTimeKind.Utc).AddTicks(4592) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 29, 11, 55, 38, 427, DateTimeKind.Utc).AddTicks(9598));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 29, 11, 55, 38, 427, DateTimeKind.Utc).AddTicks(9603));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 29, 11, 55, 38, 427, DateTimeKind.Utc).AddTicks(9606));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 29, 11, 55, 38, 427, DateTimeKind.Utc).AddTicks(9609));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 29, 11, 55, 38, 427, DateTimeKind.Utc).AddTicks(9612));
        }
    }
}
