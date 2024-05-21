using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SecureDigitalHealthcare.Models;

public partial class EasyHealthContext : DbContext
{
    public EasyHealthContext()
    {
    }

    public EasyHealthContext(DbContextOptions<EasyHealthContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Appointment> Appointments { get; set; }

    public virtual DbSet<Availability> Availabilities { get; set; }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<Doctor> Doctors { get; set; }

    public virtual DbSet<ForgetPasswordToken> ForgetPasswordTokens { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<RoomCall> RoomCalls { get; set; }

    public virtual DbSet<Speciality> Specialities { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => new { e.PatientId, e.DoctorId });

            entity.ToTable("Appointment");

            entity.Property(e => e.PatientId).HasDefaultValue(-1);
            entity.Property(e => e.DoctorId).HasDefaultValue(-1);
            entity.Property(e => e.Disease).HasColumnType("text");
            entity.Property(e => e.DoctorDescription).HasColumnType("text");
            entity.Property(e => e.RoomCallId).HasMaxLength(100);
            entity.Property(e => e.Symptom).HasColumnType("text");

            entity.HasOne(d => d.Availability).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.AvailabilityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Appointment_Availability");

            entity.HasOne(d => d.Doctor).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.DoctorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Appointment_Doctor");

            entity.HasOne(d => d.Patient).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Appointment_Patient");

            entity.HasOne(d => d.RoomCall).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.RoomCallId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Appointment_RoomCall");
        });

        modelBuilder.Entity<Availability>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Availability_1");

            entity.ToTable("Availability");

            entity.Property(e => e.EndTime).HasColumnType("datetime");
            entity.Property(e => e.StartTime).HasColumnType("datetime");

            entity.HasOne(d => d.Doctor).WithMany(p => p.Availabilities)
                .HasForeignKey(d => d.DoctorId)
                .HasConstraintName("FK_Availability_Doctor");
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.ToTable("Comment");

            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.ReceiverId).HasDefaultValue(0);
            entity.Property(e => e.SenderId).HasDefaultValue(0);
            entity.Property(e => e.Text).HasColumnType("text");

            entity.HasOne(d => d.Receiver).WithMany(p => p.CommentReceivers)
                .HasForeignKey(d => d.ReceiverId)
                .HasConstraintName("FK_CommentReceiver_User");

            entity.HasOne(d => d.ReplyToNavigation).WithMany(p => p.InverseReplyToNavigation)
                .HasForeignKey(d => d.ReplyTo)
                .HasConstraintName("FK_CommentReplyTo_Comment");

            entity.HasOne(d => d.Sender).WithMany(p => p.CommentSenders)
                .HasForeignKey(d => d.SenderId)
                .HasConstraintName("FK_CommentSender_User");
        });

        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.ToTable("Doctor");

            entity.Property(e => e.Id).HasDefaultValue(-1);

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.Doctor)
                .HasForeignKey<Doctor>(d => d.Id)
                .HasConstraintName("FK_Doctor_User");

            entity.HasOne(d => d.Speciality).WithMany(p => p.Doctors)
                .HasForeignKey(d => d.SpecialityId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Doctor_Speciality");
        });

        modelBuilder.Entity<ForgetPasswordToken>(entity =>
        {
            entity.ToTable("ForgetPasswordToken");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.ExpirationDate).HasColumnType("datetime");
            entity.Property(e => e.Token).HasMaxLength(128);

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.ForgetPasswordToken)
                .HasForeignKey<ForgetPasswordToken>(d => d.Id)
                .HasConstraintName("FK_ForgetPasswordToken_User");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Role");

            entity.Property(e => e.Name).HasMaxLength(10);
        });

        modelBuilder.Entity<RoomCall>(entity =>
        {
            entity.ToTable("RoomCall");

            entity.Property(e => e.Id).HasMaxLength(100);
        });

        modelBuilder.Entity<Speciality>(entity =>
        {
            entity.ToTable("Speciality");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");

            entity.Property(e => e.Address).HasMaxLength(50);
            entity.Property(e => e.BirthDate).HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.PhoneNumber).HasMaxLength(50);
            entity.Property(e => e.ProfileImagePath).HasMaxLength(50);
            entity.Property(e => e.RegistrationDate).HasColumnType("datetime");
            entity.Property(e => e.RoleId).HasDefaultValue(0);

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_User_Role");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
