using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace FilesMonitoringWebSite.Db.TrackerDb
{
    public partial class TrackerDb : DbContext
    {
        public virtual DbSet<Changes> Changes { get; set; }
        public virtual DbSet<ClientExceptions> ClientExceptions { get; set; }
        public virtual DbSet<Contents> Contents { get; set; }
        public virtual DbSet<Files> Files { get; set; }
        public virtual DbSet<Trackers> Trackers { get; set; }
        public virtual DbSet<Users> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Changes>(entity =>
            {
                entity.HasIndex(e => e.ContentId)
                    .IsUnique()
                    .HasFilter("([ContentId] IS NOT NULL)");

                entity.HasIndex(e => e.FileId);

                entity.Property(e => e.UserId).HasDefaultValueSql("((0))");

                entity.HasOne(d => d.Content)
                    .WithOne(p => p.Changes)
                    .HasForeignKey<Changes>(d => d.ContentId);

                entity.HasOne(d => d.File)
                    .WithMany(p => p.Changes)
                    .HasForeignKey(d => d.FileId);
            });

            modelBuilder.Entity<ClientExceptions>(entity =>
            {
                entity.HasKey(e => e.ExceptionId);

                entity.HasIndex(e => e.TrackerId);

                entity.Property(e => e.ExceptionInner)
                    .IsRequired()
                    .HasDefaultValueSql("(N'')");

                entity.HasOne(d => d.Tracker)
                    .WithMany(p => p.ClientExceptions)
                    .HasForeignKey(d => d.TrackerId);
            });

            modelBuilder.Entity<Contents>(entity =>
            {
                entity.HasKey(e => e.ContentId);
            });

            modelBuilder.Entity<Files>(entity =>
            {
                entity.HasKey(e => e.FileId);

                entity.HasIndex(e => e.TrackerId);

                entity.Property(e => e.FileName).IsRequired();

                entity.Property(e => e.FullName).IsRequired();

                entity.Property(e => e.IsNeedDelete).HasDefaultValueSql("((1))");

                entity.HasOne(d => d.Tracker)
                    .WithMany(p => p.Files)
                    .HasForeignKey(d => d.TrackerId);
            });

            modelBuilder.Entity<Trackers>(entity =>
            {
                entity.HasKey(e => e.TrackerId);

                entity.Property(e => e.TrackerId).ValueGeneratedNever();
            });

            modelBuilder.Entity<Users>(entity =>
            {
                entity.HasKey(e => e.UserId);

                entity.HasIndex(e => e.TrackerId);

                entity.Property(e => e.UserName).IsRequired();

                entity.HasOne(d => d.Tracker)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.TrackerId);
            });
        }
    }
}
