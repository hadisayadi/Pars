using Microsoft.EntityFrameworkCore;
using Pars.Domain.Entities;
using Pars.Domain.Entities.Attendance;
using Pars.Domain.Entities.Auth;
using Pars.Domain.Entities.Core;
using Pars.Domain.Entities.Security;
using Pars.Domain.Entities.Enterprise;

namespace Pars.Infrastructure.Persistence;

public sealed class ParsDbContext : DbContext
{
    public ParsDbContext(DbContextOptions<ParsDbContext> options) : base(options) { }
    public DbSet<Personal> Personals => Set<Personal>(); public DbSet<PersonalChild> PersonalChildren => Set<PersonalChild>(); public DbSet<PersonalFile> PersonalFiles => Set<PersonalFile>();
    public DbSet<Permission> Permissions => Set<Permission>(); public DbSet<RolePermission> RolePermissions => Set<RolePermission>(); public DbSet<User> Users => Set<User>(); public DbSet<Role> Roles => Set<Role>(); public DbSet<UserRole> UserRoles => Set<UserRole>(); public DbSet<LegacyMenuItem> LegacyMenu => Set<LegacyMenuItem>(); public DbSet<LegacyAccess> LegacyAccess => Set<LegacyAccess>(); public DbSet<LegacyAccessScope> LegacyAccessScopes => Set<LegacyAccessScope>();
    public DbSet<EnterpriseRequest> EnterpriseRequests => Set<EnterpriseRequest>(); public DbSet<EnterpriseApproval> EnterpriseApprovals => Set<EnterpriseApproval>(); public DbSet<LeaveBalance> LeaveBalances => Set<LeaveBalance>(); public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<AttendanceEntry> AttendanceEntries => Set<AttendanceEntry>(); public DbSet<EmployeeGroup> EmployeeGroups => Set<EmployeeGroup>(); public DbSet<EmployeeGroupPerson> EmployeeGroupPersons => Set<EmployeeGroupPerson>(); public DbSet<ShiftType> ShiftTypes => Set<ShiftType>(); public DbSet<Shift> Shifts => Set<Shift>(); public DbSet<WeekDay> WeekDays => Set<WeekDay>(); public DbSet<ShiftDetail> ShiftDetails => Set<ShiftDetail>(); public DbSet<GroupShiftAssignment> GroupShiftAssignments => Set<GroupShiftAssignment>(); public DbSet<ManualAttendanceEntry> ManualAttendanceEntries => Set<ManualAttendanceEntry>(); public DbSet<RequestType> RequestTypes => Set<RequestType>(); public DbSet<MissionEntry> MissionEntries => Set<MissionEntry>(); public DbSet<Holiday> Holidays => Set<Holiday>(); public DbSet<WorkCalendarDay> Calendar => Set<WorkCalendarDay>(); public DbSet<RequestAllocationCatalogue> RequestAllocationCatalogue => Set<RequestAllocationCatalogue>(); public DbSet<DailyAttendanceCalculation> DailyAttendanceCalculations => Set<DailyAttendanceCalculation>();
    protected override void OnModelCreating(ModelBuilder modelBuilder){base.OnModelCreating(modelBuilder);modelBuilder.ApplyConfigurationsFromAssembly(typeof(ParsDbContext).Assembly);}
}
