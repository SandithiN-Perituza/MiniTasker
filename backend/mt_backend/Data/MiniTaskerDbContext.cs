using Microsoft.EntityFrameworkCore;
using mt_backend.Models;
using System.Collections.Generic;

namespace mt_backend.Data
{
    public class MiniTaskerDbContext : DbContext
    {
        public MiniTaskerDbContext(DbContextOptions<MiniTaskerDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<TaskItem> Tasks { get; set; }

        public DbSet<Comment> Comments { get; set; }

        public DbSet<Subtask> Subtasks { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TaskItem>()
                .HasOne(t => t.AssignedUser)
                .WithMany(u => u.Tasks)
                .HasForeignKey(t => t.AssignedTo)
                .OnDelete(DeleteBehavior.SetNull);

            // Seed Users
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Name = "Alice", Email = "alice@example.com", Password = "hashed-password-1", CreatedAt = DateTime.UtcNow },
                new User { Id = 2, Name = "Bob", Email = "bob@example.com", Password = "hashed-password-1", CreatedAt = DateTime.UtcNow },
                new User { Id = 3, Name = "Anne", Email = "anne@example.com", Password = "hashed-password-1", CreatedAt = DateTime.UtcNow },
                new User { Id = 4, Name = "Peter", Email = "peter@example.com", Password = "hashed-password-1", CreatedAt = DateTime.UtcNow },
                new User { Id = 5, Name = "Jenny", Email = "jenny@example.com", Password = "hashed-password-1", CreatedAt = DateTime.UtcNow }
            );

            // TaskItem → SubTask
            modelBuilder.Entity<Subtask>()
                .HasOne(s => s.Task)
                .WithMany(t => t.Subtasks)
                .HasForeignKey(s => s.TaskId)
                .IsRequired();

            // Comment → TaskItem

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Task)
                .WithMany(t => t.Comments)
                .HasForeignKey(c => c.TaskId)
                .IsRequired();

            // Comment → User
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .IsRequired();

            // Seed Tasks
            modelBuilder.Entity<TaskItem>().HasData(
                new TaskItem
                {
                    Id = 1,
                    Title = "Sample Task",
                    Description = "First task",
                    Status = Models.TaskStatus.Pending,
                    AssignedTo = 1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new TaskItem
                {
                    Id = 2,
                    Title = "Sample Task 2",
                    Description = "Second task",
                    Status = Models.TaskStatus.InProgress,
                    AssignedTo = 2,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    DueDate = DateTime.UtcNow.AddDays(7)
                },
                new TaskItem
                {
                    Id = 3,
                    Title = "Sample Task 3",
                    Description = "Third task",
                    Status = Models.TaskStatus.Complete,
                    AssignedTo = 3,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    DueDate = DateTime.UtcNow.AddDays(7)
                },
                new TaskItem
                {
                    Id = 4,
                    Title = "Sample Task 4",
                    Description = "Fourth task",
                    Status = Models.TaskStatus.InProgress,
                    AssignedTo = 4,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    DueDate = DateTime.UtcNow.AddDays(7)
                },
                new TaskItem
                {
                    Id = 5,
                    Title = "Sample Task 5",
                    Description = "Fifth task",
                    Status = Models.TaskStatus.Pending,
                    AssignedTo = 5,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    DueDate = DateTime.UtcNow.AddDays(7)
                },
                new TaskItem
                {
                    Id = 6,
                    Title = "Sample Task 6",
                    Description = "sixth task",
                    Status = Models.TaskStatus.InProgress,
                    AssignedTo = 1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    DueDate = DateTime.UtcNow.AddDays(7)
                }
            );
        }
    }
}
