using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FishClubAlginet.Infrastructure.Persistence.Configurations;

public class LeagueConfiguration : IEntityTypeConfiguration<League>
{
    public void Configure(EntityTypeBuilder<League> builder)
    {
        builder.ToTable("Leagues");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(LeagueConstraints.NameMaxLength);

        builder.Property(x => x.Year)
            .IsRequired();

        builder.HasIndex(x => x.Year)
            .IsUnique()
            .HasDatabaseName("IX_Leagues_Year");

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.IsArchived)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.MinPoints)
            .IsRequired()
            .HasDefaultValue(5);

        builder.Property(x => x.WorstResultsToDiscard)
            .IsRequired()
            .HasDefaultValue(0);

        builder.HasMany(x => x.Competitions)
            .WithOne(c => c.League)
            .HasForeignKey(c => c.LeagueId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
