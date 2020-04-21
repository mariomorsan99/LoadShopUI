using System;
using Loadshop.Data;
using Microsoft.EntityFrameworkCore;

namespace Loadshop.DomainServices.Loadshop.DataProvider.Entities
{
    public class SecurityUserAccessRoleEntity : SecurityUserAccessRole
    {
        public override void OnModelCreating(ModelBuilder modelBuilder, Type configuringType)
        {
            modelBuilder.Entity<SecurityUserAccessRoleEntity>().HasKey(securityUserAccessRoleEntity => new { securityUserAccessRoleEntity.AccessRoleId, securityUserAccessRoleEntity.UserId });

            modelBuilder.Entity<SecurityUserAccessRoleEntity>()
                .HasOne(securityUserAccessRole => securityUserAccessRole.User)
                .WithMany(user => user.SecurityUserAccessRoles)
                .HasForeignKey(securityUserAccessRole => securityUserAccessRole.UserId);

            modelBuilder.Entity<SecurityUserAccessRoleEntity>()
                .HasOne(securityUserAccessRole => securityUserAccessRole.SecurityAccessRole)
                .WithMany(securityAccessRole => securityAccessRole.SecurityUserAccessRoles)
                .HasForeignKey(securityUserAccessRole => securityUserAccessRole.AccessRoleId);
        }

        public virtual SecurityAccessRoleEntity SecurityAccessRole { get; set; }
        public virtual UserEntity User { get; set; }
    }
}
