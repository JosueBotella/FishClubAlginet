using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FishClubAlginet.Infrastructure.Persistence.Configurations;

public class CompetitionResultConfiguration : IEntityTypeConfiguration<CompetitionResult>
{
    public void Configure(EntityTypeBuilder<CompetitionResult> builder)
    {
        builder.ToTable("CompetitionResults");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.RegistrationDate)
            .IsRequired();

        builder.Property(x => x.IsValidated)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.AssignedSpotNumber)
            .IsRequired(false);

        builder.Property(x => x.DidAttend)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.WeightInGrams)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.BiggestCatchWeight)
            .IsRequired(false);

        builder.Property(x => x.Points)
            .IsRequired()
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0);

        builder.Property(x => x.Ranking)
            .IsRequired()
            .HasDefaultValue(0);

        builder.HasOne(x => x.Competition)
            .WithMany()
            .HasForeignKey(x => x.CompetitionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Fisherman)
            .WithMany()
            .HasForeignKey(x => x.FishermanId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique: one result per fisherman per competition
        builder.HasIndex(x => new { x.CompetitionId, x.FishermanId })
            .IsUnique()
            .HasDatabaseName("IX_CompetitionResults_CompetitionId_FishermanId");

        // Unique: one spot per competition (NULLs allowed — spot assigned after draw)
        builder.HasIndex(x => new { x.CompetitionId, x.AssignedSpotNumber })
            .IsUnique()
            .HasFilter("[AssignedSpotNumber] IS NOT NULL")
            .HasDatabaseName("IX_CompetitionResults_CompetitionId_SpotNumber");
    }
}
