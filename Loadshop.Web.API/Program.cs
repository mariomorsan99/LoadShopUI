using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Loadshop.Web.DbLogger;
using TMS.Infrastructure.Common.Configuration;
using TMS.Infrastructure.Common.Configuration.ConfigDbProvider;
using TMS.Infrastructure.Common.Logging;
using TMS.Infrastructure.Common.Logging.DbLogger;

namespace Loadshop.Web.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var mobileApiConfig = new ConfigManagerOptions
                {
                    LoadDbConfiguration = true,
                    DbSettings = new ConfigDbSettings
                    {
                        new ConfigDbSetting("Shared.Endpoint", "MobileApiAddress")
                    }
                };

                var project44ApiConfig = new ConfigManagerOptions
                {
                    LoadDbConfiguration = true,
                    DbSettings = new ConfigDbSettings
                    {
                        new ConfigDbSetting("Shared.Endpoint", "VisibilityApiAddress")
                    }
                };

                //Todo: LoadBoard.Web.Api should be removed when the database gets changes from old loadboard.web.api to loadshop.web.api
                var additionalConfig = new ConfigManagerOptions
                {
                    ProcessNames = new List<string> { "LoadBoard.Web.Api", "Loadshop.Shared" },
                    LoadDbConfiguration = true,
                };

                new WebHostBuilder()
                    .UseKestrel()
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .ConfigureAppConfiguration((hostingContext, config) =>
                    {
                        //Load the default tops configuration
                        //The order loaded is RegionDefault, Command Line, Process Settings (Globals, ProgID, ProcessNames, Process Settings), Region.config, Web/app.config, appsettings.json, Runtime Options Settings
                        config.AddTmsSettings(additionalConfig);
                        config.AddTmsSettings(new ConfigManagerOptions());
                        config.AddTmsSettings(mobileApiConfig);
                        config.AddTmsSettings(project44ApiConfig);

                        ////Load the local development app settings file 
                        //.AddJsonFile("appsettings.development.json", true, true);
                    })
                    .ConfigureLogging((hostingContext, logging) =>
                    {
                        //Configure logging based on the local config file
                        logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"))
                            //Add the default logging configuration (include Console & Debug loggers by default)
                            .AddTmsLogger(new TmsLoggerOptions(hostingContext.Configuration)
                            {
                                ////Setup DB logging
                                DbLoggerOptions = new TmsDbLoggerOptions()
                                {
                                    ConnectionString = hostingContext.Configuration.GetValue<string>("LoadBoardSQLServer"),
                                    LoggingLevel = LogLevel.Error,
                                    TableTemplate = new LoadBoardLogDbTemplate(hostingContext.Configuration.CommonSettings().ProgramId, "LB") //hostingContext.Configuration.CommonSettings().ApplicationCode)
                                },

                                ////Setup Email Logging
                                //EmailLoggerOptions = new TrnEmailLoggerOptions()
                                //{
                                //    LoggingLevel = LogLevel.Information,
                                //    SmtpClientAddress = "",
                                //    EmailToAddress = "brady.potaczek@kbxlogistics.com"
                                //},

                                //Setup file logging
                                //FileLoggerOptions = new TrnFileLoggerOptions((IConfigurationRoot)hostingContext.Configuration)
                                //{
                                //    CreateFolderForLogFiles = true,
                                //    ApplyHeaderOnStartup = true,
                                //    LogFilePath = @"c:\logs"
                                //}
                            });

                    })
                    .UseUrls("http://localhost:5003")
                    .UseStartup<Startup>()
                    .Build()
                    .Run();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Debug.WriteLine(e);
                File.WriteAllText("StartupErrorLog.txt", e.ToString());

                throw;
            }
        }

    }
}
