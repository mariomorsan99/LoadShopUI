using System;
using Loadshop.Data;
using Microsoft.EntityFrameworkCore;

namespace Loadshop.DomainServices.Loadshop.DataProvider.Entities
{
    public class SecurityAccessRoleParentEntity : SecurityAccessRoleParent
    {
        public override void OnModelCreating(ModelBuilder modelBuilder, Type configuringType)
        {
            modelBuilder.Entity<SecurityAccessRoleParentEntity>()
                .HasOne(securityAccessRoleParentEntity => securityAccessRoleParentEntity.TopLevelAccessRole)
                .WithMany(securityAccessRole => securityAccessRole.ChildAccessRoles)
                .HasForeignKey(x => x.TopLevelAccessRoleId);

            modelBuilder.Entity<SecurityAccessRoleParentEntity>()
               .HasOne(securityAccessRoleParentEntity => securityAccessRoleParentEntity.ChildAccessRole)
               .WithMany(securityAccessRole => securityAccessRole.ParentAccessRoles)
               .HasForeignKey(x => x.ChildAccessRoleId);
        }

        public virtual SecurityAccessRoleEntity TopLevelAccessRole { get; set; }
        public virtual SecurityAccessRoleEntity ChildAccessRole { get; set; }
    }
}
