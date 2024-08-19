﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ModularMonolith.Infrastructure.DataAccess.SqlServer;

#nullable disable

namespace ModularMonolith.Infrastructure.Migrations.SqlServer.Migrations
{
    [DbContext(typeof(SqlServerDbContext))]
    partial class SqlServerDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("MassTransit.EntityFrameworkCoreIntegration.InboxState", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<DateTime?>("Consumed")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("ConsumerId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("Delivered")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("ExpirationTime")
                        .HasColumnType("datetime2");

                    b.Property<long?>("LastSequenceNumber")
                        .HasColumnType("bigint");

                    b.Property<Guid>("LockId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("MessageId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("ReceiveCount")
                        .HasColumnType("int");

                    b.Property<DateTime>("Received")
                        .HasColumnType("datetime2");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.HasKey("Id");

                    b.HasAlternateKey("MessageId", "ConsumerId");

                    b.HasIndex("Delivered");

                    b.ToTable("InboxState", "Shared");

                    b.HasAnnotation("AuditIgnoreAnnotation", true);
                });

            modelBuilder.Entity("MassTransit.EntityFrameworkCoreIntegration.OutboxMessage", b =>
                {
                    b.Property<long>("SequenceNumber")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("SequenceNumber"));

                    b.Property<string>("Body")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ContentType")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<Guid?>("ConversationId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("CorrelationId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("DestinationAddress")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<DateTime?>("EnqueueTime")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("ExpirationTime")
                        .HasColumnType("datetime2");

                    b.Property<string>("FaultAddress")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("Headers")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("InboxConsumerId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("InboxMessageId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("InitiatorId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("MessageId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("MessageType")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("OutboxId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Properties")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("RequestId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ResponseAddress")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<DateTime>("SentTime")
                        .HasColumnType("datetime2");

                    b.Property<string>("SourceAddress")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("SequenceNumber");

                    b.HasIndex("EnqueueTime");

                    b.HasIndex("ExpirationTime");

                    b.HasIndex("OutboxId", "SequenceNumber")
                        .IsUnique()
                        .HasFilter("[OutboxId] IS NOT NULL");

                    b.HasIndex("InboxMessageId", "InboxConsumerId", "SequenceNumber")
                        .IsUnique()
                        .HasFilter("[InboxMessageId] IS NOT NULL AND [InboxConsumerId] IS NOT NULL");

                    b.ToTable("OutboxMessage", "Shared");

                    b.HasAnnotation("AuditIgnoreAnnotation", true);
                });

            modelBuilder.Entity("MassTransit.EntityFrameworkCoreIntegration.OutboxState", b =>
                {
                    b.Property<Guid>("OutboxId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("Delivered")
                        .HasColumnType("datetime2");

                    b.Property<long?>("LastSequenceNumber")
                        .HasColumnType("bigint");

                    b.Property<Guid>("LockId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.HasKey("OutboxId");

                    b.HasIndex("Created");

                    b.ToTable("OutboxState", "Shared");

                    b.HasAnnotation("AuditIgnoreAnnotation", true);
                });

            modelBuilder.Entity("ModularMonolith.CategoryManagement.Domain.Categories.Category", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<Guid?>("ParentId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Version")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.HasIndex("ParentId");

                    b.ToTable("category", "category_management");
                });

            modelBuilder.Entity("ModularMonolith.Identity.Domain.Common.Entities.UserToken", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("LoginProvider")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("Name")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasMaxLength(1024)
                        .HasColumnType("nvarchar(1024)");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("user_token", "identity");
                });

            modelBuilder.Entity("ModularMonolith.Identity.Domain.Roles.Role", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ConcurrencyStamp")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)")
                        .HasAnnotation("AuditIgnoreAnnotation", true);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("NormalizedName")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique();

                    b.ToTable("role", "identity");
                });

            modelBuilder.Entity("ModularMonolith.Identity.Domain.Roles.RoleClaim", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ClaimType")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("ClaimValue")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<Guid>("RoleId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("role_claim", "identity");
                });

