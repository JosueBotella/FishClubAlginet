namespace FishClubAlginet.Core.Domain.ValueObjects;

public class Address
{
    public string Street { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string FloorDoor { get; set; } = string.Empty; 
    public string ZipCode { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty; 
    public string Province { get; set; } = "Valencia"; 

    
    public override string ToString()
    {
        var baseAddress = $"{Street} {Number}";
        if (!string.IsNullOrWhiteSpace(FloorDoor))
        {
            baseAddress += $", {FloorDoor}";
        }
        return $"{baseAddress}, {ZipCode} {City} ({Province})";
    }
}
