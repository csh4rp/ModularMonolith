using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModularMonolith.Infrastructure.Migrations.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class InitialCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "category_management");

            migrationBuilder.CreateTable(
                name: "category",
                schema: "category_management",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_category", x => x.Id);
                    table.ForeignKey(
                        name: "FK_category_category_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "category_management",
                        principalTable: "category",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_category_Name",
                schema: "category_management",
                table: "category",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_category_ParentId",
                schema: "category_management",
                table: "category",
                column: "ParentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "category",
                schema: "category_management");
        }
    }
}
