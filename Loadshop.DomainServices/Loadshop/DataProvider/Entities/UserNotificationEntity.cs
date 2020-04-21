using System;
using Loadshop.Data;
using Microsoft.EntityFrameworkCore;

namespace Loadshop.DomainServices.Loadshop.DataProvider.Entities
{
    public class UserNotificationEntity : UserNotification
    {
        public override void OnModelCreating(ModelBuilder modelBuilder, Type configuringType)
        {
            base.OnModelCreating(modelBuilder, configuringType);
            modelBuilder.Entity<UserNotificationEntity>()
                .HasOne(x => x.MessageType);

            modelBuilder.Entity<UserNotificationEntity>()
                .HasOne(x => x.User)
                .WithMany(x => x.UserNotifications);
        }

        public virtual MessageTypeEntity MessageType { get; set; }

        public virtual UserEntity User { get; set; }
    }
}
