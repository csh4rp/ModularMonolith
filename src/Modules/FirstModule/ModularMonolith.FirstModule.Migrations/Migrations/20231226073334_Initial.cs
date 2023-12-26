using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModularMonolith.FirstModule.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "first_module");

            migrationBuilder.CreateTable(
                name: "category",
                schema: "first_module",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_id = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_category", x => x.id);
                    table.ForeignKey(
                        name: "fk_category_category_category_id",
                        column: x => x.parent_id,
                        principalSchema: "first_module",
                        principalTable: "category",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_category_name",
                schema: "first_module",
                table: "category",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_category_parent_id",
                schema: "first_module",
                table: "category",
                column: "parent_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "category",
                schema: "first_module");
        }
    }
}
