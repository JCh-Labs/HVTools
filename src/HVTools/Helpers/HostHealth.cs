using System.Management.Automation;
using System.Text.Json;

namespace HVTools.Helpers
{
    #region Data Models

    /// <summary>
    /// Represents host information from the health
    /// </summary>
    public class HostHealthHostInfo
    {
        public string ComputerName { get; set; } = "";
        public string FullyQualifiedDomainName { get; set; } = "";
        public string HyperVVersion { get; set; } = "";
        public int LogicalProcessors { get; set; }
        public int PhysicalProcessors { get; set; }
        public int ProcessorSockets { get; set; }
        public string NodeState { get; set; } = "Standalone";
        public string ClusterName { get; set; } = "N/A";
        public double TotalMemoryGb { get; set; }
        public string VirtualHardDiskPath { get; set; } = "";
        public string VirtualMachinePath { get; set; } = "";
        public bool EnableEnhancedSessionMode { get; set; }
        public bool NumaSpanningEnabled { get; set; }
        
        // Cluster node information
        public int ClusterNodeCount { get; set; }
        public int ClusterNodesOnline { get; set; }
        public int ClusterNodesOffline { get; set; }
        public int ClusterNodesPaused { get; set; }
        public List<ClusterNodeSummary> ClusterNodes { get; set; } = [];
    }

    /// <summary>
    /// Represents summary information for a cluster node
    /// </summary>
    public class ClusterNodeSummary
    {
        public string Name { get; set; } = "";
        public string Fqdn { get; set; } = "";
        public string State { get; set; } = "";
        public bool IsCurrentNode { get; set; }
    }

    /// <summary>
    /// Represents resource allocation information
    /// </summary>
    public class ResourceAllocationInfo
    {
        public int TotalVmProcessors { get; set; }
        public long TotalVmMemoryMb { get; set; }
        public long TotalVmStartupMemoryMb { get; set; }
        public double CpuOvercommitRatio { get; set; }
        public double MemoryOvercommitRatio { get; set; }
    }

    /// <summary>
    /// Represents storage information for a drive
    /// </summary>
    public class StorageDriveInfo
    {
        public string DriveLetter { get; set; } = "";
        public double TotalGb { get; set; }
        public double UsedGb { get; set; }
        public double FreeGb { get; set; }
        public double UsedPercent { get; set; }
        public int VmFileCount { get; set; }
    }

    /// <summary>
    /// Represents network adapter information
    /// </summary>
    public class NetworkAdapterInfo
    {
        public string Name { get; set; } = "";
        public string InterfaceDescription { get; set; } = "";
        public int VirtualSwitches { get; set; }
    }

    /// <summary>
    /// Represents performance data
    /// </summary>
    public class PerformanceDataInfo
    {
        public double CpuUsagePercent { get; set; }
        public double AvailableMemoryMb { get; set; }
        public double MemoryUsagePercent { get; set; }
        public bool DataAvailable { get; set; } = true;
    }

    /// <summary>
    /// Represents workload analysis data
    /// </summary>
    public class WorkloadAnalysisInfo
    {
        public int TotalVMs { get; set; }
        public int RunningVMs { get; set; }
        public int StoppedVMs { get; set; }
        public int PausedVMs { get; set; }
        public int SavedVMs { get; set; }
        public int Generation1VMs { get; set; }
        public int Generation2VMs { get; set; }
        public int ReplicatedVMs { get; set; }
        public int CheckpointedVMs { get; set; }
    }

    /// <summary>
    /// Represents idle resource information
    /// </summary>
    public class IdleResourcesInfo
    {
        public List<string> IdleVmNames { get; set; } = [];
        public List<string> LowUtilizationVmNames { get; set; } = [];
        public List<string> UnusedNetworkAdapterNames { get; set; } = [];
    }

    /// <summary>
    /// Complete host health information
    /// </summary>
    public class HostHealthInfo
    {
        public HostHealthHostInfo HostInfo { get; set; } = new();
        public ResourceAllocationInfo ResourceAllocation { get; set; } = new();
        public List<StorageDriveInfo> StorageInfo { get; set; } = [];
        public List<NetworkAdapterInfo> NetworkInfo { get; set; } = [];
        public PerformanceDataInfo PerformanceData { get; set; } = new();
        public WorkloadAnalysisInfo WorkloadAnalysis { get; set; } = new();
        public IdleResourcesInfo IdleResources { get; set; } = new();
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    #endregion

