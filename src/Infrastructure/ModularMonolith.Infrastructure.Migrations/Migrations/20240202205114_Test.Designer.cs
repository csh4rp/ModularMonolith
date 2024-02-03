﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ModularMonolith.Bootstrapper.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ModularMonolith.Infrastructure.Migrations.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20240202205114_Test")]
    partial class Test
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("MassTransit.EntityFrameworkCoreIntegration.InboxState", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<DateTime?>("Consumed")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("consumed");

                    b.Property<Guid>("ConsumerId")
                        .HasColumnType("uuid")
                        .HasColumnName("consumer_id");

                    b.Property<DateTime?>("Delivered")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("delivered");

                    b.Property<DateTime?>("ExpirationTime")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("expiration_time");

                    b.Property<long?>("LastSequenceNumber")
                        .HasColumnType("bigint")
                        .HasColumnName("last_sequence_number");

                    b.Property<Guid>("LockId")
                        .HasColumnType("uuid")
                        .HasColumnName("lock_id");

                    b.Property<Guid>("MessageId")
                        .HasColumnType("uuid")
                        .HasColumnName("message_id");

                    b.Property<int>("ReceiveCount")
                        .HasColumnType("integer")
                        .HasColumnName("receive_count");

                    b.Property<DateTime>("Received")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("received");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("bytea")
                        .HasColumnName("row_version");

                    b.HasKey("Id")
                        .HasName("pk_inbox_state");

                    b.HasAlternateKey("MessageId", "ConsumerId")
                        .HasName("ak_inbox_state_message_id_consumer_id");

                    b.HasIndex("Delivered")
                        .HasDatabaseName("ix_inbox_state_delivered");

                    b.ToTable("inbox_state", "shared");

                    b.HasAnnotation("AuditIgnoreAnnotation", true);
                });

            modelBuilder.Entity("MassTransit.EntityFrameworkCoreIntegration.OutboxMessage", b =>
                {
                    b.Property<long>("SequenceNumber")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("sequence_number");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("SequenceNumber"));

                    b.Property<string>("Body")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("body");

                    b.Property<string>("ContentType")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)")
                        .HasColumnName("content_type");

                    b.Property<Guid?>("ConversationId")
                        .HasColumnType("uuid")
                        .HasColumnName("conversation_id");

                    b.Property<Guid?>("CorrelationId")
                        .HasColumnType("uuid")
                        .HasColumnName("correlation_id");

                    b.Property<string>("DestinationAddress")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)")
                        .HasColumnName("destination_address");

                    b.Property<DateTime?>("EnqueueTime")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("enqueue_time");

                    b.Property<DateTime?>("ExpirationTime")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("expiration_time");

                    b.Property<string>("FaultAddress")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)")
                        .HasColumnName("fault_address");

                    b.Property<string>("Headers")
                        .HasColumnType("text")
                        .HasColumnName("headers");

                    b.Property<Guid?>("InboxConsumerId")
                        .HasColumnType("uuid")
                        .HasColumnName("inbox_consumer_id");

                    b.Property<Guid?>("InboxMessageId")
                        .HasColumnType("uuid")
                        .HasColumnName("inbox_message_id");

                    b.Property<Guid?>("InitiatorId")
                        .HasColumnType("uuid")
                        .HasColumnName("initiator_id");

                    b.Property<Guid>("MessageId")
                        .HasColumnType("uuid")
                        .HasColumnName("message_id");

                    b.Property<string>("MessageType")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("message_type");

                    b.Property<Guid?>("OutboxId")
                        .HasColumnType("uuid")
                        .HasColumnName("outbox_id");

                    b.Property<string>("Properties")
                        .HasColumnType("text")
                        .HasColumnName("properties");

                    b.Property<Guid?>("RequestId")
                        .HasColumnType("uuid")
                        .HasColumnName("request_id");

                    b.Property<string>("ResponseAddress")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)")
                        .HasColumnName("response_address");

                    b.Property<DateTime>("SentTime")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("sent_time");

                    b.Property<string>("SourceAddress")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)")
                        .HasColumnName("source_address");

                    b.HasKey("SequenceNumber")
                        .HasName("pk_outbox_message");

                    b.HasIndex("EnqueueTime")
                        .HasDatabaseName("ix_outbox_message_enqueue_time");

                    b.HasIndex("ExpirationTime")
                        .HasDatabaseName("ix_outbox_message_expiration_time");

                    b.HasIndex("OutboxId", "SequenceNumber")
                        .IsUnique()
                        .HasDatabaseName("ix_outbox_message_outbox_id_sequence_number");

                    b.HasIndex("InboxMessageId", "InboxConsumerId", "SequenceNumber")
                        .IsUnique()
                        .HasDatabaseName("ix_outbox_message_inbox_message_id_inbox_consumer_id_sequence_");

                    b.ToTable("outbox_message", "shared");

                    b.HasAnnotation("AuditIgnoreAnnotation", true);
                });

            modelBuilder.Entity("MassTransit.EntityFrameworkCoreIntegration.OutboxState", b =>
                {
                    b.Property<Guid>("OutboxId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("outbox_id");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created");

                    b.Property<DateTime?>("Delivered")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("delivered");

                    b.Property<long?>("LastSequenceNumber")
                        .HasColumnType("bigint")
                        .HasColumnName("last_sequence_number");

                    b.Property<Guid>("LockId")
                        .HasColumnType("uuid")
                        .HasColumnName("lock_id");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("bytea")
                        .HasColumnName("row_version");

                    b.HasKey("OutboxId")
                        .HasName("pk_outbox_state");

                    b.HasIndex("Created")
                        .HasDatabaseName("ix_outbox_state_created");

                    b.ToTable("outbox_state", "shared");

                    b.HasAnnotation("AuditIgnoreAnnotation", true);
                });

            modelBuilder.Entity("ModularMonolith.CategoryManagement.Domain.Categories.Category", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)")
                        .HasColumnName("name");

                    b.Property<Guid?>("ParentId")
                        .HasColumnType("uuid")
                        .HasColumnName("parent_id");

                    b.Property<int>("Version")
                        .HasColumnType("integer")
                        .HasColumnName("version");

                    b.HasKey("Id")
                        .HasName("pk_category");

                    b.HasIndex("Name")
                        .IsUnique()
                        .HasDatabaseName("ix_category_name");

                    b.HasIndex("ParentId")
                        .HasDatabaseName("ix_category_parent_id");

                    b.ToTable("category", "category_management");
                });

            modelBuilder.Entity("ModularMonolith.Identity.Domain.Common.Entities.Role", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("ConcurrencyStamp")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)")
                        .HasColumnName("concurrency_stamp")
                        .HasAnnotation("AuditIgnoreAnnotation", true);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)")
                        .HasColumnName("name");

                    b.Property<string>("NormalizedName")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)")
                        .HasColumnName("normalized_name");

                    b.HasKey("Id")
                        .HasName("pk_role");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("ix_role_normalized_name");

                    b.ToTable("role", "identity");
                });

            modelBuilder.Entity("ModularMonolith.Identity.Domain.Common.Entities.RoleClaim", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("ClaimType")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)")
                        .HasColumnName("claim_type");

                    b.Property<string>("ClaimValue")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)")
                        .HasColumnName("claim_value");

                    b.Property<Guid>("RoleId")
                        .HasColumnType("uuid")
                        .HasColumnName("role_id");

                    b.HasKey("Id")
                        .HasName("pk_role_claim");

                    b.HasIndex("RoleId")
                        .HasDatabaseName("ix_role_claim_role_id");

                    b.ToTable("role_claim", "identity");
                });

            modelBuilder.Entity("ModularMonolith.Identity.Domain.Common.Entities.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("integer")
                        .HasColumnName("access_failed_count");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)")
                        .HasColumnName("concurrency_stamp")
                        .HasAnnotation("AuditIgnoreAnnotation", true);

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)")
                        .HasColumnName("email");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("boolean")
                        .HasColumnName("email_confirmed");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("boolean")
                        .HasColumnName("lockout_enabled");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("lockout_end");

                    b.Property<string>("NormalizedEmail")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)")
                        .HasColumnName("normalized_email");

                    b.Property<string>("NormalizedUserName")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)")
                        .HasColumnName("normalized_user_name");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)")
                        .HasColumnName("password_hash")
                        .HasAnnotation("AuditIgnoreAnnotation", true);

                    b.Property<string>("PhoneNumber")
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)")
                        .HasColumnName("phone_number");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("boolean")
                        .HasColumnName("phone_number_confirmed");

                    b.Property<string>("SecurityStamp")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)")
                        .HasColumnName("security_stamp")
                        .HasAnnotation("AuditIgnoreAnnotation", true);

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("boolean")
                        .HasColumnName("two_factor_enabled");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)")
                        .HasColumnName("user_name");

                    b.HasKey("Id")
                        .HasName("pk_user");

                    b.HasIndex("NormalizedEmail")
                        .IsUnique()
                        .HasDatabaseName("ix_user_normalized_email");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("ix_user_normalized_user_name");

                    b.ToTable("user", "identity");
                });

            modelBuilder.Entity("ModularMonolith.Identity.Domain.Common.Entities.UserClaim", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("ClaimType")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)")
                        .HasColumnName("claim_type");

                    b.Property<string>("ClaimValue")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)")
                        .HasColumnName("claim_value");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid")
                        .HasColumnName("user_id");

                    b.HasKey("Id")
                        .HasName("pk_user_claim");

                    b.HasIndex("UserId")
                        .HasDatabaseName("ix_user_claim_user_id");

                    b.ToTable("user_claim", "identity");
                });

            modelBuilder.Entity("ModularMonolith.Identity.Domain.Common.Entities.UserLogin", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid")
                        .HasColumnName("user_id");

                    b.Property<string>("LoginProvider")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)")
                        .HasColumnName("login_provider");

                    b.Property<string>("ProviderKey")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)")
                        .HasColumnName("provider_key");

                    b.Property<string>("ProviderDisplayName")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)")
                        .HasColumnName("provider_display_name");

                    b.HasKey("UserId", "LoginProvider", "ProviderKey")
                        .HasName("pk_user_login");

                    b.ToTable("user_login", "identity");
                });

            modelBuilder.Entity("ModularMonolith.Identity.Domain.Common.Entities.UserRole", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid")
                        .HasColumnName("user_id");

                    b.Property<Guid>("RoleId")
                        .HasColumnType("uuid")
                        .HasColumnName("role_id");

                    b.HasKey("UserId", "RoleId")
                        .HasName("pk_user_role");

                    b.HasIndex("RoleId")
                        .HasDatabaseName("ix_user_role_role_id");

                    b.ToTable("user_role", "identity");
                });

            modelBuilder.Entity("ModularMonolith.Identity.Domain.Common.Entities.UserToken", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid")
                        .HasColumnName("user_id");

                    b.Property<string>("LoginProvider")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)")
                        .HasColumnName("login_provider");

                    b.Property<string>("Name")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)")
                        .HasColumnName("name");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasMaxLength(1024)
                        .HasColumnType("character varying(1024)")
                        .HasColumnName("value");

                    b.HasKey("UserId", "LoginProvider", "Name")
                        .HasName("pk_user_token");

                    b.ToTable("user_token", "identity");
                });

            modelBuilder.Entity("ModularMonolith.Shared.Domain.Entities.AuditLog", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<string>("EntityState")
                        .IsRequired()
                        .HasMaxLength(8)
                        .HasColumnType("character varying(8)")
                        .HasColumnName("entity_state");

                    b.Property<string>("EntityType")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)")
                        .HasColumnName("entity_type");

                    b.Property<string>("IpAddress")
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)")
                        .HasColumnName("ip_address");

                    b.Property<string>("OperationName")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)")
                        .HasColumnName("operation_name");

                    b.Property<string>("ParentSpanId")
                        .HasMaxLength(16)
                        .IsUnicode(false)
                        .HasColumnType("character varying(16)")
                        .HasColumnName("parent_span_id");

                    b.Property<string>("SpanId")
                        .IsRequired()
                        .HasMaxLength(16)
                        .IsUnicode(false)
                        .HasColumnType("character varying(16)")
                        .HasColumnName("span_id");

                    b.Property<string>("TraceId")
                        .IsRequired()
                        .HasMaxLength(32)
                        .IsUnicode(false)
                        .HasColumnType("character varying(32)")
                        .HasColumnName("trace_id");

                    b.Property<string>("UserAgent")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)")
                        .HasColumnName("user_agent");

                    b.Property<Guid?>("UserId")
                        .HasColumnType("uuid")
                        .HasColumnName("user_id");

                    b.Property<string>("UserName")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)")
                        .HasColumnName("user_name");

                    b.HasKey("Id")
                        .HasName("pk_audit_log");

                    b.ToTable("audit_log", "shared");
                });

            modelBuilder.Entity("ModularMonolith.Shared.Domain.Entities.EventLog", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<string>("EventPayload")
                        .IsRequired()
                        .HasColumnType("jsonb")
                        .HasColumnName("event_payload");

                    b.Property<string>("EventType")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)")
                        .HasColumnName("event_type");

                    b.Property<Guid?>("UserId")
                        .HasColumnType("uuid")
                        .HasColumnName("user_id");

                    b.HasKey("Id")
                        .HasName("pk_event_log");

                    b.HasIndex("UserId", "EventType", "CreatedAt")
                        .HasDatabaseName("ix_event_log_user_id_event_type_created_at");

                    b.ToTable("event_log", "shared");
                });

            modelBuilder.Entity("ModularMonolith.CategoryManagement.Domain.Categories.Category", b =>
                {
                    b.HasOne("ModularMonolith.CategoryManagement.Domain.Categories.Category", null)
                        .WithMany()
                        .HasForeignKey("ParentId")
                        .HasConstraintName("fk_category_category_parent_id");
                });

            modelBuilder.Entity("ModularMonolith.Identity.Domain.Common.Entities.RoleClaim", b =>
                {
                    b.HasOne("ModularMonolith.Identity.Domain.Common.Entities.Role", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_role_claim_role_role_id");
                });

            modelBuilder.Entity("ModularMonolith.Identity.Domain.Common.Entities.UserClaim", b =>
                {
                    b.HasOne("ModularMonolith.Identity.Domain.Common.Entities.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_user_claim_user_user_id");

                    b.Navigation("User");
                });

            modelBuilder.Entity("ModularMonolith.Identity.Domain.Common.Entities.UserLogin", b =>
                {
                    b.HasOne("ModularMonolith.Identity.Domain.Common.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_user_login_user_user_id");
                });

            modelBuilder.Entity("ModularMonolith.Identity.Domain.Common.Entities.UserRole", b =>
                {
                    b.HasOne("ModularMonolith.Identity.Domain.Common.Entities.Role", "Role")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_user_role_role_role_id");

                    b.HasOne("ModularMonolith.Identity.Domain.Common.Entities.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_user_role_user_user_id");

                    b.Navigation("Role");

                    b.Navigation("User");
                });

            modelBuilder.Entity("ModularMonolith.Identity.Domain.Common.Entities.UserToken", b =>
                {
                    b.HasOne("ModularMonolith.Identity.Domain.Common.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_user_token_user_user_id");
                });

            modelBuilder.Entity("ModularMonolith.Shared.Domain.Entities.AuditLog", b =>
                {
                    b.OwnsMany("ModularMonolith.Shared.Domain.ValueObjects.EntityKey", "EntityKeys", b1 =>
                        {
                            b1.Property<Guid>("AuditLogId")
                                .HasColumnType("uuid");

                            b1.Property<int>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("integer");

                            b1.Property<string>("PropertyName")
                                .IsRequired()
                                .HasColumnType("text");

                            b1.Property<string>("Value")
                                .HasColumnType("text");

                            b1.HasKey("AuditLogId", "Id");

                            b1.ToTable("audit_log", "shared");

                            b1.ToJson("entity_keys");

                            b1.WithOwner()
                                .HasForeignKey("AuditLogId")
                                .HasConstraintName("fk_audit_log_audit_log_audit_log_id");
                        });

                    b.OwnsMany("ModularMonolith.Shared.Domain.ValueObjects.PropertyChange", "EntityPropertyChanges", b1 =>
                        {
                            b1.Property<Guid>("AuditLogId")
                                .HasColumnType("uuid");

                            b1.Property<int>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("integer");

                            b1.Property<string>("CurrentValue")
                                .HasColumnType("text");

                            b1.Property<string>("OriginalValue")
                                .HasColumnType("text");

                            b1.Property<string>("PropertyName")
                                .IsRequired()
                                .HasColumnType("text");

                            b1.HasKey("AuditLogId", "Id");

                            b1.ToTable("audit_log", "shared");

                            b1.ToJson("entity_property_changes");

                            b1.WithOwner()
                                .HasForeignKey("AuditLogId")
                                .HasConstraintName("fk_audit_log_audit_log_audit_log_id");
                        });

                    b.Navigation("EntityKeys");

                    b.Navigation("EntityPropertyChanges");
                });
#pragma warning restore 612, 618
        }
    }
}