            modelBuilder.Entity("ModularMonolith.Identity.Domain.Users.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)")
                        .HasAnnotation("AuditIgnoreAnnotation", true);

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("bit");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("NormalizedEmail")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("NormalizedUserName")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)")
                        .HasAnnotation("AuditIgnoreAnnotation", true);

                    b.Property<string>("PhoneNumber")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("SecurityStamp")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)")
                        .HasAnnotation("AuditIgnoreAnnotation", true);

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("bit");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .IsUnique();

                    b.HasIndex("NormalizedUserName")
                        .IsUnique();

                    b.ToTable("user", "identity");
                });

            modelBuilder.Entity("ModularMonolith.Identity.Domain.Users.UserClaim", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ClaimType")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("ClaimValue")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("user_claim", "identity");
                });

            modelBuilder.Entity("ModularMonolith.Identity.Domain.Users.UserLogin", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("LoginProvider")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("ProviderKey")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("ProviderDisplayName")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.HasKey("UserId", "LoginProvider", "ProviderKey");

                    b.ToTable("user_login", "identity");
                });

            modelBuilder.Entity("ModularMonolith.Identity.Domain.Users.UserRole", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("RoleId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("user_role", "identity");
                });

            modelBuilder.Entity("ModularMonolith.Shared.DataAccess.EntityFramework.SqlServer.AuditLogs.Models.AuditLogEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("EntityTypeName")
                        .IsRequired()
                        .HasMaxLength(512)
                        .HasColumnType("nvarchar(512)");

                    b.Property<int>("OperationType")
                        .HasColumnType("int");

                    b.Property<DateTimeOffset>("Timestamp")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("Id");

                    b.HasIndex("Timestamp");

                    b.HasIndex("EntityTypeName", "Timestamp");

                    b.ToTable("AuditLog", "Shared");

                    b.HasAnnotation("AuditIgnoreAnnotation", true);
                });

            modelBuilder.Entity("ModularMonolith.Shared.DataAccess.EntityFramework.SqlServer.EventLogs.Models.EventLogEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("EventPayload")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("EventTypeName")
                        .IsRequired()
                        .HasMaxLength(512)
                        .HasColumnType("nvarchar(512)");

                    b.Property<DateTimeOffset>("Timestamp")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("Id");

                    b.HasIndex("Timestamp");

                    b.HasIndex("EventTypeName", "Timestamp");

                    b.ToTable("EventLog", "Shared");
                });

            modelBuilder.Entity("ModularMonolith.CategoryManagement.Domain.Categories.Category", b =>
                {
                    b.HasOne("ModularMonolith.CategoryManagement.Domain.Categories.Category", null)
                        .WithMany()
                        .HasForeignKey("ParentId");
                });

            modelBuilder.Entity("ModularMonolith.Identity.Domain.Common.Entities.UserToken", b =>
                {
                    b.HasOne("ModularMonolith.Identity.Domain.Users.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ModularMonolith.Identity.Domain.Roles.RoleClaim", b =>
                {
                    b.HasOne("ModularMonolith.Identity.Domain.Roles.Role", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ModularMonolith.Identity.Domain.Users.UserClaim", b =>
                {
                    b.HasOne("ModularMonolith.Identity.Domain.Users.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("ModularMonolith.Identity.Domain.Users.UserLogin", b =>
                {
                    b.HasOne("ModularMonolith.Identity.Domain.Users.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ModularMonolith.Identity.Domain.Users.UserRole", b =>
                {
                    b.HasOne("ModularMonolith.Identity.Domain.Roles.Role", "Role")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ModularMonolith.Identity.Domain.Users.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Role");

                    b.Navigation("User");
                });

            modelBuilder.Entity("ModularMonolith.Shared.DataAccess.EntityFramework.SqlServer.AuditLogs.Models.AuditLogEntity", b =>
                {
                    b.OwnsMany("ModularMonolith.Shared.DataAccess.AudiLogs.EntityField", "EntityKey", b1 =>
                        {
                            b1.Property<Guid>("AuditLogEntityId")
                                .HasColumnType("uniqueidentifier");

                            b1.Property<int>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("int");

                            b1.Property<string>("Name")
                                .IsRequired()
                                .HasColumnType("nvarchar(max)");

                            b1.Property<string>("Value")
                                .HasColumnType("nvarchar(max)");

                            b1.HasKey("AuditLogEntityId", "Id");

                            b1.ToTable("AuditLog", "Shared");

                            b1
                                .ToJson("EntityKey")
                                .HasAnnotation("AuditIgnoreAnnotation", true);

                            b1.WithOwner()
                                .HasForeignKey("AuditLogEntityId");
                        });

                    b.OwnsMany("ModularMonolith.Shared.DataAccess.AudiLogs.EntityFieldChange", "EntityChanges", b1 =>
                        {
                            b1.Property<Guid>("AuditLogEntityId")
                                .HasColumnType("uniqueidentifier");

                            b1.Property<int>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("int");

                            b1.Property<string>("CurrentValue")
                                .HasColumnType("nvarchar(max)");

                            b1.Property<string>("Name")
                                .IsRequired()
                                .HasColumnType("nvarchar(max)");

                            b1.Property<string>("OriginalValue")
                                .HasColumnType("nvarchar(max)");

                            b1.HasKey("AuditLogEntityId", "Id");

                            b1.ToTable("AuditLog", "Shared");

                            b1
                                .ToJson("EntityChanges")
                                .HasAnnotation("AuditIgnoreAnnotation", true);

                            b1.WithOwner()
                                .HasForeignKey("AuditLogEntityId");
                        });

                    b.OwnsOne("ModularMonolith.Shared.DataAccess.EntityFramework.SqlServer.AuditLogs.Models.AuditLogEntityMetaData", "MetaData", b1 =>
                        {
                            b1.Property<Guid>("AuditLogEntityId")
                                .HasColumnType("uniqueidentifier");

                            b1.Property<string>("OperationName")
                                .HasColumnType("nvarchar(max)");

                            b1.Property<string>("ParentSpanId")
                                .HasColumnType("nvarchar(max)");

                            b1.Property<string>("SpanId")
                                .HasColumnType("nvarchar(max)");

                            b1.Property<string>("Subject")
                                .HasColumnType("nvarchar(max)");

                            b1.Property<string>("TraceId")
                                .HasColumnType("nvarchar(max)");

                            b1.HasKey("AuditLogEntityId");

                            b1.ToTable("AuditLog", "Shared");

                            b1
                                .ToJson("MetaData")
                                .HasAnnotation("AuditIgnoreAnnotation", true);

                            b1.WithOwner()
                                .HasForeignKey("AuditLogEntityId");

                            b1.OwnsMany("ModularMonolith.Shared.DataAccess.EntityFramework.SqlServer.AuditLogs.Models.ExtraData", "ExtraData", b2 =>
                                {
                                    b2.Property<Guid>("AuditLogEntityMetaDataAuditLogEntityId")
                                        .HasColumnType("uniqueidentifier");

                                    b2.Property<int>("Id")
                                        .ValueGeneratedOnAdd()
                                        .HasColumnType("int");

                                    b2.Property<string>("Key")
                                        .IsRequired()
                                        .HasColumnType("nvarchar(max)");

                                    b2.Property<string>("Value")
                                        .HasColumnType("nvarchar(max)");

                                    b2.HasKey("AuditLogEntityMetaDataAuditLogEntityId", "Id");

                                    b2.ToTable("AuditLog", "Shared");

                                    b2.WithOwner()
                                        .HasForeignKey("AuditLogEntityMetaDataAuditLogEntityId");
                                });

                            b1.Navigation("ExtraData");
                        });

                    b.Navigation("EntityChanges");

                    b.Navigation("EntityKey");

                    b.Navigation("MetaData")
                        .IsRequired();
                });

            modelBuilder.Entity("ModularMonolith.Shared.DataAccess.EntityFramework.SqlServer.EventLogs.Models.EventLogEntity", b =>
                {
                    b.OwnsOne("ModularMonolith.Shared.DataAccess.EntityFramework.SqlServer.EventLogs.Models.EventLogEntityMetaData", "MetaData", b1 =>
                        {
                            b1.Property<Guid>("EventLogEntityId")
                                .HasColumnType("uniqueidentifier");

                            b1.Property<string>("IpAddress")
                                .HasColumnType("nvarchar(max)");

                            b1.Property<string>("OperationName")
                                .HasColumnType("nvarchar(max)");

                            b1.Property<string>("ParentSpanId")
                                .HasColumnType("nvarchar(max)");

                            b1.Property<string>("SpanId")
                                .HasColumnType("nvarchar(max)");

                            b1.Property<string>("Subject")
                                .HasColumnType("nvarchar(max)");

                            b1.Property<string>("TraceId")
                                .HasColumnType("nvarchar(max)");

                            b1.Property<string>("Uri")
                                .HasColumnType("nvarchar(max)");

                            b1.HasKey("EventLogEntityId");

                            b1.ToTable("EventLog", "Shared");

                            b1.ToJson("MetaData");

                            b1.WithOwner()
                                .HasForeignKey("EventLogEntityId");
                        });

                    b.Navigation("MetaData")
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