    /// <summary>
    /// Provides functionality to retrieve comprehensive Hyper-V host health
    /// </summary>
    public static class HostHealth
    {
        /// <summary>
        /// Gets comprehensive host health information
        /// </summary>
        /// <param name="executePowerShellCommand">Function to execute PowerShell commands</param>
        /// <param name="executePowerShellCommandOnNode">Optional function to execute commands on specific cluster nodes</param>
        /// <param name="includeDetailedVMs">Whether to include detailed VM information</param>
        /// <returns>Host health information or null if failed</returns>
        public static HostHealthInfo? GetHyperVHostHealth(
            Func<string, System.Collections.ObjectModel.Collection<PSObject>?> executePowerShellCommand,
            Func<string, string, System.Collections.ObjectModel.Collection<PSObject>?>? executePowerShellCommandOnNode = null,
            bool includeDetailedVMs = false)
        {
            try
            {
                // Check if we have an active session
                if (!SessionContext.IsSessionActive())
                {
                    FileLogger.Message("No active Hyper-V connection found",
                        FileLogger.EventType.Error, 7001);
                    return null;
                }

                FileLogger.Message("Starting Hyper-V host health collection...",
                    FileLogger.EventType.Information, 7002);

                bool isLocal = SessionContext.IsLocal;

                HostHealthInfo? health;

                if (isLocal)
                {
                    // For local execution, use Windows PowerShell process for full WMI support
                    FileLogger.Message("Using Windows PowerShell process for local health collection...",
                        FileLogger.EventType.Information, 7003);
                    health = GetHealthViaWindowsPowerShell();
                }
                else
                {
                    // For remote, use embedded PowerShell with Invoke-Command
                    FileLogger.Message("Executing health script via remote PowerShell...",
                        FileLogger.EventType.Information, 7004);
                    health = GetHealthViaRemote(executePowerShellCommand);
                }

                if (health != null)
                {
                    FileLogger.Message("Host health collection completed successfully",
                        FileLogger.EventType.Information, 7005);
                }
                else
                {
                    FileLogger.Message("Host  collection returned null",
                        FileLogger.EventType.Warning, 7006);
                }

                return health;
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Error getting host health: {ex.Message}",
                    FileLogger.EventType.Error, 7007);
                FileLogger.Message($"Stack trace: {ex.StackTrace}",
                    FileLogger.EventType.Error, 7008);
                return null;
            }
        }

        /// <summary>
        /// Gets health via Windows PowerShell process (for local execution)
        /// </summary>
        private static HostHealthInfo? GetHealthViaWindowsPowerShell()
        {
            try
            {
                string script = GetHealthScript();

                // Create a temporary script file
                string tempScriptPath = Path.Combine(Path.GetTempPath(), $"HVTools_Health_{Guid.NewGuid():N}.ps1");
                File.WriteAllText(tempScriptPath, script);

                FileLogger.Message($"Created temp health script: '{tempScriptPath}'",
                    FileLogger.EventType.Information, 7010);

                try
                {
                    // Execute via Windows PowerShell process
                    var psi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = $"-NoProfile -NonInteractive -ExecutionPolicy Bypass -File \"{tempScriptPath}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using var process = System.Diagnostics.Process.Start(psi);
                    if (process == null)
                    {
                        FileLogger.Message("Failed to start Windows PowerShell process",
                            FileLogger.EventType.Error, 7011);
                        return null;
                    }

                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit(120000); // 120 second timeout

                    if (!string.IsNullOrEmpty(error))
                    {
                        FileLogger.Message($"PowerShell stderr: {error}",
                            FileLogger.EventType.Warning, 7012);
                    }

                    if (string.IsNullOrWhiteSpace(output))
                    {
                        FileLogger.Message("PowerShell process returned empty output",
                            FileLogger.EventType.Warning, 7013);
                        return null;
                    }

                    FileLogger.Message($"PowerShell output length: {output.Length} chars",
                        FileLogger.EventType.Information, 7014);

                    // Parse JSON output
                    return ParseJsonHealth(output);
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
                    catch
                    {
                        // ignored
                    }
                }
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Error in GetHealthViaWindowsPowerShell: {ex.Message}",
                    FileLogger.EventType.Error, 7015);
                return null;
            }
        }

