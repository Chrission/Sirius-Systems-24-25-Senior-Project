using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using StorkDorkMain.Models;

namespace StorkDork.Models;

public partial class StorkDorkDbContext : DbContext
{
    public StorkDorkDbContext()
    {
    }

    public StorkDorkDbContext(DbContextOptions<StorkDorkDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Bird> Birds { get; set; }

    public virtual DbSet<Checklist> Checklists { get; set; }

    public virtual DbSet<ChecklistItem> ChecklistItems { get; set; }

    public virtual DbSet<Sduser> Sdusers { get; set; }

    public virtual DbSet<Sighting> Sightings { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Bird>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Bird__3214EC27152FA685");
        });

        modelBuilder.Entity<Checklist>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Checklis__3214EC2791C5CD06");

            entity.HasOne(d => d.Sduser).WithMany(p => p.Checklists).HasConstraintName("FK_Checklist_SDUser");
        });

        modelBuilder.Entity<ChecklistItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Checklis__3214EC27391E9E0F");

            entity.HasOne(d => d.Bird).WithMany(p => p.ChecklistItems).HasConstraintName("FK_ChecklistItem_Bird");

            entity.HasOne(d => d.Checklist).WithMany(p => p.ChecklistItems).HasConstraintName("FK_ChecklistItem_Checklist");
        });

        modelBuilder.Entity<Sduser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SDUser__3214EC27D4DAB424");
        });

        modelBuilder.Entity<Sighting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Sighting__3214EC27BEAC21ED");

            entity.HasOne(d => d.Bird).WithMany(p => p.Sightings).HasConstraintName("FK_Sighting_Bird");

            entity.HasOne(d => d.Sduser).WithMany(p => p.Sightings).HasConstraintName("FK_Sighting_SDUser");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
