﻿// <auto-generated />
using System;
using IPTVRelay.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace IPTVRelay.Database.Migrations
{
    [DbContext(typeof(IPTVRelayContext))]
    [Migration("20240211200707_AddXMLTVItemsToDB")]
    partial class AddXMLTVItemsToDB
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.1");

            modelBuilder.Entity("IPTVRelay.Database.Models.M3U", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long>("Count")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Created")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("Modified")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<long?>("ParentId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Uri")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("Id")
                        .IsUnique();

                    b.HasIndex("ParentId");

                    b.ToTable("M3U");
                });

            modelBuilder.Entity("IPTVRelay.Database.Models.M3UFilter", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Created")
                        .HasColumnType("TEXT");

                    b.Property<string>("FilterContent")
                        .HasColumnType("TEXT");

                    b.Property<int>("FilterType")
                        .HasColumnType("TEXT");

                    b.Property<bool>("Invert")
                        .HasColumnType("INTEGER");

                    b.Property<long?>("M3UId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Modified")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("Id")
                        .IsUnique();

                    b.HasIndex("M3UId");

                    b.ToTable("M3UFilter");
                });

            modelBuilder.Entity("IPTVRelay.Database.Models.M3UItem", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Created")
                        .HasColumnType("TEXT");

                    b.Property<long>("M3UId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Modified")
                        .HasColumnType("TEXT");

                    b.Property<string>("Url")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("M3UItem");
                });

            modelBuilder.Entity("IPTVRelay.Database.Models.M3UItemData", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Created")
                        .HasColumnType("TEXT");

                    b.Property<string>("Key")
                        .HasColumnType("TEXT");

                    b.Property<long>("M3UItemId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Modified")
                        .HasColumnType("TEXT");

                    b.Property<string>("Value")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("M3UItemId");

                    b.ToTable("M3UItemData");
                });

            modelBuilder.Entity("IPTVRelay.Database.Models.Mapping", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("Channel")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Created")
                        .HasColumnType("TEXT");

                    b.Property<long?>("M3UId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Modified")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<long?>("XMLTVItemId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("Id")
                        .IsUnique();

                    b.HasIndex("M3UId");

                    b.HasIndex("XMLTVItemId");

                    b.ToTable("Mapping");
                });

            modelBuilder.Entity("IPTVRelay.Database.Models.MappingFilter", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Created")
                        .HasColumnType("TEXT");

                    b.Property<string>("FilterContent")
                        .HasColumnType("TEXT");

                    b.Property<int>("FilterType")
                        .HasColumnType("TEXT");

                    b.Property<bool>("Invert")
                        .HasColumnType("INTEGER");

                    b.Property<long?>("MappingId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Modified")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("Id")
                        .IsUnique();

                    b.HasIndex("MappingId");

                    b.ToTable("MappingFilter");
                });

            modelBuilder.Entity("IPTVRelay.Database.Models.Setting", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Created")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("Modified")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<long>("SettingsId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("Id")
                        .IsUnique();

                    b.HasIndex("SettingsId");

                    b.ToTable("Setting");
                });

            modelBuilder.Entity("IPTVRelay.Database.Models.Settings", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Created")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("Modified")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("Id")
                        .IsUnique();

                    b.ToTable("Settings");
                });

            modelBuilder.Entity("IPTVRelay.Database.Models.XMLTV", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Created")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("Modified")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<string>("Uri")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("Id")
                        .IsUnique();

                    b.ToTable("XMLTV");
                });

            modelBuilder.Entity("IPTVRelay.Database.Models.XMLTVItem", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ChannelId")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("Created")
                        .HasColumnType("TEXT");

                    b.Property<string>("Logo")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("Modified")
                        .HasColumnType("TEXT");

                    b.Property<long>("XMLTVId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("Id")
                        .IsUnique();

                    b.HasIndex("XMLTVId");

                    b.ToTable("XMLTVItem");
                });

            modelBuilder.Entity("IPTVRelay.Database.Models.M3U", b =>
                {
                    b.HasOne("IPTVRelay.Database.Models.M3U", "Parent")
                        .WithMany()
                        .HasForeignKey("ParentId");

                    b.Navigation("Parent");
                });

            modelBuilder.Entity("IPTVRelay.Database.Models.M3UFilter", b =>
                {
                    b.HasOne("IPTVRelay.Database.Models.M3U", "M3U")
                        .WithMany("Filters")
                        .HasForeignKey("M3UId");

                    b.Navigation("M3U");
                });

            modelBuilder.Entity("IPTVRelay.Database.Models.M3UItemData", b =>
                {
                    b.HasOne("IPTVRelay.Database.Models.M3UItem", null)
                        .WithMany("Data")
                        .HasForeignKey("M3UItemId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("IPTVRelay.Database.Models.Mapping", b =>
                {
                    b.HasOne("IPTVRelay.Database.Models.M3U", "M3U")
                        .WithMany()
                        .HasForeignKey("M3UId");

                    b.HasOne("IPTVRelay.Database.Models.XMLTVItem", "XMLTVItem")
                        .WithMany()
                        .HasForeignKey("XMLTVItemId");

                    b.Navigation("M3U");

                    b.Navigation("XMLTVItem");
                });

            modelBuilder.Entity("IPTVRelay.Database.Models.MappingFilter", b =>
                {
                    b.HasOne("IPTVRelay.Database.Models.Mapping", "Mapping")
                        .WithMany("Filters")
                        .HasForeignKey("MappingId");

                    b.Navigation("Mapping");
                });

            modelBuilder.Entity("IPTVRelay.Database.Models.Setting", b =>
                {
                    b.HasOne("IPTVRelay.Database.Models.Settings", null)
                        .WithMany("List")
                        .HasForeignKey("SettingsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("IPTVRelay.Database.Models.XMLTVItem", b =>
                {
                    b.HasOne("IPTVRelay.Database.Models.XMLTV", null)
                        .WithMany("Items")
                        .HasForeignKey("XMLTVId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("IPTVRelay.Database.Models.M3U", b =>
                {
                    b.Navigation("Filters");
                });

            modelBuilder.Entity("IPTVRelay.Database.Models.M3UItem", b =>
                {
                    b.Navigation("Data");
                });

            modelBuilder.Entity("IPTVRelay.Database.Models.Mapping", b =>
                {
                    b.Navigation("Filters");
                });

            modelBuilder.Entity("IPTVRelay.Database.Models.Settings", b =>
                {
                    b.Navigation("List");
                });

            modelBuilder.Entity("IPTVRelay.Database.Models.XMLTV", b =>
                {
                    b.Navigation("Items");
                });
#pragma warning restore 612, 618
        }
    }
}
