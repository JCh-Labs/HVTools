using System.Management.Automation;
using System.Text.Json;

namespace HVTools.Class
{
    #region Data Models

    /// <summary>
    /// Represents host information from the inventory
    /// </summary>
    public class HostInventoryHostInfo
    {
        public string ComputerName { get; set; } = "";
        public string FullyQualifiedDomainName { get; set; } = "";
        public string HyperVVersion { get; set; } = "";
        public int LogicalProcessors { get; set; }
        public int PhysicalProcessors { get; set; }
        public int ProcessorSockets { get; set; }
        public string NodeState { get; set; } = "Standalone";
        public string ClusterName { get; set; } = "N/A";
        public double TotalMemoryGB { get; set; }
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
        public string State { get; set; } = "";
        public bool IsCurrentNode { get; set; }
    }

    /// <summary>
    /// Represents resource allocation information
    /// </summary>
    public class ResourceAllocationInfo
    {
        public int TotalVMProcessors { get; set; }
        public long TotalVMMemoryMB { get; set; }
        public long TotalVMStartupMemoryMB { get; set; }
        public double CPUOvercommitRatio { get; set; }
        public double MemoryOvercommitRatio { get; set; }
    }

    /// <summary>
    /// Represents storage information for a drive
    /// </summary>
    public class StorageDriveInfo
    {
        public string DriveLetter { get; set; } = "";
        public double TotalGB { get; set; }
        public double UsedGB { get; set; }
        public double FreeGB { get; set; }
        public double UsedPercent { get; set; }
        public int VMFileCount { get; set; }
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
        public double CPUUsagePercent { get; set; }
        public double AvailableMemoryMB { get; set; }
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
        public List<string> IdleVMNames { get; set; } = [];
        public List<string> LowUtilizationVMNames { get; set; } = [];
        public List<string> UnusedNetworkAdapterNames { get; set; } = [];
    }

    /// <summary>
    /// Complete host inventory information
    /// </summary>
    public class HostInventoryInfo
    {
        public HostInventoryHostInfo HostInfo { get; set; } = new();
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
    /// Provides functionality to retrieve comprehensive Hyper-V host inventory
    /// </summary>
    public static class HostInventory
    {
        /// <summary>
        /// Gets comprehensive host inventory information
        /// </summary>
        /// <param name="executePowerShellCommand">Function to execute PowerShell commands</param>
        /// <param name="executePowerShellCommandOnNode">Optional function to execute commands on specific cluster nodes</param>
        /// <param name="includeDetailedVMs">Whether to include detailed VM information</param>
        /// <returns>Host inventory information or null if failed</returns>
        public static HostInventoryInfo? GetHyperVHostInventory(
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

                FileLogger.Message("Starting Hyper-V host inventory collection...",
                    FileLogger.EventType.Information, 7002);

                bool isLocal = SessionContext.IsLocal;

                HostInventoryInfo? inventory;

                if (isLocal)
                {
                    // For local execution, use Windows PowerShell process for full WMI support
                    FileLogger.Message("Using Windows PowerShell process for local inventory collection...",
                        FileLogger.EventType.Information, 7003);
                    inventory = GetInventoryViaWindowsPowerShell(includeDetailedVMs);
                }
                else
                {
                    // For remote, use embedded PowerShell with Invoke-Command
                    FileLogger.Message("Executing inventory script via remote PowerShell...",
                        FileLogger.EventType.Information, 7004);
                    inventory = GetInventoryViaRemote(executePowerShellCommand, includeDetailedVMs);
                }

                if (inventory != null)
                {
                    FileLogger.Message("Host inventory collection completed successfully",
                        FileLogger.EventType.Information, 7005);
                }
                else
                {
                    FileLogger.Message("Host inventory collection returned null",
                        FileLogger.EventType.Warning, 7006);
                }

                return inventory;
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Error getting host inventory: {ex.Message}",
                    FileLogger.EventType.Error, 7007);
                FileLogger.Message($"Stack trace: {ex.StackTrace}",
                    FileLogger.EventType.Error, 7008);
                return null;
            }
        }

