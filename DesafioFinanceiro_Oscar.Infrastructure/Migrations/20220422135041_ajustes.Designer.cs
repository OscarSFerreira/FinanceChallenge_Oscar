﻿// <auto-generated />
using System;
using DesafioFinanceiro_Oscar.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DesafioFinanceiro_Oscar.Infrastructure.Migrations
{
    [DbContext(typeof(DataContext))]
    [Migration("20220422135041_ajustes")]
    partial class ajustes
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.16")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("DesafioFinanceiro_Oscar.Domain.Entities.BankRecord", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("Origin")
                        .HasColumnType("int");

                    b.Property<Guid?>("OriginId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("BankRecords");
                });

            modelBuilder.Entity("DesafioFinanceiro_Oscar.Domain.Entities.BuyRequest", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("City")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClientDescription")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClientEmail")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("ClientId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ClientPhone")
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("Code")
                        .HasColumnType("bigint");

                    b.Property<string>("Complement")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("CostPrice")
                        .HasColumnType("decimal(18,2)");

                    b.Property<DateTimeOffset>("Date")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset>("DeliveryDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<decimal>("Discount")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("ProductPrices")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("Sector")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("State")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<string>("Street")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("StreetNum")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("TotalPricing")
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("Id");

                    b.ToTable("BuyRequests");
                });

            modelBuilder.Entity("DesafioFinanceiro_Oscar.Domain.Entities.ProductRequest", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("ProductCategory")
                        .HasColumnType("int");

                    b.Property<string>("ProductDescription")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("ProductId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<decimal>("ProductPrice")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("ProductQuantity")
                        .HasColumnType("decimal(18,2)");

                    b.Property<Guid>("RequestId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<decimal>("Total")
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("Id");

                    b.ToTable("ProductRequests");
                });
#pragma warning restore 612, 618
        }
    }
}
