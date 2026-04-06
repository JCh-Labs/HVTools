using System.Security;

namespace HVTools.Helpers
{
    /// <summary>
    /// Holds connection settings for Hyper-V remote connections
    /// </summary>
    public class ConnectionSettings
    {
        public bool UseSSL { get; set; } = false;
        public int Port { get; set; } = 0; // 0 = auto (5985 for HTTP, 5986 for HTTPS)
        public bool UseCurrentUser { get; set; } = true;
        public string? Username { get; set; }
        public SecureString? SecurePassword { get; set; }
        public string AuthenticationMechanism { get; set; } = "Default";
        public bool SkipCACheck { get; set; } = false;
        public bool SkipCNCheck { get; set; } = false;
        public int TimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Creates a copy of the current settings
        /// </summary>
        public ConnectionSettings Clone()
        {
            return new ConnectionSettings
            {
                UseSSL = this.UseSSL,
                Port = this.Port,
                UseCurrentUser = this.UseCurrentUser,
                Username = this.Username,
                SecurePassword = this.SecurePassword?.Copy(),
                AuthenticationMechanism = this.AuthenticationMechanism,
                SkipCACheck = this.SkipCACheck,
                SkipCNCheck = this.SkipCNCheck,
                TimeoutSeconds = this.TimeoutSeconds
            };
        }

        /// <summary>
        /// Gets the default connection settings
        /// </summary>
        public static ConnectionSettings GetDefault()
        {
            return new ConnectionSettings
            {
                UseSSL = false,
                Port = 0,
                UseCurrentUser = true,
                Username = null,
                SecurePassword = null,
                AuthenticationMechanism = "Default",
                SkipCACheck = false,
                SkipCNCheck = false,
                TimeoutSeconds = 30
            };
        }
    }
}
