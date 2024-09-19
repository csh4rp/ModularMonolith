using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModularMonolith.Infrastructure.Migrations.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        private const string TransportMigration = """
                                                  IF OBJECT_ID('{0}.TopologySequence', 'SO') IS NULL
                                                  BEGIN
                                                      CREATE SEQUENCE [{0}].[TopologySequence] AS BIGINT START WITH 1 INCREMENT BY 1
                                                  END;

                                                  IF OBJECT_ID('{0}.Queue', 'U') IS NULL
                                                  BEGIN
                                                      CREATE TABLE {0}.Queue
                                                      (
                                                          Id          bigint          not null primary key default next value for [{0}].[TopologySequence],
                                                          Updated     datetime2       not null default GETUTCDATE(),

                                                          Name        nvarchar(256)   not null,
                                                          Type        tinyint         not null,
                                                          AutoDelete  integer
                                                      )
                                                  END;

                                                  IF NOT EXISTS(SELECT TOP 1 1 FROM sys.indexes indexes
                                                      INNER JOIN sys.objects objects ON indexes.object_id = objects.object_id
                                                      WHERE indexes.name ='IX_Queue_Name_Type' AND objects.name = 'Queue')
                                                  BEGIN
                                                      CREATE INDEX IX_Queue_Name_Type ON {0}.Queue (Name, Type) INCLUDE (Id);
                                                  END;

                                                  IF NOT EXISTS(SELECT TOP 1 1 FROM sys.indexes indexes
                                                      INNER JOIN sys.objects objects ON indexes.object_id = objects.object_id
                                                      WHERE indexes.name ='IX_Queue_AutoDelete' AND objects.name = 'Queue')
                                                  BEGIN
                                                      CREATE INDEX IX_Queue_AutoDelete ON {0}.Queue (AutoDelete) INCLUDE (Id);
                                                  END;

                                                  IF OBJECT_ID('{0}.Topic', 'U') IS NULL
                                                  BEGIN
                                                      CREATE TABLE {0}.Topic
                                                      (
                                                          Id          bigint          not null primary key default next value for [{0}].[TopologySequence],
                                                          Updated     datetime2       not null default GETUTCDATE(),

                                                          Name        nvarchar(256) not null
                                                      )
                                                  END;

                                                  IF NOT EXISTS(SELECT TOP 1 1 FROM sys.indexes indexes
                                                      INNER JOIN sys.objects objects ON indexes.object_id = objects.object_id
                                                      WHERE indexes.name ='IX_Topic_Name' AND objects.name = 'Topic')
                                                  BEGIN
                                                      CREATE INDEX IX_Topic_Name ON {0}.Topic (Name) INCLUDE (Id);
                                                  END;

                                                  IF OBJECT_ID('{0}.TopicSubscription', 'U') IS NULL
                                                  BEGIN
                                                      CREATE TABLE {0}.TopicSubscription
                                                      (
                                                          Id             bigint          not null primary key default next value for [{0}].[TopologySequence],
                                                          Updated        datetime2       not null default GETUTCDATE(),

                                                          SourceId       bigint          not null references {0}.Topic (id),
                                                          DestinationId  bigint          not null references {0}.Topic (id),

                                                          SubType        tinyint         not null,
                                                          RoutingKey     nvarchar(256)   not null,
                                                          Filter         nvarchar(1024)  not null
                                                      );
                                                  END;

                                                  IF NOT EXISTS(SELECT TOP 1 1 FROM sys.indexes indexes
                                                      INNER JOIN sys.objects objects ON indexes.object_id = objects.object_id
                                                      WHERE indexes.name ='IX_TopicSubscription_Unique' AND objects.name = 'TopicSubscription')
                                                  BEGIN
                                                      CREATE UNIQUE INDEX IX_TopicSubscription_Unique ON {0}.TopicSubscription (SourceId, DestinationId, SubType, RoutingKey, Filter);
                                                  END;

                                                  IF NOT EXISTS(SELECT TOP 1 1 FROM sys.indexes indexes
                                                      INNER JOIN sys.objects objects ON indexes.object_id = objects.object_id
                                                      WHERE indexes.name ='IX_TopicSubscription_Source' AND objects.name = 'TopicSubscription')
                                                  BEGIN
                                                      CREATE INDEX IX_TopicSubscription_Source ON {0}.TopicSubscription (SourceId) INCLUDE (Id, DestinationId, SubType, RoutingKey, Filter);
                                                  END;

                                                  IF NOT EXISTS(SELECT TOP 1 1 FROM sys.indexes indexes
                                                      INNER JOIN sys.objects objects ON indexes.object_id = objects.object_id
                                                      WHERE indexes.name ='IX_TopicSubscription_Destination' AND objects.name = 'TopicSubscription')
                                                  BEGIN
                                                      CREATE INDEX IX_TopicSubscription_Destination ON {0}.TopicSubscription (DestinationId) INCLUDE (Id, SourceId, SubType, RoutingKey, Filter);
                                                  END;

                                                  IF OBJECT_ID('{0}.DELETE_Topic', 'TR') IS NULL
                                                  BEGIN
                                                      EXEC('
                                                      CREATE TRIGGER [{0}].[DELETE_Topic] ON {0}.Topic INSTEAD OF DELETE
                                                      AS
                                                      BEGIN
                                                           SET NOCOUNT ON;
                                                           DELETE FROM [{0}].[TopicSubscription] WHERE SourceId IN (SELECT Id FROM DELETED);
                                                           DELETE FROM [{0}].[TopicSubscription] WHERE DestinationId IN (SELECT Id FROM DELETED);
                                                           DELETE FROM [{0}].[Topic] WHERE Id IN (SELECT Id FROM DELETED);
                                                      END;
                                                      ');
                                                  END;

                                                  IF OBJECT_ID('{0}.QueueSubscription', 'U') IS NULL
                                                  BEGIN
                                                      CREATE TABLE {0}.QueueSubscription
                                                      (
                                                          Id             bigint          not null primary key default next value for [{0}].[TopologySequence],
                                                          Updated        datetime2       not null default GETUTCDATE(),

                                                          SourceId       bigint          not null references {0}.Topic (id) ON DELETE CASCADE,
                                                          DestinationId  bigint          not null references {0}.Queue (id) ON DELETE CASCADE,

                                                          SubType        tinyint         not null,
                                                          RoutingKey     nvarchar(256)   not null,
                                                          Filter         nvarchar(1024)  not null
                                                      );
                                                  END;

                                                  IF NOT EXISTS(SELECT TOP 1 1 FROM sys.indexes indexes
                                                      INNER JOIN sys.objects objects ON indexes.object_id = objects.object_id
                                                      WHERE indexes.name ='IX_QueueSubscription_Unique' AND objects.name = 'QueueSubscription')
                                                  BEGIN
                                                      CREATE UNIQUE INDEX IX_QueueSubscription_Unique ON {0}.QueueSubscription (SourceId, DestinationId, SubType, RoutingKey, Filter);
                                                  END;

                                                  IF NOT EXISTS(SELECT TOP 1 1 FROM sys.indexes indexes
                                                      INNER JOIN sys.objects objects ON indexes.object_id = objects.object_id
                                                      WHERE indexes.name ='IX_QueueSubscription_Source' AND objects.name = 'QueueSubscription')
                                                  BEGIN
                                                      CREATE INDEX IX_QueueSubscription_Source ON {0}.QueueSubscription (SourceId) INCLUDE (Id, DestinationId, SubType, RoutingKey, Filter);
                                                  END;

                                                  IF NOT EXISTS(SELECT TOP 1 1 FROM sys.indexes indexes
                                                      INNER JOIN sys.objects objects ON indexes.object_id = objects.object_id
                                                      WHERE indexes.name ='IX_QueueSubscription_Destination' AND objects.name = 'QueueSubscription')
                                                  BEGIN
                                                      CREATE INDEX IX_QueueSubscription_Destination ON {0}.QueueSubscription (DestinationId) INCLUDE (Id, SourceId, SubType, RoutingKey, Filter);
                                                  END;

                                                  IF OBJECT_ID('{0}.Message', 'U') IS NULL
                                                  BEGIN
                                                      CREATE TABLE {0}.Message
                                                      (
                                                          TransportMessageId  uniqueidentifier  not null primary key,

                                                          ContentType         nvarchar(max),
                                                          MessageType         nvarchar(max),
                                                          Body                nvarchar(max),
                                                          BinaryBody          varbinary(max),

                                                          MessageId           uniqueidentifier,
                                                          CorrelationId       uniqueidentifier,
                                                          ConversationId      uniqueidentifier,
                                                          RequestId           uniqueidentifier,
                                                          InitiatorId         uniqueidentifier,
                                                          SourceAddress       nvarchar(max),
                                                          DestinationAddress  nvarchar(max),
                                                          ResponseAddress     nvarchar(max),
                                                          FaultAddress        nvarchar(max),

                                                          SentTime            datetime2 NOT NULL DEFAULT GETUTCDATE(),

                                                          Headers             nvarchar(max),
                                                          Host                nvarchar(max),

                                                          SchedulingTokenId   uniqueidentifier
                                                      );
                                                  END;

                                                  IF NOT EXISTS(SELECT TOP 1 1 FROM sys.indexes indexes
                                                      INNER JOIN sys.objects objects ON indexes.object_id = objects.object_id
                                                      WHERE indexes.name ='IX_Message_SchedulingTokenId' AND objects.name = 'Message')
                                                  BEGIN
                                                      CREATE INDEX IX_Message_SchedulingTokenId ON {0}.Message (SchedulingTokenId) where Message.SchedulingTokenId IS NOT NULL;
                                                  END;

                                                  IF OBJECT_ID('{0}.DeliverySequence', 'SO') IS NULL
                                                  BEGIN
                                                      CREATE SEQUENCE [{0}].[DeliverySequence] AS BIGINT START WITH 1 INCREMENT BY 1
                                                  END;

                                                  IF OBJECT_ID('{0}.MessageDelivery', 'U') IS NULL
                                                  BEGIN
                                                      CREATE TABLE {0}.MessageDelivery
                                                      (
                                                          MessageDeliveryId  bigint               not null primary key default next value for [{0}].[DeliverySequence],

                                                          TransportMessageId  uniqueidentifier    not null REFERENCES {0}.Message ON DELETE CASCADE,
                                                          QueueId             bigint              not null,

                                                          Priority            smallint            not null,
                                                          EnqueueTime         datetime2           not null,
                                                          ExpirationTime      datetime2,

                                                          PartitionKey        nvarchar(128),
                                                          RoutingKey          nvarchar(256),

                                                          ConsumerId          uniqueidentifier,
                                                          LockId              uniqueidentifier,

                                                          DeliveryCount       int                 not null,
                                                          MaxDeliveryCount    int                 not null,
                                                          LastDelivered       datetime2,
                                                          TransportHeaders    nvarchar(max)
                                                      );
                                                  END;

                                                  IF NOT EXISTS(SELECT TOP 1 1 FROM sys.indexes indexes
                                                      INNER JOIN sys.objects objects ON indexes.object_id = objects.object_id
                                                      WHERE indexes.name ='IX_MessageDelivery_Fetch' AND objects.name = 'MessageDelivery')
                                                  BEGIN
                                                      CREATE INDEX IX_MessageDelivery_Fetch ON {0}.MessageDelivery (QueueId, Priority, EnqueueTime, MessageDeliveryId);
                                                  END;

                                                  IF NOT EXISTS(SELECT TOP 1 1 FROM sys.indexes indexes
                                                      INNER JOIN sys.objects objects ON indexes.object_id = objects.object_id
                                                      WHERE indexes.name ='IX_MessageDelivery_FetchPart' AND objects.name = 'MessageDelivery')
                                                  BEGIN
                                                      CREATE INDEX IX_MessageDelivery_FetchPart ON {0}.MessageDelivery (QueueId, PartitionKey, Priority, EnqueueTime, MessageDeliveryId);
                                                  END;

                                                  IF NOT EXISTS(SELECT TOP 1 1 FROM sys.indexes indexes
                                                      INNER JOIN sys.objects objects ON indexes.object_id = objects.object_id
                                                      WHERE indexes.name ='IX_MessageDelivery_TransportMessageId' AND objects.name = 'MessageDelivery')
                                                  BEGIN
                                                      CREATE INDEX IX_MessageDelivery_TransportMessageId ON {0}.MessageDelivery (TransportMessageId);
                                                  END;

                                                  IF OBJECT_ID('{0}.QueueMetricCapture', 'U') IS NULL
                                                  BEGIN
                                                      CREATE TABLE {0}.QueueMetricCapture
                                                      (
                                                          Id              bigint              not null identity(1,1),

                                                          Captured        datetime2           not null,
                                                          QueueId         bigint              not null,
                                                          ConsumeCount    bigint              not null,
                                                          ErrorCount      bigint              not null,
                                                          DeadLetterCount bigint              not null,

                                                          CONSTRAINT [PK_QueueMetricCapture] PRIMARY KEY CLUSTERED
                                                          (
                                                              [Id] ASC
                                                          ) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
                                                      );
                                                  END;

                                                  IF OBJECT_ID('{0}.QueueMetric', 'U') IS NULL
                                                  BEGIN
                                                      CREATE TABLE {0}.QueueMetric
                                                      (
                                                          Id              bigint              not null identity(1,1),

                                                          StartTime       datetime2           not null,
                                                          Duration        int                 not null,
                                                          QueueId         bigint              not null,
                                                          ConsumeCount    bigint              not null,
                                                          ErrorCount      bigint              not null,
                                                          DeadLetterCount bigint              not null,

                                                          CONSTRAINT [PK_QueueMetric] PRIMARY KEY CLUSTERED
                                                          (
                                                              [Id] ASC
                                                          ) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
                                                      );
                                                  END;

                                                  IF NOT EXISTS(SELECT TOP 1 1 FROM sys.indexes indexes
                                                      INNER JOIN sys.objects objects ON indexes.object_id = objects.object_id
                                                      WHERE indexes.name ='IX_QueueMetric_Unique' AND objects.name = 'QueueMetric')
                                                  BEGIN
                                                      CREATE UNIQUE INDEX IX_QueueMetric_Unique ON {0}.QueueMetric (StartTime, Duration, QueueId);
                                                  END;
                                                  """;

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Shared");

            migrationBuilder.CreateTable(
                name: "AuditLog",
                schema: "Shared",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EntityTypeName = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    OperationType = table.Column<int>(type: "int", nullable: false),
                    EntityChanges = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EntityKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MetaData = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EventLog",
                schema: "Shared",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EventTypeName = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    EventPayload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MetaData = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InboxState",
                schema: "Shared",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConsumerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LockId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    Received = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReceiveCount = table.Column<int>(type: "int", nullable: false),
                    ExpirationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Consumed = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Delivered = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastSequenceNumber = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InboxState", x => x.Id);
                    table.UniqueConstraint("AK_InboxState_MessageId_ConsumerId", x => new { x.MessageId, x.ConsumerId });
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessage",
                schema: "Shared",
                columns: table => new
                {
                    SequenceNumber = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EnqueueTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SentTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Headers = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Properties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InboxMessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    InboxConsumerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OutboxId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    MessageType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CorrelationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    InitiatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SourceAddress = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    DestinationAddress = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ResponseAddress = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    FaultAddress = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ExpirationTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessage", x => x.SequenceNumber);
                });

            migrationBuilder.CreateTable(
                name: "OutboxState",
                schema: "Shared",
                columns: table => new
                {
                    OutboxId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LockId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Delivered = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastSequenceNumber = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxState", x => x.OutboxId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_EntityTypeName_Timestamp",
                schema: "Shared",
                table: "AuditLog",
                columns: new[] { "EntityTypeName", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_Timestamp",
                schema: "Shared",
                table: "AuditLog",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_EventLog_EventTypeName_Timestamp",
                schema: "Shared",
                table: "EventLog",
                columns: new[] { "EventTypeName", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_EventLog_Timestamp",
                schema: "Shared",
                table: "EventLog",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_InboxState_Delivered",
                schema: "Shared",
                table: "InboxState",
                column: "Delivered");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_EnqueueTime",
                schema: "Shared",
                table: "OutboxMessage",
                column: "EnqueueTime");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_ExpirationTime",
                schema: "Shared",
                table: "OutboxMessage",
                column: "ExpirationTime");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_InboxMessageId_InboxConsumerId_SequenceNumber",
                schema: "Shared",
                table: "OutboxMessage",
                columns: new[] { "InboxMessageId", "InboxConsumerId", "SequenceNumber" },
                unique: true,
                filter: "[InboxMessageId] IS NOT NULL AND [InboxConsumerId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_OutboxId_SequenceNumber",
                schema: "Shared",
                table: "OutboxMessage",
                columns: new[] { "OutboxId", "SequenceNumber" },
                unique: true,
                filter: "[OutboxId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxState_Created",
                schema: "Shared",
                table: "OutboxState",
                column: "Created");

            migrationBuilder.Sql(string.Format(TransportMigration, "Shared"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLog",
                schema: "Shared");

            migrationBuilder.DropTable(
                name: "EventLog",
                schema: "Shared");

            migrationBuilder.DropTable(
                name: "InboxState",
                schema: "Shared");

            migrationBuilder.DropTable(
                name: "OutboxMessage",
                schema: "Shared");

            migrationBuilder.DropTable(
                name: "OutboxState",
                schema: "Shared");
        }
    }
}
