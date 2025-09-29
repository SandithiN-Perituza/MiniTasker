using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace mt_backend.Migrations
{
    /// <inheritdoc />
    public partial class Adderrorlogs : Migration
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
                    Timestamp = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Message = table.Column<string>(type: "longtext", nullable: false),
                    StackTrace = table.Column<string>(type: "longtext", nullable: false),
                    Source = table.Column<string>(type: "longtext", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErrorLogs", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "DueDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 9, 26, 7, 3, 51, 834, DateTimeKind.Utc).AddTicks(9729), new DateTime(2025, 10, 3, 7, 3, 51, 834, DateTimeKind.Utc).AddTicks(9733), new DateTime(2025, 9, 26, 7, 3, 51, 834, DateTimeKind.Utc).AddTicks(9730) });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "DueDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 9, 26, 7, 3, 51, 834, DateTimeKind.Utc).AddTicks(9746), new DateTime(2025, 10, 3, 7, 3, 51, 834, DateTimeKind.Utc).AddTicks(9748), new DateTime(2025, 9, 26, 7, 3, 51, 834, DateTimeKind.Utc).AddTicks(9747) });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "DueDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 9, 26, 7, 3, 51, 834, DateTimeKind.Utc).AddTicks(9752), new DateTime(2025, 10, 3, 7, 3, 51, 834, DateTimeKind.Utc).AddTicks(9754), new DateTime(2025, 9, 26, 7, 3, 51, 834, DateTimeKind.Utc).AddTicks(9753) });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "DueDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 9, 26, 7, 3, 51, 834, DateTimeKind.Utc).AddTicks(9758), new DateTime(2025, 10, 3, 7, 3, 51, 834, DateTimeKind.Utc).AddTicks(9760), new DateTime(2025, 9, 26, 7, 3, 51, 834, DateTimeKind.Utc).AddTicks(9759) });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "DueDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 9, 26, 7, 3, 51, 834, DateTimeKind.Utc).AddTicks(9764), new DateTime(2025, 10, 3, 7, 3, 51, 834, DateTimeKind.Utc).AddTicks(9765), new DateTime(2025, 9, 26, 7, 3, 51, 834, DateTimeKind.Utc).AddTicks(9765) });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "DueDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 9, 26, 7, 3, 51, 834, DateTimeKind.Utc).AddTicks(9770), new DateTime(2025, 10, 3, 7, 3, 51, 834, DateTimeKind.Utc).AddTicks(9771), new DateTime(2025, 9, 26, 7, 3, 51, 834, DateTimeKind.Utc).AddTicks(9771) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 26, 7, 3, 51, 834, DateTimeKind.Utc).AddTicks(4201));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 26, 7, 3, 51, 834, DateTimeKind.Utc).AddTicks(4205));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 26, 7, 3, 51, 834, DateTimeKind.Utc).AddTicks(4209));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 26, 7, 3, 51, 834, DateTimeKind.Utc).AddTicks(4213));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 26, 7, 3, 51, 834, DateTimeKind.Utc).AddTicks(4216));
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
