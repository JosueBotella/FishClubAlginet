using System.Collections.Concurrent;
using System.Reflection;

namespace FishClubAlginet.Infrastructure.Services;

public class DomainEventTypeResolver : IDomainEventTypeResolver
{
    private readonly ConcurrentDictionary<string, Type?> _typeCache = new();

    public DomainEventTypeResolver()
    {
        InitializeCache();
    }

    private void InitializeCache()
    {
        var domainEventInterface = typeof(IDomainEvent);

        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.FullName));

        foreach (var assembly in assemblies)
        {
            RegisterAssemblyTypes(assembly, domainEventInterface);
        }
    }

    private void RegisterAssemblyTypes(Assembly assembly, Type domainEventInterface)
    {
        try
        {
            var types = assembly.GetTypes()
                .Where(t => domainEventInterface.IsAssignableFrom(t) && !t.IsAbstract && t.IsClass);

            foreach (var type in types)
            {
                // 1. Nombre simple (ej: "FishermanAddedDomainEvent")
                _typeCache.TryAdd(type.Name, type);

                // 2. Nombre completo (ej: "FishClubAlginet.Application.Features.Events.Commands.Fishermen.FishermanAddedDomainEvent")
                if (type.FullName != null)
                {
                    _typeCache.TryAdd(type.FullName, type);
                }

                // 3. AssemblyQualifiedName
                if (type.AssemblyQualifiedName != null)
                {
                    _typeCache.TryAdd(type.AssemblyQualifiedName, type);
                }
            }
        }
        catch
        {
            // Ignorar ensamblados dinámicos o que fallen al listar tipos
        }
    }

    public Type? Resolve(string typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName))
            return null;

        if (_typeCache.TryGetValue(typeName, out var cachedType))
            return cachedType;

        // Intentar Type.GetType directo (por si viene un AssemblyQualifiedName completo no cacheado)
        var type = Type.GetType(typeName);
        if (type != null && typeof(IDomainEvent).IsAssignableFrom(type) && !type.IsAbstract)
        {
            _typeCache[typeName] = type;
            return type;
        }

        // Re-escaneo de respaldo por si se cargó un ensamblado nuevo dinámicamente
        InitializeCache();

        _typeCache.TryGetValue(typeName, out type);
        return type;
    }
}
