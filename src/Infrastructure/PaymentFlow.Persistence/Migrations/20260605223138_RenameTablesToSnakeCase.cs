using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaymentFlow.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameTablesToSnakeCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Payments",
                table: "Payments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProcessedMessages",
                table: "ProcessedMessages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OutboxMessages",
                table: "OutboxMessages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AuditLogs",
                table: "AuditLogs");

            migrationBuilder.RenameTable(
                name: "Payments",
                newName: "payments");

            migrationBuilder.RenameTable(
                name: "ProcessedMessages",
                newName: "processed_messages");

            migrationBuilder.RenameTable(
                name: "OutboxMessages",
                newName: "outbox_messages");

            migrationBuilder.RenameTable(
                name: "AuditLogs",
                newName: "audit_logs");

            migrationBuilder.RenameIndex(
                name: "IX_Payments_CustomerId",
                table: "payments",
                newName: "IX_payments_CustomerId");

            migrationBuilder.RenameIndex(
                name: "IX_OutboxMessages_Processed_CreatedAtUtc",
                table: "outbox_messages",
                newName: "IX_outbox_messages_Processed_CreatedAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_AuditLogs_PaymentId",
                table: "audit_logs",
                newName: "IX_audit_logs_PaymentId");

            migrationBuilder.RenameIndex(
                name: "IX_AuditLogs_EventId",
                table: "audit_logs",
                newName: "IX_audit_logs_EventId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_payments",
                table: "payments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_processed_messages",
                table: "processed_messages",
                columns: new[] { "MessageId", "ConsumerName" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_outbox_messages",
                table: "outbox_messages",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_audit_logs",
                table: "audit_logs",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_payments",
                table: "payments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_processed_messages",
                table: "processed_messages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_outbox_messages",
                table: "outbox_messages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_audit_logs",
                table: "audit_logs");

            migrationBuilder.RenameTable(
                name: "payments",
                newName: "Payments");

            migrationBuilder.RenameTable(
                name: "processed_messages",
                newName: "ProcessedMessages");

            migrationBuilder.RenameTable(
                name: "outbox_messages",
                newName: "OutboxMessages");

            migrationBuilder.RenameTable(
                name: "audit_logs",
                newName: "AuditLogs");

            migrationBuilder.RenameIndex(
                name: "IX_payments_CustomerId",
                table: "Payments",
                newName: "IX_Payments_CustomerId");

            migrationBuilder.RenameIndex(
                name: "IX_outbox_messages_Processed_CreatedAtUtc",
                table: "OutboxMessages",
                newName: "IX_OutboxMessages_Processed_CreatedAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_audit_logs_PaymentId",
                table: "AuditLogs",
                newName: "IX_AuditLogs_PaymentId");

            migrationBuilder.RenameIndex(
                name: "IX_audit_logs_EventId",
                table: "AuditLogs",
                newName: "IX_AuditLogs_EventId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Payments",
                table: "Payments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProcessedMessages",
                table: "ProcessedMessages",
                columns: new[] { "MessageId", "ConsumerName" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_OutboxMessages",
                table: "OutboxMessages",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AuditLogs",
                table: "AuditLogs",
                column: "Id");
        }
    }
}
