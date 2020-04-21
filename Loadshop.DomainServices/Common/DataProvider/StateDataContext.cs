using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Loadshop.DomainServices.Common.DataProvider.Entities;
using TMS.Infrastructure.EntityFramework;

namespace Loadshop.DomainServices.Common.DataProvider
{
    public class StateDataContext : DbContextCore
    {
        public static readonly LoggerFactory _loggerFactory = new LoggerFactory(new[] { new ConsoleLoggerProvider((_, __) => true, true) });
        public StateDataContext(DbContextOptions<StateDataContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.UseLoggerFactory(_loggerFactory);
            base.OnConfiguring(optionsBuilder);
        }

        public DbSet<StateEntity> States { get; set; }
    }
}
