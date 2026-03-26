# Project Context: FishClubAlginet

## Objective
Management platform for a local fishing club. The system manages members (Fishermen), annual Leagues, and specific Competitions.

## Technical Stack
- **Backend:** .NET 9 using Clean Architecture.
- **Persistence:** Entity Framework Core with SQL Server.
- **Frontend:** Blazor WebAssembly.
- **Communication:** MediatR (CQRS Pattern) in the Application layer.
- **Authentication:** ASP.NET Core Identity (Roles: Admin and Fisherman).
- **Testing:** xUnit, Moq, FluentAssertions.

## Development Standards (i18n Ready)
- **Internationalization (i18n):** The Backend must NOT have hardcoded user-facing messages in Spanish.
- **Error Codes:** All errors in `Errors.cs` must use unique codes (e.g., `"Auth.InvalidCredentials"`).
- **Resource Files:** User-facing descriptions must be stored in `.resx` files (Resources) or handled by the Frontend using the Error Codes. 
- **Logging:** Use structured logging with constants for templates, but keep technical logs in English.

## General Business Rules
- Only Administrators can create Leagues, Competitions, and manage users.
- Leagues are annual, spanning from January 1st to December 31st.
- A Competition belongs to a single League.
- Each Competition has a specific number of "Fishing Spots" (puestos).
- Competitions are defined by a date, a time range, and a location.