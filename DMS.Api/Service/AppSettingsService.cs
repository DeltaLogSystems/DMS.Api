using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS.Api.Shared
{
    public class AppSettingsService
    {
        private static AppSettingsService? _instance;
        private readonly IConfiguration _configuration;

        private AppSettingsService(IConfiguration configuration)
        {
            _configuration = configuration;
            LoadConfigurationToVariables();
        }

        public static void Initialize(IConfiguration configuration)
        {
            _instance = new AppSettingsService(configuration);
        }

        public static AppSettingsService Instance
        {
            get
            {
                if (_instance == null)
                    throw new InvalidOperationException("AppSettingsService must be initialized before use.");
                return _instance;
            }
        }

        /// <summary>
        /// Loads all configuration values into Variables class
        /// </summary>
        private void LoadConfigurationToVariables()
        {
            // Load connection string
            Variables.ConnectionString = _configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("MySQL connection string not found.");

        }
    }
}
