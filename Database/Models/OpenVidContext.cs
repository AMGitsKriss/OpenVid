using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

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

        public virtual DbSet<Ratings> Ratings { get; set; }
        public virtual DbSet<Tag> Tag { get; set; }
        public virtual DbSet<Video> Video { get; set; }
        public virtual DbSet<VideoEncodeQueue> VideoEncodeQueue { get; set; }
        public virtual DbSet<VideoSource> VideoSource { get; set; }
        public virtual DbSet<VideoTag> VideoTag { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("name=ConnectionStrings:DefaultDatabase");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Ratings>(entity =>
            {
                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(256)
                    .IsUnicode(false);
            });

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

                entity.Property(e => e.Description)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.Length).HasColumnType("time(0)");

                entity.Property(e => e.MetaText).IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(256)
                    .IsUnicode(false);

                entity.Property(e => e.RatingId).HasColumnName("RatingID");

                entity.HasOne(d => d.Rating)
                    .WithMany(p => p.Video)
                    .HasForeignKey(d => d.RatingId)
                    .HasConstraintName("FK__Video__RatingID__5CD6CB2B");
            });

            modelBuilder.Entity<VideoEncodeQueue>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Encoder)
                    .IsRequired()
                    .HasMaxLength(16)
                    .IsUnicode(false);

                entity.Property(e => e.Format)
                    .IsRequired()
                    .HasMaxLength(8)
                    .IsUnicode(false);

                entity.Property(e => e.InputDirectory)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.OutputDirectory)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.RenderSpeed)
                    .IsRequired()
                    .HasMaxLength(16)
                    .IsUnicode(false);

                entity.Property(e => e.VideoId).HasColumnName("VideoID");

                entity.HasOne(d => d.Video)
                    .WithMany(p => p.VideoEncodeQueue)
                    .HasForeignKey(d => d.VideoId)
                    .HasConstraintName("FK_VideoEncodeQueue_Video");
            });

            modelBuilder.Entity<VideoSource>(entity =>
            {
                entity.HasIndex(e => e.Md5)
                    .HasName("IX_VideoSource_Unique")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Extension)
                    .IsRequired()
                    .HasMaxLength(4)
                    .IsUnicode(false);

                entity.Property(e => e.Md5)
                    .IsRequired()
                    .HasColumnName("MD5")
                    .HasMaxLength(32)
                    .IsUnicode(false);

                entity.Property(e => e.VideoId).HasColumnName("VideoID");

                entity.HasOne(d => d.Video)
                    .WithMany(p => p.VideoSource)
                    .HasForeignKey(d => d.VideoId)
                    .HasConstraintName("FK_VideoSource_Video");
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
