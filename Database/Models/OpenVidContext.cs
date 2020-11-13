using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Database.Models
{
    public partial class OpenVidContext : DbContext
    {
        public OpenVidContext()
        {
        }

        public OpenVidContext(DbContextOptions<OpenVidContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Tag> Tag { get; set; }
        public virtual DbSet<Video> Video { get; set; }
        public virtual DbSet<VideoTag> VideoTag { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Server=orion;Database=OpenVid;Trusted_Connection=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Tag>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Video>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Extension)
                    .IsRequired()
                    .HasMaxLength(4)
                    .IsUnicode(false);

                entity.Property(e => e.Length).HasColumnType("time(0)");

                entity.Property(e => e.Md5)
                    .IsRequired()
                    .HasColumnName("MD5")
                    .HasMaxLength(32)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(256)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<VideoTag>(entity =>
            {
                entity.HasKey(e => new { e.VideoId, e.TagId });

                entity.Property(e => e.VideoId).HasColumnName("VideoID");

                entity.Property(e => e.TagId).HasColumnName("TagID");

                entity.HasOne(d => d.Tag)
                    .WithMany(p => p.VideoTag)
                    .HasForeignKey(d => d.TagId)
                    .HasConstraintName("FK_VideoTag_TagID");

                entity.HasOne(d => d.Video)
                    .WithMany(p => p.VideoTag)
                    .HasForeignKey(d => d.VideoId)
                    .HasConstraintName("FK_VideoTag_VideoID");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
