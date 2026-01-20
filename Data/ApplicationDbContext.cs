using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SosyalAjandam.Models;

namespace SosyalAjandam.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<TodoItem> TodoItems { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<GroupMember> GroupMembers { get; set; }
    public DbSet<LifeItem> LifeItems { get; set; }
    public DbSet<UserBadge> UserBadges { get; set; }
    public DbSet<VisionBoardItem> VisionBoardItems { get; set; }
    public DbSet<GroupJoinRequest> GroupJoinRequests { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);

        // TodoItem - User Relationship
        builder.Entity<TodoItem>()
            .HasOne(t => t.Owner)
            .WithMany(u => u.TodoItems)
            .HasForeignKey(t => t.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);

        // TodoItem - Group Relationship
        builder.Entity<TodoItem>()
            .HasOne(t => t.Group)
            .WithMany(g => g.SharedTasks)
            .HasForeignKey(t => t.GroupId)
            .OnDelete(DeleteBehavior.SetNull);

        // Group - Leader Relationship
        builder.Entity<Group>()
            .HasOne(g => g.Leader)
            .WithMany()
            .HasForeignKey(g => g.LeaderId)
            .OnDelete(DeleteBehavior.Restrict);

        // GroupMember Relationships
        builder.Entity<GroupMember>()
            .HasOne(gm => gm.Group)
            .WithMany(g => g.Members)
            .HasForeignKey(gm => gm.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<GroupMember>()
            .HasOne(gm => gm.User)
            .WithMany(u => u.GroupMemberships)
            .HasForeignKey(gm => gm.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // GroupJoinRequest Relationships
        builder.Entity<GroupJoinRequest>()
            .HasOne(gjr => gjr.Group)
            .WithMany(g => g.JoinRequests)
            .HasForeignKey(gjr => gjr.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<GroupJoinRequest>()
            .HasOne(gjr => gjr.User)
            .WithMany()
            .HasForeignKey(gjr => gjr.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