        /// <summary>
        /// Gets health via remote PowerShell execution
        /// </summary>
        private static HostHealthInfo? GetHealthViaRemote(
            Func<string, System.Collections.ObjectModel.Collection<PSObject>?> executePowerShellCommand)
        {
            try
            {
                // Use the same JSON-outputting script as local, so we get consistent data format
                string script = GetHealthScript();
                var result = executePowerShellCommand(script);

                if (result == null || result.Count == 0)
                {
                    FileLogger.Message("Remote health script returned no results",
                        FileLogger.EventType.Warning, 7020);
                    return null;
                }

                FileLogger.Message($"Remote health script returned {result.Count} result(s), parsing JSON...",
                    FileLogger.EventType.Information, 7021);

                // The script outputs JSON, so we need to extract the JSON string from the result
                // When executed remotely, the JSON string comes back as a PSObject wrapping the string
                string jsonOutput = "";
                
                // Collect all output lines (JSON may be split across multiple results)
                foreach (var item in result)
                {
                    {
                        string itemStr = item.BaseObject?.ToString() ?? item.ToString();
                        if (!string.IsNullOrWhiteSpace(itemStr))
                        {
                            jsonOutput += itemStr;
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(jsonOutput))
                {
                    FileLogger.Message("Remote health script returned empty JSON",
                        FileLogger.EventType.Warning, 7023);
                    return null;
                }

#if DEBUG
                FileLogger.Message($"Remote JSON output length: {jsonOutput.Length} chars",
                    FileLogger.EventType.Information, 7024);
#endif
                // Parse the JSON using the same method as local
                return ParseJsonHealth(jsonOutput);
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Error in GetHealthViaRemote: {ex.Message}",
                    FileLogger.EventType.Error, 7022);
                return null;
            }
        }

        /// <summary>
        /// Parses JSON output into HostHealthInfo
        /// </summary>
        private static HostHealthInfo? ParseJsonHealth(string json)
        {
            try
            {
                json = json.Trim();

                FileLogger.Message("Parsing JSON health...",
                    FileLogger.EventType.Information, 7030);

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                var health = new HostHealthInfo
                {
                    Timestamp = DateTime.Now
                };

                // Parse HostInfo
                if (root.TryGetProperty("HostInfo", out var hostInfoElement))
                {
                    health.HostInfo = new HostHealthHostInfo
                    {
                        ComputerName = GetJsonString(hostInfoElement, "ComputerName"),
                        FullyQualifiedDomainName = GetJsonString(hostInfoElement, "FullyQualifiedDomainName"),
                        HyperVVersion = GetJsonString(hostInfoElement, "HyperVVersion"),
                        LogicalProcessors = GetJsonInt(hostInfoElement, "LogicalProcessors"),
                        PhysicalProcessors = GetJsonInt(hostInfoElement, "PhysicalProcessors"),
                        ProcessorSockets = GetJsonInt(hostInfoElement, "ProcessorSockets"),
                        NodeState = GetJsonString(hostInfoElement, "NodeState"),
                        ClusterName = GetJsonString(hostInfoElement, "ClusterName"),
                        TotalMemoryGb = GetJsonDouble(hostInfoElement, "TotalMemoryGB"),
                        VirtualHardDiskPath = GetJsonString(hostInfoElement, "VirtualHardDiskPath"),
                        VirtualMachinePath = GetJsonString(hostInfoElement, "VirtualMachinePath"),
                        EnableEnhancedSessionMode = GetJsonBool(hostInfoElement, "EnableEnhancedSessionMode"),
                        NumaSpanningEnabled = GetJsonBool(hostInfoElement, "NumaSpanningEnabled"),
                        ClusterNodeCount = GetJsonInt(hostInfoElement, "ClusterNodeCount"),
                        ClusterNodesOnline = GetJsonInt(hostInfoElement, "ClusterNodesOnline"),
                        ClusterNodesOffline = GetJsonInt(hostInfoElement, "ClusterNodesOffline"),
                        ClusterNodesPaused = GetJsonInt(hostInfoElement, "ClusterNodesPaused")
                    };

                    // Parse ClusterNodes array
                    if (hostInfoElement.TryGetProperty("ClusterNodes", out var clusterNodesElement) &&
                        clusterNodesElement.ValueKind == JsonValueKind.Array)
                    {
                        // Get domain suffix from SessionContext for constructing FQDNs
                        string domainSuffix = GetDomainSuffixFromSessionContext();
                        
                        foreach (var nodeElement in clusterNodesElement.EnumerateArray())
                        {
                            string nodeName = GetJsonString(nodeElement, "Name");
                            string nodeFqdn = GetJsonString(nodeElement, "Fqdn");
                            
                            // Check if FQDN is invalid (empty, same as name, or contains WORKGROUP)
                            bool isFqdnInvalid = string.IsNullOrEmpty(nodeFqdn) || 
                                                 nodeFqdn == nodeName ||
                                                 nodeFqdn.Contains("WORKGROUP", StringComparison.OrdinalIgnoreCase) ||
                                                 !nodeFqdn.Contains('.');
                            
                            // If FQDN is invalid, construct it from SessionContext domain
                            if (isFqdnInvalid && !string.IsNullOrEmpty(domainSuffix))
                            {
                                nodeFqdn = $"{nodeName}.{domainSuffix}";
                                FileLogger.Message($"Corrected FQDN for node '{nodeName}': '{nodeFqdn}'",
                                    FileLogger.EventType.Information, 7042);
                            }
                            else if (isFqdnInvalid)
                            {
                                // No valid domain suffix available, use just the node name
                                nodeFqdn = nodeName;
                                FileLogger.Message($"No valid domain suffix available for node '{nodeName}', using short name",
                                    FileLogger.EventType.Warning, 7043);
                            }
                            
                            health.HostInfo.ClusterNodes.Add(new ClusterNodeSummary
                            {
                                Name = nodeName,
                                Fqdn = nodeFqdn,
                                State = GetJsonString(nodeElement, "State"),
                                IsCurrentNode = GetJsonBool(nodeElement, "IsCurrentNode")
                            });
                        }
                    }
                }

                // Parse ResourceAllocation
                if (root.TryGetProperty("ResourceAllocation", out var resourceElement))
                {
                    health.ResourceAllocation = new ResourceAllocationInfo
                    {
                        TotalVmProcessors = GetJsonInt(resourceElement, "TotalVMProcessors"),
                        TotalVmMemoryMb = GetJsonLong(resourceElement, "TotalVMMemoryMB"),
                        TotalVmStartupMemoryMb = GetJsonLong(resourceElement, "TotalVMStartupMemoryMB"),
                        CpuOvercommitRatio = GetJsonDouble(resourceElement, "CPUOvercommitRatio"),
                        MemoryOvercommitRatio = GetJsonDouble(resourceElement, "MemoryOvercommitRatio")
                    };
                }

                // Parse StorageInfo
                if (root.TryGetProperty("StorageInfo", out var storageElement) &&
                    storageElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var driveElement in storageElement.EnumerateArray())
                    {
                        health.StorageInfo.Add(new StorageDriveInfo
                        {
                            DriveLetter = GetJsonString(driveElement, "DriveLetter"),
                            TotalGb = GetJsonDouble(driveElement, "TotalGB"),
                            UsedGb = GetJsonDouble(driveElement, "UsedGB"),
                            FreeGb = GetJsonDouble(driveElement, "FreeGB"),
                            UsedPercent = GetJsonDouble(driveElement, "UsedPercent"),
                            VmFileCount = GetJsonInt(driveElement, "VMFileCount")
                        });
                    }
                }

                // Parse NetworkInfo
                if (root.TryGetProperty("NetworkInfo", out var networkElement) &&
                    networkElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var adapterElement in networkElement.EnumerateArray())
                    {
                        health.NetworkInfo.Add(new NetworkAdapterInfo
                        {
                            Name = GetJsonString(adapterElement, "Name"),
                            InterfaceDescription = GetJsonString(adapterElement, "InterfaceDescription"),
                            VirtualSwitches = GetJsonInt(adapterElement, "VirtualSwitches")
                        });
                    }
                }

                // Parse PerformanceData
                if (root.TryGetProperty("PerformanceData", out var perfElement))
                {
                    // Check if DataAvailable is explicitly set, or infer from CPU value
                    bool dataAvailable = GetJsonBool(perfElement, "DataAvailable");
                    
                    // Also check if CPU value is a number (not "N/A" string)
                    if (!dataAvailable && perfElement.TryGetProperty("CPUUsagePercent", out var cpuProp))
                    {
                        dataAvailable = cpuProp.ValueKind == JsonValueKind.Number;
                    }

                    health.PerformanceData = new PerformanceDataInfo
                    {
                        CpuUsagePercent = GetJsonDoubleOrDefault(perfElement, "CPUUsagePercent", 0),
                        AvailableMemoryMb = GetJsonDoubleOrDefault(perfElement, "AvailableMemoryMB", 0),
                        MemoryUsagePercent = GetJsonDoubleOrDefault(perfElement, "MemoryUsagePercent", 0),
                        DataAvailable = dataAvailable
                    };
                }

                // Parse WorkloadAnalysis
                if (root.TryGetProperty("WorkloadAnalysis", out var workloadElement))
                {
                    health.WorkloadAnalysis = new WorkloadAnalysisInfo
                    {
                        TotalVMs = GetJsonInt(workloadElement, "TotalVMs"),
                        RunningVMs = GetJsonInt(workloadElement, "RunningVMs"),
                        StoppedVMs = GetJsonInt(workloadElement, "StoppedVMs"),
                        PausedVMs = GetJsonInt(workloadElement, "PausedVMs"),
                        SavedVMs = GetJsonInt(workloadElement, "SavedVMs"),
                        Generation1VMs = GetJsonInt(workloadElement, "Generation1VMs"),
                        Generation2VMs = GetJsonInt(workloadElement, "Generation2VMs"),
                        ReplicatedVMs = GetJsonInt(workloadElement, "ReplicatedVMs"),
                        CheckpointedVMs = GetJsonInt(workloadElement, "CheckpointedVMs")
                    };
                }

                // Parse IdleResources
                if (root.TryGetProperty("IdleResources", out var idleElement))
                {
                    health.IdleResources = new IdleResourcesInfo();

                    if (idleElement.TryGetProperty("IdleVMNames", out var idleVMs) &&
                        idleVMs.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var vm in idleVMs.EnumerateArray())
                        {
                            health.IdleResources.IdleVmNames.Add(vm.GetString() ?? "");
                        }
                    }

                    if (idleElement.TryGetProperty("LowUtilizationVMNames", out var lowUtilVMs) &&
                        lowUtilVMs.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var vm in lowUtilVMs.EnumerateArray())
                        {
                            health.IdleResources.LowUtilizationVmNames.Add(vm.GetString() ?? "");
                        }
                    }

