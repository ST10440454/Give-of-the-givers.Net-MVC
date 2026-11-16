using GiftOfTheGivers.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Volunteer> Volunteers => Set<Volunteer>();
        public DbSet<ReliefProject> ReliefProjects => Set<ReliefProject>();
        public DbSet<ReliefUpdate> ReliefUpdates => Set<ReliefUpdate>();
        public DbSet<VolunteerAssignment> VolunteerAssignments => Set<VolunteerAssignment>();
        public DbSet<Donation> Donations => Set<Donation>();
        public DbSet<TaxCertificate> TaxCertificates => Set<TaxCertificate>();
        public DbSet<ContactEnquiry> ContactEnquiries => Set<ContactEnquiry>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Explicit primary keys — these property names don't match EF Core's
            // "Id" / "{ClassName}Id" convention, so they must be configured manually
            // or EF throws "requires a primary key to be defined".
            builder.Entity<ReliefProject>().HasKey(p => p.ProjectId);
            builder.Entity<VolunteerAssignment>().HasKey(a => a.AssignmentId);
            builder.Entity<TaxCertificate>().HasKey(c => c.CertificateId);

            // Users.Email UNIQUE
            builder.Entity<ApplicationUser>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Volunteers -> Users (optional FK, NULL allowed)
            builder.Entity<Volunteer>()
                .HasOne(v => v.User)
                .WithOne(u => u.VolunteerProfile)
                .HasForeignKey<Volunteer>(v => v.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // ReliefProjects -> CreatedByUser (must be an Employee, enforced at controller level)
            builder.Entity<ReliefProject>()
                .HasOne(p => p.CreatedByUser)
                .WithMany()
                .HasForeignKey(p => p.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ReliefUpdates -> Project / PostedByUser
            builder.Entity<ReliefUpdate>()
                .HasOne(ru => ru.Project)
                .WithMany(p => p.Updates)
                .HasForeignKey(ru => ru.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ReliefUpdate>()
                .HasOne(ru => ru.PostedByUser)
                .WithMany()
                .HasForeignKey(ru => ru.PostedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // VolunteerAssignments junction table (many-to-many resolver)
            builder.Entity<VolunteerAssignment>()
                .HasOne(a => a.Volunteer)
                .WithMany(v => v.Assignments)
                .HasForeignKey(a => a.VolunteerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<VolunteerAssignment>()
                .HasOne(a => a.Project)
                .WithMany(p => p.VolunteerAssignments)
                .HasForeignKey(a => a.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Donations -> DonorUser (nullable, guest donations allowed)
            builder.Entity<Donation>()
                .HasOne(d => d.DonorUser)
                .WithMany(u => u.Donations)
                .HasForeignKey(d => d.DonorUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Donations -> ReliefProject (nullable = general fund)
            builder.Entity<Donation>()
                .HasOne(d => d.Project)
                .WithMany(p => p.Donations)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.SetNull);

            // TaxCertificates 1:1 with Donations, DonationId UNIQUE
            builder.Entity<TaxCertificate>()
                .HasOne(c => c.Donation)
                .WithOne(d => d.TaxCertificate)
                .HasForeignKey<TaxCertificate>(c => c.DonationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TaxCertificate>()
                .HasIndex(c => c.DonationId)
                .IsUnique();

            builder.Entity<TaxCertificate>()
                .HasIndex(c => c.CertificateNumber)
                .IsUnique();

            // Performance indexes — matches the "Optimizing Performance" section of Doc 1
            builder.Entity<Donation>()
                .HasIndex(d => d.DonorUserId);

            // Note: SQL Server's covering-index IncludeProperties() has no SQLite equivalent,
            // so this is a plain index on ProjectId rather than a covering index.
            builder.Entity<Donation>()
                .HasIndex(d => d.ProjectId);

            builder.Entity<VolunteerAssignment>()
                .HasIndex(a => a.ProjectId);

            builder.Entity<VolunteerAssignment>()
                .HasIndex(a => a.VolunteerId);
        }
    }
}