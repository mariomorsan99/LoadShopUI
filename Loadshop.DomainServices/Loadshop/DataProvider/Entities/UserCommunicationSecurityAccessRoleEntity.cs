using Microsoft.EntityFrameworkCore;
using System;
using Loadshop.Data;

namespace Loadshop.DomainServices.Loadshop.DataProvider.Entities
{
    public class UserCommunicationSecurityAccessRoleEntity : UserCommunicationSecurityAccessRole
    {
        public override void OnModelCreating(ModelBuilder modelBuilder, Type configuringType)
        {
            modelBuilder.Entity<UserCommunicationSecurityAccessRoleEntity>()
                .HasOne(x => x.UserCommunication)
                .WithMany(x => x.UserCommunicationSecurityAccessRoles)
                .HasForeignKey(x => x.UserCommunicationId);

            modelBuilder.Entity<UserCommunicationSecurityAccessRoleEntity>()
               .HasOne(x => x.SecurityAccessRole)
               .WithMany(x => x.UserCommunicationSecurityAccessRoles)
               .HasForeignKey(x => x.UserCommunicationId);
        }
        public UserCommunicationEntity UserCommunication { get; set; }
        public SecurityAccessRoleEntity SecurityAccessRole { get; set; }
    }
}
