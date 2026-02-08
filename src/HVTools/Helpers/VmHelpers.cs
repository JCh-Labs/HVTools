using System.Management.Automation;

namespace HVTools.Helpers
{
    /// <summary>
    /// Helper methods for VM-related operations
    /// </summary>
    public static class VmHelpers
    {
        /// <summary>
        /// Gets the total disk size in GB for a VM
        /// </summary>
        /// <param name="vmName">The name of the VM</param>
        /// <param name="executePowerShellCommand">Function to execute PowerShell commands</param>
        /// <param name="isLocal">Whether this is a local connection</param>
        /// <returns>Total disk size in GB</returns>
        public static double GetVmTotalDiskSize(string vmName, 
            Func<string, System.Collections.ObjectModel.Collection<PSObject>?> executePowerShellCommand,
            bool isLocal)
        {
            try
            {
                var results = executePowerShellCommand($"Get-VMHardDiskDrive -VMName '{vmName}'");

                if (results == null || results.Count == 0)
                    return 0;

                double totalGb = 0;
                foreach (var hdd in results)
                {
                    var path = hdd.Properties["Path"]?.Value?.ToString();
                    if (!string.IsNullOrEmpty(path))
                    {
                        // For remote, we need to get file size through PS
                        if (!isLocal)
                        {
                            var sizeResult = executePowerShellCommand($"(Get-Item '{path}').Length");
                            if (sizeResult != null && sizeResult.Count > 0)
                            {
                                var size = Convert.ToInt64(sizeResult[0].BaseObject);
                                totalGb += size / (1024.0 * 1024.0 * 1024.0);
                            }
                        }
                        else if (File.Exists(path))
                        {
                            var fileInfo = new FileInfo(path);
                            totalGb += fileInfo.Length / (1024.0 * 1024.0 * 1024.0);
                        }
                    }
                }

                return totalGb;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Gets the count of network adapters for a VM
        /// </summary>
        /// <param name="vmName">The name of the VM</param>
        /// <param name="executePowerShellCommand">Function to execute PowerShell commands</param>
        /// <returns>Number of network adapters</returns>
        public static int GetVmNetworkAdapterCount(string vmName,
            Func<string, System.Collections.ObjectModel.Collection<PSObject>?> executePowerShellCommand)
        {
            try
            {
                var results = executePowerShellCommand($"Get-VMNetworkAdapter -VMName '{vmName}'");
                return results?.Count ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Gets the count of checkpoints/snapshots for a VM
        /// </summary>
        /// <param name="vmName">The name of the VM</param>
        /// <param name="executePowerShellCommand">Function to execute PowerShell commands</param>
        /// <returns>Number of checkpoints</returns>
        public static int GetVmCheckpointCount(string vmName,
            Func<string, System.Collections.ObjectModel.Collection<PSObject>?> executePowerShellCommand)
        {
            try
            {
                var results = executePowerShellCommand($"Get-VMSnapshot -VMName '{vmName}'");
                return results?.Count ?? 0;
            }
            catch
            {
                return 0;
            }
        }
    }
}
