# Post-Build Script for HyperView
# Renames the output file with version and timestamp, creates SHA-256 hash, and archives old builds

param(
    [Parameter(Mandatory=$true)]
    [string]$TargetPath,      # Full path to the built .exe file (e.g., bin\Release\net10.0-windows\hyperview.exe)
    
    [Parameter(Mandatory=$true)]
    [string]$TargetDir,       # Directory containing the built .exe (e.g., bin\Release\net10.0-windows\)
    
    [Parameter(Mandatory=$true)]
    [string]$ProjectDir       # Project directory
)

Write-Host "=== Post-Build: Rename and Cleanup Script ==="
Write-Host "Target Path: $TargetPath"
Write-Host "Target Dir: $TargetDir"
Write-Host "Project Dir: $ProjectDir"
Write-Host ""

# Check if TargetPath is a DLL instead of EXE
$targetExtension = [System.IO.Path]::GetExtension($TargetPath)
if ($targetExtension -eq ".dll") {
    Write-Warning "WARNING: Target is a DLL, not an EXE. Looking for executable in the same directory..."
    
    # Try to find the actual executable in the target directory
    $possibleExeNames = @("HyperView.exe", "hyperview.exe")
    $foundExe = $null
    
    foreach ($exeName in $possibleExeNames) {
        $exePath = Join-Path $TargetDir $exeName
        if (Test-Path $exePath) {
            $foundExe = $exePath
            Write-Host "Found executable: $exePath"
            break
        }
    }
    
    if ($null -eq $foundExe) {
        Write-Error "ERROR: Could not find HyperView.exe or hyperview.exe in: $TargetDir"
        Write-Error "Please ensure this post-build event is attached to the main executable project, not a library project."
        exit 1
    }
    
    # Update TargetPath to the found executable
    $TargetPath = $foundExe
    Write-Host "Using executable: $TargetPath"
    Write-Host ""
}

# Verify the target file exists
if (-not (Test-Path $TargetPath)) {
    Write-Error "ERROR: Target file not found at: $TargetPath"
    exit 1
}

# Get file version from the built executable
try {
    $FileVersion = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($TargetPath).FileVersion
    Write-Host "File Version: $FileVersion"
}
catch {
    Write-Error "ERROR: Could not read file version from $TargetPath"
    Write-Error $_.Exception.Message
    exit 1
}

# Remove old timestamped builds
Write-Host ""
Write-Host "Removing old builds..."
$oldBuilds = Get-ChildItem -Path $TargetDir -Filter "*Build at*.exe" -File -ErrorAction SilentlyContinue
if ($oldBuilds) {
    foreach ($oldBuild in $oldBuilds) {
        Write-Host "  Removing: $($oldBuild.Name)"
        Remove-Item -Path $oldBuild.FullName -Force
        
        # Also remove associated .sha256 files if they exist
        $hashFile = Join-Path $TargetDir "$($oldBuild.BaseName).sha256"
        if (Test-Path $hashFile) {
            Write-Host "  Removing: $($oldBuild.BaseName).sha256"
            Remove-Item -Path $hashFile -Force
        }
    }
    Write-Host "Removed $($oldBuilds.Count) old build(s)"
}
else {
    Write-Host "No old builds to remove"
}

# Create new filename with version and timestamp
$timestamp = Get-Date -Format "ddMMyyyy-HHmmss"
$baseFileName = [System.IO.Path]::GetFileNameWithoutExtension($TargetPath)
$newFileName = "$baseFileName v. $FileVersion - Build at $timestamp.exe"
$newFilePath = Join-Path $TargetDir $newFileName

Write-Host ""
Write-Host "Creating renamed copy of build..."
Write-Host "  Source: $TargetPath"
Write-Host "  Destination: $newFileName"

# Copy the signed executable to the new timestamped filename
try {
    Copy-Item -Path $TargetPath -Destination $newFilePath -Force
    Write-Host "Successfully created: $newFileName"
}
catch {
    Write-Error "ERROR: Failed to copy file"
    Write-Error $_.Exception.Message
    exit 1
}

# Generate SHA-256 hash for the timestamped file
Write-Host ""
Write-Host "Generating SHA-256 hash..."
try {
    $hash = Get-FileHash -Path $newFilePath -Algorithm SHA256
    $hashFileName = Join-Path $TargetDir "$([System.IO.Path]::GetFileNameWithoutExtension($newFileName)).sha256"
    
    $hashFileContent = @"
HyperView Build Hash
====================
File: $newFileName
Version: $FileVersion
Build Time: $timestamp
SHA-256: $($hash.Hash)
"@
    
    $hashFileContent | Set-Content -Path $hashFileName -Encoding UTF8
    Write-Host "SHA-256 hash file created: $([System.IO.Path]::GetFileName($hashFileName))"
    Write-Host "Hash: $($hash.Hash)"
}
catch {
    Write-Warning "WARNING: Failed to create hash file"
    Write-Warning $_.Exception.Message
}

Write-Host ""
Write-Host "=== Post-Build Complete ==="
Write-Host "Output files in: $TargetDir"
Write-Host "  - $baseFileName.exe (original)"
Write-Host "  - $newFileName (timestamped)"
Write-Host "  - $([System.IO.Path]::GetFileName($hashFileName)) (hash)"
Write-Host ""