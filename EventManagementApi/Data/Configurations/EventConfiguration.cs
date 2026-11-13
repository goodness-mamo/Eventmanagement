using EventManagementApi.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventManagementApi.Data.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.HasKey(e => e.EventId);

        builder.Property(e => e.EventName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(e => e.Description)
            .HasMaxLength(2000);

        builder.Property(e => e.Location)
            .IsRequired()
            .HasMaxLength(250);

        builder.HasOne(e => e.Organiser)
            .WithMany(o => o.Events)
            .HasForeignKey(e => e.OrganiserId);
    }
}
