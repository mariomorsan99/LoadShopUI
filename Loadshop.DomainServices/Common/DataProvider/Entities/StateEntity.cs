
using System;
using Microsoft.EntityFrameworkCore;
using Tops.Data.State;

namespace Loadshop.DomainServices.Common.DataProvider.Entities
{
    public class StateEntity: State
    {
        public override void OnModelCreating(ModelBuilder modelBuilder, Type configuringType)
        {
            base.OnModelCreating(modelBuilder, configuringType);
        }
    }
}
