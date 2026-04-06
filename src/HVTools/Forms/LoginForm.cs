using System.Diagnostics;
using System.Management;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Net.Sockets;
using System.Security;
using System.Security.Cryptography;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using HVTools.Helpers;
using static HVTools.Helpers.FileLogger;

namespace HVTools.Forms
{
    public partial class LoginForm : Form
    {
        public LoginResult Result { get; private set; } = null!;
        private string _lastServerChecked = string.Empty;
        private readonly bool _isInitializing;
        private bool _isConnecting; // Prevent double login attempts
        private ConnectionSettings _currentConnectionSettings = ConnectionSettings.GetDefault();

        public class LoginResult
        {
            public bool Success { get; set; }
            public bool Cancelled { get; set; }
            public string? ServerName { get; set; }
            public bool UseWindowsAuth { get; set; }
            public PSCredential? Credentials { get; set; }
            public string? ConnectedUser { get; set; }
            public string? ConnectionType { get; set; }
            public int VmCount { get; set; }
        }

        private class ConnectionTestResult
        {
            public bool Success { get; set; }
            public string? Error { get; set; }
            public int VmCount { get; set; }
            public bool RequiresElevation { get; set; }
            public bool CanAutoElevate { get; set; }
            public bool IsLocal { get; set; }

            // Enhanced properties
            public string? HostName { get; set; }
            public string? HyperVVersion { get; set; }
            public int LogicalProcessorCount { get; set; }
            public double TotalMemoryGb { get; set; }
            public bool IsCluster { get; set; }
            public string? ClusterName { get; set; }
            public string? FullyQualifiedDomainName { get; set; }
        }

        public LoginForm()
        {
            InitializeComponent();
            InitializeFormEvents();
            InitializeConnectionSettings();
            InitializeHyperVDefaults();
            LoadSavedCredentials();
            SetToolName();

            // Mark initialization as complete
            _isInitializing = false;
        }

        private void SetToolName()
        {
            labelLoginFormToolName.Text = Globals.ToolName.ShortName + @" v." + Globals.ToolProperties.ToolVersion;
            Text = $@"{Globals.ToolName.ShortName} - Login";
        }

        /// <summary>
        /// Ensures the form is activated and brought to the foreground when shown
        /// </summary>
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            // Temporarily set TopMost to force the window to the foreground
            // This is needed because Windows may not always activate a new window
            TopMost = true;
            TopMost = false;

            // Activate and bring the form to the foreground
            Activate();
            BringToFront();
            Focus();

            // Set focus to the server textbox for immediate typing
            textboxServer.Focus();
            textboxServer.SelectAll();
        }

        private void InitializeHyperVDefaults()
        {
            try
            {
                Message("Determining default Hyper-V server name...",
                    EventType.Information, 1047);

                // Check if local machine has Hyper-V role installed
                bool hyperVInstalled = TestLocalHyperVInstallation();

                // Set default server name based on Hyper-V availability
                if (hyperVInstalled)
                {
                    // Set to local machine name
                    textboxServer.Text = Environment.MachineName;

                    //UpdateStatusLabel("Ready - Local Hyper-V detected", isSuccess: true);
                    UpdateStatusLabel("Ready - Local Hyper-V detected");
                    Message($"Local Hyper-V installation detected - default server set to: '{Environment.MachineName}'",
                        EventType.Information, 1048);
                }
                else
                {
                    // No local Hyper-V, set to machine name anyway for now
                    textboxServer.Text = Environment.MachineName;

                    //UpdateStatusLabel("Ready - No local Hyper-V detected", isSuccess: false);
                    UpdateStatusLabel("Ready - No local Hyper-V detected");
                    Message("No local Hyper-V detected - default server set to current machine name",
                        EventType.Information, 1049);
                }
            }
            catch (Exception ex)
            {
                // Fallback to machine name if anything goes wrong
                textboxServer.Text = Environment.MachineName;
                UpdateStatusLabel("Ready", isSuccess: null);
                Message($"Error determining Hyper-V status, defaulting to machine name: {ex.Message}",
                    EventType.Warning, 1050);
            }
        }

        private void InitializeFormEvents()
        {
            // Set password char
            textboxPassword.UseSystemPasswordChar = true;

            // Setup tooltip for Windows authentication radio button to show current user
            ToolTip toolTip = new ToolTip();
            toolTip.SetToolTip(radioWindows,
                $"Connect using your current Windows credentials: {WindowsIdentity.GetCurrent().Name}");

            // Update UI based on initial selection
            RadioAuth_CheckedChanged(null!, null!);
        }

        private void InitializeConnectionSettings()
        {
            // Set default values for connection settings controls
            checkboxUseSSL.Checked = _currentConnectionSettings.UseSSL;
            numericPort.Value = _currentConnectionSettings.Port;
            numericTimeout.Value = _currentConnectionSettings.TimeoutSeconds;
            checkboxSkipCACheck.Checked = _currentConnectionSettings.SkipCACheck;
            checkboxSkipCNCheck.Checked = _currentConnectionSettings.SkipCNCheck;

            // Set authentication mechanism
            comboAuthMechanism.SelectedItem = _currentConnectionSettings.AuthenticationMechanism;
            if (comboAuthMechanism.SelectedIndex == -1)
            {
                comboAuthMechanism.SelectedIndex = 0; // Default
            }

            // Enable/disable SSL-related controls
            UpdateSSLControls();

            // Update status label to show current settings
            UpdateConnectionSettingsStatus();

            // Hide advanced settings by default
            groupConnectionSettings.Visible = false;

            Message("Connection settings initialized from defaults",
                EventType.Information, 2007);
        }

        private void UpdateSSLControls()
        {
            bool sslEnabled = checkboxUseSSL.Checked;
            checkboxSkipCACheck.Enabled = sslEnabled;
            checkboxSkipCNCheck.Enabled = sslEnabled;

            // Auto-set port when SSL checkbox changes
            if (numericPort.Value == 0 || numericPort.Value == 5985 || numericPort.Value == 5986)
            {
                // Only auto-change if user hasn't set a custom port
                numericPort.Value = 0; // Will be auto-selected as 5985/5986 based on SSL
            }
        }

        private void UpdateConnectionSettingsStatus()
        {
            // Build status message showing current connection settings
            string protocol = checkboxUseSSL.Checked ? "HTTPS" : "HTTP";
            string port = numericPort.Value == 0 ? "Auto" : numericPort.Value.ToString();
            string authMechanism = comboAuthMechanism.SelectedItem?.ToString() ?? "Default";

            // Show auth context from authentication method selection
            string authContext = radioWindows.Checked 
                ? $"Windows ({WindowsIdentity.GetCurrent().Name})" 
                : "Custom Credentials";

            // Build comprehensive status message
            string statusMessage = $"{authContext} | {protocol}:{port} | {authMechanism}";

            UpdateStatusLabel(statusMessage);

            Message($"Status updated - {statusMessage}",
                EventType.Information, 2015);
        }

        private void ApplyUIToConnectionSettings()
        {
            _currentConnectionSettings.UseSSL = checkboxUseSSL.Checked;
            _currentConnectionSettings.Port = (int)numericPort.Value;
            _currentConnectionSettings.AuthenticationMechanism = comboAuthMechanism.SelectedItem?.ToString() ?? "Default";
            _currentConnectionSettings.SkipCACheck = checkboxSkipCACheck.Checked;
            _currentConnectionSettings.SkipCNCheck = checkboxSkipCNCheck.Checked;
            _currentConnectionSettings.TimeoutSeconds = (int)numericTimeout.Value;

            string protocol = _currentConnectionSettings.UseSSL ? "HTTPS" : "HTTP";
            Message($"Connection settings updated from UI - {protocol}, Port: {_currentConnectionSettings.Port}, Auth: {_currentConnectionSettings.AuthenticationMechanism}",
                EventType.Information, 2008);
        }

        #region Hyper-V Detection Methods

        private bool TestLocalHyperVInstallation()
        {
            // Show we are testing for Hyper-V status
            Message("Testing for local Hyper-V installation...",
                EventType.Information, 1051);

            // Method 1: Service check
            if (TestHyperVService())
                return true;

            // Method 2: Windows Feature check
            if (TestHyperVWindowsFeature())
                return true;

            // Method 3: Server Role check
            if (TestHyperVServerRole())
                return true;

            // If no methods detected Hyper-V, return false
            Message("No local Hyper-V installation detected",
                EventType.Information, 1052);
            return false;
        }