        /// <summary>
        /// Gets inventory via Windows PowerShell process (for local execution)
        /// </summary>
        private static HostInventoryInfo? GetInventoryViaWindowsPowerShell(bool includeDetailedVMs)
        {
            try
            {
                string script = GetInventoryScript(includeDetailedVMs);

                // Create a temporary script file
                string tempScriptPath = Path.Combine(Path.GetTempPath(), $"HVTools_Inventory_{Guid.NewGuid():N}.ps1");
                File.WriteAllText(tempScriptPath, script);

                FileLogger.Message($"Created temp inventory script: '{tempScriptPath}'",
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
                    return ParseJsonInventory(output);
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
                FileLogger.Message($"Error in GetInventoryViaWindowsPowerShell: {ex.Message}",
                    FileLogger.EventType.Error, 7015);
                return null;
            }
        }

        /// <summary>
        /// Gets inventory via remote PowerShell execution
        /// </summary>
        private static HostInventoryInfo? GetInventoryViaRemote(
            Func<string, System.Collections.ObjectModel.Collection<PSObject>?> executePowerShellCommand,
            bool includeDetailedVMs)
        {
            try
            {
                // Use the same JSON-outputting script as local, so we get consistent data format
                string script = GetInventoryScript(includeDetailedVMs);
                var result = executePowerShellCommand(script);

                if (result == null || result.Count == 0)
                {
                    FileLogger.Message("Remote inventory script returned no results",
                        FileLogger.EventType.Warning, 7020);
                    return null;
                }

                FileLogger.Message($"Remote inventory script returned {result.Count} result(s), parsing JSON...",
                    FileLogger.EventType.Information, 7021);

                // The script outputs JSON, so we need to extract the JSON string from the result
                // When executed remotely, the JSON string comes back as a PSObject wrapping the string
                string jsonOutput = "";
                
                // Collect all output lines (JSON may be split across multiple results)
                foreach (var item in result)
                {
                    if (item != null)
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
                    FileLogger.Message("Remote inventory script returned empty JSON",
                        FileLogger.EventType.Warning, 7023);
                    return null;
                }

                FileLogger.Message($"Remote JSON output length: {jsonOutput.Length} chars",
                    FileLogger.EventType.Information, 7024);

                // Parse the JSON using the same method as local
                return ParseJsonInventory(jsonOutput);
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Error in GetInventoryViaRemote: {ex.Message}",
                    FileLogger.EventType.Error, 7022);
                return null;
            }
        }

