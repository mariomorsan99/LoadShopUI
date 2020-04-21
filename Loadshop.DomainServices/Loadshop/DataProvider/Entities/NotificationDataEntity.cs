using System;
using Loadshop.Data;
using Microsoft.EntityFrameworkCore;

namespace Loadshop.DomainServices.Loadshop.DataProvider.Entities
{
    public class NotificationDataEntity : NotificationData
    {
        public override void OnModelCreating(ModelBuilder modelBuilder, Type configuringType)
        {
            base.OnModelCreating(modelBuilder, configuringType);
            modelBuilder.Entity<NotificationDataEntity>()
                .HasOne(x => x.Load)
                .WithMany(x => x.NotificationDetails);

            modelBuilder.Entity<NotificationDataEntity>()
                .HasOne(x => x.User)
                .WithMany(x => x.NotificationDatas);

            modelBuilder.Entity<NotificationDataEntity>()
                .HasOne(x => x.Notification)
                .WithOne(x => x.NotificationData);
        }

        public virtual LoadEntity Load { get; set; }

        public virtual UserEntity User { get; set; }

        public virtual NotificationEntity Notification { get; set; }
    }
}
