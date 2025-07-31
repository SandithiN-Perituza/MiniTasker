using System;
using Microsoft.EntityFrameworkCore.Migrations;

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
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AssignedTo = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
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
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "Name" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 7, 30, 11, 0, 34, 507, DateTimeKind.Utc).AddTicks(5849), "alice@example.com", "Alice" },
                    { 2, new DateTime(2025, 7, 30, 11, 0, 34, 507, DateTimeKind.Utc).AddTicks(5851), "bob@example.com", "Bob" },
                    { 3, new DateTime(2025, 7, 30, 11, 0, 34, 507, DateTimeKind.Utc).AddTicks(5853), "anne@example.com", "Anne" },
                    { 4, new DateTime(2025, 7, 30, 11, 0, 34, 507, DateTimeKind.Utc).AddTicks(5854), "peter@example.com", "Peter" },
                    { 5, new DateTime(2025, 7, 30, 11, 0, 34, 507, DateTimeKind.Utc).AddTicks(5855), "jenny@example.com", "Jenny" }
                });

            migrationBuilder.InsertData(
                table: "Tasks",
                columns: new[] { "Id", "AssignedTo", "CreatedAt", "Description", "Status", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2025, 7, 30, 11, 0, 34, 507, DateTimeKind.Utc).AddTicks(5954), "First task", 0, "Sample Task", new DateTime(2025, 7, 30, 11, 0, 34, 507, DateTimeKind.Utc).AddTicks(5954) },
                    { 2, 2, new DateTime(2025, 7, 30, 11, 0, 34, 507, DateTimeKind.Utc).AddTicks(5957), "Second task", 1, "Sample Task 2", new DateTime(2025, 7, 30, 11, 0, 34, 507, DateTimeKind.Utc).AddTicks(5957) },
                    { 3, 3, new DateTime(2025, 7, 30, 11, 0, 34, 507, DateTimeKind.Utc).AddTicks(5960), "Third task", 2, "Sample Task 3", new DateTime(2025, 7, 30, 11, 0, 34, 507, DateTimeKind.Utc).AddTicks(5960) },
                    { 4, 4, new DateTime(2025, 7, 30, 11, 0, 34, 507, DateTimeKind.Utc).AddTicks(5962), "Fourth task", 1, "Sample Task 4", new DateTime(2025, 7, 30, 11, 0, 34, 507, DateTimeKind.Utc).AddTicks(5962) },
                    { 5, 5, new DateTime(2025, 7, 30, 11, 0, 34, 507, DateTimeKind.Utc).AddTicks(5964), "Fifth task", 0, "Sample Task 5", new DateTime(2025, 7, 30, 11, 0, 34, 507, DateTimeKind.Utc).AddTicks(5964) },
                    { 6, 1, new DateTime(2025, 7, 30, 11, 0, 34, 507, DateTimeKind.Utc).AddTicks(5966), "sixth task", 1, "Sample Task 6", new DateTime(2025, 7, 30, 11, 0, 34, 507, DateTimeKind.Utc).AddTicks(5966) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_AssignedTo",
                table: "Tasks",
                column: "AssignedTo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tasks");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
