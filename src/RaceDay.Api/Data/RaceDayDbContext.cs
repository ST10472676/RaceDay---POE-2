using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;
using RaceDayRoute = RaceDay.Api.Models.Route;

namespace RaceDay.Api.Data;

public class RaceDayDbContext : DbContext
{
    public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<RaceDayRoute> Routes => Set<RaceDayRoute>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Enrolment> Enrolments => Set<Enrolment>();
    public DbSet<Result> Results => Set<Result>();
    public DbSet<EventWeather> EventWeather => Set<EventWeather>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(x => x.UserID);
            entity.Property(x => x.FirstName).HasMaxLength(50).IsRequired();
            entity.Property(x => x.LastName).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(x => x.PasswordHash).HasMaxLength(255).IsRequired();
            entity.Property(x => x.PhoneNumber).HasMaxLength(20);
            entity.Property(x => x.Role).HasMaxLength(20).IsRequired();
            entity.Property(x => x.DateCreated).HasDefaultValueSql("GETDATE()");
            entity.HasCheckConstraint("CK_Users_Role", "[Role] IN ('Organiser','Participant')");
        });

        modelBuilder.Entity<RaceDayRoute>(entity =>
        {
            entity.ToTable("Routes");
            entity.HasKey(x => x.RouteID);
            entity.Property(x => x.StartLocation).HasMaxLength(100).IsRequired();
            entity.Property(x => x.EndLocation).HasMaxLength(100).IsRequired();
            entity.Property(x => x.RouteDirection).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Distance).HasPrecision(6, 2).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(500);
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.ToTable("Events");
            entity.HasKey(x => x.EventID);
            entity.Property(x => x.EventName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.Property(x => x.EventDate).HasColumnType("date").IsRequired();
            entity.Property(x => x.Location).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Distance).HasPrecision(6, 2).IsRequired();
            entity.Property(x => x.EventType).HasMaxLength(20).IsRequired();
            entity.Property(x => x.BannerImageUrl).HasMaxLength(500);
            entity.HasCheckConstraint("CK_Events_EventType", "[EventType] IN ('Run','Walk','Cycle')");
            entity.HasOne(x => x.Organiser)
                .WithMany(x => x.OrganisedEvents)
                .HasForeignKey(x => x.OrganiserID)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Route)
                .WithMany(x => x.Events)
                .HasForeignKey(x => x.RouteID)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Categories");
            entity.HasKey(x => x.CategoryID);
            entity.Property(x => x.CategoryName).HasMaxLength(100).IsRequired();
            entity.HasCheckConstraint("CK_Categories_Age", "[MaxAge] >= [MinAge]");
            entity.HasOne(x => x.Event)
                .WithMany(x => x.Categories)
                .HasForeignKey(x => x.EventID)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Enrolment>(entity =>
        {
            entity.ToTable("Enrolments");
            entity.HasKey(x => x.EnrolmentID);
            entity.Property(x => x.EnrolmentDate).HasDefaultValueSql("GETDATE()");
            entity.Property(x => x.Status).HasMaxLength(30).HasDefaultValue("Pending").IsRequired();
            entity.HasCheckConstraint("CK_Enrolments_Status", "[Status] IN ('Pending','Confirmed','Cancelled')");
            entity.HasOne(x => x.Participant)
                .WithMany(x => x.Enrolments)
                .HasForeignKey(x => x.ParticipantID)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Event)
                .WithMany(x => x.Enrolments)
                .HasForeignKey(x => x.EventID)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Category)
                .WithMany(x => x.Enrolments)
                .HasForeignKey(x => x.CategoryID)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Result>(entity =>
        {
            entity.ToTable("Results");
            entity.HasKey(x => x.ResultID);
            entity.HasIndex(x => x.EnrolmentID).IsUnique();
            entity.Property(x => x.FinishTime).HasColumnType("time");
            entity.Property(x => x.ResultStatus).HasMaxLength(30).HasDefaultValue("Completed").IsRequired();
            entity.HasCheckConstraint("CK_Results_Position", "[FinishingPosition] IS NULL OR [FinishingPosition] > 0");
            entity.HasOne(x => x.Enrolment)
                .WithOne(x => x.Result)
                .HasForeignKey<Result>(x => x.EnrolmentID)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<EventWeather>(entity =>
        {
            entity.ToTable("EventWeather");
            entity.HasKey(x => x.WeatherID);
            entity.Property(x => x.WeatherDate).HasColumnType("date").IsRequired();
            entity.Property(x => x.Temperature).HasPrecision(4, 1);
            entity.Property(x => x.WeatherCondition).HasMaxLength(100);
            entity.HasCheckConstraint("CK_EventWeather_Humidity", "[Humidity] IS NULL OR [Humidity] BETWEEN 0 AND 100");
            entity.HasOne(x => x.Event)
                .WithMany(x => x.WeatherRecords)
                .HasForeignKey(x => x.EventID)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
