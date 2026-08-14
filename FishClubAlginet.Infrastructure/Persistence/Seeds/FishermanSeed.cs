namespace FishClubAlginet.Infrastructure.Persistence.Seeds;

[SuppressMessage("Security", "S2245:Using weak random number generators", Justification = "Deterministic seed generation for local development and test database seeding only")]
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
        var realNames = new[]
        {
            "LORENZO MONGE ALVAREZ", "ANTONIO MARTINEZ MEDRANO", "MIGUEL JUAN MARTINEZ", "CRISTIAN VOINESCU",
            "JUAN ALCARAZ VIUDEZ", "JUAN MANUEL GARCIA ALOS", "PEDRO VIUDES SOLER", "EMILIO GALAN DIRANZO",
            "ANTONIO RUBIO RUIZ", "FRANCISCO JAVIER MINGUEZ LOPEZ", "DAVID ROIG FERNANDEZ", "JOSE SALVADOR GUIJARRO MACHI",
            "SALVADOR CASTELLOTE BAIXAULI", "JOSE VICENTE PALAU REGAL", "JUAN VICENTE GUARDIOLA RIVES", "VICENTE QUIJAL LOPEZ",
            "SALVADOR CASTELLOTE RUEDA", "SALVADOR BAYARRI CATALA", "STAVAR NAVALICI", "DAVID SOLER SATORRES",
            "MIGUEL ESPERT JUAN", "VIRGIL COTMEANA", "ANDRES SOLER SATORRES", "JUAN RAFAEL HERNAIZ RUESCAS",
            "PEDRO JOSE GARCIA PALLAS", "JOSE GABRIEL BOTELLA DIRANZO", "SERGIO VALCACER LOPEZ", "LAURENTIU ILIE",
            "VICENTE ALIAGA REGAL", "MARIUS LAURENTIU DUMITRIU", "JORGE FERRANDO CANET", "SALVADOR NAVARRO BELMONT",
            "BERNARDO SOLER ESCUTIA", "JUAN BAUTISTA BOSCH SOLER", "IVAN MARTINEZ VELLÓ", "CALIXTO DALMAU GALLART",
            "FERNADO MARTINEZ SANCHEZ", "TEODORO FERNANDEZ SERRANO", "JOSE ORTA GADEA", "JOSE MIGUEL GALIAN VILLANUEVA",
            "ISAIAS DEL RIO AGUILAR", "SALVADOR GOMARIS ROSELLÓ", "CASIMIRO GIL CHOLVI", "FRANCISCO VIDAL CALABUIG",
            "JOSE ANTONIO CAÑAMERO JIMENEZ", "JUAN BAUTISTA BOSCH LOPEZ", "JUAN GARRIDO PARRA", "JUAN VICENTE ROIG SANCHIS",
            "PEDRO ESCRIBANO MORALES", "RUBEN BOSCH ESTEVE", "ALEJANDRO VICENTE PIQUERAS", "ALEXANDRE PAVIA TEN"
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

        for (int i = 0; i < realNames.Length; i++)
        {
            var fullName = realNames[i];
            var nameParts = fullName.Split(' ', 2);
            var firstName = nameParts.Length > 0 ? nameParts[0] : "Unknown";
            var lastName = nameParts.Length > 1 ? nameParts[1] : "Unknown";

            var cityIndex = random.Next(cities.Length);
            var city = cities[cityIndex];
            var province = provinces[cityIndex];
            var documentType = documentTypes[random.Next(documentTypes.Length)];

            var birthDate = GenerateRandomBirthDate(random);
            var documentNumber = GenerateDocumentNumber(documentType, random, i + 1);
            var federationLicense = $"FED{(i + 1):D5}";
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
                    Street = $"Calle {lastName} {i + 1}",
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
