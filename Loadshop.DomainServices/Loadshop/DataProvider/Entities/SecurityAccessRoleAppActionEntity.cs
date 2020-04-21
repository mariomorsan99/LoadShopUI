using System;
using Loadshop.Data;
using Microsoft.EntityFrameworkCore;

namespace Loadshop.DomainServices.Loadshop.DataProvider.Entities
{
    public class SecurityAccessRoleAppActionEntity : SecurityAccessRoleAppAction
    {

        public override void OnModelCreating(ModelBuilder modelBuilder, Type configuringType)
        {
            modelBuilder.Entity<SecurityAccessRoleAppActionEntity>().HasKey(securityAccessRoleAppActionEntity => new { securityAccessRoleAppActionEntity.AppActionId, securityAccessRoleAppActionEntity.AccessRoleId });

            modelBuilder.Entity<SecurityAccessRoleAppActionEntity>()
                .HasOne(securityAccessRoleAppActionEntity => securityAccessRoleAppActionEntity.SecurityAccessRole)
                .WithMany(securityAccessRole => securityAccessRole.SecurityAccessRoleAppActions)
                .HasForeignKey(securityAccessRoleAppActionEntity => securityAccessRoleAppActionEntity.AccessRoleId);

            modelBuilder.Entity<SecurityAccessRoleAppActionEntity>()
                .HasOne(securityAccessRoleAppActionEntity => securityAccessRoleAppActionEntity.SecurityAppAction)
                .WithMany(securityAppAction => securityAppAction.SecurityAccessRoleAppActions)
                .HasForeignKey(securityAccessRoleAppActionEntity => securityAccessRoleAppActionEntity.AppActionId); ;
        }

        public SecurityAccessRoleEntity SecurityAccessRole { get; set; }
        public SecurityAppActionEntity SecurityAppAction { get; set; }
    }
}