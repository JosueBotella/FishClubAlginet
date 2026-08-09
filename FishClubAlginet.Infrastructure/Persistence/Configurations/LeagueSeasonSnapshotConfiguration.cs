using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FishClubAlginet.Infrastructure.Persistence.Configurations;

public class LeagueSeasonSnapshotConfiguration : IEntityTypeConfiguration<LeagueSeasonSnapshot>
{
    public void Configure(EntityTypeBuilder<LeagueSeasonSnapshot> builder)
    {
        builder.ToTable("LeagueSeasonSnapshots");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.LeagueId)
            .IsRequired();

        builder.Property(x => x.Year)
            .IsRequired();

        builder.Property(x => x.ArchivedAtUtc)
            .IsRequired();

        builder.Property(x => x.SnapshotDataJson)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.HasIndex(x => x.LeagueId)
            .IsUnique()
            .HasDatabaseName("IX_LeagueSeasonSnapshots_LeagueId");

        builder.HasOne(x => x.League)
            .WithMany()
            .HasForeignKey(x => x.LeagueId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
