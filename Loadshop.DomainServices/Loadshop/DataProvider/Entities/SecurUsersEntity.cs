using System;
using Microsoft.EntityFrameworkCore;
using Tops.Data.Secur;

namespace Loadshop.DomainServices.Loadshop.DataProvider.Entities
{
    public class SecurUsersEntity : SecurUser
    {
        public override void OnModelCreating(ModelBuilder modelBuilder, Type configureType)
        {
            base.OnModelCreating(modelBuilder, configureType);
        }
    }
}
