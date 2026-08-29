namespace Pars.Application.Security;
public interface IPermissionService
{
 Task<bool> HasPermissionAsync(Guid userId,string permission,CancellationToken ct=default);
 Task<IReadOnlyList<string>> GetUserPermissionsAsync(Guid userId,CancellationToken ct=default);
}
