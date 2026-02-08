using System.Diagnostics;
using System.Management.Automation;
using System.Text.Json;

namespace HVTools.Helpers
{
    /// <summary>
    /// Represents detailed information about a Hyper-V host
    /// </summary>
    public class HostDetailsInfo
    {
        public string HostName { get; set; } = "";
        public string ClusterName { get; set; } = "N/A";
        public string NodeState { get; set; } = "Standalone";
        public string Domain { get; set; } = "";
        public string OperatingSystem { get; set; } = "";
        public string OsVersion { get; set; } = "";
        public string BuildNumber { get; set; } = "";
        public string BootTime { get; set; } = "";
        public string Uptime { get; set; } = "";
        public string TimeZone { get; set; } = "";
        public string NtpServers { get; set; } = "";
        public string NtpStatus { get; set; } = "";
        public string LicenseStatus { get; set; } = "Unknown";
        public string LicenseType { get; set; } = "Unknown";
        public string ProductKey { get; set; } = "Unknown";
        public string GracePeriod { get; set; } = "N/A";
        public string LicenseDescription { get; set; } = "";
        public string Manufacturer { get; set; } = "";
        public string Model { get; set; } = "";
        public string SerialNumber { get; set; } = "";
        public string Processor { get; set; } = "";
        public int Sockets { get; set; } = 1;
        public int Cores { get; set; } = 1;
        public int LogicalCpUs { get; set; } = 1;
        public string HyperThreading { get; set; } = "No";
        public string SlatSupport { get; set; } = "No";
        public double TotalMemoryGb { get; set; }
        public double UsedMemoryGb { get; set; }
        public double FreeMemoryGb { get; set; }
        public double MemoryUsagePercent { get; set; }
        public int TotalVMs { get; set; }
        public int RunningVMs { get; set; }
        public int StoppedVMs { get; set; }
        public int VirtualSwitches { get; set; }
        public int ExternalSwitches { get; set; }
        public string IpAddresses { get; set; } = "";
        public string LiveMigration { get; set; } = "Disabled";
        public string EnhancedSession { get; set; } = "No";
        public string NumaSpanning { get; set; } = "No";
        public string VhdPath { get; set; } = "";
        public string VmConfigPath { get; set; } = "";
    }

    /// <summary>
    /// Provides functionality to retrieve Hyper-V host details
    /// </summary>
    public static class HostDetails
    {
        /// <summary>
        /// Gets detailed information about Hyper-V host(s)
        /// </summary>
        public static List<HostDetailsInfo> GetHyperVHostDetails(
            Func<string, System.Collections.ObjectModel.Collection<PSObject>> executePowerShellCommand,
            Func<string, string, System.Collections.ObjectModel.Collection<PSObject>> executePowerShellCommandOnNode = null)
        {
            var allHosts = new List<HostDetailsInfo>();

            try
            {
                FileLogger.Message("Getting Hyper-V host details...",
                    FileLogger.EventType.Information, 4001);

                // Check if connected to a cluster
                bool isCluster = SessionContext.IsCluster;
                var clusterNodes = new List<string>();

                if (isCluster && !SessionContext.IsLocal)
                {
                    // Get cluster nodes
                    FileLogger.Message("Detected cluster environment, getting cluster nodes...",
                        FileLogger.EventType.Information, 4002);

                    string getNodesScript = @"Get-ClusterNode -ErrorAction Stop | Select-Object -ExpandProperty Name";
                    var nodesResult = executePowerShellCommand(getNodesScript);

                    if (nodesResult != null && nodesResult.Count > 0)
                    {
                        foreach (var nodeObj in nodesResult)
                        {
                            string nodeName = nodeObj.BaseObject?.ToString();
                            if (!string.IsNullOrEmpty(nodeName))
                            {
                                // If the original connection used FQDN, construct FQDNs for cluster nodes
                                if (SessionContext.ServerName.Contains('.') && !nodeName.Contains('.'))
                                {
                                    string domain = SessionContext.ServerName.Substring(SessionContext.ServerName.IndexOf('.'));
                                    nodeName = nodeName + domain;
                                }
                                clusterNodes.Add(nodeName);
                            }
                        }

                        FileLogger.Message($"Found {clusterNodes.Count} cluster nodes: {string.Join(", ", clusterNodes)}",
                            FileLogger.EventType.Information, 4003);
                    }
                }

                if (clusterNodes.Count > 0 && executePowerShellCommandOnNode != null)
                {
                    // Get details for all cluster nodes
                    int nodeIndex = 0;
                    foreach (var node in clusterNodes)
                    {
                        nodeIndex++;
                        try
                        {
                            FileLogger.Message($"Getting host details for cluster node {nodeIndex} of {clusterNodes.Count}: {node}",
                                FileLogger.EventType.Information, 4004);

                            var hostInfo = GetHostDetailsFromNode(node, executePowerShellCommandOnNode);
                            if (hostInfo != null)
                            {
                                allHosts.Add(hostInfo);
                                FileLogger.Message($"Successfully retrieved details for node: {node}",
                                    FileLogger.EventType.Information, 4005);
                            }
                        }
                        catch (Exception ex)
                        {
                            FileLogger.Message($"Failed to get details for cluster node {node}: {ex.Message}",
                                FileLogger.EventType.Warning, 4006);
                        }
                    }
                }
                else
                {
                    // Single host
                    FileLogger.Message("Getting host details for single host...",
                        FileLogger.EventType.Information, 4007);

                    var hostInfo = GetHostDetailsFromSession(executePowerShellCommand);
                    if (hostInfo != null)
                    {
                        allHosts.Add(hostInfo);
                    }
                }

                FileLogger.Message($"Successfully retrieved details for {allHosts.Count} host(s)",
                    FileLogger.EventType.Information, 4008);

                return allHosts;
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Error getting Hyper-V host details: {ex.Message}",
                    FileLogger.EventType.Error, 4009);
                return allHosts;
            }
        }

