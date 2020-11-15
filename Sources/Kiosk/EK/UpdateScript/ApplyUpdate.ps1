$packageName = "KioskApp"
$targetLocation = Get-Location

$DependencyPackages = Get-ChildItem (Join-Path (Join-Path $targetLocation "Dependencies") "*.appx")

if ($DependencyPackages.Count -gt 0)
{
    Add-AppxPackage -Path "$targetLocation\$packageName.appx" -DependencyPath $DependencyPackages.FullName -ForceApplicationShutdown -Verbose
}
else
{
    Add-AppxPackage -Path "$targetLocation\$packageName.appx" -ForceApplicationShutdown -Verbose
}

Restart-Computer -Force