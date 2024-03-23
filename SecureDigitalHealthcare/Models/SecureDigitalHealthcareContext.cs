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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Student>(entity =>
        {
            entity.ToTable("Student");

            entity.Property(e => e.LastName)
                .HasMaxLength(10)
                .IsFixedLength();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
