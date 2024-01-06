using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModularMonolith.Shared.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "shared");

            migrationBuilder.CreateTable(
                name: "audit_log",
                schema: "shared",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    entity_type = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    entity_state = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    user_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    operation_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    trace_id = table.Column<string>(type: "character varying(32)", unicode: false, maxLength: 32, nullable: false),
                    span_id = table.Column<string>(type: "character varying(16)", unicode: false, maxLength: 16, nullable: false),
                    parent_span_id = table.Column<string>(type: "character varying(16)", unicode: false, maxLength: 16, nullable: true),
                    ip_address = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    user_agent = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    entity_keys = table.Column<string>(type: "jsonb", nullable: true),
                    entity_property_changes = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_log", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "event_correlation_lock",
                schema: "shared",
                columns: table => new
                {
                    correlation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    acquired_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_event_correlation_lock", x => x.correlation_id);
                });

            migrationBuilder.CreateTable(
                name: "event_log",
                schema: "shared",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    published_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    user_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    topic = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    correlation_id = table.Column<Guid>(type: "uuid", nullable: true),
                    event_type = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    event_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    event_payload = table.Column<string>(type: "jsonb", nullable: false),
                    operation_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    trace_id = table.Column<string>(type: "character varying(32)", unicode: false, maxLength: 32, nullable: false),
                    span_id = table.Column<string>(type: "character varying(16)", unicode: false, maxLength: 16, nullable: false),
                    parent_span_id = table.Column<string>(type: "character varying(16)", unicode: false, maxLength: 16, nullable: true),
                    ip_address = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    user_agent = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_event_log", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "event_log_lock",
                schema: "shared",
                columns: table => new
                {
                    event_log_id = table.Column<Guid>(type: "uuid", nullable: false),
                    acquired_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_event_log_lock", x => x.event_log_id);
                });

            migrationBuilder.CreateTable(
                name: "event_log_publish_attempt",
                schema: "shared",
                columns: table => new
                {
                    event_log_id = table.Column<Guid>(type: "uuid", nullable: false),
                    attempt_number = table.Column<int>(type: "integer", nullable: false),
                    next_attempt_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_event_log_publish_attempt", x => new { x.event_log_id, x.attempt_number });
                });

            migrationBuilder.CreateIndex(
                name: "ix_event_log_correlation_id",
                schema: "shared",
                table: "event_log",
                column: "correlation_id");

            migrationBuilder.CreateIndex(
                name: "ix_event_log_published_at",
                schema: "shared",
                table: "event_log",
                column: "published_at",
                filter: "published_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_event_log_user_id_event_type_created_at",
                schema: "shared",
                table: "event_log",
                columns: new[] { "user_id", "event_type", "created_at" });
            
            migrationBuilder.Sql(
                """
                CREATE OR REPLACE FUNCTION shared.event_log_inserted()
                    RETURNS TRIGGER AS
                $$
                DECLARE
                    payload text;
                BEGIN
                    payload := pg_notify('event_log_queue', NEW.id::text || '/' || COALESCE(NEW.correlation_id::text, ''));
                    RETURN NEW;
                END
                $$ LANGUAGE plpgsql;
                """);

            migrationBuilder.Sql(
                """
                CREATE OR REPLACE TRIGGER tr_event_log_inserted
                AFTER INSERT ON shared.event_log
                FOR EACH ROW
                EXECUTE FUNCTION shared.event_log_inserted();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_log",
                schema: "shared");

            migrationBuilder.DropTable(
                name: "event_correlation_lock",
                schema: "shared");

            migrationBuilder.DropTable(
                name: "event_log",
                schema: "shared");

            migrationBuilder.DropTable(
                name: "event_log_lock",
                schema: "shared");

            migrationBuilder.DropTable(
                name: "event_log_publish_attempt",
                schema: "shared");
        }
    }
}
