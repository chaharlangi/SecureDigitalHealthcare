using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SecureDigitalHealthcare.Models;

public partial class SecureDigitalHealthcareContext : DbContext
{
    public SecureDigitalHealthcareContext()
    {
    }

    public SecureDigitalHealthcareContext(DbContextOptions<SecureDigitalHealthcareContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Student>(entity =>
        {
            entity.ToTable("Student");

            entity.Property(e => e.LastName)
                .HasMaxLength(10)
                .IsFixedLength();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.NationalId);

            entity.ToTable("User");

            entity.Property(e => e.NationalId)
                .HasMaxLength(50)
                .HasColumnName("NationalID");
            entity.Property(e => e.Address).HasColumnType("text");
            entity.Property(e => e.Birthdate).HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.Password).HasColumnType("text");
            entity.Property(e => e.Phone).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
