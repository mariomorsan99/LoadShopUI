using System;
using System.Collections.Generic;
using Loadshop.Data;
using Microsoft.EntityFrameworkCore;

namespace Loadshop.DomainServices.Loadshop.DataProvider.Entities
{
    public class SecurityAppActionEntity : SecurityAppAction
    {
        public override void OnModelCreating(ModelBuilder modelBuilder, Type configuringType)
        {
            modelBuilder.Entity<SecurityAppActionEntity>()
                .HasMany(securityAppAction => securityAppAction.SecurityAccessRoleAppActions)
                .WithOne(securityAccessRoleAppAction => securityAccessRoleAppAction.SecurityAppAction)
                .HasForeignKey(securityAccessRoleAppAction => securityAccessRoleAppAction.AppActionId);
        }
        public virtual List<SecurityAccessRoleAppActionEntity> SecurityAccessRoleAppActions { get; set; }
    }
}