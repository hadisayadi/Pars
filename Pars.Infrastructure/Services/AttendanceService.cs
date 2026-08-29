using Microsoft.EntityFrameworkCore;
using Pars.Application.Attendance;
using Pars.Application.Attendance.DTOs;
using Pars.Domain.Entities.Attendance;
using Pars.Infrastructure.Persistence;

namespace Pars.Infrastructure.Services;

public sealed class AttendanceService : IAttendanceService
{
    private readonly ParsDbContext _db;
    public AttendanceService(ParsDbContext db) => _db = db;

    public async Task<IReadOnlyList<AttendanceEntryDto>> GetEntriesAsync(AttendanceSearchRequest r, CancellationToken ct = default)
    {
        var q = _db.AttendanceEntries.AsNoTracking().Where(x => x.PersonId == r.PersonId);
        if (r.From.HasValue) q = q.Where(x => x.DateTime >= r.From.Value);
        if (r.To.HasValue) q = q.Where(x => x.DateTime < r.To.Value.Date.AddDays(1));
        return await q.OrderByDescending(x => x.DateTime).Take(Math.Clamp(r.Take,1,500))
            .Select(x => new AttendanceEntryDto(x.Id,x.PersonId,x.DateTime,x.AddBy,x.Updated)).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<DailyAttendanceDto>> GetDailyAsync(DailyAttendanceRequest r, CancellationToken ct = default)
    {
        var q = _db.DailyAttendanceCalculations.AsNoTracking().Where(x => x.PersonId == r.PersonId);
        if (!string.IsNullOrWhiteSpace(r.FromPersianDate)) q = q.Where(x => x.PersianDate != null && string.Compare(x.PersianDate,r.FromPersianDate) >= 0);
        if (!string.IsNullOrWhiteSpace(r.ToPersianDate)) q = q.Where(x => x.PersianDate != null && string.Compare(x.PersianDate,r.ToPersianDate) <= 0);
        return await q.OrderByDescending(x=>x.PersianDate).Select(x => new DailyAttendanceDto(
            x.PersianDate,x.PersonId,x.AttendanceMins??0,x.DutyAttendanceMins??0,x.ArrivalDateTime,x.DepartureDateTime,
            x.ArrivalLateMinutes??0,x.DepartureEarlyMinutes??0,x.OvertimeMinute??0,x.IsOffDay??false,x.IsCalendarHoliday??false,
            x.HourlyLeaveRequestMinutes??0,x.HourlyMissionRequestMinutes??0,x.Status,x.Entries)).ToListAsync(ct);
    }

    public async Task<long> CreateManualEntryAsync(ManualAttendanceCreateDto dto, string? actor, CancellationToken ct = default)
    {
        if (!await _db.Personals.AnyAsync(x=>x.Id==dto.PersonId,ct)) throw new KeyNotFoundException("پرسنل یافت نشد.");
        var e=new ManualAttendanceEntry{PersonId=dto.PersonId,Kind=dto.Kind,Date=dto.Date,Time=dto.Time,Description=dto.Description,SendTo=dto.SendTo,Status="New",AddBy=actor??"System"};
        _db.ManualAttendanceEntries.Add(e); await _db.SaveChangesAsync(ct); return e.Id;
    }

    public async Task<IReadOnlyList<ManualAttendanceDto>> GetManualEntriesAsync(string personId,int take=100,CancellationToken ct=default)
        => await _db.ManualAttendanceEntries.AsNoTracking().Where(x=>x.PersonId==personId).OrderByDescending(x=>x.Id).Take(Math.Clamp(take,1,500))
            .Select(x=>new ManualAttendanceDto(x.Id,x.PersonId,x.Kind,x.Date,x.Time,x.Description,x.Status)).ToListAsync(ct);

    public async Task<long> CreateMissionAsync(MissionCreateDto dto,string? actor,CancellationToken ct=default)
    {
        if (!await _db.Personals.AnyAsync(x=>x.Id==dto.PersonId,ct)) throw new KeyNotFoundException("پرسنل یافت نشد.");
        var e=new MissionEntry{PersonId=dto.PersonId,RequestTypeId=dto.RequestTypeId,DateFrom=dto.DateFrom,DateTo=dto.DateTo,TimeFrom=dto.TimeFrom,TimeTo=dto.TimeTo,Destination=dto.Destination,Subject=dto.Subject,HasVehicle=dto.HasVehicle,Status="New",AddBy=actor??"System"};
        _db.MissionEntries.Add(e); await _db.SaveChangesAsync(ct); return e.Id;
    }

    public async Task<IReadOnlyList<MissionDto>> GetMissionsAsync(string? personId,string? status,int take=100,CancellationToken ct=default)
    {
        var q=_db.MissionEntries.AsNoTracking().AsQueryable();
        if(!string.IsNullOrWhiteSpace(personId))q=q.Where(x=>x.PersonId==personId);
        if(!string.IsNullOrWhiteSpace(status))q=q.Where(x=>x.Status==status);
        return await q.OrderByDescending(x=>x.Id).Take(Math.Clamp(take,1,500)).Select(x=>new MissionDto(x.Id,x.PersonId,x.Code,x.DateFrom,x.DateTo,x.TimeFrom,x.TimeTo,x.Destination,x.Subject,x.HasVehicle,x.Status)).ToListAsync(ct);
    }

    public async Task<AttendanceDashboardDto> GetDashboardAsync(DateTime date,string? persianDate,CancellationToken ct=default)
    {
        var from=date.Date; var to=from.AddDays(1);
        var raw=_db.AttendanceEntries.AsNoTracking().Where(x=>x.DateTime>=from&&x.DateTime<to);
        var manual=_db.ManualAttendanceEntries.AsNoTracking().AsQueryable();
        var missions=_db.MissionEntries.AsNoTracking().AsQueryable();
        if(!string.IsNullOrWhiteSpace(persianDate)){manual=manual.Where(x=>x.Date==persianDate);missions=missions.Where(x=>x.DateFrom!=null&&string.Compare(x.DateFrom,persianDate)<=0&&x.DateTo!=null&&string.Compare(x.DateTo,persianDate)>=0);}
        var daily=_db.DailyAttendanceCalculations.AsNoTracking().AsQueryable(); if(!string.IsNullOrWhiteSpace(persianDate)) daily=daily.Where(x=>x.PersianDate==persianDate);
        return new AttendanceDashboardDto(await raw.Select(x=>x.PersonId).Distinct().CountAsync(ct),await raw.CountAsync(ct),await manual.CountAsync(ct),await missions.CountAsync(ct),await daily.CountAsync(x=>(x.ArrivalLateMinutes??0)>0,ct),await daily.CountAsync(x=>(x.OvertimeMinute??0)>0,ct));
    }

    public async Task<IReadOnlyList<ShiftLookupDto>> GetShiftsAsync(CancellationToken ct=default)=>await _db.Shifts.AsNoTracking().OrderBy(x=>x.Name).Select(x=>new ShiftLookupDto(x.Id,x.Name,x.ShiftCode,x.Description)).ToListAsync(ct);
    public async Task<IReadOnlyList<EmployeeGroupDto>> GetGroupsAsync(CancellationToken ct=default)=>await _db.EmployeeGroups.AsNoTracking().OrderBy(x=>x.Name).Select(x=>new EmployeeGroupDto(x.Id,x.Name,x.Description,x.Persons.Count)).ToListAsync(ct);
}