                    if (idleElement.TryGetProperty("UnusedNetworkAdapterNames", out var unusedAdapters) &&
                        unusedAdapters.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var adapter in unusedAdapters.EnumerateArray())
                        {
                            health.IdleResources.UnusedNetworkAdapterNames.Add(adapter.GetString() ?? "");
                        }
                    }
                }

                // Parse Timestamp
                if (root.TryGetProperty("Timestamp", out var timestampElement))
                {
                    if (DateTime.TryParse(timestampElement.GetString(), out var timestamp))
                    {
                        health.Timestamp = timestamp;
                    }
                }
#if DEBUG
                FileLogger.Message($"Successfully parsed JSON health for '{health.HostInfo.ComputerName}'",
                    FileLogger.EventType.Information, 7031);
#endif
                return health;
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Error parsing JSON health: {ex.Message}",
                    FileLogger.EventType.Error, 7032);
                FileLogger.Message($"JSON content (first 500 chars): {json[..Math.Min(500, json.Length)]}",
                    FileLogger.EventType.Error, 7033);
                return null;
            }
        }

        #region JSON Parsing Helpers

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

        private static long GetJsonLong(JsonElement element, string propertyName)
        {
            try
            {
                if (element.TryGetProperty(propertyName, out var prop))
                {
                    if (prop.ValueKind == JsonValueKind.Number)
                    {
                        return prop.GetInt64();
                    }
                    if (prop.ValueKind == JsonValueKind.String && long.TryParse(prop.GetString(), out long val))
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
        /// Gets a double value from JSON, returning default if the value is "N/A" or not a number
        /// </summary>
        private static double GetJsonDoubleOrDefault(JsonElement element, string propertyName, double defaultValue)
        {
            try
            {
                if (element.TryGetProperty(propertyName, out var prop))
                {
                    // If it's a number, return it
                    if (prop.ValueKind == JsonValueKind.Number)
                    {
                        return prop.GetDouble();
                    }
                    // If it's a string that's NOT "N/A", try to parse it
                    if (prop.ValueKind == JsonValueKind.String)
                    {
                        string strVal = prop.GetString() ?? "";
                        if (!strVal.Equals("N/A", StringComparison.OrdinalIgnoreCase) && 
                            double.TryParse(strVal, out double val))
                        {
                            return val;
                        }
                    }
                }
                return defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        private static bool GetJsonBool(JsonElement element, string propertyName)
        {
            try
            {
                if (element.TryGetProperty(propertyName, out var prop))
                {
                    if (prop.ValueKind == JsonValueKind.True) return true;
                    if (prop.ValueKind == JsonValueKind.False) return false;
                    if (prop.ValueKind == JsonValueKind.String && bool.TryParse(prop.GetString(), out bool val))
                    {
                        return val;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Extracts the domain suffix from the connection server name (SessionContext.ServerName)
        /// This is more reliable than FullyQualifiedDomainName which can return "WORKGROUP"
        /// For example: "az-clu00.appelcloud.dk" returns "appelcloud.dk"
        /// </summary>
        private static string GetDomainSuffixFromSessionContext()
        {
            try
            {
                // Use ServerName (the actual connection string) instead of FullyQualifiedDomainName
                // because FullyQualifiedDomainName can return "WORKGROUP" from the host
                string? serverName = SessionContext.ServerName;
                
                // If ServerName doesn't have a domain, fall back to FullyQualifiedDomainName
                if (string.IsNullOrEmpty(serverName) || !serverName.Contains('.'))
                {
                    serverName = SessionContext.FullyQualifiedDomainName;
                }
                
                if (string.IsNullOrEmpty(serverName))
                    return "";

                // Skip if it's just "WORKGROUP" or similar non-domain value
                if (serverName.Equals("WORKGROUP", StringComparison.OrdinalIgnoreCase))
                    return "";

                // Find the first dot to extract domain suffix
                int dotIndex = serverName.IndexOf('.');
                if (dotIndex > 0 && dotIndex < serverName.Length - 1)
                {
                    string domainSuffix = serverName[(dotIndex + 1)..];
                    FileLogger.Message($"Extracted domain suffix from connection: '{domainSuffix}' (from '{serverName}')",
                        FileLogger.EventType.Information, 7040);
                    return domainSuffix;
                }

                return "";
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Error extracting domain suffix: {ex.Message}",
                    FileLogger.EventType.Warning, 7041);
                return "";
            }
        }

        #endregion

        #region PowerShell Scripts

        /// <summary>
        /// Gets the PowerShell script for local Windows PowerShell execution (outputs JSON)
        /// </summary>
        private static string GetHealthScript()
        {
            return $@"
$ErrorActionPreference = 'SilentlyContinue'

try {{
# Get Host Information
    $vmHost = Get-VMHost -ErrorAction SilentlyContinue
    $computerInfo = Get-ComputerInfo -ErrorAction SilentlyContinue
    
# Get cluster information if available
    $clusterNode = $null
    $clusterName = 'N/A'
    $nodeState = 'Standalone'
    $clusterNodeCount = 0
    $clusterNodesOnline = 0
    $clusterNodesOffline = 0
    $clusterNodesPaused = 0
    $clusterNodesArray = @()
    
    try {{
        $clusterNode = Get-ClusterNode -Name $env:COMPUTERNAME -ErrorAction SilentlyContinue
        if ($clusterNode) {{
            $cluster = Get-Cluster -ErrorAction SilentlyContinue
            $clusterName = if ($cluster) {{ $cluster.Name }} else {{ 'Cluster Detected' }}
            $nodeState = if ($clusterNode.State) {{ $clusterNode.State.ToString() }} else {{ 'Online' }}
            
# Get the domain suffix for FQDN construction
            $domainSuffix = ''
            try {{
                $domainSuffix = (Get-WmiObject Win32_ComputerSystem -ErrorAction SilentlyContinue).Domain
                if (-not $domainSuffix) {{
                    $domainSuffix = [System.Net.NetworkInformation.IPGlobalProperties]::GetIPGlobalProperties().DomainName
                }}
            }} catch {{ }}
            
# Get all cluster nodes and their states
            $allClusterNodes = @(Get-ClusterNode -ErrorAction SilentlyContinue)
            $clusterNodeCount = $allClusterNodes.Count
            $clusterNodesOnline = @($allClusterNodes | Where-Object {{ $_.State -eq 'Up' }}).Count
            $clusterNodesOffline = @($allClusterNodes | Where-Object {{ $_.State -eq 'Down' }}).Count
            $clusterNodesPaused = @($allClusterNodes | Where-Object {{ $_.State -eq 'Paused' }}).Count
            
# Build cluster nodes array with details including FQDN
            foreach ($node in $allClusterNodes) {{
# Construct FQDN for the node
                $nodeFqdn = $node.Name
                if ($domainSuffix) {{
                    $nodeFqdn = ""$($node.Name).$domainSuffix""
                }}
                
                $clusterNodesArray += @{{
                    Name = $node.Name
                    Fqdn = $nodeFqdn
                    State = $node.State.ToString()
                    IsCurrentNode = ($node.Name -eq $env:COMPUTERNAME)
                }}
            }}
        }}
    }} catch {{ }}
    
# Get all VMs
    $allVMs = @(Get-VM -ErrorAction SilentlyContinue)
    $runningVMs = @($allVMs | Where-Object {{ $_.State -eq 'Running' }})
    
# Calculate VM Resource Usage
    $totalVMProcessors = ($allVMs | Measure-Object ProcessorCount -Sum).Sum
    if (-not $totalVMProcessors) {{ $totalVMProcessors = 0 }}
    
    $totalVMMemoryMB = [Math]::Round(($allVMs | Measure-Object MemoryAssigned -Sum).Sum / 1MB, 0)
    if (-not $totalVMMemoryMB) {{ $totalVMMemoryMB = 0 }}
    
    $totalVMStartupMemoryMB = [Math]::Round(($allVMs | Measure-Object MemoryStartup -Sum).Sum / 1MB, 0)
    if (-not $totalVMStartupMemoryMB) {{ $totalVMStartupMemoryMB = 0 }}
    
# Get Physical Hardware Info
    $processors = @(Get-WmiObject Win32_Processor -ErrorAction SilentlyContinue)
    $physicalProcessors = ($processors | Measure-Object NumberOfCores -Sum).Sum
    if (-not $physicalProcessors) {{ $physicalProcessors = 1 }}
    
    $processorSockets = $processors.Count
    if ($processorSockets -eq 0) {{ $processorSockets = 1 }}
    
# Get Physical RAM Info
    try {{
        $physicalMemoryGB = [Math]::Round((Get-WmiObject Win32_ComputerSystem).TotalPhysicalMemory / 1GB, 2)
        if ($physicalMemoryGB -eq 0) {{
            $physicalMemoryGB = [Math]::Round((Get-CimInstance Win32_PhysicalMemory | Measure-Object Capacity -Sum).Sum / 1GB, 2)
        }}
    }} catch {{
        $physicalMemoryGB = if ($computerInfo) {{ [Math]::Round($computerInfo.TotalPhysicalMemory / 1GB, 2) }} else {{ 0 }}
    }}
    
# Calculate Overcommit Ratios
    $cpuOvercommitRatio = if ($physicalProcessors -gt 0) {{ [Math]::Round($totalVMProcessors / $physicalProcessors, 2) }} else {{ 0 }}
    $memoryOvercommitRatio = if ($physicalMemoryGB -gt 0) {{ [Math]::Round($totalVMMemoryMB / ($physicalMemoryGB * 1024), 2) }} else {{ 0 }}
    
# Get Storage Information
    $vmStoragePaths = @()
    foreach ($vm in $allVMs) {{
        $drives = Get-VMHardDiskDrive -VM $vm -ErrorAction SilentlyContinue
        foreach ($drive in $drives) {{
            if ($drive.Path) {{
                $vmStoragePaths += $drive.Path
            }}
        }}
    }}
    
    $storageInfoArray = @()
    $drives = Get-WmiObject Win32_LogicalDisk -ErrorAction SilentlyContinue | Where-Object {{ $_.DriveType -eq 3 }}
    foreach ($drive in $drives) {{
        $driveLetter = $drive.DeviceID
        $totalSpaceGB = [Math]::Round($drive.Size / 1GB, 2)
        $freeSpaceGB = [Math]::Round($drive.FreeSpace / 1GB, 2)
        $usedSpaceGB = $totalSpaceGB - $freeSpaceGB
        $usedPercent = if ($totalSpaceGB -gt 0) {{ [Math]::Round(($usedSpaceGB / $totalSpaceGB) * 100, 1) }} else {{ 0 }}
        
# Count VM files on this drive
        $vmFilesOnDrive = ($vmStoragePaths | Where-Object {{ $_ -like ""$driveLetter*"" }}).Count
        
        $storageInfoArray += @{{
            DriveLetter = $driveLetter
            TotalGB = $totalSpaceGB
            UsedGB = $usedSpaceGB
            FreeGB = $freeSpaceGB
            UsedPercent = $usedPercent
            VMFileCount = $vmFilesOnDrive
        }}
    }}
    
# Network Adapter Information
    $networkAdapters = Get-NetAdapter -ErrorAction SilentlyContinue | Where-Object {{ $_.Status -eq 'Up' }}
    $networkInfoArray = @()
    foreach ($adapter in $networkAdapters) {{
        
        $vmSwitchCount = @(Get-VMSwitch -ErrorAction SilentlyContinue | Where-Object {{ $_.NetAdapterInterfaceDescription -eq $adapter.InterfaceDescription }}).Count
        
        $networkInfoArray += @{{
            Name = $adapter.Name
            InterfaceDescription = $adapter.InterfaceDescription
            VirtualSwitches = $vmSwitchCount
        }}
    }}
    
# Identify Idle Resources
    $idleVMs = @($allVMs | Where-Object {{
        $_.State -eq 'Off' -and
        $_.CreationTime -lt (Get-Date).AddDays(-30)
    }})
    
    $idleVMNames = @()
    foreach ($vm in $idleVMs) {{
        $idleVMNames += $vm.Name
    }}
    
    $unusedAdapterNames = @()
    foreach ($adapter in $networkAdapters) {{
        $vmSwitches = @(Get-VMSwitch -ErrorAction SilentlyContinue | Where-Object {{ $_.NetAdapterInterfaceDescription -eq $adapter.InterfaceDescription }})
        if ($vmSwitches.Count -eq 0) {{
            $unusedAdapterNames += $adapter.Name
        }}
    }}
    
# Performance Counters
    $performanceData = @{{
        CPUUsagePercent = 'N/A'
        AvailableMemoryMB = 'N/A'
        MemoryUsagePercent = 'N/A'
        DataAvailable = $false
    }}
    
    try {{
# Get CPU usage - '\Processor(_Total)\% Processor Time' returns the % of time CPU is busy (usage)
# This matches what Task Manager shows for CPU usage
        $cpuCounter = Get-Counter '\Processor(_Total)\% Processor Time' -SampleInterval 1 -MaxSamples 3 -ErrorAction Stop
        $cpuUsage = ($cpuCounter.CounterSamples.CookedValue | Measure-Object -Average).Average
        $performanceData.CPUUsagePercent = [Math]::Round($cpuUsage, 1)
        
        $memCounter = Get-Counter '\Memory\Available MBytes' -ErrorAction Stop
        $availableMemory = $memCounter.CounterSamples[0].CookedValue
        $performanceData.AvailableMemoryMB = $availableMemory
        $performanceData.MemoryUsagePercent = [Math]::Round((($physicalMemoryGB * 1024 - $availableMemory) / ($physicalMemoryGB * 1024)) * 100, 1)
        $performanceData.DataAvailable = $true
    }} catch {{
# Performance data not available
    }}
    
# VM Workload Analysis
    $stoppedVMs = @($allVMs | Where-Object {{ $_.State -eq 'Off' }}).Count
    $pausedVMs = @($allVMs | Where-Object {{ $_.State -eq 'Paused' }}).Count
    $savedVMs = @($allVMs | Where-Object {{ $_.State -eq 'Saved' }}).Count
    $gen1VMs = @($allVMs | Where-Object {{ $_.Generation -eq 1 }}).Count
    $gen2VMs = @($allVMs | Where-Object {{ $_.Generation -eq 2 }}).Count
    $replicatedVMs = @($allVMs | Where-Object {{ $_.ReplicationHealth -ne $null }}).Count
    
    $checkpointedVMs = 0
    foreach ($vm in $allVMs) {{
        $snapshots = @(Get-VMSnapshot -VM $vm -ErrorAction SilentlyContinue)
        if ($snapshots.Count -gt 0) {{
            $checkpointedVMs++
        }}
    }}
    
# Build result object
    $result = @{{
        HostInfo = @{{
            ComputerName = if ($vmHost) {{ $vmHost.ComputerName }} else {{ $env:COMPUTERNAME }}
            FullyQualifiedDomainName = if ($vmHost) {{ $vmHost.FullyQualifiedDomainName }} else {{ $env:COMPUTERNAME }}
            HyperVVersion = if ($computerInfo) {{ $computerInfo.WindowsProductName }} else {{ 'Unknown' }}
            LogicalProcessors = if ($vmHost) {{ $vmHost.LogicalProcessorCount }} else {{ 0 }}
            PhysicalProcessors = $physicalProcessors
            ProcessorSockets = $processorSockets
            NodeState = $nodeState
            ClusterName = $clusterName
            TotalMemoryGB = $physicalMemoryGB
            VirtualHardDiskPath = if ($vmHost) {{ $vmHost.VirtualHardDiskPath }} else {{ '' }}
            VirtualMachinePath = if ($vmHost) {{ $vmHost.VirtualMachinePath }} else {{ '' }}
            EnableEnhancedSessionMode = if ($vmHost) {{ $vmHost.EnableEnhancedSessionMode }} else {{ $false }}
            NumaSpanningEnabled = if ($vmHost) {{ $vmHost.NumaSpanningEnabled }} else {{ $false }}
            ClusterNodeCount = $clusterNodeCount
            ClusterNodesOnline = $clusterNodesOnline
            ClusterNodesOffline = $clusterNodesOffline
            ClusterNodesPaused = $clusterNodesPaused
            ClusterNodes = $clusterNodesArray
        }}
        ResourceAllocation = @{{
            TotalVMProcessors = $totalVMProcessors
            TotalVMMemoryMB = $totalVMMemoryMB
            TotalVMStartupMemoryMB = $totalVMStartupMemoryMB
            CPUOvercommitRatio = $cpuOvercommitRatio
            MemoryOvercommitRatio = $memoryOvercommitRatio
        }}
        StorageInfo = $storageInfoArray
        NetworkInfo = $networkInfoArray
        PerformanceData = $performanceData
        WorkloadAnalysis = @{{
            TotalVMs = $allVMs.Count
            RunningVMs = $runningVMs.Count
            StoppedVMs = $stoppedVMs
            PausedVMs = $pausedVMs
            SavedVMs = $savedVMs
            Generation1VMs = $gen1VMs
            Generation2VMs = $gen2VMs
            ReplicatedVMs = $replicatedVMs
            CheckpointedVMs = $checkpointedVMs
        }}
        IdleResources = @{{
            IdleVMNames = $idleVMNames
            LowUtilizationVMNames = @()
            UnusedNetworkAdapterNames = $unusedAdapterNames
        }}
        Timestamp = (Get-Date).ToString('o')
    }}
    
# Output as JSON
    $result | ConvertTo-Json -Depth 10 -Compress
    
}} catch {{
# Return error object as JSON
    @{{
        Error = $_.Exception.Message
        HostInfo = @{{
            ComputerName = $env:COMPUTERNAME
            NodeState = 'Error'
        }}
    }} | ConvertTo-Json -Depth 10 -Compress
}}
";
        }

        /// <summary>
        /// Generates health recommendations based on metrics
        /// </summary>
        /// <param name="criticalCount">Number of critical issues</param>
        /// <param name="warningCount">Number of warnings</param>
        /// <param name="cpuOvercommit">CPU overcommit ratio as string (e.g., "4.5:1")</param>
        /// <param name="memoryOvercommit">Memory overcommit ratio as string (e.g., "1.2:1")</param>
        /// <returns>Formatted recommendations string</returns>
        public static string GetHealthRecommendations(int criticalCount, int warningCount, string? cpuOvercommit, string? memoryOvercommit)
        {
            var recommendations = new List<string>();

            if (criticalCount > 0)
            {
                recommendations.Add("• ?? Critical issues detected - investigate immediately");
            }

            if (warningCount > 0)
            {
                recommendations.Add($"• ? {warningCount} warning(s) detected - review and address");
            }

            // Parse overcommit ratios
            if (double.TryParse(cpuOvercommit?.Replace(":1", ""), out double cpuRatio))
            {
                if (cpuRatio > 4)
                {
                    recommendations.Add($"• CPU overcommit ratio ({cpuRatio:F1}:1) is high - consider adding processors or reducing VM count");
                }
            }

            if (double.TryParse(memoryOvercommit?.Replace(":1", ""), out double memRatio))
            {
                if (memRatio > 1.0)
                {
                    recommendations.Add($"• Memory overcommit ratio ({memRatio:F1}:1) exceeds 1:1 - monitor for memory pressure");
                }
            }

            if (recommendations.Count == 0)
            {
                recommendations.Add("• ? Host appears healthy - no immediate actions required");
            }

            return string.Join("\n", recommendations);
        }
    }
}

#endregion