        private bool TestHyperVService()
        {
            try
            {
                Message("Checking Hyper-V service...",
                    EventType.Information, 1072);

                using (ServiceController sc = new ServiceController("vmms"))
                {
                    // Check if service exists by accessing its status
                    // This will throw InvalidOperationException if service doesn't exist
                    var status = sc.Status;

                    Message($"Hyper-V service detected (Status: {status})",
                        EventType.Information, 1073);

                    if (status == ServiceControllerStatus.Running)
                    {
                        Message("Hyper-V Virtual Machine Management service is running",
                            EventType.Information, 1074);
                        return true;
                    }
                    else
                    {
                        Message($"Hyper-V service exists but is not running (Status: {status})",
                            EventType.Warning, 1075);
                        return false;
                    }
                }
            }
            catch (InvalidOperationException)
            {
                // Service doesn't exist
                Message("Hyper-V Virtual Machine Management service not found",
                    EventType.Information, 1076);
                return false;
            }
            catch (Exception ex)
            {
                Message($"Error checking Hyper-V service: {ex.Message}",
                    EventType.Warning, 1077);
                return false;
            }
        }

        private bool TestHyperVWindowsFeature()
        {
            try
            {
                Message("Checking Windows Optional Feature for Hyper-V using (WMI)...",
                    EventType.Information, 1053);

                // Query Win32_OptionalFeature for Hyper-V related features
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(
                    "root\\CIMV2",
                    "SELECT Name, InstallState FROM Win32_OptionalFeature WHERE Name LIKE '%Hyper-V%' OR Name = 'Microsoft-Hyper-V'"))
                {
                    foreach (var o in searcher.Get())
                    {
                        var feature = (ManagementObject)o;
                        string? featureName = feature["Name"]?.ToString();
                        uint installState = Convert.ToUInt32(feature["InstallState"]);

                        // InstallState values: 1 = Enabled, 2 = Disabled, 3 = Absent
                        if (installState == 1)
                        {
                            Message($"Hyper-V feature detected as enabled: {featureName}",
                                EventType.Information, 1054);

                            // Verify service is running using 
                            if (TestHyperVServiceStatus())
                            {
                                return true;
                            }
                            else
                            {
                                Message("Hyper-V feature is enabled but service is not running",
                                    EventType.Warning, 1055);
                                return false;
                            }
                        }
                    }

                    Message("No enabled Hyper-V features found in WMI",
                        EventType.Information, 1083);
                }
            }
            catch (ManagementException ex)
            {
                Message($"WMI query for Windows Optional Features failed: {ex.Message}",
                    EventType.Information, 1056);
            }
            catch (Exception ex)
            {
                Message($"Windows Optional Feature check failed: {ex.Message}",
                    EventType.Information, 1056);
            }

