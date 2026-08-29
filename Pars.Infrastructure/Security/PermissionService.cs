using Microsoft.EntityFrameworkCore;
using Pars.Application.Security;
using Pars.Infrastructure.Persistence;
namespace Pars.Infrastructure.Security;
public class PermissionService:IPermissionService
{
 private readonly ParsDbContext db;
 public PermissionService(ParsDbContext db)=>this.db=db;
 public async Task<bool> HasPermissionAsync(Guid userId,string permission,CancellationToken ct=default)
 => await db.UserRoles.AnyAsync(x=>x.UserId==userId && x.Role.RolePermissions.Any(p=>p.Allowed && p.Permission.Code==permission),ct);
 public async Task<IReadOnlyList<string>> GetUserPermissionsAsync(Guid userId,CancellationToken ct=default)
 => await db.UserRoles.Where(x=>x.UserId==userId).SelectMany(x=>x.Role.RolePermissions).Where(x=>x.Allowed).Select(x=>x.Permission.Code).Distinct().ToListAsync(ct);
}
