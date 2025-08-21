using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mt_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddSubtasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Subtasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    TaskId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subtasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subtasks_Tasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_Subtasks_TaskId",
                table: "Subtasks",
                column: "TaskId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Subtasks");

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
        }
    }
}