            return false;
        }

        private bool TestHyperVServerRole()
        {
            try
            {
                Message("Checking Windows Server role for Hyper-V using (WMI)...",
                    EventType.Information, 1062);

                // Query Win32_ServerFeature for Hyper-V role
                // Hyper-V role ID is typically 20 (Microsoft-Hyper-V)
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(
                    "root\\CIMV2",
                    "SELECT Name, ID FROM Win32_ServerFeature WHERE Name LIKE '%Hyper-V%'"))
                {
                    foreach (var o in searcher.Get())
                    {
                        var feature = (ManagementObject)o;
                        string? featureName = feature["Name"]?.ToString();

                        Message($"Hyper-V role detected on Windows Server: {featureName}",
                            EventType.Information, 1063);

                        // Verify service is running using 
                        if (TestHyperVServiceStatus())
                        {
                            return true;
                        }
                        else
                        {
                            Message("Hyper-V role is installed but service is not running",
                                EventType.Warning, 1064);
                            return false;
                        }
                    }

                    Message("No Hyper-V server roles found in WMI",
                        EventType.Information, 1084);
                }
            }
            catch (ManagementException ex)
            {
                Message($"WMI query for Windows Server roles failed: {ex.Message}",
                    EventType.Information, 1065);
            }
            catch (Exception ex)
            {
                Message($"Windows Server role check failed: {ex.Message}",
                    EventType.Information, 1065);
            }

            return false;
        }

        private bool TestHyperVServiceStatus()
        {
            try
            {
                Message("Checking Hyper-V service status...",
                    EventType.Information, 1078);

                using (ServiceController sc = new ServiceController("vmms"))
                {
                    var status = sc.Status;

                    if (status == ServiceControllerStatus.Running)
                    {
                        Message("Hyper-V Virtual Machine Management service is running",
                            EventType.Information, 1079);
                        return true;
                    }
                    else
                    {
                        Message($"Hyper-V Virtual Machine Management service is not running (Status: {status})",
                            EventType.Warning, 1080);
                        return false;
                    }
                }
            }
            catch (InvalidOperationException)
            {
                Message("Hyper-V Virtual Machine Management service not found",
                    EventType.Warning, 1081);
                return false;
            }
            catch (Exception ex)
            {
                Message($"Error checking Hyper-V service status: {ex.Message}",
                    EventType.Error, 1082);
                return false;
            }
        }

        #endregion Hyper-V Detection Methods

        #region Tests

        private async Task<ConnectionTestResult> TestHyperVConnection(string serverName, PSCredential? credential)
        {
            return await Task.Run(() =>
            {
                try
                {
                    bool isLocal = IsLocalComputer(serverName);

                    Message($"Testing connection to '{serverName}' (Local: {isLocal})...",
                        EventType.Information, 1003);

                    if (isLocal)
                    {
                        return TestLocalHyperV();
                    }
                    else
                    {
                        return TestRemoteHyperV(serverName, credential);
                    }
                }
                catch (Exception ex)
                {
                    Message($"Connection test exception: {ex.Message}",
                        EventType.Error, 1004);
                    return new ConnectionTestResult
                    {
                        Success = false,
                        Error = ex.Message
                    };
                }
            });
        }

        private static bool IsLocalComputer(string computerName)
        {
            if (string.IsNullOrWhiteSpace(computerName))
                return true;

            computerName = computerName.Trim();

            // Check common local names
            if (computerName == "." ||
                string.Equals(computerName, "localhost", StringComparison.OrdinalIgnoreCase) ||
                computerName == "127.0.0.1" ||
                computerName == "::1")
                return true;

            // Check against actual computer name
            if (string.Equals(computerName, Environment.MachineName, StringComparison.OrdinalIgnoreCase))
                return true;

            // Check if it's a local IP address (IPv4)
            if (System.Text.RegularExpressions.Regex.IsMatch(computerName, @"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$"))
            {
                try
                {
                    // Get all local IPv4 addresses
                    var localIPs = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
                        .Where(ni => ni.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up)
                        .SelectMany(ni => ni.GetIPProperties().UnicastAddresses)
                        .Where(addr => addr.Address.AddressFamily == AddressFamily.InterNetwork)
                        .Select(addr => addr.Address.ToString())
                        .Where(ip => !ip.StartsWith("169.254.")) // Exclude APIPA addresses as they are not routable
                        .Distinct()
                        .ToList();

                    if (localIPs.Contains(computerName))
                    {
                        Message($"'{computerName}' found in local IP addresses",
                            EventType.Information, 1043);
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Message($"Error checking local IP addresses: {ex.Message}",
                        EventType.Warning, 1046);
                }
            }

            // Check against hostname
            try
            {
                string hostname = System.Net.Dns.GetHostName();
                if (string.Equals(computerName, hostname, StringComparison.OrdinalIgnoreCase))
                    return true;

                // Check against FQDN
                var hostEntry = System.Net.Dns.GetHostEntry(hostname);
                if (string.Equals(computerName, hostEntry.HostName, StringComparison.OrdinalIgnoreCase))
                    return true;

                // Check against all aliases
                foreach (var alias in hostEntry.Aliases)
                {
                    if (string.Equals(computerName, alias, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }
            catch
            {
                // Ignore DNS lookup failures
            }

            return false;
        }

        private ConnectionTestResult TestLocalHyperV()
        {
            try
            {
                Message($"Starting local Hyper-V connection test...",
                    EventType.Information, 1030);

                // Check if Hyper-V module is available
                using (Runspace runspace = RunspaceFactory.CreateRunspace())
                {
                    runspace.Open();
                    Message($"Local runspace opened successfully",
                        EventType.Information, 1031);

                    using (PowerShell ps = PowerShell.Create())
                    {
                        ps.Runspace = runspace;

                        // Check for Hyper-V module
                        ps.AddScript("Get-Module -ListAvailable -Name Hyper-V");
                        Message($"Checking for Hyper-V PowerShell module...",
                            EventType.Information, 1032);

                        var moduleResult = ps.Invoke();

                        if (moduleResult == null || moduleResult.Count == 0)
                        {
                            Message($"Hyper-V PowerShell module is not available on this system",
                                EventType.Error, 1038);

                            return new ConnectionTestResult
                            {
                                Success = false,
                                Error = "Hyper-V PowerShell module is not installed or available on this machine."
                            };
                        }

                        Message($"Hyper-V module found, importing module...",
                            EventType.Information, 1033);

                        // Import Hyper-V module
                        ps.Commands.Clear();
                        ps.AddScript("Import-Module Hyper-V -Force");
                        ps.Invoke();

                        if (ps.HadErrors)
                        {
                            var error = ps.Streams.Error[0];
                            Message($"Failed to import Hyper-V module: {error.Exception?.Message}",
                                EventType.Error, 1085);
                        }
                        else
                        {
                            Message("Hyper-V module imported successfully",
                                EventType.Information, 1086);
                        }

                        // Check administrator privileges
                        bool isAdmin = new WindowsPrincipal(WindowsIdentity.GetCurrent())
                            .IsInRole(WindowsBuiltInRole.Administrator);

                        if (!isAdmin)
                        {
                            Message("Administrator privileges required for local Hyper-V access if not member of 'Hyper-V Administrators' Group",
                                EventType.Warning, 1087);
                        }

                        // Check Hyper-V service
                        Message("Checking Hyper-V Virtual Machine Management service...",
                            EventType.Information, 1088);

                        using (ServiceController sc = new ServiceController(@"vmms"))
                        {
                            var status = sc.Status;

                            if (status != ServiceControllerStatus.Running)
                            {
                                Message($"Hyper-V service is not running (Status: {status})",
                                    EventType.Error, 1089);

                                return new ConnectionTestResult
                                {
                                    Success = false,
                                    Error = $"Hyper-V Virtual Machine Management service is not running (Status: {status})"
                                };
                            }

                            Message("Hyper-V service is running",
                                EventType.Information, 1090);
                        }

                        // Get VMHost information
                        ps.Commands.Clear();
                        ps.AddScript("Get-VMHost -ErrorAction Stop");

                        Message($"Retrieving Hyper-V host information...",
                            EventType.Information, 1091);

                        var hostResult = ps.Invoke();

                        if (ps.HadErrors)
                        {
                            var error = ps.Streams.Error[0];
                            string errorMessage = error.Exception?.Message ?? error.ToString();

                            Message($"Failed to get VMHost information: {errorMessage}",
                                EventType.Error, 1017);

                            // Check if it's an elevation/permission issue
                            if (errorMessage.Contains("required permission") ||
                                errorMessage.Contains("Access is denied") ||
                                errorMessage.Contains("Administrator") ||
                                errorMessage.Contains("authorization policy") ||
                                errorMessage.Contains("elevation"))
                            {
                                Message($"Access denied detected - elevation required",
                                    EventType.Warning, 1036);

                                return new ConnectionTestResult
                                {
                                    Success = false,
                                    Error = "Access denied. Administrator privileges or membership in the 'Hyper-V Administrators' group is required for local Hyper-V management.",
                                    RequiresElevation = true,
                                    CanAutoElevate = true
                                };
                            }

                            return new ConnectionTestResult
                            {
                                Success = false,
                                Error = $"Failed to retrieve Hyper-V host information: {errorMessage}"
                            };
                        }

                        if (hostResult == null || hostResult.Count == 0)
                        {
                            Message("No VMHost information returned",
                                EventType.Error, 1092);

                            return new ConnectionTestResult
                            {
                                Success = false,
                                Error = "Unable to retrieve Hyper-V host information"
                            };
                        }

                        var vmHost = hostResult[0];
                        string hostName = vmHost.Properties["Name"]?.Value?.ToString() ?? Environment.MachineName;
                        string fqdn = vmHost.Properties["FullyQualifiedDomainName"]?.Value?.ToString() ?? hostName;
                        int logicalProcessors = Convert.ToInt32(vmHost.Properties["LogicalProcessorCount"]?.Value ?? 0);
                        long memoryCapacity = Convert.ToInt64(vmHost.Properties["MemoryCapacity"]?.Value ?? 0);
                        double memoryGb = Math.Round(memoryCapacity / 1024.0 / 1024.0 / 1024.0, 2);

                        Message($"Host information retrieved - Name: '{hostName}', FQDN: '{fqdn}', Processors: {logicalProcessors}, Memory: {memoryGb} GB",
                            EventType.Information, 1093);

                        // Get Hyper-V version
                        string? hyperVVersion = "Unknown";
                        try
                        {
                            var versionProperty = vmHost.Properties["HyperVVersion"]?.Value;
                            if (versionProperty != null)
                            {
                                hyperVVersion = versionProperty.ToString();
                            }
                            else
                            {
                                versionProperty = vmHost.Properties["Version"]?.Value;
                                if (versionProperty != null)
                                {
                                    hyperVVersion = versionProperty.ToString();
                                }
                            }

                            Message($"Hyper-V version: {hyperVVersion}",
                                EventType.Information, 1094);
                        }
                        catch (Exception ex)
                        {
                            Message($"Could not determine Hyper-V version: {ex.Message}",
                                EventType.Warning, 1095);
                        }

                        // Get VM count
                        ps.Commands.Clear();
                        ps.AddScript("Get-VM -ErrorAction SilentlyContinue");

                        Message($"Retrieving VM list...",
                            EventType.Information, 1034);

                        var vmResult = ps.Invoke();
                        int vmCount = vmResult?.Count ?? 0;

                        Message($"Found {vmCount} virtual machines",
                            EventType.Information, 1096);

                        // Check for cluster configuration
                        bool isCluster = false;
                        string? clusterName = null;

                        try
                        {
                            ps.Commands.Clear();
                            ps.AddScript(@"
                                $cluster = Get-Cluster -ErrorAction SilentlyContinue
                                if ($cluster) {
                                    return @{
                                        IsCluster = $true
                                        ClusterName = $cluster.Name
                                    }
                                } else {
                                    return @{ IsCluster = $false }
                                }
                            ");

                            Message("Testing for cluster configuration...",
                                EventType.Information, 1097);

                            var clusterResult = ps.Invoke();

                            if (clusterResult != null && clusterResult.Count > 0)
                            {
                                var clusterInfo = clusterResult[0];
                                var hashtable = (System.Collections.Hashtable)clusterInfo.BaseObject;
                                isCluster = (bool)hashtable["IsCluster"]!;

                                if (isCluster)
                                {
                                    clusterName = hashtable["ClusterName"]?.ToString();
                                    Message($"Connected to Hyper-V cluster: '{clusterName}' (Node: '{hostName}')",
                                        EventType.Information, 1098);
                                }
                                else
                                {
                                    Message($"Connected to standalone Hyper-V host: '{hostName}'",
                                        EventType.Information, 1099);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Message($"Cluster detection check failed (this is normal for standalone hosts): {ex.Message}",
                                EventType.Information, 1100);
                        }

                        Message($"Local Hyper-V connection successful",
                            EventType.Information, 1005);

                        return new ConnectionTestResult
                        {
                            Success = true,
                            VmCount = vmCount,
                            IsLocal = true,
                            HostName = hostName,
                            FullyQualifiedDomainName = fqdn,
                            HyperVVersion = hyperVVersion,
                            LogicalProcessorCount = logicalProcessors,
                            TotalMemoryGb = memoryGb,
                            IsCluster = isCluster,
                            ClusterName = clusterName
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Message($"Local Hyper-V test fatal error: {ex.GetType().Name} - {ex.Message}",
                    EventType.Error, 1006);
                Message($"Stack trace: {ex.StackTrace}",
                    EventType.Error, 1006);

                // Check if it's a permission/elevation issue
                string errorMessage = ex.Message;
                string exceptionType = ex.GetType().Name;

                // ActionPreferenceStopException is thrown when -ErrorAction Stop is used
                // and a non-terminating error occurs (like permission denied)
                if (exceptionType == "ActionPreferenceStopException" ||
                    errorMessage.Contains("required permission") ||
                    errorMessage.Contains("Access is denied") ||
                    errorMessage.Contains("Administrator") ||
                    errorMessage.Contains("authorization policy") ||
                    errorMessage.Contains("Hyper-V Administrators") ||
                    errorMessage.Contains("elevation"))
                {
                    Message("Permission error detected - Administrator privileges or Hyper-V Administrators group membership required",
                        EventType.Warning, 1095);

                    return new ConnectionTestResult
                    {
                        Success = false,
                        Error = "Access denied. To manage local Hyper-V, you must:\n\n" +
                                "• Run this application as Administrator, OR\n" +
                                "• Be a member of the 'Hyper-V Administrators' group\n\n" +
                                "Would you like to restart the application as Administrator?",
                        RequiresElevation = true,
                        CanAutoElevate = true
                    };
                }

                return new ConnectionTestResult
                {
                    Success = false,
                    Error = $"Failed to connect to local Hyper-V: {errorMessage}"
                };
            }
        }

        private ConnectionTestResult TestRemoteHyperV(string serverName, PSCredential? credential)
        {
            Runspace? tempRunspace = null;

            try
            {
                Message($"Testing remote connection to '{serverName}' with credentials of '{credential?.UserName ?? "Windows Authentication"}'",
                    EventType.Information, 1007);

                // Test basic connectivity
                Message($"Testing network connectivity to '{serverName}' on WinRM ports...",
                    EventType.Information, 1101);

                if (!TestNetworkConnection(serverName, 5985) && !TestNetworkConnection(serverName, 5986))
                {
                    Message($"Network connectivity test failed - WinRM ports not accessible",
                        EventType.Error, 1102);

                    return new ConnectionTestResult
                    {
                        Success = false,
                        Error = $"Cannot connect to '{serverName}' on WinRM ports (5985/5986). " +
                                "Ensure WinRM is enabled and accessible."
                    };
                }

                Message($"Network connectivity test successful",
                    EventType.Information, 1103);

                // Test PowerShell remoting with advanced connection settings
                tempRunspace = RunspaceFactory.CreateRunspace();
                tempRunspace.Open();

                Message($"Temporary runspace opened successfully",
                    EventType.Information, 1104);

                using (PowerShell ps = PowerShell.Create())
                {
                    ps.Runspace = tempRunspace;

                    // Create WSManConnectionInfo with user-configured settings
                    int port = _currentConnectionSettings.Port;
                    if (port <= 0)
                    {
                        // Auto-select port based on SSL setting
                        port = _currentConnectionSettings.UseSSL ? 5986 : 5985;
                    }

                    string credentialContext = credential != null
                        ? $"Explicit Credentials ({credential.UserName})"
                        : $"Current User ({WindowsIdentity.GetCurrent().Name})";

                    Message($"Connection Settings Applied - Target: '{serverName}', Protocol: {(_currentConnectionSettings.UseSSL ? "HTTPS" : "HTTP")}, Port: {port}, Auth: {_currentConnectionSettings.AuthenticationMechanism}, Credentials: {credentialContext}, Timeout: {_currentConnectionSettings.TimeoutSeconds}s",
                        EventType.Information, 1113);

                    var connectionInfo = new WSManConnectionInfo
                    {
                        ComputerName = serverName,
                        Port = port,
                        Scheme = _currentConnectionSettings.UseSSL ? "https" : "http",
                        AuthenticationMechanism = (AuthenticationMechanism)Enum.Parse(
                            typeof(AuthenticationMechanism),
                            _currentConnectionSettings.AuthenticationMechanism,
                            true),
                        OperationTimeout = (int)TimeSpan.FromSeconds(_currentConnectionSettings.TimeoutSeconds).TotalMilliseconds,
                        OpenTimeout = (int)TimeSpan.FromSeconds(_currentConnectionSettings.TimeoutSeconds).TotalMilliseconds
                    };

                    // Apply certificate validation settings
                    if (_currentConnectionSettings.UseSSL)
                    {
                        connectionInfo.SkipCACheck = _currentConnectionSettings.SkipCACheck;
                        connectionInfo.SkipCNCheck = _currentConnectionSettings.SkipCNCheck;

                        Message($"SSL/HTTPS enabled - Certificate validation: CA Check: {!_currentConnectionSettings.SkipCACheck}, CN Check: {!_currentConnectionSettings.SkipCNCheck}",
                            EventType.Information, 1114);
                    }

                    // Set credentials if provided
                    if (credential != null)
                    {
                        connectionInfo.Credential = credential;
                        Message($"Authentication: Using explicit credentials for user '{credential.UserName}'",
                            EventType.Information, 1115);
                    }
                    else
                    {
                        Message($"Authentication: Using current Windows user context '{WindowsIdentity.GetCurrent().Name}'",
                            EventType.Information, 1116);
                    }

                    // Log the actual connection URI being used
                    string connectionUri = $"{connectionInfo.Scheme}://{connectionInfo.ComputerName}:{connectionInfo.Port}/wsman";
                    Message($"WinRM Connection URI: {connectionUri}",
                        EventType.Information, 1120);

                    // Create the remote runspace using the configured connection info
                    Runspace remoteRunspace;
                    try
                    {
                        remoteRunspace = RunspaceFactory.CreateRunspace(connectionInfo);
                        remoteRunspace.Open();

                        Message($"Remote runspace opened successfully to '{serverName}'",
                            EventType.Information, 1117);

                        // Validate connection state
                        Message($"Connection State: {remoteRunspace.RunspaceStateInfo.State}, Availability: {remoteRunspace.RunspaceAvailability}",
                            EventType.Information, 1121);

                        // Get the authenticated user on remote system to validate credentials
                        using (PowerShell validatePs = PowerShell.Create())
                        {
                            validatePs.Runspace = remoteRunspace;
                            validatePs.AddScript("$env:USERNAME + '@' + $env:USERDOMAIN");
                            var userResult = validatePs.Invoke();

                            if (userResult != null && userResult.Count > 0)
                            {
                                string authenticatedAs = userResult[0]?.ToString() ?? "Unknown";
                                Message($"Successfully authenticated on remote system as: {authenticatedAs}",
                                    EventType.Information, 1122);
                            }
                        }

                        Message($"PowerShell session created successfully to '{serverName}'",
                            EventType.Information, 1025);
                    }
                    catch (Exception ex)
                    {
                        Message($"Failed to open remote runspace: {ex.Message}",
                            EventType.Error, 1118);

                        return new ConnectionTestResult
                        {
                            Success = false,
                            Error = $"Failed to create PowerShell session: {ex.Message}"
                        };
                    }

                    // Enhanced Hyper-V availability and information gathering
                    // Execute commands directly on the remote runspace
                    using (PowerShell remotePs = PowerShell.Create())
                    {
                        remotePs.Runspace = remoteRunspace;
                        remotePs.AddScript(@"
                            $result = @{
                                Available = $false
                                VMCount = 0
                                Error = $null
                                HostName = $null
                                FQDN = $null
                                LogicalProcessors = 0
                                MemoryGB = 0
                                HyperVVersion = 'Unknown'
                                IsCluster = $false
                                ClusterName = $null
                            }

                            try {
                                # Check for Hyper-V module
                                $module = Get-Module -ListAvailable -Name Hyper-V -ErrorAction SilentlyContinue
                                if (-not $module) {
                                    $result.Error = 'Hyper-V module not found'
                                    return $result
                                }

                                # Import Hyper-V module
                                Import-Module Hyper-V -Force -ErrorAction SilentlyContinue

                                # Get VM count
                                $vms = Get-VM -ErrorAction SilentlyContinue
                                $result.VMCount = ($vms | Measure-Object).Count

                                # Get VMHost information
                                $vmHost = Get-VMHost -ErrorAction SilentlyContinue
                                if ($vmHost) {
                                    $result.HostName = $vmHost.Name
                                    $result.FQDN = $vmHost.FullyQualifiedDomainName
                                    $result.LogicalProcessors = $vmHost.LogicalProcessorCount
                                    $result.MemoryGB = [math]::Round($vmHost.MemoryCapacity / 1GB, 2)

                                    # Try to get Hyper-V version
                                    if ($vmHost.HyperVVersion) {
                                        $result.HyperVVersion = $vmHost.HyperVVersion
                                    } elseif ($vmHost.Version) {
                                        $result.HyperVVersion = $vmHost.Version
                                    }
                                }

                                # Check for cluster - wrap in separate try/catch to handle missing FailoverClustering module
                                try {
                                    $cluster = Get-Cluster -ErrorAction Stop
                                    if ($cluster) {
                                        $result.IsCluster = $true
                                        $result.ClusterName = $cluster.Name
                                    }
                                }
                                catch {
                                    # Cluster cmdlet not available or host not in cluster - this is normal for standalone hosts
                                    $result.IsCluster = $false
                                    $result.ClusterName = $null
                                }

                                $result.Available = $true
                            }
                            catch {
                                $result.Error = $_.Exception.Message
                            }

                            return $result
                        ");

                        Message($"Retrieving Hyper-V information from '{serverName}'...",
                            EventType.Information, 1026);

                        var hyperVResult = remotePs.Invoke();

                        if (remotePs.HadErrors)
                        {
                            var error = remotePs.Streams.Error[0];
                            string errorMsg = error.Exception?.Message ?? error.ToString();

                            Message($"Hyper-V information retrieval failed: {errorMsg}",
                                EventType.Error, 1027);

                            // Close the remote runspace before returning
                            try { remoteRunspace.Close(); remoteRunspace.Dispose(); } catch { /* ignore */ }

                            return new ConnectionTestResult
                            {
                                Success = false,
                                Error = $"Failed to retrieve Hyper-V information from '{serverName}': {errorMsg}"
                            };
                        }

                        if (hyperVResult != null && hyperVResult.Count > 0)
                        {
                            var result = hyperVResult[0];
                            var hashtable = (System.Collections.Hashtable)result.BaseObject;

                            bool available = (bool)hashtable["Available"]!;

                            if (!available)
                            {
                                string error = hashtable["Error"]?.ToString() ?? "Unknown error";

                                Message($"Hyper-V not available on '{serverName}': {error}",
                                    EventType.Warning, 1028);

                                // Close the remote runspace before returning
                                try { remoteRunspace.Close(); remoteRunspace.Dispose(); } catch { /* ignore */ }

                                return new ConnectionTestResult
                                {
                                    Success = false,
                                    Error = $"Hyper-V module not available or accessible on '{serverName}'. {error}"
                                };
                            }

                            // Extract all information
                            int vmCount = Convert.ToInt32(hashtable["VMCount"] ?? 0);
                            string hostName = hashtable["HostName"]?.ToString() ?? serverName;
                            string fqdn = hashtable["FQDN"]?.ToString() ?? hostName;
                            int logicalProcessors = Convert.ToInt32(hashtable["LogicalProcessors"] ?? 0);
                            double memoryGb = Convert.ToDouble(hashtable["MemoryGB"] ?? 0);
                            string hyperVVersion = hashtable["HyperVVersion"]?.ToString() ?? "Unknown";
                            bool isCluster = Convert.ToBoolean(hashtable["IsCluster"] ?? false);
                            string clusterName = hashtable["ClusterName"]?.ToString()!;

                            Message($"Host information retrieved - Name: '{hostName}', FQDN: '{fqdn}', Processors: {logicalProcessors}, Memory: {memoryGb} GB",
                                EventType.Information, 1105);

                            Message($"Hyper-V version: {hyperVVersion}",
                                EventType.Information, 1106);

                            Message($"Found {vmCount} virtual machines",
                                EventType.Information, 1107);

                            if (isCluster)
                            {
                                Message($"Connected to Hyper-V cluster: '{clusterName}' (Node: '{hostName}')",
                                    EventType.Information, 1108);
                            }
                            else
                            {
                                Message($"Connected to standalone Hyper-V host: '{hostName}'",
                                    EventType.Information, 1109);
                            }

                            Message($"Remote Hyper-V connection successful",
                                EventType.Information, 1008);

                            // Close the remote runspace after successful test
                            try { remoteRunspace.Close(); remoteRunspace.Dispose(); } catch { /* ignore */ }

                            return new ConnectionTestResult
                            {
                                Success = true,
                                VmCount = vmCount,
                                IsLocal = false,
                                HostName = hostName,
                                FullyQualifiedDomainName = fqdn,
                                HyperVVersion = hyperVVersion,
                                LogicalProcessorCount = logicalProcessors,
                                TotalMemoryGb = memoryGb,
                                IsCluster = isCluster,
                                ClusterName = clusterName
                            };
                        }

                        Message($"No results returned from Hyper-V information query on '{serverName}'",
                            EventType.Error, 1029);

                        // Close the remote runspace before returning
                        try { remoteRunspace.Close(); remoteRunspace.Dispose(); } catch { /* ignore */ }

                        return new ConnectionTestResult
                        {
                            Success = false,
                            Error = $"No response from Hyper-V information query on '{serverName}'"
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Message($"Remote connection test exception: {ex.GetType().Name} - {ex.Message}",
                    EventType.Error, 1009);
                Message($"Stack trace: {ex.StackTrace}",
                    EventType.Error, 1009);

                return new ConnectionTestResult
                {
                    Success = false,
                    Error = $"Connection test failed: {ex.Message}"
                };
            }
            finally
            {
                // Clean up the temporary runspace
                if (tempRunspace != null)
                {
                    try
                    {
                        tempRunspace.Close();
                        tempRunspace.Dispose();
                    }
                    catch
                    {
                        // Ignore cleanup errors
                    }
                }
            }
        }

        #endregion Tests

        #region Credential Storage

        private class SavedCredential
        {
            public string? Username { get; set; }
            public string? Password { get; set; }
        }

        private void SaveCredentials(string server, string username, string password)
        {
            try
            {
                if (!Directory.Exists(FileManager.ProgramDataFilePath))
                {
                    Directory.CreateDirectory(FileManager.ProgramDataFilePath);
                }

                // Create a safe filename from the server name
                string safeServerName = GetSafeFileName(server);
                string credFile = Path.Combine(FileManager.ProgramDataFilePath, $"cred_{safeServerName}.dat");

                // Encrypt credentials using DPAPI
                byte[] serverBytes = Encoding.UTF8.GetBytes(server);
                byte[] usernameBytes = Encoding.UTF8.GetBytes(username);
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

                byte[] encryptedServer = ProtectedData.Protect(serverBytes, null, DataProtectionScope.CurrentUser);
                byte[] encryptedUsername = ProtectedData.Protect(usernameBytes, null, DataProtectionScope.CurrentUser);
                byte[] encryptedPassword = ProtectedData.Protect(passwordBytes, null, DataProtectionScope.CurrentUser);

                using (FileStream fs = new FileStream(credFile, FileMode.Create, FileAccess.Write))
                using (BinaryWriter writer = new BinaryWriter(fs))
                {
                    writer.Write(encryptedServer.Length);
                    writer.Write(encryptedServer);
                    writer.Write(encryptedUsername.Length);
                    writer.Write(encryptedUsername);
                    writer.Write(encryptedPassword.Length);
                    writer.Write(encryptedPassword);
                }

                Message($"Credentials saved (encrypted) for server '{server}'",
                    EventType.Information, 1010);
            }
            catch (Exception ex)
            {
                Message($"Failed to save credentials: {ex.Message}",
                    EventType.Error, 1011);
            }
        }

        private SavedCredential LoadServerCredentials(string serverName)
        {
            try
            {
                // Create a safe filename from the server name
                string safeServerName = GetSafeFileName(serverName);
                string credFile = Path.Combine(FileManager.ProgramDataFilePath, $"cred_{safeServerName}.dat");

                if (!File.Exists(credFile))
                    return null!;

                using (FileStream fs = new FileStream(credFile, FileMode.Open, FileAccess.Read))
                using (BinaryReader reader = new BinaryReader(fs))
                {
                    // Read and decrypt credentials
                    int serverLength = reader.ReadInt32();
                    byte[] encryptedServer = reader.ReadBytes(serverLength);

                    // Get username
                    int usernameLength = reader.ReadInt32();
                    byte[] encryptedUsername = reader.ReadBytes(usernameLength);

                    // Get password
                    int passwordLength = reader.ReadInt32();
                    byte[] encryptedPassword = reader.ReadBytes(passwordLength);

                    // Decrypt based on current user
                    byte[] serverBytes = ProtectedData.Unprotect(encryptedServer, null, DataProtectionScope.CurrentUser);
                    byte[] usernameBytes = ProtectedData.Unprotect(encryptedUsername, null, DataProtectionScope.CurrentUser);
                    byte[] passwordBytes = ProtectedData.Unprotect(encryptedPassword, null, DataProtectionScope.CurrentUser);

                    string storedServer = Encoding.UTF8.GetString(serverBytes);

                    // Verify the server name matches (security check)
                    if (!storedServer.Equals(serverName, StringComparison.OrdinalIgnoreCase))
                    {
                        Message($"Server name mismatch in credential file for '{serverName}'",
                            EventType.Warning, 1020);
                        return null!;
                    }

                    // Return the decrypted credentials
                    return new SavedCredential
                    {
                        Username = Encoding.UTF8.GetString(usernameBytes),
                        Password = Encoding.UTF8.GetString(passwordBytes)
                    };
                }
            }
            catch (Exception ex)
            {
                Message($"Failed to load credentials for '{serverName}': {ex.Message}",
                    EventType.Warning, 1013);
                return null!;
            }
        }

        private void LoadSavedCredentials()
        {
            try
            {
                // Try to load the default/last used credentials file (legacy support)
                string credFile = Path.Combine(FileManager.ProgramDataFilePath, "credentials.dat");

                if (!File.Exists(credFile))
                    return;

                // Get credentials from file and decrypt
                using (FileStream fs = new FileStream(credFile, FileMode.Open, FileAccess.Read))
                using (BinaryReader reader = new BinaryReader(fs))
                {
                    // Read and decrypt credentials
                    int serverLength = reader.ReadInt32();
                    byte[] encryptedServer = reader.ReadBytes(serverLength);

                    // Read username
                    int usernameLength = reader.ReadInt32();
                    byte[] encryptedUsername = reader.ReadBytes(usernameLength);

                    // Read password
                    int passwordLength = reader.ReadInt32();
                    byte[] encryptedPassword = reader.ReadBytes(passwordLength);

                    // Decrypt based on current user
                    byte[] serverBytes = ProtectedData.Unprotect(encryptedServer, null, DataProtectionScope.CurrentUser);
                    byte[] usernameBytes = ProtectedData.Unprotect(encryptedUsername, null, DataProtectionScope.CurrentUser);
                    byte[] passwordBytes = ProtectedData.Unprotect(encryptedPassword, null, DataProtectionScope.CurrentUser);

                    // Populate UI fields
                    textboxServer.Text = Encoding.UTF8.GetString(serverBytes);
                    textboxUsername.Text = Encoding.UTF8.GetString(usernameBytes);
                    textboxPassword.Text = Encoding.UTF8.GetString(passwordBytes);

                    // Set UI state
                    radioCustom.Checked = true;
                    checkboxRemember.Checked = true;

                    // Log
                    Message("Legacy credentials loaded", EventType.Information, 1012);

                    // Migrate to new format
                    string server = textboxServer.Text;
                    string username = textboxUsername.Text;
                    string password = textboxPassword.Text;
                    SaveCredentials(server, username, password);

                    // Delete old file
                    try
                    {
                        File.Delete(credFile);
                        Message("Migrated credentials to new server-specific format",
                            EventType.Information, 1021);
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
            catch (Exception ex)
            {
                Message($"Failed to load legacy credentials: {ex.Message}",
                    EventType.Warning, 1013);
                // Silently fail - credentials might be corrupted or from different user
            }
        }

        private void ClearSavedCredentials()
        {
            try
            {
                string serverName = textboxServer.Text.Trim();

                if (string.IsNullOrWhiteSpace(serverName))
                    return;

                // Delete server-specific credential file
                string safeServerName = GetSafeFileName(serverName);
                string credFile = Path.Combine(FileManager.ProgramDataFilePath, $"cred_{safeServerName}.dat");

                // Check if file exists before attempting deletion
                if (File.Exists(credFile))
                {
                    File.Delete(credFile);
                    Message($"Saved credentials cleared for server '{serverName}'",
                        EventType.Information, 1014);
                }
            }
            catch (Exception ex)
            {
                Message($"Failed to clear credentials: {ex.Message}",
                    EventType.Warning, 1015);
            }
        }

        private string GetSafeFileName(string serverName)
        {
            // Remove invalid filename characters and convert to safe format
            string safe = serverName.ToLower();
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                safe = safe.Replace(c, '_');
            }
            // Also replace some additional characters that might cause issues
            safe = safe.Replace('.', '_').Replace(':', '_').Replace('\\', '_').Replace('/', '_');
            return safe;
        }

        #endregion

        #region UI Handlers     

        private void UpdateStatusLabel(string message, bool? isSuccess = null)
        {
            try
            {
                if (toolStripStatusLabelTextLoginForm != null)
                {
                    toolStripStatusLabelTextLoginForm.Text = message;

                    // Update color based on status
                    if (isSuccess.HasValue)
                    {
                        toolStripStatusLabelTextLoginForm.ForeColor = isSuccess.Value
                            ? Color.Green
                            : Color.Orange;
                    }
                    else
                    {
                        toolStripStatusLabelTextLoginForm.ForeColor = SystemColors.ControlText;
                    }
                }
            }
            catch (Exception ex)
            {
                Message($"Error updating status label: {ex.Message}",
                    EventType.Warning, 1071);
            }
        }

        private void RadioAuth_CheckedChanged(object sender, EventArgs e)
        {
            // Set enabled state based on selected authentication method
            bool useCustomAuth = radioCustom.Checked;
            labelUsername.Enabled = useCustomAuth;
            textboxUsername.Enabled = useCustomAuth;
            labelPassword.Enabled = useCustomAuth;
            textboxPassword.Enabled = useCustomAuth;
            checkboxRemember.Enabled = useCustomAuth;

            // Uncheck "Remember Me" if switching to Windows Auth
            if (!useCustomAuth)
            {
                checkboxRemember.Checked = false;
            }

            // Update status to show current authentication context with connection settings
            UpdateConnectionSettingsStatus();

            string authContext = useCustomAuth 
                ? "Custom Credentials" 
                : $"Windows Auth ({WindowsIdentity.GetCurrent().Name})";

            Message($"Authentication method changed to: {authContext}",
                EventType.Information, 1126);
        }

        private void TextboxServer_KeyDown(object sender, KeyEventArgs e)
        {
            // Trigger login on Enter key
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;

                // Don't trigger login if already in progress
                if (_isConnecting)
                    return;

                if (radioWindows.Checked)
                {
                    ButtonLogin.PerformClick();
                }
                else
                {
                    textboxUsername.Focus();
                }
            }
        }

        private void TextboxUsername_KeyDown(object sender, KeyEventArgs e)
        {
            // Trigger login on Enter key
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;

                // Don't trigger login if already in progress
                if (_isConnecting)
                    return;

                if (string.IsNullOrWhiteSpace(textboxPassword.Text))
                {
                    textboxPassword.Focus();
                }
                else
                {
                    ButtonLogin.PerformClick();
                }
            }
        }

        private void TextboxPassword_KeyDown(object sender, KeyEventArgs e)
        {
            // Trigger login on Enter key
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;

                // Don't trigger login if already in progress
                if (_isConnecting)
                    return;

                ButtonLogin.PerformClick();
            }
        }

        private async void ButtonLogin_Click(object sender, EventArgs e)
        {
            // Prevent double-click or multiple simultaneous login attempts
            if (_isConnecting || !ButtonLogin.Enabled)
            {
                Message($"Login attempt blocked - already in progress or button disabled",
                    EventType.Warning, 1042);
                return;
            }

            // Apply UI connection settings to the current settings object
            ApplyUIToConnectionSettings();

            // Set flag to indicate connection in progress
            _isConnecting = true;

            // Immediately disable the button to prevent any possibility of re-entry
            ButtonLogin.Enabled = false;
            buttonCancel.Enabled = false;

            // Get server name
            string serverName = textboxServer.Text.Trim();

            // Validate input
            if (string.IsNullOrWhiteSpace(serverName))
            {
                MessageBox.Show(@"Please enter a server name or IP address.", Globals.MsgBox.Warning,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textboxServer.Focus();
                ButtonLogin.Enabled = true;
                buttonCancel.Enabled = true;
                _isConnecting = false;
                return;
            }

            // Validate custom credentials if selected
            if (radioCustom.Checked)
            {
                if (string.IsNullOrWhiteSpace(textboxUsername.Text))
                {
                    MessageBox.Show(@"Please enter a username.", Globals.MsgBox.Warning,
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textboxUsername.Focus();
                    ButtonLogin.Enabled = true;
                    buttonCancel.Enabled = true;
                    _isConnecting = false;
                    return;
                }

                if (string.IsNullOrWhiteSpace(textboxPassword.Text))
                {
                    MessageBox.Show(@"Please enter a password.", Globals.MsgBox.Warning,
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textboxPassword.Focus();
                    ButtonLogin.Enabled = true;
                    buttonCancel.Enabled = true;
                    _isConnecting = false;
                    return;
                }
            }

            // Save credentials if requested
            if (checkboxRemember.Checked && radioCustom.Checked)
            {
                SaveCredentials(serverName, textboxUsername.Text, textboxPassword.Text);
            }
            else if (!checkboxRemember.Checked)
            {
                ClearSavedCredentials();
            }

            // Disable UI and show progress (button already disabled above)
            string originalText = ButtonLogin.Text;
            ButtonLogin.Text = @"Connecting...";
            Cursor = Cursors.WaitCursor;

            // Log start of connection attempt
            Message($"Starting connection test to '{serverName}'",
                EventType.Information, 1044);

            bool useWindowsAuth = radioWindows.Checked;

            // Validate authentication method alignment
            string expectedAuthMechanism = useWindowsAuth ? "Default" : _currentConnectionSettings.AuthenticationMechanism;

            // Warn if there might be authentication conflicts
            if (useWindowsAuth && (_currentConnectionSettings.AuthenticationMechanism == "Basic" ||
                                   _currentConnectionSettings.AuthenticationMechanism == "CredSSP"))
            {
                Message($"Warning: Windows Authentication selected but Connection Settings specify '{_currentConnectionSettings.AuthenticationMechanism}'. This may cause authentication issues.",
                    EventType.Warning, 1123);
            }
            else if (!useWindowsAuth && _currentConnectionSettings.AuthenticationMechanism == "Kerberos")
            {
                Message($"Info: Using explicit credentials with Kerberos authentication. Ensure credentials are domain credentials.",
                    EventType.Information, 1124);
            }

            Message($"Authentication Mode: {(useWindowsAuth ? "Windows Authentication (Current User)" : "Custom Credentials")}",
                EventType.Information, 1125);

            try
            {
                PSCredential? credentials = null;

                if (!useWindowsAuth)
                {
                    SecureString securePassword = new SecureString();
                    foreach (char c in textboxPassword.Text)
                    {
                        securePassword.AppendChar(c);
                    }
                    securePassword.MakeReadOnly();
                    credentials = new PSCredential(textboxUsername.Text.Trim(), securePassword);
                }

                // Test connection
                var connectionResult = await TestHyperVConnection(serverName, credentials);

                if (connectionResult.Success)
                {
                    string connectedUser = useWindowsAuth
                        ? WindowsIdentity.GetCurrent().Name
                        : textboxUsername.Text.Trim();

                    string connectionType = useWindowsAuth
                        ? "Windows Authentication"
                        : "Custom Credentials";

                    // Build detailed connection summary
                    string protocol = _currentConnectionSettings.UseSSL ? "HTTPS" : "HTTP";
                    int actualPort = _currentConnectionSettings.Port > 0
                        ? _currentConnectionSettings.Port
                        : (_currentConnectionSettings.UseSSL ? 5986 : 5985);
                    string authMechanism = _currentConnectionSettings.AuthenticationMechanism;

                    string connectionSummary = $"{connectionType} via {protocol}:{actualPort} using {authMechanism}";

                    // Store result for legacy compatibility
                    Result = new LoginResult
                    {
                        Success = true,
                        ServerName = serverName,
                        UseWindowsAuth = useWindowsAuth,
                        Credentials = credentials,
                        ConnectedUser = connectedUser,
                        ConnectionType = connectionSummary,
                        VmCount = connectionResult.VmCount
                    };

                    // Initialize global session context for reuse across the application
                    SessionContext.Initialize(
                        serverName,
                        useWindowsAuth,
                        credentials,
                        connectedUser,
                        connectionSummary,
                        connectionResult.VmCount,
                        connectionResult.IsLocal,
                        connectionResult.HostName,
                        connectionResult.HyperVVersion,
                        connectionResult.LogicalProcessorCount,
                        connectionResult.TotalMemoryGb,
                        connectionResult.IsCluster,
                        connectionResult.ClusterName,
                        connectionResult.FullyQualifiedDomainName,
                        _currentConnectionSettings
                    );

                    Message($"Login successful for '{serverName}' as '{connectedUser}' using {connectionSummary}",
                        EventType.Information, 1016);

                    // Hide login form and show main form
                    Message($"Hiding login form and showing MainForm...",
                        EventType.Information, 1039);

                    Hide();

                    using (MainForm mainForm = new MainForm())
                    {
                        Message($"MainForm created, showing dialog...",
                            EventType.Information, 1040);

                        // Show main form as dialog
                        var mainResult = mainForm.ShowDialog();

                        Message($"MainForm closed with result: {mainResult}",
                            EventType.Information, 1041);

                        // If main form closes, clear session and close application
                        if (mainResult == DialogResult.OK || mainResult == DialogResult.Cancel)
                        {
                            SessionContext.Clear();
                            DialogResult = DialogResult.OK;
                            Close();
                        }
                    }
                }
                else
                {
                    // Handle connection failures
                    if (connectionResult.RequiresElevation && connectionResult.CanAutoElevate)
                    {
                        // Show detailed permission error with restart option
                        var elevationPrompt = "🔒 Administrator Privileges Required\n\n" +
                            "To manage local Hyper-V, you need ONE of the following:\n\n" +
                            "  • Run this application as Administrator\n" +
                            "  • Be a member of the 'Hyper-V Administrators' group\n\n" +
                            "Would you like to restart this application as Administrator now?";

                        Message("Prompting user for elevation due to permission error",
                            EventType.Information, 1096);

                        var result = MessageBox.Show(elevationPrompt, @"Elevation Required",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        if (result == DialogResult.Yes)
                        {
                            try
                            {
                                Message("User requested application restart with elevation",
                                    EventType.Information, 1000);
                                ApplicationFunctions.RestartAsAdmin();
                                Application.Exit();
                            }
                            catch (Exception ex)
                            {
                                Message($"Failed to start as admin: {ex.Message}",
                                    EventType.Error, 1001);
                                MessageBox.Show($@"Failed to restart as administrator:

{ex.Message}

" +
                                    @"Please close this application and manually run it as Administrator.",
                                    @"Elevation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        else
                        {
                            Message("User declined elevation, showing error details",
                                EventType.Information, 1097);

                            // Show alternative if user declines elevation
                            MessageBox.Show(
                                @"Unable to connect without proper permissions.

" +
                                @"Alternative solutions:
" +
                                @"  • Right-click the application and select 'Run as administrator'
" +
                                @"  • Ask your administrator to add you to the 'Hyper-V Administrators' group
" +
                                @"  • Connect to a remote Hyper-V host instead of localhost",
                                @"Connection Failed",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    else
                    {
                        string errorMessage = connectionResult.Error ?? "Unknown error";
                        string tips = "";

                        // Provide specific troubleshooting tips based on error type
                        if (errorMessage.Contains("Access is denied"))
                        {
                            tips = "\n\nTips:\n• Use Connection Settings to specify credentials\n• Ensure you have admin rights on the target host\n• For clusters, use FQDN and Kerberos authentication";
                        }
                        else if (errorMessage.Contains("WinRM") || errorMessage.Contains("cannot connect"))
                        {
                            tips = "\n\nTips:\n• Ensure WinRM is enabled: Enable-PSRemoting -Force\n• Check firewall rules for TCP 5985/5986\n• Try using SSL with port 5986\n• For clusters, try FQDN with Kerberos";
                        }
                        else if (errorMessage.Contains("Hyper-V"))
                        {
                            tips = "\n\nTips:\n• Ensure Hyper-V role is installed\n• Ensure Hyper-V PowerShell module is available";
                        }
                        else if (errorMessage.Contains("Kerberos") || errorMessage.Contains("SPN") || errorMessage.Contains("authentication"))
                        {
                            tips = "\n\nTips:\n• Use the fully qualified domain name (FQDN)\n• Enable Kerberos in Connection Settings\n• Verify SPNs are registered for the host";
                        }

                        MessageBox.Show($"Failed to connect to {serverName}\n\nError: {errorMessage}{tips}",
                            "Connection Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        UpdateStatusLabel("Connection failed");
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                string tips = "";

                // Provide specific troubleshooting tips based on error type
                if (message.Contains("Access is denied"))
                {
                    tips = "\n\nTips:\n• Use Connection Settings to specify credentials\n• Ensure you have admin rights on the target host\n• For clusters, use FQDN and Kerberos authentication";
                }
                else if (message.Contains("WinRM") || message.Contains("cannot connect"))
                {
                    tips = "\n\nTips:\n• Ensure WinRM is enabled: Enable-PSRemoting -Force\n• Check firewall rules for TCP 5985/5986\n• Try using SSL with port 5986\n• For clusters, try FQDN with Kerberos";
                }
                else if (message.Contains("Hyper-V"))
                {
                    tips = "\n\nTips:\n• Ensure Hyper-V role is installed\n• Ensure Hyper-V PowerShell module is available";
                }
                else if (message.Contains("Kerberos") || message.Contains("SPN") || message.Contains("authentication"))
                {
                    tips = "\n\nTips:\n• Use the fully qualified domain name (FQDN)\n• Enable Kerberos in Connection Settings\n• Verify SPNs are registered for the host";
                }

                Message($"Connection error: {message}", EventType.Error, 1002);
                MessageBox.Show($"Failed to connect:\n\n{message}{tips}",
                    "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                UpdateStatusLabel("Connection failed");
            }
            finally
            {
                ButtonLogin.Text = originalText;
                ButtonLogin.Enabled = true;
                buttonCancel.Enabled = true;
                Cursor = Cursors.Default;
                _isConnecting = false; // Reset the flag

                Message($"Login attempt completed, resetting UI",
                    EventType.Information, 1045);
            }
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            Result = new LoginResult { Success = false, Cancelled = true };
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private bool TestNetworkConnection(string hostname, int port)
        {
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    var result = client.BeginConnect(hostname, port, null, null);
                    var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(3));
                    if (success)
                    {
                        client.EndConnect(result);
                        return true;
                    }
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        private void textboxServer_TextChanged(object sender, EventArgs e)
        {
            // Skip during form initialization
            if (_isInitializing)
                return;

            // Trim server name for spaces
            string serverName = textboxServer.Text.Trim();

            // Try to load saved credentials when server changes
            if (radioCustom.Checked && serverName.Length > 2 && serverName != _lastServerChecked)
            {
                // Update last server checked
                _lastServerChecked = serverName;

                // Try to load server-specific credentials
                var savedCreds = LoadServerCredentials(serverName);

                // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract - else it closes the case in first run
                if (savedCreds != null)
                {
                    // Add username to UI
                    textboxUsername.Text = savedCreds.Username;
                    textboxPassword.Text = savedCreds.Password;

                    // Update UI
                    checkboxRemember.Checked = true;

                    // Log
                    Message($"Loading saved credentials for server '{serverName}' into application",
                        EventType.Information, 1019);
                }
                else
                {
                    // Clear credentials if no saved credentials found
                    if (!string.IsNullOrEmpty(textboxUsername.Text) || !string.IsNullOrEmpty(textboxPassword.Text))
                    {
                        textboxUsername.Text = string.Empty;
                        textboxPassword.Text = string.Empty;
                    }

                    // Update UI
                    checkboxRemember.Checked = false;
                }
            }
            else if (serverName.Length <= 2)
            {
                // Reset check when server name is too short
                _lastServerChecked = string.Empty;
            }
        }

        private void buttonHelpConnectGuide_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                @"🔍 To directly manage a single host, enter the IP address or host name. To manage multiple hosts, enter the IP address or name of a Cluster.

" +
                @"To connect to a Hyper-V server or Cluster, ensure the following prerequisites are met:

" +
                @"1. The Hyper-V/Cluster role(s) is installed on the target server.
" +
                @"2. PowerShell Remoting (WinRM) is enabled and accessible on the target server.
" +
                @"3. You have the necessary permissions to manage Hyper-V/Cluster on the target server.
" +
                @"4. If using custom credentials, ensure they are valid and have Hyper-V management rights.

" +
                @"For local connections, ensure you run this application with Administrator privileges or " +
                @"that your user account is a member of the 'Hyper-V Administrators' group.",
                @"Connection Guide", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion UI Handlers

        private void pictureboxSupportMe_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = Globals.ToolStings.UrlBuyMeaCoffie,
                    UseShellExecute = true
                });

                // Log the opening of the URL message
                Message("User clicked the 'Buy me a coffie' picture in MainForm to open the URL: '" + Globals.ToolStings.UrlBuyMeaCoffie + "'", EventType.Information, 1052);
            }
            catch (Exception ex)
            {
                // Show an error message if the URL could not be opened
                MessageBox.Show(@"Failed to open the URL '" + Globals.ToolStings.UrlBuyMeaCoffie + @"'. Error: " + ex.Message, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Log the error message
                Message("Failed to open the URL: " + ex.Message, EventType.Error, 1041);
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Log the user's action to open the About form
            Message("User clicked the 'About' menu item to open the About form", EventType.Information, 1056);

            // Open the About form
            AboutForm f2 = new AboutForm();
            f2.ShowDialog();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void myWebpageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = Globals.ToolStings.UrlMyWebPage,
                    UseShellExecute = true
                });

                // Log the opening of the URL message
                Message("User clicked the 'My webpage' link to open the URL: '" + Globals.ToolStings.UrlMyWebPage + "'", EventType.Information, 1084);
            }
            catch (Exception ex)
            {
                // Show an error message if the URL could not be opened
                MessageBox.Show(@"Failed to open the URL '" + Globals.ToolStings.UrlMyWebPage + @"'. Error: " + ex.Message, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Log the error message
                Message("Failed to open the URL: " + ex.Message, EventType.Error, 1085);
            }
        }

        private void myBlogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = Globals.ToolStings.UrlMyBlog,
                    UseShellExecute = true
                });

                // Log the opening of the URL message
                Message("User clicked the 'My webpage' link to open the URL: '" + Globals.ToolStings.UrlMyBlog + "'", EventType.Information, 1086);
            }
            catch (Exception ex)
            {
                // Show an error message if the URL could not be opened
                MessageBox.Show(@"Failed to open the URL '" + Globals.ToolStings.UrlMyBlog + @"'. Error: " + ex.Message, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Log the error message
                Message("Failed to open the URL: " + ex.Message, EventType.Error, 1087);
            }
        }

        private void guideToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = Globals.ToolStings.UrlGitHub,
                    UseShellExecute = true
                });

                // Log the opening of the URL message
                Message("User clicked the 'Guide' link to open the URL: '" + Globals.ToolStings.UrlGitHub + "'", EventType.Information, 1094);
            }
            catch (Exception ex)
            {
                // Show an error message if the URL could not be opened
                MessageBox.Show(@"Failed to open the URL '" + Globals.ToolStings.UrlMyBlog + @"'. Error: " + ex.Message, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Log the error message
                Message("Failed to open the URL: " + ex.Message, EventType.Error, 1095);
            }
        }

        #region Connection Settings Event Handlers

        private void CheckboxUseSSL_CheckedChanged(object sender, EventArgs e)
        {
            UpdateSSLControls();
            UpdateConnectionSettingsStatus();
            Message($"SSL/HTTPS {(checkboxUseSSL.Checked ? "enabled" : "disabled")}",
                EventType.Information, 2009);
        }

        private void NumericPort_ValueChanged(object sender, EventArgs e)
        {
            UpdateConnectionSettingsStatus();
            Message($"Connection port set to: {(numericPort.Value == 0 ? "Auto" : numericPort.Value.ToString())}",
                EventType.Information, 2010);
        }

        private void ComboAuthMechanism_SelectedIndexChanged(object sender, EventArgs e)
        {
            string mechanism = comboAuthMechanism.SelectedItem?.ToString() ?? "Default";
            UpdateConnectionSettingsStatus();
            Message($"Authentication mechanism set to: {mechanism}",
                EventType.Information, 2011);
        }

        private void CheckboxSkipCACheck_CheckedChanged(object sender, EventArgs e)
        {
            Message($"Skip CA Check: {checkboxSkipCACheck.Checked}",
                EventType.Information, 2012);
        }

        private void CheckboxSkipCNCheck_CheckedChanged(object sender, EventArgs e)
        {
            Message($"Skip CN Check: {checkboxSkipCNCheck.Checked}",
                EventType.Information, 2013);
        }

        private void NumericTimeout_ValueChanged(object sender, EventArgs e)
        {
            Message($"Connection timeout set to: {numericTimeout.Value} seconds",
                EventType.Information, 2014);
        }

        #endregion

        private void LinkLabelToggleAdvanced_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Toggle visibility of advanced settings
            bool isExpanded = !groupConnectionSettings.Visible;
            groupConnectionSettings.Visible = isExpanded;

            // Update link text and form height
            if (isExpanded)
            {
                linkLabelToggleAdvanced.Text = "▲ Hide Advanced Settings";
                ClientSize = new Size(500, 633); // Expanded height
                statusStripLoginForm.Location = new Point(0, 611);
                Message("Advanced connection settings expanded", EventType.Information, 2016);
            }
            else
            {
                linkLabelToggleAdvanced.Text = "▼ Show Advanced Settings";
                ClientSize = new Size(500, 480); // Collapsed height
                statusStripLoginForm.Location = new Point(0, 458);
                Message("Advanced connection settings collapsed", EventType.Information, 2017);
            }
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to reset all connection settings to their default values?",
                "Reset Settings",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                if (result == DialogResult.Yes)
                {
                    _currentConnectionSettings = ConnectionSettings.GetDefault();
                    InitializeConnectionSettings();
                    Message("Connection settings reset to defaults", EventType.Information, 2002);
                }
            }
        }
    }
}