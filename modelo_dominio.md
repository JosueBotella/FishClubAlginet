# Definición de Entidades y Relaciones

## Entidades Existentes
- **Fisherman:** Representa al socio del club.

## Nuevas Entidades a Implementar
- **League (Liga):**
  - Propiedades: Id, Name (ej: "Liga 2026"), Year, IsActive.
  - Relación: 1 a N con Competitions.

- **Competition (Competición):**
  - Propiedades: Id, Name, Date, StartTime, EndTime, Location, MaxSpots (Número de pesqueras).
  - Relación: Pertenece a una League.

- **FishingSpot (Pesquera/Puesto):**
  - Propiedades: Id, Number (ej: Puesto 1), IsOccupied.
  - Relación: Pertenece a una Competition.
  - Registro: Relaciona a un Fisherman con una Competition y un puesto específico.

## Flujo de Competitividad
1. Se crea la Liga del año.
2. Se añaden fechas de competición.
3. Se definen cuántos puestos hay disponibles por competición.
4. Los pescadores se inscriben a los puestos.