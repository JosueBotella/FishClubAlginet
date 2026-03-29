# Development Roadmap

- [x] Base Solution Structure & Fisherman Implementation (CRUD).
- [x] Phase 1: Identity, Security & Backend User Management.

- [x] **Phase 1.5: Blazor Admin MVP (The "Showcase")**
    - [x] Install and configure Radzen Blazor (NuGet, CSS/JS in `index.html`, `_Imports.razor`).
    - [x] **Admin Layout:** Side nav with `<AuthorizeView Roles="Admin">` for Users & Fishermen links.
    - [x] **i18n:** All Spanish strings removed from Blazor pages and constants — full English throughout.
    - [x] **Users UI:** `RadzenDataGrid` listing Identity Users (email, roles, lock status). `RadzenDialog` to create Admin/Fisherman users. Block/Unblock row actions.
    - [x] **Fishermen UI:** `RadzenDataGrid` listing Fishermen. `[Authorize(Roles="Admin")]` enforced.
    - [x] **Admin Layout** `RadzenDataGrid` reloads after user creation to reflect new data.
- [ ] ** Phase 1.75: Admin UI Enhancements**
    - [ ] Add search/filter to Users and Fishermen grids. 
    - [ ] Add pagination to grids for better performance (skip and take).
    - [ ] Implement role management (assign/remove roles from users).

- [ ] **Phase 2: League Management (Backend & Frontend)**
    - [ ] Create `League` entity and MediatR logic.
    - [ ] Create EF Core Migration and update database.
    - [ ] **UI:** League Management Dashboard (RadzenDataGrid).
    
- [ ] **Phase 3: Competitions & Registration**
    - [ ] Create `Competition` and `CompetitionRegistration` entities.
    - [ ] Create EF Core Migration and update database.
    - [ ] **UI:** Competition Calendar and Enrollment UI for Fishermen.