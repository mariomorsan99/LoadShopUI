using System;
using Loadshop.Data;
using Microsoft.EntityFrameworkCore;

namespace Loadshop.DomainServices.Loadshop.DataProvider.Entities
{
    public class NotificationEntity : Notification
    {
        public override void OnModelCreating(ModelBuilder modelBuilder, Type configuringType)
        {
            base.OnModelCreating(modelBuilder, configuringType);
            modelBuilder.Entity<NotificationEntity>()
                .HasOne(x => x.NotificationData)
                .WithOne(x => x.Notification);

            modelBuilder.Entity<NotificationEntity>()
                .HasOne(x => x.MessageType);

            modelBuilder.Entity<NotificationEntity>()
                .HasOne(x => x.NotificationMessage)
                .WithOne(x => x.Notifications)
                .HasForeignKey<NotificationMessageEntity>(x => x.NotificationId);
        }

        public virtual NotificationDataEntity NotificationData { get; set; }

        public virtual NotificationMessageEntity NotificationMessage { get; set; }

        public virtual MessageTypeEntity MessageType { get; set; }
    }
}
