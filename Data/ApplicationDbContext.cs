using Microsoft.EntityFrameworkCore;
using ASCEND.Models;

namespace ASCEND.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Habit> Habits { get; set; } = null!;
    public DbSet<HabitLog> HabitLogs { get; set; } = null!;
    public DbSet<Achievement> Achievements { get; set; } = null!;
    public DbSet<UserAchievement> UserAchievements { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Username).IsUnique();
            entity.HasIndex(u => u.Email).IsUnique();
        });

        // Configure Habit relationships
        modelBuilder.Entity<Habit>()
            .HasOne(h => h.User)
            .WithMany(u => u.Habits)
            .HasForeignKey(h => h.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure HabitLog relationships
        modelBuilder.Entity<HabitLog>()
            .HasOne(hl => hl.Habit)
            .WithMany(h => h.HabitLogs)
            .HasForeignKey(hl => hl.HabitId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure UserAchievement relationships
        modelBuilder.Entity<UserAchievement>()
            .HasOne(ua => ua.User)
            .WithMany(u => u.UserAchievements)
            .HasForeignKey(ua => ua.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserAchievement>()
            .HasOne(ua => ua.Achievement)
            .WithMany(a => a.UserAchievements)
            .HasForeignKey(ua => ua.AchievementId)
            .OnDelete(DeleteBehavior.Cascade);

        // Seed Achievements
        modelBuilder.Entity<Achievement>().HasData(
            new Achievement
            {
                AchievementId = 1,
                Title = "Early Riser",
                Description = "Complete a habit quest before 8:00 AM.",
                RequiredValue = 1,
                Type = "EarlyRiser"
            },
            new Achievement
            {
                AchievementId = 2,
                Title = "Week Warrior",
                Description = "Achieve a 7-day habit streak.",
                RequiredValue = 7,
                Type = "Streak"
            },
            new Achievement
            {
                AchievementId = 3,
                Title = "Discipline Master",
                Description = "Achieve a 30-day habit streak.",
                RequiredValue = 30,
                Type = "Streak"
            },
            new Achievement
            {
                AchievementId = 4,
                Title = "Unstoppable",
                Description = "Achieve a 50-day habit streak.",
                RequiredValue = 50,
                Type = "Streak"
            },
            new Achievement
            {
                AchievementId = 5,
                Title = "The Scholar",
                Description = "Complete 100 study sessions.",
                RequiredValue = 100,
                Type = "CategoryStudy"
            },
            new Achievement
            {
                AchievementId = 6,
                Title = "Iron Body",
                Description = "Complete 100 exercise sessions.",
                RequiredValue = 100,
                Type = "CategoryExercise"
            }
        );
    }
}
