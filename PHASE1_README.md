# Pars System — Phase 1

## Scope
Phase 1 standardizes the backend around an **existing SQL Server database**. It does not run migrations, `EnsureCreated`, `EnsureDeleted`, or automatic seed writes at startup.

Pipeline: **Existing Database → EF Core Mapping → Domain/Application → API → UI**.

## Projects in the buildable backend solution
- `Pars.Domain`
- `Pars.Application`
- `Pars.Infrastructure`
- `Pars.API`

Open/build: `ParsSystem.sln`.

## Mapped legacy areas
### Personal (`dbo`)
- `personal`
- `personalChild`
- `personalFile`

### Attendance (`att`)
- `AttendanceEntry`
- `EmployeeGroup`
- `EmployeeGroupPerson`
- `ShiftType`
- `Shift`
- `WeekDay`
- `ShiftDetail`
- `GroupShiftAssignment`
- `ManualAttendanceEntry`
- `RequestType`
- `MissionEntry`
- `Holiday`
- `Calendar`
- `RequestAllocationCatalogue`
- `DailyAttendanceCalculations` (keyless because the supplied database documentation reports no PK)

### Core / legacy access (`dbo`)
- `tblMenu`
- `tblDatresi`
- `tblDatresi_Child`

### Application auth (`auth`)
- `Users`
- `Roles`
- `UserRoles`

**Important:** the supplied 172-table legacy database documentation does not list the `auth` schema tables. They are retained as application-owned models because the current API/AuthService depends on them. No automatic creation is performed. Before enabling authentication against production, confirm that these tables already exist or map authentication to the real legacy identity source.

## Source inconsistencies deliberately not guessed
- The FK section says `att.AttendanceEntry.groupid -> att.EmployeeGroup.id`, but the documented column list for `att.AttendanceEntry` has no `groupid`; therefore Phase 1 does **not** map this FK.
- The documented FK for `dbo.personalFile` does not state `pid -> dbo.personal.id`; therefore the `PersonalFile.Personal` navigation is ignored in EF for now.
- `tblDatresi_Child` has `frmname`, but the supplied FK section does not document it as a FK to `tblDatresi.frmname`; therefore that relationship is not assumed.

## Build
Requires .NET 8 SDK:

```bash
dotnet restore ParsSystem.sln
dotnet build ParsSystem.sln -c Debug
```

Set the real connection string in `Pars.API/appsettings.json` or environment variables before running. The shipped value is a placeholder.

## Database safety rule
Do not add EF migrations against the production legacy database in this phase. Mapping changes belong in `Pars.Infrastructure/Persistence/Configurations`.
