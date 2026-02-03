namespace FishClubAlginet.Infrastructure.Persistence.Configurations;

public class FishermanConfiguration : IEntityTypeConfiguration<Fisherman>
{
    public void Configure(EntityTypeBuilder<Fisherman> builder)
    {
        builder.ToTable("Fishermen");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FirstName)
            .IsRequired()
            .HasMaxLength(FisherManConstraints.FistNameMaxLength);

        builder.Property(x => x.LastName)
            .IsRequired()
            .HasMaxLength(FisherManConstraints.LastNameMaxLength);

        builder.Property(x => x.FederationLicense)
            .IsRequired()
            .HasMaxLength(FisherManConstraints.FederationLicenseMaxLength);

        builder.HasIndex(x => x.FederationLicense)
            .IsUnique();

        builder.Property(x => x.DocumentType)
            .HasConversion<string>() 
            .HasMaxLength(FisherManConstraints.DocumentTypeMaxLength);

        builder.Property(x => x.DocumentNumber)
            .IsRequired()
            .HasMaxLength(FisherManConstraints.DocumentNumberMaxLength);

        builder.OwnsOne(x => x.Address, addressBuilder =>
        {
            addressBuilder.Property(a => a.Street).HasMaxLength(100).HasColumnName("Address_Street");
            addressBuilder.Property(a => a.City).HasMaxLength(50).HasColumnName("Address_City");
            addressBuilder.Property(a => a.ZipCode).HasMaxLength(10).HasColumnName("Address_ZipCode");
            addressBuilder.Property(a => a.Province).HasMaxLength(50).HasColumnName("Address_Province");

            addressBuilder.Property(a => a.Street).IsRequired();
        });

        builder.Property(x => x.UserId)
            .IsRequired(false) 
            .HasMaxLength(450);
    }
}
