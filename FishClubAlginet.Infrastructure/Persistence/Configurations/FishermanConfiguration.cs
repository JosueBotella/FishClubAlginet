namespace FishClubAlginet.Infrastructure.Persistence.Configurations;

public class FishermanConfiguration : IEntityTypeConfiguration<Fisherman>
{
    public void Configure(EntityTypeBuilder<Fisherman> builder)
    {
        // 1. Table Name
        builder.ToTable("Fishermen");

        // 2. Primary Key
        builder.HasKey(x => x.Id);

        // 3. Properties Configuration
        builder.Property(x => x.FirstName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.FederationLicense)
            .IsRequired()
            .HasMaxLength(20);

        // Index on Federation License (Must be unique usually?)
        builder.HasIndex(x => x.FederationLicense)
            .IsUnique();

        // 4. Enum Conversion (Save as Int or String?)
        // Recommended: String for readability in DB, Int for performance. Let's use string here for clarity if you prefer, or default int.
        builder.Property(x => x.DocumentType)
            .HasConversion<string>() // Saves as "Dni", "Nie" in DB
            .HasMaxLength(10);

        builder.Property(x => x.DocumentNumber)
            .IsRequired()
            .HasMaxLength(20);

        // 5. VALUE OBJECT MAPPING (Address) !!!
        // This is the specific part for Owned Types
        builder.OwnsOne(x => x.Address, addressBuilder =>
        {
            addressBuilder.Property(a => a.Street).HasMaxLength(100).HasColumnName("Address_Street");
            addressBuilder.Property(a => a.City).HasMaxLength(50).HasColumnName("Address_City");
            addressBuilder.Property(a => a.ZipCode).HasMaxLength(10).HasColumnName("Address_ZipCode");
            addressBuilder.Property(a => a.Province).HasMaxLength(50).HasColumnName("Address_Province");

            // If Address implies required fields inside DB:
            addressBuilder.Property(a => a.Street).IsRequired();
        });

        // 6. Relationship with Identity User (Optional)
        builder.Property(x => x.UserId)
            .IsRequired(false) // Nullable
            .HasMaxLength(450); // Standard Identity ID length
    }
}
