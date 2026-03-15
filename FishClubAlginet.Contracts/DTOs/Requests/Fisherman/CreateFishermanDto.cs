using System.ComponentModel.DataAnnotations;

namespace FishClubAlginet.Contracts.Dtos.Requests.Fisherman;

public class CreateFishermanDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(50, ErrorMessage = "El nombre no puede superar los 50 caracteres.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los apellidos son obligatorios.")]
    [MaxLength(50, ErrorMessage = "Los apellidos no pueden superar los 50 caracteres.")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
    public DateTime DateOfBirth { get; set; }

    [Required(ErrorMessage = "El tipo de documento es obligatorio.")]
    public TypeNationalIdentifier DocumentType { get; set; } = TypeNationalIdentifier.Dni;

    [Required(ErrorMessage = "El numero de documento es obligatorio.")]
    [MinLength(10, ErrorMessage = "El numero de documento debe tener al menos 10 caracteres.")]
    [MaxLength(20, ErrorMessage = "El numero de documento no puede superar los 20 caracteres.")]
    [RegularExpression(@"^[A-Za-z0-9\-\s]+$", ErrorMessage = "El numero de documento solo puede contener letras, numeros, guiones y espacios.")]
    public string DocumentNumber { get; set; } = string.Empty;

    [MaxLength(20, ErrorMessage = "La licencia federativa no puede superar los 20 caracteres.")]
    public string? FederationLicense { get; set; }

    public string? AddressStreet { get; set; }

    public string? AddressCity { get; set; }

    public string? AddressZipCode { get; set; }

    public string? AddressProvince { get; set; }
}
