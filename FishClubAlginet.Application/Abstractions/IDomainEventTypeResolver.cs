namespace FishClubAlginet.Application.Abstractions;

public interface IDomainEventTypeResolver
{
    Type? Resolve(string typeName);
}
