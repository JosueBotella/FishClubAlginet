# Domain Entities and Relationships

## Existing Entities
- **Fisherman:** Represents a club member.

## New Entities to Implement
- **League:**
  - Properties: `Guid Id`, `string Name`, `int Year`, `bool IsActive`.
  - Relationship: One-to-Many with `Competitions`.

- **Competition:**
  - Properties: `Guid Id`, `string Name`, `DateTime Date`, `TimeSpan StartTime`, `TimeSpan EndTime`, `string Location`, `int MaxSpots`.
  - Relationship: Belongs to a `League`.
  - Relationship: One-to-Many with `CompetitionRegistrations`.

- **CompetitionRegistration (The "Enrollment" System):**
  - Properties: `Guid Id`, `Guid FishermanId`, `Guid CompetitionId`, `DateTime RegistrationDate`, `bool IsValidated`, `int? AssignedSpotNumber`.
  - Logic: `IsValidated` is false by default. Admin must set it to true.
  - Logic: `AssignedSpotNumber` is assigned only after validation.

- **FishingSpot (Status):**
  - Properties: `Guid Id`, `int SpotNumber`, `bool IsOccupied`.
  - Relationship: Derived from `Competition` and its `MaxSpots`.