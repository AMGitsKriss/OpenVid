using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;

namespace Database.Models
{
    public partial class OpenVidContext : DbContext
    {
        private IConfiguration _configuration;
        public OpenVidContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public OpenVidContext(DbContextOptions<OpenVidContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Ratings> Ratings { get; set; }
        public virtual DbSet<Tag> Tag { get; set; }
        public virtual DbSet<Video> Video { get; set; }
        public virtual DbSet<VideoTag> VideoTag { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(_configuration["Database:ConnectionString"]);
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
                entity.HasIndex(e => e.Md5)
                    .HasName("IX_Video_Unique_MD5")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Description)
                    .HasMaxLength(500)
                    .IsUnicode(false);

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

                entity.Property(e => e.MetaText).IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(256)
                    .IsUnicode(false);

                entity.HasOne(d => d.Rating)
                    .WithMany(p => p.Video)
                    .HasForeignKey(d => d.RatingId)
                    .HasConstraintName("FK__Video__Rating__49C3F6B7");
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
