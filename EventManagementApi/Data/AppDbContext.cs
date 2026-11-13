
using Microsoft.EntityFrameworkCore;
using EventManagementApi.Models.Entities;

namespace EventManagementApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Organiser> Organisers => Set<Organiser>();
    public DbSet<Participant> Participants => Set<Participant>();
    public DbSet<Categories> Categories => Set<Categories>();
    public DbSet<EventCategory> EventCategories => Set<EventCategory>();
    public DbSet<Enrolments> Enrolments => Set<Enrolments>();
    public DbSet<Result> Results => Set<Result>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users", "dbo");
            entity.HasKey(u => u.UserId);

            entity.Property(u => u.UserId).HasColumnName("UserID");
        });

        modelBuilder.Entity<Categories>()
            .HasKey(c => c.CategoryId);

        modelBuilder.Entity<Enrolments>()
            .HasKey(e => e.EnrolmentId);

        modelBuilder.Entity<EventCategory>()
            .HasKey(ec => new { ec.EventId, ec.CategoryId });

        modelBuilder.Entity<Organiser>()
            .HasOne(o => o.User)
            .WithOne()
            .HasForeignKey<Organiser>(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Participant>()
            .HasOne(p => p.User)
            .WithOne()
            .HasForeignKey<Participant>(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Enrolments>()
            .HasOne(e => e.EventCategory)
            .WithMany(ec => ec.Enrolments)
            .HasForeignKey(e => new { e.EventId, e.CategoryId })
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Enrolments>()
            .HasOne(e => e.Participant)
            .WithMany(p => p.Enrolments)
            .HasForeignKey(e => e.ParticipantId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Result>()
            .HasOne(r => r.Enrolment)
            .WithMany()
            .HasForeignKey(r => r.EnrolmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
