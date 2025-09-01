using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace mt_backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateAndSeeding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(255)", nullable: false),
                    Email = table.Column<string>(type: "varchar(255)", nullable: false),
                    Password = table.Column<string>(type: "varchar(255)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Tasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Title = table.Column<string>(type: "varchar(255)", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    AssignedTo = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tasks_Users_AssignedTo",
                        column: x => x.AssignedTo,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false)
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
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Subtasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Title = table.Column<string>(type: "varchar(255)", nullable: false),
                    IsCompleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
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
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "Name", "Password" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 8, 29, 11, 55, 38, 427, DateTimeKind.Utc).AddTicks(9598), "alice@example.com", "Alice", "hashed-password-1" },
                    { 2, new DateTime(2025, 8, 29, 11, 55, 38, 427, DateTimeKind.Utc).AddTicks(9603), "bob@example.com", "Bob", "hashed-password-1" },
                    { 3, new DateTime(2025, 8, 29, 11, 55, 38, 427, DateTimeKind.Utc).AddTicks(9606), "anne@example.com", "Anne", "hashed-password-1" },
                    { 4, new DateTime(2025, 8, 29, 11, 55, 38, 427, DateTimeKind.Utc).AddTicks(9609), "peter@example.com", "Peter", "hashed-password-1" },
                    { 5, new DateTime(2025, 8, 29, 11, 55, 38, 427, DateTimeKind.Utc).AddTicks(9612), "jenny@example.com", "Jenny", "hashed-password-1" }
                });

            migrationBuilder.InsertData(
                table: "Tasks",
                columns: new[] { "Id", "AssignedTo", "CreatedAt", "Description", "DueDate", "Status", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2025, 8, 29, 11, 55, 38, 428, DateTimeKind.Utc).AddTicks(4555), "First task", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0, "Sample Task", new DateTime(2025, 8, 29, 11, 55, 38, 428, DateTimeKind.Utc).AddTicks(4556) },
                    { 2, 2, new DateTime(2025, 8, 29, 11, 55, 38, 428, DateTimeKind.Utc).AddTicks(4561), "Second task", new DateTime(2025, 9, 5, 11, 55, 38, 428, DateTimeKind.Utc).AddTicks(4562), 1, "Sample Task 2", new DateTime(2025, 8, 29, 11, 55, 38, 428, DateTimeKind.Utc).AddTicks(4562) },
                    { 3, 3, new DateTime(2025, 8, 29, 11, 55, 38, 428, DateTimeKind.Utc).AddTicks(4574), "Third task", new DateTime(2025, 9, 5, 11, 55, 38, 428, DateTimeKind.Utc).AddTicks(4576), 2, "Sample Task 3", new DateTime(2025, 8, 29, 11, 55, 38, 428, DateTimeKind.Utc).AddTicks(4575) },
                    { 4, 4, new DateTime(2025, 8, 29, 11, 55, 38, 428, DateTimeKind.Utc).AddTicks(4580), "Fourth task", new DateTime(2025, 9, 5, 11, 55, 38, 428, DateTimeKind.Utc).AddTicks(4582), 1, "Sample Task 4", new DateTime(2025, 8, 29, 11, 55, 38, 428, DateTimeKind.Utc).AddTicks(4581) },
                    { 5, 5, new DateTime(2025, 8, 29, 11, 55, 38, 428, DateTimeKind.Utc).AddTicks(4586), "Fifth task", new DateTime(2025, 9, 5, 11, 55, 38, 428, DateTimeKind.Utc).AddTicks(4587), 0, "Sample Task 5", new DateTime(2025, 8, 29, 11, 55, 38, 428, DateTimeKind.Utc).AddTicks(4587) },
                    { 6, 1, new DateTime(2025, 8, 29, 11, 55, 38, 428, DateTimeKind.Utc).AddTicks(4591), "sixth task", new DateTime(2025, 9, 5, 11, 55, 38, 428, DateTimeKind.Utc).AddTicks(4593), 1, "Sample Task 6", new DateTime(2025, 8, 29, 11, 55, 38, 428, DateTimeKind.Utc).AddTicks(4592) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comments_TaskId",
                table: "Comments",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_UserId",
                table: "Comments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Subtasks_TaskId",
                table: "Subtasks",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_AssignedTo",
                table: "Tasks",
                column: "AssignedTo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "Subtasks");

            migrationBuilder.DropTable(
                name: "Tasks");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
