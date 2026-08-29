using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pars.Domain.Entities.Security;
namespace Pars.Infrastructure.Persistence.Configurations.Security;
public class PermissionConfiguration:IEntityTypeConfiguration<Permission>
{
 public void Configure(EntityTypeBuilder<Permission> b){
 b.ToTable("Permissions"); b.HasKey(x=>x.Id);
 b.Property(x=>x.Code).HasMaxLength(100).IsRequired();
 b.HasIndex(x=>x.Code).IsUnique();
 }
}
public class RolePermissionConfiguration:IEntityTypeConfiguration<RolePermission>
{
 public void Configure(EntityTypeBuilder<RolePermission> b){
 b.ToTable("RolePermissions"); b.HasKey(x=>new{x.RoleId,x.PermissionId});
 b.HasOne(x=>x.Permission).WithMany(x=>x.RolePermissions).HasForeignKey(x=>x.PermissionId);
 }
}
