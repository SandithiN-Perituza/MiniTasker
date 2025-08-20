using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mt_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Tasks",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comments_Tasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Comments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 20, 9, 40, 34, 906, DateTimeKind.Utc).AddTicks(7919), new DateTime(2025, 8, 20, 9, 40, 34, 906, DateTimeKind.Utc).AddTicks(7919) });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "DueDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 20, 9, 40, 34, 906, DateTimeKind.Utc).AddTicks(7921), new DateTime(2025, 8, 27, 9, 40, 34, 906, DateTimeKind.Utc).AddTicks(7922), new DateTime(2025, 8, 20, 9, 40, 34, 906, DateTimeKind.Utc).AddTicks(7922) });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "DueDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 20, 9, 40, 34, 906, DateTimeKind.Utc).AddTicks(7929), new DateTime(2025, 8, 27, 9, 40, 34, 906, DateTimeKind.Utc).AddTicks(7930), new DateTime(2025, 8, 20, 9, 40, 34, 906, DateTimeKind.Utc).AddTicks(7930) });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "DueDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 20, 9, 40, 34, 906, DateTimeKind.Utc).AddTicks(7932), new DateTime(2025, 8, 27, 9, 40, 34, 906, DateTimeKind.Utc).AddTicks(7933), new DateTime(2025, 8, 20, 9, 40, 34, 906, DateTimeKind.Utc).AddTicks(7932) });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "DueDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 20, 9, 40, 34, 906, DateTimeKind.Utc).AddTicks(7954), new DateTime(2025, 8, 27, 9, 40, 34, 906, DateTimeKind.Utc).AddTicks(7955), new DateTime(2025, 8, 20, 9, 40, 34, 906, DateTimeKind.Utc).AddTicks(7955) });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "DueDate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 20, 9, 40, 34, 906, DateTimeKind.Utc).AddTicks(7957), new DateTime(2025, 8, 27, 9, 40, 34, 906, DateTimeKind.Utc).AddTicks(7958), new DateTime(2025, 8, 20, 9, 40, 34, 906, DateTimeKind.Utc).AddTicks(7957) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 20, 9, 40, 34, 906, DateTimeKind.Utc).AddTicks(6166));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 20, 9, 40, 34, 906, DateTimeKind.Utc).AddTicks(6168));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 20, 9, 40, 34, 906, DateTimeKind.Utc).AddTicks(6169));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 20, 9, 40, 34, 906, DateTimeKind.Utc).AddTicks(6215));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 20, 9, 40, 34, 906, DateTimeKind.Utc).AddTicks(6221));

            migrationBuilder.CreateIndex(
                name: "IX_Comments_TaskId",
                table: "Comments",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_UserId",
                table: "Comments",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Tasks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 20, 8, 43, 43, 634, DateTimeKind.Utc).AddTicks(8548), new DateTime(2025, 8, 20, 8, 43, 43, 634, DateTimeKind.Utc).AddTicks(8549) });

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
    }
}