        /// <summary>
        /// Gets host details from a specific cluster node
        /// </summary>
        private static HostDetailsInfo GetHostDetailsFromNode(string nodeName,
            Func<string, string, System.Collections.ObjectModel.Collection<PSObject>> executePowerShellCommandOnNode)
        {
            try
            {
                string script = GetHostDetailsScript();
                
                FileLogger.Message($"Executing host details script on node '{nodeName}'...",
                    FileLogger.EventType.Information, 4034);
                
                var result = executePowerShellCommandOnNode(nodeName, script);

                if (result == null)
                {
                    FileLogger.Message($"Host details script returned null for node '{nodeName}'",
                        FileLogger.EventType.Warning, 4035);
                    return null;
                }

                if (result.Count == 0)
                {
                    FileLogger.Message($"Host details script returned empty collection for node '{nodeName}'",
                        FileLogger.EventType.Warning, 4036);
                    return null;
                }

                FileLogger.Message($"Host details script for node '{nodeName}' returned {result.Count} result(s), parsing...",
                    FileLogger.EventType.Information, 4037);

                var hostInfo = ParseHostDetails(result[0]);
                
                if (hostInfo != null)
                {
                    FileLogger.Message($"Successfully parsed host details for node '{nodeName}' - hostname: '{hostInfo.HostName}'",
                        FileLogger.EventType.Information, 4038);
                }
                else
                {
                    FileLogger.Message($"Failed to parse host details for node '{nodeName}' - ParseHostDetails returned null",
                        FileLogger.EventType.Error, 4039);
                }

                return hostInfo;
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Error getting host details from node '{nodeName}': {ex.Message}",
                    FileLogger.EventType.Error, 4040);
                FileLogger.Message($"Stack trace: {ex.StackTrace}",
                    FileLogger.EventType.Error, 4041);
                return null;
            }
        }

        /// <summary>
        /// Gets host details from the current session
        /// </summary>
        private static HostDetailsInfo GetHostDetailsFromSession(
            Func<string, System.Collections.ObjectModel.Collection<PSObject>> executePowerShellCommand)
        {
            try
            {
                // For local execution, use Windows PowerShell process to get full WMI support
                if (SessionContext.IsLocal)
                {
                    FileLogger.Message("Using Windows PowerShell process for local host details...",
                        FileLogger.EventType.Information, 4026);
                    
                    return GetHostDetailsViaWindowsPowerShell();
                }
                
                // For remote, use the embedded PowerShell with Invoke-Command (which uses Windows PS on target)
                FileLogger.Message("Executing host details script via embedded PowerShell...",
                    FileLogger.EventType.Information, 4026);
                
                string script = GetHostDetailsScript();
                var result = executePowerShellCommand(script);

                if (result == null)
                {
                    FileLogger.Message("Host details script returned null result",
                        FileLogger.EventType.Warning, 4027);
                    return null;
                }

                if (result.Count == 0)
                {
                    FileLogger.Message("Host details script returned empty collection",
                        FileLogger.EventType.Warning, 4028);
                    return null;
                }

                FileLogger.Message($"Host details script returned {result.Count} result(s), parsing...",
                    FileLogger.EventType.Information, 4029);

                var hostInfo = ParseHostDetails(result[0]);
                
                if (hostInfo != null)
                {
                    FileLogger.Message($"Successfully parsed host details for '{hostInfo.HostName}'",
                        FileLogger.EventType.Information, 4030);
                }
                else
                {
                    FileLogger.Message("Failed to parse host details - ParseHostDetails returned null",
                        FileLogger.EventType.Error, 4031);
                }

                return hostInfo;
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Error in GetHostDetailsFromSession: {ex.Message}",
                    FileLogger.EventType.Error, 4032);
                FileLogger.Message($"Stack trace: {ex.StackTrace}",
                    FileLogger.EventType.Error, 4033);
                return null;
            }
        }

        /// <summary>
        /// Gets host details by executing Windows PowerShell.exe process (for local execution)
        /// This provides full WMI support that's not available in embedded PowerShell Core
        /// </summary>
        private static HostDetailsInfo GetHostDetailsViaWindowsPowerShell()
        {
            try
            {
                string script = GetWmiHostDetailsScript();
                
                // Create a temporary script file
                string tempScriptPath = Path.Combine(Path.GetTempPath(), $"HVTools_HostDetails_{Guid.NewGuid():N}.ps1");
                File.WriteAllText(tempScriptPath, script);
                
                FileLogger.Message($"Created temp script: '{tempScriptPath}'",
                    FileLogger.EventType.Information, 4050);
                
                FileLogger.Message("Executing Windows PowerShell process to get host details...",
                    FileLogger.EventType.Information, 4057);
                try
                {
                    // Execute via Windows PowerShell process
                    var psi = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = $"-NoProfile -NonInteractive -ExecutionPolicy Bypass -File \"{tempScriptPath}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    
                    using var process = Process.Start(psi);
                    if (process == null)
                    {
                        FileLogger.Message("Failed to start Windows PowerShell process",
                            FileLogger.EventType.Error, 4051);
                        return null;
                    }
                    
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit(60000); // 60 second timeout
                    
                    if (!string.IsNullOrEmpty(error))
                    {
                        FileLogger.Message($"PowerShell stderr: {error}",
                            FileLogger.EventType.Warning, 4052);
                    }
                    
                    if (string.IsNullOrWhiteSpace(output))
                    {
                        FileLogger.Message("PowerShell process returned empty output",
                            FileLogger.EventType.Warning, 4053);
                        return null;
                    }
                    
                    FileLogger.Message($"PowerShell output length: {output.Length} chars",
                        FileLogger.EventType.Information, 4054);
                    
                    // Parse JSON output
                    return ParseJsonHostDetails(output);
                }
                finally
                {
                    // Clean up temp file
                    try
                    {
                        if (File.Exists(tempScriptPath))
                        {
                            File.Delete(tempScriptPath);
                        }
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Error in GetHostDetailsViaWindowsPowerShell: {ex.Message}",
                    FileLogger.EventType.Error, 4055);
                FileLogger.Message($"Stack trace: {ex.StackTrace}",
                    FileLogger.EventType.Error, 4056);
                return null;
            }
        }

        /// <summary>
        /// Parses JSON output from Windows PowerShell into HostDetailsInfo
        /// </summary>
        private static HostDetailsInfo ParseJsonHostDetails(string json)
        {
            try
            {
                // Trim any extra whitespace/newlines
                json = json.Trim();
                
                FileLogger.Message("Parsing JSON host details...",
                    FileLogger.EventType.Information, 4060);
                
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                
                var hostInfo = new HostDetailsInfo
                {
                    HostName = GetJsonString(root, "HostName"),
                    ClusterName = GetJsonString(root, "ClusterName"),
                    NodeState = GetJsonString(root, "NodeState"),
                    Domain = GetJsonString(root, "Domain"),
                    OperatingSystem = GetJsonString(root, "OperatingSystem"),
                    OsVersion = GetJsonString(root, "OSVersion"),
                    BuildNumber = GetJsonString(root, "BuildNumber"),
                    BootTime = GetJsonString(root, "BootTime"),
                    Uptime = GetJsonString(root, "Uptime"),
                    TimeZone = GetJsonString(root, "TimeZone"),
                    NtpServers = GetJsonString(root, "NtpServers"),
                    NtpStatus = GetJsonString(root, "NtpStatus"),
                    LicenseStatus = GetJsonString(root, "LicenseStatus"),
                    LicenseType = GetJsonString(root, "LicenseType"),
                    ProductKey = GetJsonString(root, "ProductKey"),
                    GracePeriod = GetJsonString(root, "GracePeriod"),
                    LicenseDescription = GetJsonString(root, "LicenseDescription"),
                    Manufacturer = GetJsonString(root, "Manufacturer"),
                    Model = GetJsonString(root, "Model"),
                    SerialNumber = GetJsonString(root, "SerialNumber"),
                    Processor = GetJsonString(root, "Processor"),
                    Sockets = GetJsonInt(root, "Sockets"),
                    Cores = GetJsonInt(root, "Cores"),
                    LogicalCpUs = GetJsonInt(root, "LogicalCPUs"),
                    HyperThreading = GetJsonString(root, "HyperThreading"),
                    SlatSupport = GetJsonString(root, "SLATSupport"),
                    TotalMemoryGb = GetJsonDouble(root, "TotalMemoryGB"),
                    UsedMemoryGb = GetJsonDouble(root, "UsedMemoryGB"),
                    FreeMemoryGb = GetJsonDouble(root, "FreeMemoryGB"),
                    MemoryUsagePercent = GetJsonDouble(root, "MemoryUsagePercent"),
                    TotalVMs = GetJsonInt(root, "TotalVMs"),
                    RunningVMs = GetJsonInt(root, "RunningVMs"),
                    StoppedVMs = GetJsonInt(root, "StoppedVMs"),
                    VirtualSwitches = GetJsonInt(root, "VirtualSwitches"),
                    ExternalSwitches = GetJsonInt(root, "ExternalSwitches"),
                    IpAddresses = GetJsonString(root, "IPAddresses"),
                    LiveMigration = GetJsonString(root, "LiveMigration"),
                    EnhancedSession = GetJsonString(root, "EnhancedSession"),
                    NumaSpanning = GetJsonString(root, "NUMASpanning"),
                    VhdPath = GetJsonString(root, "VHDPath"),
                    VmConfigPath = GetJsonString(root, "VMConfigPath")
                };
                
                FileLogger.Message($"Successfully parsed JSON host details for '{hostInfo.HostName}'",
                    FileLogger.EventType.Information, 4061);
                
                return hostInfo;
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Error parsing JSON host details: {ex.Message}",
                    FileLogger.EventType.Error, 4062);
                FileLogger.Message($"JSON content (first 500 chars): {json.Substring(0, Math.Min(500, json.Length))}",
                    FileLogger.EventType.Error, 4063);
                return null;
            }
        }

        private static string GetJsonString(JsonElement element, string propertyName)
        {
            try
            {
                if (element.TryGetProperty(propertyName, out var prop))
                {
                    return prop.ValueKind == JsonValueKind.Null ? "" : prop.ToString();
                }
                return "";
            }
            catch
            {
                return "";
            }
        }

        private static int GetJsonInt(JsonElement element, string propertyName)
        {
            try
            {
                if (element.TryGetProperty(propertyName, out var prop))
                {
                    if (prop.ValueKind == JsonValueKind.Number)
                    {
                        return prop.GetInt32();
                    }
                    if (prop.ValueKind == JsonValueKind.String && int.TryParse(prop.GetString(), out int val))
                    {
                        return val;
                    }
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        private static double GetJsonDouble(JsonElement element, string propertyName)
        {
            try
            {
                if (element.TryGetProperty(propertyName, out var prop))
                {
                    if (prop.ValueKind == JsonValueKind.Number)
                    {
                        return prop.GetDouble();
                    }
                    if (prop.ValueKind == JsonValueKind.String && double.TryParse(prop.GetString(), out double val))
                    {
                        return val;
                    }
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Gets the PowerShell script to retrieve host details (for remote execution via Invoke-Command)
        /// Remote execution has full WMI support since it runs on Windows PowerShell on the target
        /// </summary>
        private static string GetHostDetailsScript()
        {
            return @"
                $ErrorActionPreference = 'SilentlyContinue'
                $errorDetails = ''
                try {
                    # Get Host Information using WMI (full support via Invoke-Command)
                    $vmHost = Get-VMHost -ErrorAction SilentlyContinue
                    if (-not $vmHost) {
                        $errorDetails += 'Get-VMHost failed; '
                    }

                    $computerSystem = Get-WmiObject Win32_ComputerSystem -ErrorAction SilentlyContinue
                    $operatingSystem = Get-WmiObject Win32_OperatingSystem -ErrorAction SilentlyContinue
                    $bios = Get-WmiObject Win32_BIOS -ErrorAction SilentlyContinue
                    $processors = @(Get-WmiObject Win32_Processor -ErrorAction SilentlyContinue)

                    # Calculate processor info
                    $processorSockets = if ($processors) { $processors.Count } else { 1 }
                    $totalCores = ($processors | Measure-Object NumberOfCores -Sum).Sum
                    $totalLogicalProcessors = ($processors | Measure-Object NumberOfLogicalProcessors -Sum).Sum
                    if ($totalCores -eq 0) { $totalCores = 1 }
                    if ($totalLogicalProcessors -eq 0) { $totalLogicalProcessors = $totalCores }
                    if ($processorSockets -eq 0) { $processorSockets = 1 }

                    # Get memory information
                    $totalMemoryGB = [Math]::Round($computerSystem.TotalPhysicalMemory / 1GB, 2)
                    $availableMemoryGB = [Math]::Round(($operatingSystem.FreePhysicalMemory * 1KB) / 1GB, 2)
                    $usedMemoryGB = $totalMemoryGB - $availableMemoryGB
                    $memoryUsagePercent = if ($totalMemoryGB -gt 0) { [Math]::Round(($usedMemoryGB / $totalMemoryGB) * 100, 1) } else { 0 }

                    # Get SLAT support
                    $slatSupport = 'No'
                    try {
                        $slat = (Get-WmiObject -Namespace root\virtualization\v2 -Class Msvm_ProcessorSettingData -ErrorAction SilentlyContinue | Select-Object -First 1).SecondLevelAddressTranslationEnabled
                        if ($slat) { $slatSupport = 'Yes' }
                    } catch { }

                    # Get VM counts
                    $allVMs = @(Get-VM -ErrorAction SilentlyContinue)
                    $runningVMs = @($allVMs | Where-Object { $_.State -eq 'Running' })
                    $stoppedVMs = @($allVMs | Where-Object { $_.State -eq 'Off' })

                    # Get virtual switches
                    $virtualSwitches = @(Get-VMSwitch -ErrorAction SilentlyContinue)
                    $externalSwitches = @($virtualSwitches | Where-Object { $_.SwitchType -eq 'External' })

                    # Calculate uptime
                    $uptime = (Get-Date) - $operatingSystem.ConvertToDateTime($operatingSystem.LastBootUpTime)
                    $uptimeString = ""$($uptime.Days)d $($uptime.Hours)h $($uptime.Minutes)m""
                    $bootTime = $operatingSystem.ConvertToDateTime($operatingSystem.LastBootUpTime).ToString('yyyy-MM-dd HH:mm:ss')

                    # Get cluster info
                    $clusterName = 'N/A'
                    $nodeState = 'Standalone'
                    try {
                        $clusterNode = Get-ClusterNode -Name $env:COMPUTERNAME -ErrorAction SilentlyContinue
                        if ($clusterNode) {
                            $cluster = Get-Cluster -ErrorAction SilentlyContinue
                            $clusterName = if ($cluster) { $cluster.Name } else { 'Cluster Detected' }
                            $nodeState = if ($clusterNode.State) { $clusterNode.State.ToString() } else { 'Online' }
                        }
                    } catch { }

                    # Get IP addresses
                    $ipList = @()
                    try {
                        $networkAdapters = Get-WmiObject Win32_NetworkAdapterConfiguration -ErrorAction SilentlyContinue |
                            Where-Object { $_.IPEnabled -eq $true -and $_.IPAddress -ne $null }
                        foreach ($adapter in $networkAdapters) {
                            foreach ($ip in $adapter.IPAddress) {
                                if ($ip -notlike '169.254.*' -and $ip -notlike 'fe80:*' -and $ip -ne '::1' -and $ip -ne '127.0.0.1') {
                                    $ipList += $ip
                                }
                            }
                        }
                    } catch { }
                    $ipAddresses = if ($ipList.Count -gt 0) { $ipList -join ', ' } else { 'N/A' }

                    # Get time zone
                    $timeZone = try { (Get-WmiObject Win32_TimeZone -ErrorAction SilentlyContinue).Description } catch { 'Unknown' }
                    if (-not $timeZone) { $timeZone = [System.TimeZoneInfo]::Local.DisplayName }

                    # Get NTP Configuration
                    $ntpServers = 'Unknown'
                    $ntpStatus = 'Unknown'
                    try {
                        $w32timeKey = 'HKLM:\SYSTEM\CurrentControlSet\Services\W32Time\Parameters'
                        if (Test-Path $w32timeKey) {
                            $ntpServerReg = Get-ItemProperty -Path $w32timeKey -Name 'NtpServer' -ErrorAction SilentlyContinue
                            if ($ntpServerReg) {
                                $ntpServers = $ntpServerReg.NtpServer -replace ',0x[0-9a-fA-F]+', '' -replace ' ', ', '
                            }
                        }
                        try {
                            $w32tmOutput = & w32tm /query /status 2>$null
                            if ($w32tmOutput -and $w32tmOutput -match 'Leap Indicator: (\d+)') {
                                $ntpStatus = 'Active'
                            }
                        } catch {
                            $ntpStatus = 'Service Not Available'
                        }
                    } catch { }

                    # Get license info
                    $licenseStatus = 'Unknown'
                    $licenseType = 'Unknown'
                    $productKey = 'Unknown'
                    $graceRemaining = 'N/A'
                    $licenseDescription = 'Unknown'
                    try {
                        $windowsLicense = Get-WmiObject -Class SoftwareLicensingProduct -ErrorAction SilentlyContinue |
                            Where-Object { $_.Name -like '*Windows*' -and $_.LicenseStatus -eq 1 } | 
                            Select-Object -First 1
                        if ($windowsLicense) {
                            $licenseStatus = switch ($windowsLicense.LicenseStatus) {
                                0 { 'Unlicensed' }
                                1 { 'Licensed' }
                                2 { 'OOB Grace Period' }
                                3 { 'OOT Grace Period' }
                                4 { 'Non-Genuine Grace' }
                                5 { 'Notification' }
                                6 { 'Extended Grace' }
                                default { 'Unknown' }
                            }
                            $licenseDescription = $windowsLicense.Description
                            if ($windowsLicense.Description -like '*VOLUME*') { $licenseType = 'Volume License' }
                            elseif ($windowsLicense.Description -like '*OEM*') { $licenseType = 'OEM' }
                            elseif ($windowsLicense.Description -like '*RETAIL*') { $licenseType = 'Retail' }
                            else { $licenseType = 'Standard' }
                            if ($windowsLicense.PartialProductKey) {
                                $productKey = '*****-*****-*****-*****-' + $windowsLicense.PartialProductKey
                            }
                            if ($windowsLicense.GracePeriodRemaining) {
                                $graceRemaining = ""$([Math]::Round($windowsLicense.GracePeriodRemaining / 1440, 1)) days""
                            }
                        }
                    } catch { }

                    # Serial number
                    $serialNumber = 'Unknown'
                    try {
                        $biosSerial = $bios.SerialNumber
                        if ([string]::IsNullOrWhiteSpace($biosSerial)) {
                            $enclosureSerial = (Get-WmiObject Win32_SystemEnclosure -ErrorAction SilentlyContinue).SerialNumber
                            if (-not [string]::IsNullOrWhiteSpace($enclosureSerial)) {
                                $serialNumber = $enclosureSerial
                            }
                        } else {
                            $serialNumber = $biosSerial
                        }
                    } catch { }

                    # Return object with all info
                    [PSCustomObject]@{
                        HostName = if ($vmHost) { $vmHost.Name } else { $env:COMPUTERNAME }
                        ClusterName = $clusterName
                        NodeState = $nodeState
                        Domain = if ($computerSystem.Domain) { $computerSystem.Domain } else { 'WORKGROUP' }
                        OperatingSystem = if ($operatingSystem.Caption) { $operatingSystem.Caption -replace 'Microsoft ', '' } else { 'Unknown' }
                        OSVersion = if ($operatingSystem.Version) { $operatingSystem.Version } else { '' }
                        BuildNumber = if ($operatingSystem.BuildNumber) { $operatingSystem.BuildNumber } else { '' }
                        BootTime = $bootTime
                        Uptime = $uptimeString
                        TimeZone = $timeZone
                        NtpServers = $ntpServers
                        NtpStatus = $ntpStatus
                        LicenseStatus = $licenseStatus
                        LicenseType = $licenseType
                        ProductKey = $productKey
                        GracePeriod = $graceRemaining
                        LicenseDescription = $licenseDescription
                        Manufacturer = if ($computerSystem.Manufacturer) { $computerSystem.Manufacturer } else { '' }
                        Model = if ($computerSystem.Model) { $computerSystem.Model } else { '' }
                        SerialNumber = $serialNumber
                        Processor = if ($processors -and $processors[0].Name) { $processors[0].Name -replace '\s+', ' ' } else { '' }
                        Sockets = $processorSockets
                        Cores = $totalCores
                        LogicalCPUs = $totalLogicalProcessors
                        HyperThreading = if ($totalLogicalProcessors -gt $totalCores) { 'Yes' } else { 'No' }
                        SLATSupport = $slatSupport
                        TotalMemoryGB = $totalMemoryGB
                        UsedMemoryGB = [Math]::Round($usedMemoryGB, 2)
                        FreeMemoryGB = [Math]::Round($availableMemoryGB, 2)
                        MemoryUsagePercent = $memoryUsagePercent
                        TotalVMs = $allVMs.Count
                        RunningVMs = $runningVMs.Count
                        StoppedVMs = $stoppedVMs.Count
                        VirtualSwitches = $virtualSwitches.Count
                        ExternalSwitches = $externalSwitches.Count
                        IPAddresses = $ipAddresses
                        LiveMigration = if ($vmHost -and $vmHost.VirtualMachineMigrationEnabled) { 'Enabled' } else { 'Disabled' }
                        EnhancedSession = if ($vmHost -and $vmHost.EnableEnhancedSessionMode) { 'Yes' } else { 'No' }
                        NUMASpanning = if ($vmHost -and $vmHost.NumaSpanningEnabled) { 'Yes' } else { 'No' }
                        VHDPath = if ($vmHost -and $vmHost.VirtualHardDiskPath) { $vmHost.VirtualHardDiskPath } else { '' }
                        VMConfigPath = if ($vmHost -and $vmHost.VirtualMachinePath) { $vmHost.VirtualMachinePath } else { '' }
                        ErrorDetails = $errorDetails
                    }
                } catch {
                    # Capture the actual error message
                    $actualError = $_.Exception.Message
                    
                    # Return minimal info on error
                    [PSCustomObject]@{
                        HostName = $env:COMPUTERNAME
                        ClusterName = 'N/A'
                        NodeState = 'Error'
                        Domain = ''
                        OperatingSystem = ""Error: $actualError""
                        OSVersion = ''
                        BuildNumber = ''
                        BootTime = ''
                        Uptime = ''
                        TimeZone = ''
                        NtpServers = ''
                        NtpStatus = ''
                        LicenseStatus = 'Unknown'
                        LicenseType = 'Unknown'
                        ProductKey = 'Unknown'
                        GracePeriod = 'N/A'
                        LicenseDescription = ''
                        Manufacturer = ''
                        Model = ''
                        SerialNumber = ''
                        Processor = ''
                        Sockets = 0
                        Cores = 0
                        LogicalCPUs = 0
                        HyperThreading = 'No'
                        SLATSupport = 'No'
                        TotalMemoryGB = 0
                        UsedMemoryGB = 0
                        FreeMemoryGB = 0
                        MemoryUsagePercent = 0
                        TotalVMs = 0
                        RunningVMs = 0
                        StoppedVMs = 0
                        VirtualSwitches = 0
                        ExternalSwitches = 0
                        IPAddresses = ''
                        LiveMigration = 'Unknown'
                        EnhancedSession = 'Unknown'
                        NUMASpanning = 'Unknown'
                        VHDPath = ''
                        VMConfigPath = ''
                        ErrorDetails = ""Error: $actualError | $errorDetails""
                    }
                }
            ";
        }

        /// <summary>
        /// Gets the PowerShell script for local Windows PowerShell execution (with full WMI support)
        /// This script outputs JSON for easy parsing
        /// </summary>
        private static string GetWmiHostDetailsScript()
        {
            return @"
$ErrorActionPreference = 'SilentlyContinue'
try {
    # Get Host Information using WMI (full support in Windows PowerShell)
    $vmHost = Get-VMHost -ErrorAction SilentlyContinue
    $computerSystem = Get-WmiObject Win32_ComputerSystem -ErrorAction SilentlyContinue
    $operatingSystem = Get-WmiObject Win32_OperatingSystem -ErrorAction SilentlyContinue
    $bios = Get-WmiObject Win32_BIOS -ErrorAction SilentlyContinue
    $processors = @(Get-WmiObject Win32_Processor -ErrorAction SilentlyContinue)

    # Calculate processor info
    $processorSockets = if ($processors) { $processors.Count } else { 1 }
    $totalCores = ($processors | Measure-Object NumberOfCores -Sum).Sum
    $totalLogicalProcessors = ($processors | Measure-Object NumberOfLogicalProcessors -Sum).Sum
    if ($totalCores -eq 0) { $totalCores = 1 }
    if ($totalLogicalProcessors -eq 0) { $totalLogicalProcessors = $totalCores }
    if ($processorSockets -eq 0) { $processorSockets = 1 }

    # Get memory information
    $totalMemoryGB = [Math]::Round($computerSystem.TotalPhysicalMemory / 1GB, 2)
    $availableMemoryGB = [Math]::Round(($operatingSystem.FreePhysicalMemory * 1KB) / 1GB, 2)
    $usedMemoryGB = $totalMemoryGB - $availableMemoryGB
    $memoryUsagePercent = if ($totalMemoryGB -gt 0) { [Math]::Round(($usedMemoryGB / $totalMemoryGB) * 100, 1) } else { 0 }

    # Get SLAT support
    $slatSupport = 'No'
    try {
        $slat = (Get-WmiObject -Namespace root\virtualization\v2 -Class Msvm_ProcessorSettingData -ErrorAction SilentlyContinue | Select-Object -First 1).SecondLevelAddressTranslationEnabled
        if ($slat) { $slatSupport = 'Yes' }
    } catch { }

    # Get VM counts
    $allVMs = @(Get-VM -ErrorAction SilentlyContinue)
    $runningVMs = @($allVMs | Where-Object { $_.State -eq 'Running' })
    $stoppedVMs = @($allVMs | Where-Object { $_.State -eq 'Off' })

    # Get virtual switches
    $virtualSwitches = @(Get-VMSwitch -ErrorAction SilentlyContinue)
    $externalSwitches = @($virtualSwitches | Where-Object { $_.SwitchType -eq 'External' })

    # Calculate uptime
    $uptime = (Get-Date) - $operatingSystem.ConvertToDateTime($operatingSystem.LastBootUpTime)
    $uptimeString = ""$($uptime.Days)d $($uptime.Hours)h $($uptime.Minutes)m""
    $bootTime = $operatingSystem.ConvertToDateTime($operatingSystem.LastBootUpTime).ToString('yyyy-MM-dd HH:mm:ss')

    # Get cluster info
    $clusterName = 'N/A'
    $nodeState = 'Standalone'
    try {
        $clusterNode = Get-ClusterNode -Name $env:COMPUTERNAME -ErrorAction SilentlyContinue
        if ($clusterNode) {
            $cluster = Get-Cluster -ErrorAction SilentlyContinue
            $clusterName = if ($cluster) { $cluster.Name } else { 'Cluster Detected' }
            $nodeState = if ($clusterNode.State) { $clusterNode.State.ToString() } else { 'Online' }
        }
    } catch { }

    # Get IP addresses
    $ipList = @()
    try {
        $networkAdapters = Get-WmiObject Win32_NetworkAdapterConfiguration -ErrorAction SilentlyContinue |
            Where-Object { $_.IPEnabled -eq $true -and $_.IPAddress -ne $null }
        foreach ($adapter in $networkAdapters) {
            foreach ($ip in $adapter.IPAddress) {
                if ($ip -notlike '169.254.*' -and $ip -notlike 'fe80:*' -and $ip -ne '::1' -and $ip -ne '127.0.0.1') {
                    $ipList += $ip
                }
            }
        }
    } catch { }
    $ipAddresses = if ($ipList.Count -gt 0) { $ipList -join ', ' } else { 'N/A' }

    # Get time zone
    $timeZone = try { (Get-WmiObject Win32_TimeZone -ErrorAction SilentlyContinue).Description } catch { 'Unknown' }
    if (-not $timeZone) { $timeZone = [System.TimeZoneInfo]::Local.DisplayName }

    # Get NTP Configuration
    $ntpServers = 'Unknown'
    $ntpStatus = 'Unknown'
    try {
        $w32timeKey = 'HKLM:\SYSTEM\CurrentControlSet\Services\W32Time\Parameters'
        if (Test-Path $w32timeKey) {
            $ntpServerReg = Get-ItemProperty -Path $w32timeKey -Name 'NtpServer' -ErrorAction SilentlyContinue
            if ($ntpServerReg) {
                $ntpServers = $ntpServerReg.NtpServer -replace ',0x[0-9a-fA-F]+', '' -replace ' ', ', '
            }
        }
        try {
            $w32tmOutput = & w32tm /query /status 2>$null
            if ($w32tmOutput -and $w32tmOutput -match 'Leap Indicator: (\d+)') {
                $ntpStatus = 'Active'
            }
        } catch {
            $ntpStatus = 'Service Not Available'
        }
    } catch { }

    # Get license info
    $licenseStatus = 'Unknown'
    $licenseType = 'Unknown'
    $productKey = 'Unknown'
    $graceRemaining = 'N/A'
    $licenseDescription = 'Unknown'
    try {
        $windowsLicense = Get-WmiObject -Class SoftwareLicensingProduct -ErrorAction SilentlyContinue |
            Where-Object { $_.Name -like '*Windows*' -and $_.LicenseStatus -eq 1 } | 
            Select-Object -First 1
        if ($windowsLicense) {
            $licenseStatus = switch ($windowsLicense.LicenseStatus) {
                0 { 'Unlicensed' }
                1 { 'Licensed' }
                2 { 'OOB Grace Period' }
                3 { 'OOT Grace Period' }
                4 { 'Non-Genuine Grace' }
                5 { 'Notification' }
                6 { 'Extended Grace' }
                default { 'Unknown' }
            }
            $licenseDescription = $windowsLicense.Description
            if ($windowsLicense.Description -like '*VOLUME*') { $licenseType = 'Volume License' }
            elseif ($windowsLicense.Description -like '*OEM*') { $licenseType = 'OEM' }
            elseif ($windowsLicense.Description -like '*RETAIL*') { $licenseType = 'Retail' }
            else { $licenseType = 'Standard' }
            if ($windowsLicense.PartialProductKey) {
                $productKey = '*****-*****-*****-*****-' + $windowsLicense.PartialProductKey
            }
            if ($windowsLicense.GracePeriodRemaining) {
                $graceRemaining = ""$([Math]::Round($windowsLicense.GracePeriodRemaining / 1440, 1)) days""
            }
        }
    } catch { }

    # Serial number
    $serialNumber = 'Unknown'
    try {
        $biosSerial = $bios.SerialNumber
        if ([string]::IsNullOrWhiteSpace($biosSerial)) {
            $enclosureSerial = (Get-WmiObject Win32_SystemEnclosure -ErrorAction SilentlyContinue).SerialNumber
            if (-not [string]::IsNullOrWhiteSpace($enclosureSerial)) {
                $serialNumber = $enclosureSerial
            }
        } else {
            $serialNumber = $biosSerial
        }
    } catch { }

    # Create result object
    $result = @{
        HostName = if ($vmHost) { $vmHost.Name } else { $env:COMPUTERNAME }
        ClusterName = $clusterName
        NodeState = $nodeState
        Domain = if ($computerSystem.Domain) { $computerSystem.Domain } else { 'WORKGROUP' }
        OperatingSystem = if ($operatingSystem.Caption) { $operatingSystem.Caption -replace 'Microsoft ', '' } else { 'Unknown' }
        OSVersion = if ($operatingSystem.Version) { $operatingSystem.Version } else { '' }
        BuildNumber = if ($operatingSystem.BuildNumber) { $operatingSystem.BuildNumber.ToString() } else { '' }
        BootTime = $bootTime
        Uptime = $uptimeString
        TimeZone = $timeZone
        NtpServers = $ntpServers
        NtpStatus = $ntpStatus
        LicenseStatus = $licenseStatus
        LicenseType = $licenseType
        ProductKey = $productKey
        GracePeriod = $graceRemaining
        LicenseDescription = $licenseDescription
        Manufacturer = if ($computerSystem.Manufacturer) { $computerSystem.Manufacturer } else { '' }
        Model = if ($computerSystem.Model) { $computerSystem.Model } else { '' }
        SerialNumber = $serialNumber
        Processor = if ($processors -and $processors[0].Name) { $processors[0].Name -replace '\s+', ' ' } else { '' }
        Sockets = $processorSockets
        Cores = $totalCores
        LogicalCPUs = $totalLogicalProcessors
        HyperThreading = if ($totalLogicalProcessors -gt $totalCores) { 'Yes' } else { 'No' }
        SLATSupport = $slatSupport
        TotalMemoryGB = $totalMemoryGB
        UsedMemoryGB = [Math]::Round($usedMemoryGB, 2)
        FreeMemoryGB = [Math]::Round($availableMemoryGB, 2)
        MemoryUsagePercent = $memoryUsagePercent
        TotalVMs = $allVMs.Count
        RunningVMs = $runningVMs.Count
        StoppedVMs = $stoppedVMs.Count
        VirtualSwitches = $virtualSwitches.Count
        ExternalSwitches = $externalSwitches.Count
        IPAddresses = $ipAddresses
        LiveMigration = if ($vmHost -and $vmHost.VirtualMachineMigrationEnabled) { 'Enabled' } else { 'Disabled' }
        EnhancedSession = if ($vmHost -and $vmHost.EnableEnhancedSessionMode) { 'Yes' } else { 'No' }
        NUMASpanning = if ($vmHost -and $vmHost.NumaSpanningEnabled) { 'Yes' } else { 'No' }
        VHDPath = if ($vmHost -and $vmHost.VirtualHardDiskPath) { $vmHost.VirtualHardDiskPath } else { '' }
        VMConfigPath = if ($vmHost -and $vmHost.VirtualMachinePath) { $vmHost.VirtualMachinePath } else { '' }
    }

    # Output as JSON
    $result | ConvertTo-Json -Compress
} catch {
    # Return error as JSON
    @{
        HostName = $env:COMPUTERNAME
        ClusterName = 'N/A'
        NodeState = 'Error'
        Domain = ''
        OperatingSystem = ""Error: $($_.Exception.Message)""
        OSVersion = ''
        BuildNumber = ''
        BootTime = ''
        Uptime = ''
        TimeZone = ''
        NtpServers = ''
        NtpStatus = ''
        LicenseStatus = 'Unknown'
        LicenseType = 'Unknown'
        ProductKey = 'Unknown'
        GracePeriod = 'N/A'
        LicenseDescription = ''
        Manufacturer = ''
        Model = ''
        SerialNumber = ''
        Processor = ''
        Sockets = 0
        Cores = 0
        LogicalCPUs = 0
        HyperThreading = 'No'
        SLATSupport = 'No'
        TotalMemoryGB = 0
        UsedMemoryGB = 0
        FreeMemoryGB = 0
        MemoryUsagePercent = 0
        TotalVMs = 0
        RunningVMs = 0
        StoppedVMs = 0
        VirtualSwitches = 0
        ExternalSwitches = 0
        IPAddresses = ''
        LiveMigration = 'Unknown'
        EnhancedSession = 'Unknown'
        NUMASpanning = 'Unknown'
        VHDPath = ''
        VMConfigPath = ''
    } | ConvertTo-Json -Compress
}
";
        }

        /// <summary>
        /// Parses a PSObject into a HostDetailsInfo object
        /// </summary>
        private static HostDetailsInfo ParseHostDetails(PSObject psObject)
        {
            try
            {
                // Log all available properties for debugging
                FileLogger.Message("Parsing PSObject with the following properties:",
                    FileLogger.EventType.Information, 4042);
                
                if (psObject?.Properties != null)
                {
                    foreach (var prop in psObject.Properties)
                    {
                        var value = prop.Value?.ToString() ?? "null";
                        FileLogger.Message($"  Property: {prop.Name} = {value}",
                            FileLogger.EventType.Information, 4043);
                    }
                }
                else
                {
                    FileLogger.Message("PSObject or Properties collection is null!",
                        FileLogger.EventType.Error, 4044);
                }

                return new HostDetailsInfo
                {
                    HostName = GetStringProperty(psObject, "HostName"),
                    ClusterName = GetStringProperty(psObject, "ClusterName"),
                    NodeState = GetStringProperty(psObject, "NodeState"),
                    Domain = GetStringProperty(psObject, "Domain"),
                    OperatingSystem = GetStringProperty(psObject, "OperatingSystem"),
                    OsVersion = GetStringProperty(psObject, "OSVersion"),
                    BuildNumber = GetStringProperty(psObject, "BuildNumber"),
                    BootTime = GetStringProperty(psObject, "BootTime"),
                    Uptime = GetStringProperty(psObject, "Uptime"),
                    TimeZone = GetStringProperty(psObject, "TimeZone"),
                    NtpServers = GetStringProperty(psObject, "NtpServers"),
                    NtpStatus = GetStringProperty(psObject, "NtpStatus"),
                    LicenseStatus = GetStringProperty(psObject, "LicenseStatus"),
                    LicenseType = GetStringProperty(psObject, "LicenseType"),
                    ProductKey = GetStringProperty(psObject, "ProductKey"),
                    GracePeriod = GetStringProperty(psObject, "GracePeriod"),
                    LicenseDescription = GetStringProperty(psObject, "LicenseDescription"),
                    Manufacturer = GetStringProperty(psObject, "Manufacturer"),
                    Model = GetStringProperty(psObject, "Model"),
                    SerialNumber = GetStringProperty(psObject, "SerialNumber"),
                    Processor = GetStringProperty(psObject, "Processor"),
                    Sockets = GetIntProperty(psObject, "Sockets"),
                    Cores = GetIntProperty(psObject, "Cores"),
                    LogicalCpUs = GetIntProperty(psObject, "LogicalCPUs"),
                    HyperThreading = GetStringProperty(psObject, "HyperThreading"),
                    SlatSupport = GetStringProperty(psObject, "SLATSupport"),
                    TotalMemoryGb = GetDoubleProperty(psObject, "TotalMemoryGB"),
                    UsedMemoryGb = GetDoubleProperty(psObject, "UsedMemoryGB"),
                    FreeMemoryGb = GetDoubleProperty(psObject, "FreeMemoryGB"),
                    MemoryUsagePercent = GetDoubleProperty(psObject, "MemoryUsagePercent"),
                    TotalVMs = GetIntProperty(psObject, "TotalVMs"),
                    RunningVMs = GetIntProperty(psObject, "RunningVMs"),
                    StoppedVMs = GetIntProperty(psObject, "StoppedVMs"),
                    VirtualSwitches = GetIntProperty(psObject, "VirtualSwitches"),
                    ExternalSwitches = GetIntProperty(psObject, "ExternalSwitches"),
                    IpAddresses = GetStringProperty(psObject, "IPAddresses"),
                    LiveMigration = GetStringProperty(psObject, "LiveMigration"),
                    EnhancedSession = GetStringProperty(psObject, "EnhancedSession"),
                    NumaSpanning = GetStringProperty(psObject, "NUMASpanning"),
                    VhdPath = GetStringProperty(psObject, "VHDPath"),
                    VmConfigPath = GetStringProperty(psObject, "VMConfigPath")
                };
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Error in ParseHostDetails: {ex.Message}",
                    FileLogger.EventType.Error, 4045);
                FileLogger.Message($"Stack trace: {ex.StackTrace}",
                    FileLogger.EventType.Error, 4046);
                return null;
            }
        }

        private static string GetStringProperty(PSObject psObject, string propertyName)
        {
            try
            {
                var prop = psObject.Properties[propertyName];
                return prop?.Value?.ToString() ?? "";
            }
            catch
            {
                return "";
            }
        }

        private static int GetIntProperty(PSObject psObject, string propertyName)
        {
            try
            {
                var prop = psObject.Properties[propertyName];
                if (prop?.Value != null)
                {
                    return Convert.ToInt32(prop.Value);
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        private static double GetDoubleProperty(PSObject psObject, string propertyName)
        {
            try
            {
                var prop = psObject.Properties[propertyName];
                if (prop?.Value != null)
                {
                    return Convert.ToDouble(prop.Value);
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }
    }
}
