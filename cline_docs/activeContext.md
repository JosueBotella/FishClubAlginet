#### Backend — Entidades

- [ ] Entidad `Competition` (Domain):
  - Campos: `Id` (Guid), `LeagueId` (FK), `CompetitionNumber` (int, ordinal en la liga: 1º, 2º, ... 18º), `Name` (opcional), `Date`, `StartTime`, `EndTime`, `Venue` (string libre: "BELLUS", "PINEDO", "FORTALENY"...), `Zone` (string libre: "C", "B", "SUR", "NORTE", "A1-A2-A3", "B1-B2-B3", "E1-E2-E3"...), `Subspecialty` (enum: `Mar`, `AguaDulce`), `Category` (enum: `Seniors`, `Juvenil`), `MaxSpots` (int), `Status` (enum: `Planned`, `RegistrationOpen`, `Closed`, `ResultsDraft`, `ResultsValidated`)
    - **Decisión**: `Venue` y `Zone` son **strings libres**, no entidades catálogo. El club los introduce manualmente al crear cada concurso. Se podría sugerir autocompletado en el frontend leyendo valores ya usados, pero sin forzar el modelo.
  - Reglas:
    - `CompetitionNumber` único dentro de la misma liga
    - `Date` debe estar dentro del año de la `League`
    - `MaxSpots` > 0
    - El paso a `ResultsValidated` requiere que TODOS los inscritos tengan `CompetitionResult` registrado
- [ ] Entidad `CompetitionResult` (Domain) — combina inscripción + resultado:
  - Campos: `Id` (Guid), `CompetitionId` (FK), `FishermanId` (FK), `AssignedSpotNumber` (int? — null hasta el sorteo), `DidAttend` (bool), `WeightInGrams` (int — 0 si asistió pero no pescó), `BiggestCatchWeight` (int? — peso de la pieza mayor del concurso si la presentó), `Points` (decimal — calculado), `Ranking` (int — calculado), `RegistrationDate`, `IsValidated` (bool)
  - Reglas:
    - Un pescador solo puede tener un `CompetitionResult` por `Competition` (índice único compuesto)
    - `AssignedSpotNumber` único dentro de la misma `Competition`
    - Si `DidAttend = false`, `WeightInGrams` y `Points` deben ser 0 (vacío en planilla)
    - Si `DidAttend = true` y `WeightInGrams = 0`, recibe `MinPoints` (default 5)
- [ ] DTOs: `RegisterToCompetitionRequest`, `AssignSpotsRequest`, `EnterResultsRequest` (bulk), `CompetitionDto`, `CompetitionDetailDto` (con resultados), `MyRegistrationDto` (vista pescador)