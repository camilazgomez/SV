using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SV.Models;

public partial class InscripcionesBrDbContext : DbContext
{
    public InscripcionesBrDbContext()
    {
    }

    public InscripcionesBrDbContext(DbContextOptions<InscripcionesBrDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<MultiOwner> MultiOwners { get; set; }

    public virtual DbSet<Person> People { get; set; }

    public virtual DbSet<Persona> Personas { get; set; }

    public virtual DbSet<Commune> Commune{ get; set; }

    public virtual DbSet<RealStateForm> RealStateForms { get; set; }

 //   protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
 //       => optionsBuilder.UseSqlServer("server=CAMILAGOMEZ5ABF\\SQLEXPRESS; database=InscripcionesBrDb; integrated security=true; encrypt=false;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MultiOwner>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MultiOwn__3214EC071008A519");

            entity.ToTable("MultiOwner");

            entity.Property(e => e.Block)
                .IsUnicode(false)
                .HasColumnName("block");
            entity.Property(e => e.Commune)
                .IsUnicode(false)
                .HasColumnName("commune");
            entity.Property(e => e.InscriptionDate).HasColumnName("inscriptionDate");
            entity.Property(e => e.InscriptionNumber).HasColumnName("inscriptionNumber");
            entity.Property(e => e.OwnershipPercentage).HasColumnName("ownershipPercentage");
            entity.Property(e => e.Property)
                .IsUnicode(false)
                .HasColumnName("property");
            entity.Property(e => e.Rut)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("rut");
            entity.Property(e => e.Sheets).HasColumnName("sheets");
            entity.Property(e => e.ValidityYearBegin).HasColumnName("validityYearBegin");
            entity.Property(e => e.ValidityYearFinish).HasColumnName("validityYearFinish");

        });

        modelBuilder.Entity<Person>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__People__3214EC07DA64CFFE");

            entity.Property(e => e.FormsId).HasColumnName("formsId");
            entity.Property(e => e.Heir).HasColumnName("heir");
            entity.Property(e => e.OwnershipPercentage).HasColumnName("ownershipPercentage");
            entity.Property(e => e.Rut)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("rut");
            entity.Property(e => e.Seller).HasColumnName("seller");
            entity.Property(e => e.UncreditedOwnership).HasColumnName("uncreditedOwnership");

            entity.HasOne(d => d.Forms).WithMany(p => p.People)
                .HasForeignKey(d => d.FormsId)
                .HasConstraintName("FK_People_(ToTableColumn)");
        });

        modelBuilder.Entity<Persona>(entity =>
        {
            entity.ToTable("Persona");

            entity.Property(e => e.Dirección)
                .HasMaxLength(50)
                .IsFixedLength();
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .IsFixedLength();
            entity.Property(e => e.FechaNacimiento).HasColumnType("date");
            entity.Property(e => e.Nombre).HasMaxLength(50);
            entity.Property(e => e.Rut).HasMaxLength(10);
        });

        modelBuilder.Entity<Commune>(entity =>
        {
            entity.ToTable("Communes");
            entity.Property(e => e.Id).HasColumnName("Id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsFixedLength()
                .HasColumnName("Name");
        });

        modelBuilder.Entity<RealStateForm>(entity =>
        {
            entity.HasKey(e => e.AttentionNumber).HasName("PK__RealStat__A41F8A8A4B8899B6");

            entity.ToTable("RealStateForm");

            entity.Property(e => e.AttentionNumber).HasColumnName("attentionNumber");
            entity.Property(e => e.Block)
                .IsUnicode(false)
                .HasColumnName("block");
            entity.Property(e => e.Commune)
                .IsUnicode(false)
                .HasColumnName("commune");
            entity.Property(e => e.InscriptionDate)
                .HasColumnType("date")
                .HasColumnName("inscriptionDate");
            entity.Property(e => e.InscriptionNumber).HasColumnName("inscriptionNumber");
            entity.Property(e => e.NatureOfTheDeed)
                .IsUnicode(false)
                .HasColumnName("natureOfTheDeed");
            entity.Property(e => e.Property)
                .IsUnicode(false)
                .HasColumnName("property");
            entity.Property(e => e.Sheets).HasColumnName("sheets");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
