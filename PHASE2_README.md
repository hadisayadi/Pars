# Pars System - Phase 2

## Scope implemented
- Personal API: existing advanced search/CRUD retained, request DTO validation added.
- Attendance Application layer: DTOs and `IAttendanceService`.
- Attendance Infrastructure service backed by the existing `att` schema.
- Attendance API: raw entries, calculated daily attendance, manual entries, missions, dashboard, shifts and groups.
- Operational Blazor WebAssembly UI: Dashboard, Personals, Attendance.
- `Pars.Web` is now a real project and is included in `ParsSystem.sln`.

## Existing-database policy
No migration, `EnsureCreated`, schema creation or seed is executed. Phase 2 reads/writes only mapped existing tables.

## Important behavior
`DailyAttendanceCalculations` is treated as the source for calculated attendance values (late/early/overtime/duty minutes). It remains keyless/read-only in EF.
Manual attendance and mission creation write to the mapped legacy tables and default `Status` to `New`.

## API endpoints
- `POST /api/personals/search`
- `GET /api/personals/{id}`
- `POST/PUT /api/personals`
- `GET /api/attendance/entries`
- `GET /api/attendance/daily`
- `GET/POST /api/attendance/manual`
- `GET/POST /api/attendance/missions`
- `GET /api/attendance/dashboard`
- `GET /api/attendance/shifts`
- `GET /api/attendance/groups`

## Build
Requires .NET 8 SDK:
```
dotnet restore ParsSystem.sln
dotnet build ParsSystem.sln
```
The execution environment used to prepare this package does not contain the .NET SDK, so a real compiler build could not be run here.
