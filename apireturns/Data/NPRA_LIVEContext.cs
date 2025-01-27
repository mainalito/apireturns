using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using apireturns.Models;

namespace apireturns.Data;

public partial class NPRA_LIVEContext : DbContext
{
    public NPRA_LIVEContext()
    {
    }

    public NPRA_LIVEContext(DbContextOptions<NPRA_LIVEContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Return> Returns { get; set; }

    public virtual DbSet<ReturnSheet> ReturnSheets { get; set; }

    public virtual DbSet<ReturnSheetColumn> ReturnSheetColumns { get; set; }

    public virtual DbSet<ReturnSheetDataType> ReturnSheetDataTypes { get; set; }

    public virtual DbSet<ReturnSheetDatum> ReturnSheetData { get; set; }

    public virtual DbSet<ReturnSheetMatrix> ReturnSheetMatrices { get; set; }

    public virtual DbSet<ReturnSheetRow> ReturnSheetRows { get; set; }

    public virtual DbSet<ReturnSheetType> ReturnSheetTypes { get; set; }

    public virtual DbSet<ReturnSubmission> ReturnSubmissions { get; set; }

    public virtual DbSet<ReturnTemplate> ReturnTemplates { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:MyDbContext");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("SQL_Latin1_General_CP1_CI_AS");

        modelBuilder.Entity<Return>(entity =>
        {
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<ReturnSheet>(entity =>
        {
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Return).WithMany(p => p.ReturnSheets)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ReturnSheet_Returns");

            entity.HasOne(d => d.ReturnSheetType).WithMany(p => p.ReturnSheets)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ReturnSheet_ReturnSheetType");
        });

        modelBuilder.Entity<ReturnSheetColumn>(entity =>
        {
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<ReturnSheetDataType>(entity =>
        {
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<ReturnSheetDatum>(entity =>
        {
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Matrix).WithMany(p => p.ReturnSheetData)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ReturnSheetData_ReturnSheetMatrix");

            entity.HasOne(d => d.Submission).WithMany(p => p.ReturnSheetData)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ReturnSheetData_ReturnSubmission");
        });

        modelBuilder.Entity<ReturnSheetMatrix>(entity =>
        {
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Column).WithMany(p => p.ReturnSheetMatrices)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ReturnSheetMatrix_ReturnSheetColumns");

            entity.HasOne(d => d.DataType).WithMany(p => p.ReturnSheetMatrices)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ReturnSheetMatrix_ReturnSheetDataType");

            entity.HasOne(d => d.Row).WithMany(p => p.ReturnSheetMatrices)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ReturnSheetMatrix_ReturnSheetRows");
        });

        modelBuilder.Entity<ReturnSheetRow>(entity =>
        {
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<ReturnSheetType>(entity =>
        {
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<ReturnSubmission>(entity =>
        {
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Return).WithMany(p => p.ReturnSubmissions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ReturnSubmission_Returns");
        });

        modelBuilder.Entity<ReturnTemplate>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK_Templates");

            entity.Property(e => e.Version).HasDefaultValue("v.1.0.0");
        });


        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
