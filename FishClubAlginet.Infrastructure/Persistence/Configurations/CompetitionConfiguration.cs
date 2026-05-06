using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FishClubAlginet.Infrastructure.Persistence.Configurations;

public class CompetitionConfiguration : IEntityTypeConfiguration<Competition>
{
    public void Configure(EntityTypeBuilder<Competition> builder)
    {
        builder.ToTable("Competitions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CompetitionNumber)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(200);

        builder.Property(x => x.Venue)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Zone)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Subspecialty)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Category)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.MaxSpots)
            .IsRequired();

        builder.Property(x => x.ParticipantCount)
            .IsRequired();

        builder.HasOne(x => x.League)
            .WithMany(l => l.Competitions)
            .HasForeignKey(x => x.LeagueId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