        /// <summary>
        /// Parses JSON output into HostInventoryInfo
        /// </summary>
        private static HostInventoryInfo? ParseJsonInventory(string json)
        {
            try
            {
                json = json.Trim();

                FileLogger.Message("Parsing JSON inventory...",
                    FileLogger.EventType.Information, 7030);

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                var inventory = new HostInventoryInfo
                {
                    Timestamp = DateTime.Now
                };

                // Parse HostInfo
                if (root.TryGetProperty("HostInfo", out var hostInfoElement))
                {
                    inventory.HostInfo = new HostInventoryHostInfo
                    {
                        ComputerName = GetJsonString(hostInfoElement, "ComputerName"),
                        FullyQualifiedDomainName = GetJsonString(hostInfoElement, "FullyQualifiedDomainName"),
                        HyperVVersion = GetJsonString(hostInfoElement, "HyperVVersion"),
                        LogicalProcessors = GetJsonInt(hostInfoElement, "LogicalProcessors"),
                        PhysicalProcessors = GetJsonInt(hostInfoElement, "PhysicalProcessors"),
                        ProcessorSockets = GetJsonInt(hostInfoElement, "ProcessorSockets"),
                        NodeState = GetJsonString(hostInfoElement, "NodeState"),
                        ClusterName = GetJsonString(hostInfoElement, "ClusterName"),
                        TotalMemoryGB = GetJsonDouble(hostInfoElement, "TotalMemoryGB"),
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
                        foreach (var nodeElement in clusterNodesElement.EnumerateArray())
                        {
                            inventory.HostInfo.ClusterNodes.Add(new ClusterNodeSummary
                            {
                                Name = GetJsonString(nodeElement, "Name"),
                                State = GetJsonString(nodeElement, "State"),
                                IsCurrentNode = GetJsonBool(nodeElement, "IsCurrentNode")
                            });
                        }
                    }
                }

                // Parse ResourceAllocation
                if (root.TryGetProperty("ResourceAllocation", out var resourceElement))
                {
                    inventory.ResourceAllocation = new ResourceAllocationInfo
                    {
                        TotalVMProcessors = GetJsonInt(resourceElement, "TotalVMProcessors"),
                        TotalVMMemoryMB = GetJsonLong(resourceElement, "TotalVMMemoryMB"),
                        TotalVMStartupMemoryMB = GetJsonLong(resourceElement, "TotalVMStartupMemoryMB"),
                        CPUOvercommitRatio = GetJsonDouble(resourceElement, "CPUOvercommitRatio"),
                        MemoryOvercommitRatio = GetJsonDouble(resourceElement, "MemoryOvercommitRatio")
                    };
                }

                // Parse StorageInfo
                if (root.TryGetProperty("StorageInfo", out var storageElement) &&
                    storageElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var driveElement in storageElement.EnumerateArray())
                    {
                        inventory.StorageInfo.Add(new StorageDriveInfo
                        {
                            DriveLetter = GetJsonString(driveElement, "DriveLetter"),
                            TotalGB = GetJsonDouble(driveElement, "TotalGB"),
                            UsedGB = GetJsonDouble(driveElement, "UsedGB"),
                            FreeGB = GetJsonDouble(driveElement, "FreeGB"),
                            UsedPercent = GetJsonDouble(driveElement, "UsedPercent"),
                            VMFileCount = GetJsonInt(driveElement, "VMFileCount")
                        });
                    }
                }

                // Parse NetworkInfo
                if (root.TryGetProperty("NetworkInfo", out var networkElement) &&
                    networkElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var adapterElement in networkElement.EnumerateArray())
                    {
                        inventory.NetworkInfo.Add(new NetworkAdapterInfo
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

                    inventory.PerformanceData = new PerformanceDataInfo
                    {
                        CPUUsagePercent = GetJsonDoubleOrDefault(perfElement, "CPUUsagePercent", 0),
                        AvailableMemoryMB = GetJsonDoubleOrDefault(perfElement, "AvailableMemoryMB", 0),
                        MemoryUsagePercent = GetJsonDoubleOrDefault(perfElement, "MemoryUsagePercent", 0),
                        DataAvailable = dataAvailable
                    };
                }

                // Parse WorkloadAnalysis
                if (root.TryGetProperty("WorkloadAnalysis", out var workloadElement))
                {
                    inventory.WorkloadAnalysis = new WorkloadAnalysisInfo
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
                    inventory.IdleResources = new IdleResourcesInfo();

                    if (idleElement.TryGetProperty("IdleVMNames", out var idleVMs) &&
                        idleVMs.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var vm in idleVMs.EnumerateArray())
                        {
                            inventory.IdleResources.IdleVMNames.Add(vm.GetString() ?? "");
                        }
                    }

                    if (idleElement.TryGetProperty("LowUtilizationVMNames", out var lowUtilVMs) &&
                        lowUtilVMs.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var vm in lowUtilVMs.EnumerateArray())
                        {
                            inventory.IdleResources.LowUtilizationVMNames.Add(vm.GetString() ?? "");
                        }
                    }

                    if (idleElement.TryGetProperty("UnusedNetworkAdapterNames", out var unusedAdapters) &&
                        unusedAdapters.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var adapter in unusedAdapters.EnumerateArray())
                        {
                            inventory.IdleResources.UnusedNetworkAdapterNames.Add(adapter.GetString() ?? "");
                        }
                    }
                }

                // Parse Timestamp
                if (root.TryGetProperty("Timestamp", out var timestampElement))
                {
                    if (DateTime.TryParse(timestampElement.GetString(), out var timestamp))
                    {
                        inventory.Timestamp = timestamp;
                    }
                }

                FileLogger.Message($"Successfully parsed JSON inventory for '{inventory.HostInfo.ComputerName}'",
                    FileLogger.EventType.Information, 7031);

                return inventory;
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Error parsing JSON inventory: {ex.Message}",
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

        #endregion

        #region PowerShell Scripts

        /// <summary>
        /// Gets the PowerShell script for local Windows PowerShell execution (outputs JSON)
        /// </summary>
        private static string GetInventoryScript(bool includeDetailedVMs)
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
            
            # Get all cluster nodes and their states
            $allClusterNodes = @(Get-ClusterNode -ErrorAction SilentlyContinue)
            $clusterNodeCount = $allClusterNodes.Count
            $clusterNodesOnline = @($allClusterNodes | Where-Object {{ $_.State -eq 'Up' }}).Count
            $clusterNodesOffline = @($allClusterNodes | Where-Object {{ $_.State -eq 'Down' }}).Count
            $clusterNodesPaused = @($allClusterNodes | Where-Object {{ $_.State -eq 'Paused' }}).Count
            
            # Build cluster nodes array with details
            foreach ($node in $allClusterNodes) {{
                $clusterNodesArray += @{{
                    Name = $node.Name
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
    }
}

#endregion