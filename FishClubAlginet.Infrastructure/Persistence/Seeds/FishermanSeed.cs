using FishClubAlginet.Core.Domain.Entities;
using FishClubAlginet.Core.Domain.ValueObjects;
using FishClubAlginet.Contracts.Enums;

namespace FishClubAlginet.Infrastructure.Persistence.Seeds;

public static class FishermanSeed
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Set<Fisherman>().AnyAsync())
        {
            return;
        }

        var fishermen = GenerateFishermen();
        await context.Set<Fisherman>().AddRangeAsync(fishermen);
        await context.SaveChangesAsync();
    }

    private static List<Fisherman> GenerateFishermen()
    {
        var fishermen = new List<Fisherman>();
        var firstNames = new[]
        {
            "Juan", "Carlos", "Miguel", "José", "Francisco", "Antonio", "Manuel", "Pablo", "Luis", "Jorge",
            "Rafael", "Ricardo", "Roberto", "Raúl", "Sergio", "Salvador", "Santiago", "Tomás", "Valentín", "Vicente",
            "Víctor", "Emilio", "Enrique", "Eugenio", "Esteban", "Eduardo", "Ernesto", "Elier", "Elías", "Eliseo"
        };

        var lastNames = new[]
        {
            "García", "Rodríguez", "Martínez", "Hernández", "López", "González", "Pérez", "Sánchez", "Ramírez", "Torres",
            "Flores", "Rivera", "Moreno", "Gutiérrez", "Ortiz", "Jiménez", "Díaz", "Castillo", "Navarro", "Vargas"
        };

        var cities = new[]
        {
            "Madrid", "Barcelona", "Valencia", "Bilbao", "Alicante", "Málaga", "Murcia", "Zaragoza", "Palma", "Las Palmas",
            "Sevilla", "Granada", "Córdoba", "Valladolid", "Toledo", "Salamanca", "León", "Ávila", "Burgos", "Cuenca"
        };

        var provinces = new[]
        {
            "Madrid", "Barcelona", "Valencia", "Vizcaya", "Alicante", "Málaga", "Murcia", "Zaragoza", "Baleares", "Canarias",
            "Sevilla", "Granada", "Córdoba", "Valladolid", "Toledo", "Salamanca", "León", "Ávila", "Burgos", "Cuenca"
        };

        var documentTypes = new[] 
        { 
            TypeNationalIdentifier.Dni, 
            TypeNationalIdentifier.Nie, 
            TypeNationalIdentifier.Passport 
        };

        var random = new Random(42);

        for (int i = 1; i <= 50; i++)
        {
            var firstName = firstNames[random.Next(firstNames.Length)];
            var lastName = lastNames[random.Next(lastNames.Length)];
            var cityIndex = random.Next(cities.Length);
            var city = cities[cityIndex];
            var province = provinces[cityIndex];
            var documentType = documentTypes[random.Next(documentTypes.Length)];

            var birthDate = GenerateRandomBirthDate(random);
            var documentNumber = GenerateDocumentNumber(documentType, random, i);
            var federationLicense = $"FED{i:D5}";
            var zipCode = $"{random.Next(10000, 52000):D5}";

            var fisherman = new Fisherman
            {
                Id = 0,
                FirstName = firstName,
                LastName = lastName,
                DateOfBirth = birthDate,
                DocumentType = documentType,
                DocumentNumber = documentNumber,
                FederationLicense = federationLicense,
                Address = new Address
                {
                    Street = $"Calle {lastName} {i}",
                    City = city,
                    ZipCode = zipCode,
                    Province = province
                }
            };

            fishermen.Add(fisherman);
        }

        return fishermen;
    }

    private static DateTime GenerateRandomBirthDate(Random random)
    {
        int year = random.Next(1960, 2006);
        int month = random.Next(1, 13);
        int day = random.Next(1, DateTime.DaysInMonth(year, month) + 1);

        return new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);
    }

    private static string GenerateDocumentNumber(TypeNationalIdentifier type, Random random, int index) =>
        type switch
        {
            TypeNationalIdentifier.Dni => $"{random.Next(10000000, 99999999)}{GetDniLetter(random)}",
            TypeNationalIdentifier.Nie => $"X{random.Next(1000000, 9999999)}{GetDniLetter(random)}",
            TypeNationalIdentifier.Passport => $"ESP{index:D7}",
            _ => $"{index:D8}"
        };

    private static char GetDniLetter(Random random)
    {
        const string letters = "TRWAGMYFPDXBNJZSQVHLCKE";
        return letters[random.Next(letters.Length)];
    }
}
