namespace FishClubAlginet.Infrastructure.Persistence.Configurations;

public class FishermanConfiguration : IEntityTypeConfiguration<Fisherman>
{
    public void Configure(EntityTypeBuilder<Fisherman> builder)
    {
        builder.ToTable("Fishermen");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FirstName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.FederationLicense)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(x => x.FederationLicense)
            .IsUnique();

        // Recommended: String for readability in DB, Int for performance. Let's use string here for clarity if you prefer, or default int.
        builder.Property(x => x.DocumentType)
            .HasConversion<string>() // Saves as "Dni", "Nie" in DB
            .HasMaxLength(10);

        builder.Property(x => x.DocumentNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.OwnsOne(x => x.Address, addressBuilder =>
        {
            addressBuilder.Property(a => a.Street).HasMaxLength(100).HasColumnName("Address_Street");
            addressBuilder.Property(a => a.City).HasMaxLength(50).HasColumnName("Address_City");
            addressBuilder.Property(a => a.ZipCode).HasMaxLength(10).HasColumnName("Address_ZipCode");
            addressBuilder.Property(a => a.Province).HasMaxLength(50).HasColumnName("Address_Province");

            addressBuilder.Property(a => a.Street).IsRequired();
        });

        builder.Property(x => x.UserId)
            .IsRequired(false) // Nullable
            .HasMaxLength(450); // Standard Identity ID length
    }
}
