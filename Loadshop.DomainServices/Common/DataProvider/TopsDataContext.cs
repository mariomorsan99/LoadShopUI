using Microsoft.EntityFrameworkCore;
using System;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using TMS.Infrastructure.EntityFramework;
using System.Collections.Generic;
using System.Linq;
using Loadshop.DomainServices.Common.Services.Data;

namespace Loadshop.DomainServices.Common.DataProvider
{
    public class TopsDataContext : DbContextCore, ITopsDataContext
    {
        private string _connection { get; set; }
        public TopsDataContext(DbContextOptions<TopsDataContext> options) : base(options)
        {

        }

        public TopsDataContext(string connection) : base(connection)
        {
            _connection = connection;
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("dbo");
            modelBuilder.RemovePluralizingTableNameConvention();
            modelBuilder.RemoveEntityFromTableNameConvention();
            base.OnModelCreating(modelBuilder);
        }

        public virtual DbSet<SecurUsersEntity> SecurUsers { get; set; }
        public string Id { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public virtual List<string> GetCarrierVisibilityTypes(string username, string carrierId)
        {
            return Database
                .SqlQuery<string>($"exec etopsuser.spLoadBoardGetCarrierVisibilityTypes @UserId='{username}', @CarrierId='{carrierId}'")
                .ToList();
        }

        public virtual CapRateData GetCapRates(string loadId)
        {
            var storedProcedure = $"exec etopsuser.spLoadLaneScacHistoryGet @LoadId = '{loadId}', @RateType =";
            var result = new CapRateData()
            {
                HCapRate = Database.SqlQuery<LoadLaneScacHistoryData>($"{storedProcedure} 'HCAP'").FirstOrDefault()?.RateValue,
                XCapRate = Database.SqlQuery<LoadLaneScacHistoryData>($"{storedProcedure} 'XCAP'").FirstOrDefault()?.RateValue,
            };

            return result;
        }
    }
}
