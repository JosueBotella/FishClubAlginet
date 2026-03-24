# Project Context: FishClubAlginet

## Objective
Management platform for a local fishing club. The system manages members (Fishermen), annual Leagues, and specific Competitions.

## Technical Stack
- **Backend:** .NET 9 using Clean Architecture.
- **Persistence:** Entity Framework Core with SQL Server.
- **Frontend:** Blazor WebAssembly.
- **Communication:** MediatR (CQRS Pattern) in the Application layer.
- **Authentication:** ASP.NET Core Identity (Roles: Admin and Fisherman).

## General Business Rules
- Only Administrators can create Leagues and Competitions.
- Leagues are annual.
- A Competition belongs to a single League.
- Each Competition has a specific number of "Fishing Spots" (puestos).
- Competitions are defined by a date, a time range, and a location.