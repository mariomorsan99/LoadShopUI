using Microsoft.EntityFrameworkCore;
using System;
using Loadshop.Data;

namespace Loadshop.DomainServices.Loadshop.DataProvider.Entities
{
    public class UserCommunicationCarrierEntity : UserCommunicationCarrier
    {

        public override void OnModelCreating(ModelBuilder modelBuilder, Type configuringType)
        {
            modelBuilder.Entity<UserCommunicationCarrierEntity>()
                .HasOne(userCommunicationCarrier => userCommunicationCarrier.UserCommunication)
                .WithMany(userCommunication => userCommunication.UserCommunicationCarriers)
                .HasForeignKey(userCommunicationCarrier => userCommunicationCarrier.UserCommunicationCarrierId);

            modelBuilder.Entity<UserCommunicationCarrierEntity>()
                .HasOne(userCommunicationCarrier => userCommunicationCarrier.Carrier)
                .WithMany(carrier => carrier.UserCommunicationCarriers)
                .HasForeignKey(userCommunicationCarrier => userCommunicationCarrier.CarrierId);
        }
        public virtual UserCommunicationEntity UserCommunication { get; set; }
        public virtual CarrierEntity Carrier { get; set; }
    }
}
