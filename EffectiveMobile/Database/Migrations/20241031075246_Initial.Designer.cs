﻿// <auto-generated />
using System;
using System.Collections.Generic;
using EffectiveMobile.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EffectiveMobile.Database.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20241031075246_Initial")]
    partial class Initial
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.HasSequence("BaseDeliverySequence");

            modelBuilder.Entity("EffectiveMobile.Database.Models.Abstractions.BaseDelivery", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasDefaultValueSql("nextval('\"BaseDeliverySequence\"')");

                    NpgsqlPropertyBuilderExtensions.UseSequence(b.Property<int>("Id"));

                    b.Property<DateTime>("DeliveryDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("District")
                        .IsRequired()
                        .HasMaxLength(63)
                        .HasColumnType("character varying(63)");

                    b.Property<decimal>("Weight")
                        .HasColumnType("numeric");

                    b.HasKey("Id");

                    b.ToTable((string)null);

                    b.UseTpcMappingStrategy();
                });

            modelBuilder.Entity("EffectiveMobile.Database.Models.Log", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<string>("LevelAsText")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Dictionary<string, object>>("Properties")
                        .HasColumnType("jsonb");

                    b.Property<string>("RenderedMessage")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("TimeStamp")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("Logs");
                });

            modelBuilder.Entity("EffectiveMobile.Database.Models.FilteredDelivery", b =>
                {
                    b.HasBaseType("EffectiveMobile.Database.Models.Abstractions.BaseDelivery");

                    b.ToTable("FilteredDeliveries");
                });

            modelBuilder.Entity("EffectiveMobile.Database.Models.InitialDelivery", b =>
                {
                    b.HasBaseType("EffectiveMobile.Database.Models.Abstractions.BaseDelivery");

                    b.ToTable("InitialDeliveries");
                });
#pragma warning restore 612, 618
        }
    }
}
