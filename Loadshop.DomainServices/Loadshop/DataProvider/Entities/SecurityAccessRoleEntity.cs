using System;
using System.Collections.Generic;
using Loadshop.Data;
using Microsoft.EntityFrameworkCore;

namespace Loadshop.DomainServices.Loadshop.DataProvider.Entities
{
    public class SecurityAccessRoleEntity : SecurityAccessRole
    {
        public override void OnModelCreating(ModelBuilder modelBuilder, Type configuringType)
        {
            modelBuilder.Entity<SecurityAccessRoleEntity>()
                .HasMany(securityAccessRole => securityAccessRole.SecurityUserAccessRoles)
                .WithOne(securityUserAccessRole => securityUserAccessRole.SecurityAccessRole)
                .HasForeignKey(securityUserAccessRole => securityUserAccessRole.AccessRoleId);

            modelBuilder.Entity<SecurityAccessRoleEntity>()
               .HasMany(securityAccessRole => securityAccessRole.SecurityAccessRoleAppActions)
               .WithOne(securityAccessRoleAppActionEntity => securityAccessRoleAppActionEntity.SecurityAccessRole)
               .HasForeignKey(securityUserAccessRole => securityUserAccessRole.AppActionId);


            //Parent Roles Relationship
            modelBuilder.Entity<SecurityAccessRoleEntity>()
              .HasMany(securityAccessRole => securityAccessRole.ParentAccessRoles)
              .WithOne(parentAccessRoles => parentAccessRoles.ChildAccessRole)
              .HasForeignKey(parentAccessRoles => parentAccessRoles.ChildAccessRoleId);

            //Child Roles Relationship
            modelBuilder.Entity<SecurityAccessRoleEntity>()
              .HasMany(securityAccessRole => securityAccessRole.ChildAccessRoles)
              .WithOne(parentAccessRoles => parentAccessRoles.TopLevelAccessRole)
              .HasForeignKey(parentAccessRoles => parentAccessRoles.TopLevelAccessRoleId);

            modelBuilder.Entity<SecurityAccessRoleEntity>()
                .HasMany(securityAccessRole => securityAccessRole.UserCommunicationSecurityAccessRoles)
                .WithOne(userCommunicationSecurityAccessRole => userCommunicationSecurityAccessRole.SecurityAccessRole)
                .HasForeignKey(userCommunicationSecurityAccessRole => userCommunicationSecurityAccessRole.AccessRoleId);
        }
        
        public virtual List<SecurityUserAccessRoleEntity> SecurityUserAccessRoles { get; set; }
        public virtual List<SecurityAccessRoleAppActionEntity> SecurityAccessRoleAppActions { get; set; }

        public virtual List<SecurityAccessRoleParentEntity> ParentAccessRoles { get; set; }
        public virtual List<SecurityAccessRoleParentEntity> ChildAccessRoles { get; set; }
        public virtual List<UserCommunicationSecurityAccessRoleEntity> UserCommunicationSecurityAccessRoles { get; set; }
    }
}
