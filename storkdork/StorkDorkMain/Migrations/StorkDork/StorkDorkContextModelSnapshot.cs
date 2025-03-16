﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using StorkDorkMain.Data;

#nullable disable

namespace StorkDork.Migrations.StorkDork
{
    [DbContext(typeof(StorkDorkContext))]
    partial class StorkDorkContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.4")
                .HasAnnotation("Proxies:ChangeTracking", false)
                .HasAnnotation("Proxies:CheckEquality", false)
                .HasAnnotation("Proxies:LazyLoading", true)
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("StorkDorkMain.Models.Bird", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("ID");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Category")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");

                    b.Property<string>("CommonName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("FamilyCommonName")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("FamilyScientificName")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("Order")
                        .HasMaxLength(25)
                        .HasColumnType("nvarchar(25)");

                    b.Property<string>("Range")
                        .HasMaxLength(1000)
                        .HasColumnType("nvarchar(1000)");

                    b.Property<string>("ReportAs")
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");

                    b.Property<string>("ScientificName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("SpeciesCode")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");

                    b.HasKey("Id")
                        .HasName("PK__Bird__3214EC27B23D066D");

                    b.ToTable("Bird", (string)null);
                });

            modelBuilder.Entity("StorkDorkMain.Models.Checklist", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("ID");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ChecklistName")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<int?>("SdUserId")
                        .HasColumnType("int")
                        .HasColumnName("SDUserID");

                    b.HasKey("Id")
                        .HasName("PK__Checklis__3214EC271BFFB94F");

                    b.HasIndex("SdUserId");

                    b.ToTable("Checklist", (string)null);
                });

            modelBuilder.Entity("StorkDorkMain.Models.ChecklistItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("ID");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("BirdId")
                        .HasColumnType("int")
                        .HasColumnName("BirdID");

                    b.Property<int?>("ChecklistId")
                        .HasColumnType("int")
                        .HasColumnName("ChecklistID");

                    b.Property<bool?>("Sighted")
                        .HasColumnType("bit");

                    b.HasKey("Id")
                        .HasName("PK__Checklis__3214EC27AEE131D7");

                    b.HasIndex("BirdId");

                    b.HasIndex("ChecklistId");

                    b.ToTable("ChecklistItem", (string)null);
                });

            modelBuilder.Entity("StorkDorkMain.Models.Milestone", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("PhotosContributed")
                        .HasColumnType("int");

                    b.Property<int>("SDUserId")
                        .HasColumnType("int");

                    b.Property<int>("SightingsMade")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("Milestones");
                });

            modelBuilder.Entity("StorkDorkMain.Models.SdUser", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("ID");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("AspNetIdentityId")
                        .HasMaxLength(450)
                        .HasColumnType("nvarchar(450)")
                        .HasColumnName("AspNetIdentityID");

                    b.Property<string>("FirstName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LastName")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id")
                        .HasName("PK__SDUser__3214EC277D9B2DC9");

                    b.ToTable("SDUser", (string)null);
                });

            modelBuilder.Entity("StorkDorkMain.Models.Sighting", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("ID");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("BirdId")
                        .HasColumnType("int")
                        .HasColumnName("BirdID");

                    b.Property<DateTime?>("Date")
                        .HasColumnType("datetime2");

                    b.Property<decimal?>("Latitude")
                        .HasColumnType("decimal(8, 6)");

                    b.Property<decimal?>("Longitude")
                        .HasColumnType("decimal(9, 6)");

                    b.Property<string>("Notes")
                        .HasMaxLength(3000)
                        .HasColumnType("nvarchar(3000)");

                    b.Property<int?>("SdUserId")
                        .HasColumnType("int")
                        .HasColumnName("SDUserID");

                    b.HasKey("Id")
                        .HasName("PK__Sighting__3214EC2734C618BE");

                    b.HasIndex("BirdId");

                    b.HasIndex("SdUserId");

                    b.ToTable("Sighting", (string)null);
                });

            modelBuilder.Entity("StorkDorkMain.Models.Checklist", b =>
                {
                    b.HasOne("StorkDorkMain.Models.SdUser", "SdUser")
                        .WithMany()
                        .HasForeignKey("SdUserId");

                    b.Navigation("SdUser");
                });

            modelBuilder.Entity("StorkDorkMain.Models.ChecklistItem", b =>
                {
                    b.HasOne("StorkDorkMain.Models.Bird", "Bird")
                        .WithMany("ChecklistItems")
                        .HasForeignKey("BirdId")
                        .HasConstraintName("FK_ChecklistItem_Bird");

                    b.HasOne("StorkDorkMain.Models.Checklist", "Checklist")
                        .WithMany("ChecklistItems")
                        .HasForeignKey("ChecklistId")
                        .HasConstraintName("FK_ChecklistItem_Checklist");

                    b.Navigation("Bird");

                    b.Navigation("Checklist");
                });

            modelBuilder.Entity("StorkDorkMain.Models.Sighting", b =>
                {
                    b.HasOne("StorkDorkMain.Models.Bird", "Bird")
                        .WithMany("Sightings")
                        .HasForeignKey("BirdId")
                        .HasConstraintName("FK_Sighting_Bird");

                    b.HasOne("StorkDorkMain.Models.SdUser", "SdUser")
                        .WithMany()
                        .HasForeignKey("SdUserId");

                    b.Navigation("Bird");

                    b.Navigation("SdUser");
                });

            modelBuilder.Entity("StorkDorkMain.Models.Bird", b =>
                {
                    b.Navigation("ChecklistItems");

                    b.Navigation("Sightings");
                });

            modelBuilder.Entity("StorkDorkMain.Models.Checklist", b =>
                {
                    b.Navigation("ChecklistItems");
                });
#pragma warning restore 612, 618
        }
    }
}
