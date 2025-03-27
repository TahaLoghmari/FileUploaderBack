using FileUploaderBack.Models;
using Microsoft.EntityFrameworkCore;

namespace FileUploaderBack.Data
{
    public class FileUploaderDbContext : DbContext
    {
        public FileUploaderDbContext(DbContextOptions<FileUploaderDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Folder> Folders { get; set; }
        public DbSet<Filee> Files { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Id)
                      .ValueGeneratedOnAdd();
                entity.Property(u => u.Username)
                      .IsRequired()
                      .HasMaxLength(50);
                entity.Property(u => u.Password)
                      .IsRequired()
                      .HasMaxLength(100);
                entity.HasIndex(u => u.Username)
                      .IsUnique();
            });

            modelBuilder.Entity<Folder>(entity =>
            {
                entity.HasKey(f => f.Id);
                entity.Property(f => f.Id)
                      .ValueGeneratedOnAdd();
                entity.Property(f => f.Name)
                      .IsRequired();
                entity.Property(f => f.CreatedAt)
                      .IsRequired()
                      .HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Relationship with User:
                // Assuming User contains a navigation property ICollection<Folder> Folders.
                entity.HasOne(f => f.User) // a folder has one User 
                      .WithMany(u => u.Folders) // a User has one to many Folders
                      .HasForeignKey(f => f.UserId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Cascade);

                // Self-referencing relationship for ParentFolder-ChildFolders:
                // Assuming Folder contains ParentFolder (nullable) and ChildFolders.
                entity.HasOne(f => f.ParentFolder)
                      .WithMany(f => f.ChildFolders)
                      .HasForeignKey(f => f.ParentFolderId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Filee>(entity =>
            {
                entity.HasKey(f => f.Id);
                entity.Property(f => f.Id)
                      .ValueGeneratedOnAdd();
                entity.Property(f => f.Name)
                      .IsRequired();
                entity.Property(f => f.Size)
                      .IsRequired();
                entity.Property(f => f.CreatedAt)
                      .IsRequired()
                      .HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Relationship with Folder:
                // Assuming Folder contains a navigation property ICollection<Filee> Files.
                entity.HasOne(f => f.Folder)
                      .WithMany(folder => folder.Files)
                      .HasForeignKey(f => f.FolderId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}