﻿using Microsoft.EntityFrameworkCore;
using System;
using Loadshop.Data;

namespace Loadshop.DomainServices.Loadshop.DataProvider.Entities
{
    public class UnitOfMeasureEntity : UnitOfMeasure
    {
        public override void OnModelCreating(ModelBuilder modelBuilder, Type configuringType)
        {
            base.OnModelCreating(modelBuilder, configuringType);
        }
    }
}
