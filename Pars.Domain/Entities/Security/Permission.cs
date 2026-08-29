namespace Pars.Domain.Entities.Security;
public class Permission
{
 public int Id {get;set;}
 public string Code {get;set;}=default!;
 public string Name {get;set;}=default!;
 public string Module {get;set;}=default!;
 public bool IsActive {get;set;}=true;
 public ICollection<RolePermission> RolePermissions {get;set;}=new List<RolePermission>();
}
public class RolePermission
{
 public int RoleId {get;set;}
 public int PermissionId {get;set;}
 public Role Role {get;set;}=default!;
 public Permission Permission {get;set;}=default!;
 public bool Allowed {get;set;}=true;
}
