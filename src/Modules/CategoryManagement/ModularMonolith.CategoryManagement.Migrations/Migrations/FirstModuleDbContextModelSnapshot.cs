﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ModularMonolith.CategoryManagement.Infrastructure.Common.DataAccess;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ModularMonolith.CategoryManagement.Migrations.Migrations
{
    [DbContext(typeof(CategoryManagementDbContext))]
    partial class CategoryManagementDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("first_module")
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("ModularMonolith.CategoryManagement.Domain.Entities.Category", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
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

                    b.HasKey("Id")
                        .HasName("pk_category");

                    b.HasIndex("Name")
                        .IsUnique()
                        .HasDatabaseName("ix_category_name");

                    b.HasIndex("ParentId")
                        .HasDatabaseName("ix_category_parent_id");

                    b.ToTable("category", "first_module");
                });

            modelBuilder.Entity("ModularMonolith.CategoryManagement.Domain.Entities.Category", b =>
                {
                    b.HasOne("ModularMonolith.CategoryManagement.Domain.Entities.Category", null)
                        .WithMany()
                        .HasForeignKey("ParentId")
                        .HasConstraintName("fk_category_category_category_id");
                });
#pragma warning restore 612, 618
        }
    }
}