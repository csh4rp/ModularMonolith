using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModularMonolith.Infrastructure.Migrations.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class Test : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_category_category_category_id",
                schema: "category_management",
                table: "category");

            migrationBuilder.AddColumn<int>(
                name: "version",
                schema: "category_management",
                table: "category",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "fk_category_category_parent_id",
                schema: "category_management",
                table: "category",
                column: "parent_id",
                principalSchema: "category_management",
                principalTable: "category",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_category_category_parent_id",
                schema: "category_management",
                table: "category");

            migrationBuilder.DropColumn(
                name: "version",
                schema: "category_management",
                table: "category");

            migrationBuilder.AddForeignKey(
                name: "fk_category_category_category_id",
                schema: "category_management",
                table: "category",
                column: "parent_id",
                principalSchema: "category_management",
                principalTable: "category",
                principalColumn: "id");
        }
    }
}
