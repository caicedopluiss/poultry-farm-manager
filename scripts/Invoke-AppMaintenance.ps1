<#
.SYNOPSIS
    Toggles maintenance mode for the DigitalOcean App Platform to save costs.

.DESCRIPTION
    This script provides a function to enable or disable maintenance mode on the App Platform.
    Load this script with dot-sourcing, then call the function from the project root directory.

    When maintenance mode is enabled:
    - App instances stop running
    - Billing for app instances stops (~$5-6/month savings)
    - Database continues running (no data loss)
    - App configuration is preserved

    When maintenance mode is disabled:
    - App instances start running
    - App becomes accessible again
    - Billing resumes

.EXAMPLE
    # Load the script
    . .\scripts\Invoke-AppMaintenance.ps1

    # Enable maintenance mode
    Invoke-AppMaintenance -Action archive

    # Disable maintenance mode
    Invoke-AppMaintenance -Action restore

    # With auto-approve
    Invoke-AppMaintenance -Action archive -AutoApprove

.NOTES
    Author: Poultry Farm Manager Team
    Requires: Terraform CLI, DigitalOcean credentials configured
    Run from project root directory
#>

function Invoke-AppMaintenance {
    <#
    .SYNOPSIS
        Toggles maintenance mode for the DigitalOcean App Platform.

    .PARAMETER Action
        The action to perform:
        - archive: Enable maintenance mode (stop app instances)
        - restore: Disable maintenance mode (start app instances)

    .PARAMETER Environment
        The environment to manage (default: development)

    .PARAMETER WorkingDirectory
        The Terraform working directory (default: ./IaC/application)

    .PARAMETER AutoApprove
        Skip Terraform confirmation prompts

    .EXAMPLE
        Invoke-AppMaintenance -Action archive
        Enables maintenance mode (stops app instances, saves ~$5-6/month).

    .EXAMPLE
        Invoke-AppMaintenance -Action restore
        Disables maintenance mode (starts app instances).

    .EXAMPLE
        Invoke-AppMaintenance -Action archive -AutoApprove
        Enables maintenance mode with automatic approval.
    #>

    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet('archive', 'restore')]
        [string]$Action,

        [Parameter(Mandatory = $false)]
        [ValidateSet('production', 'staging', 'development')]
        [string]$Environment = 'development',

        [Parameter(Mandatory = $false)]
        [string]$WorkingDirectory = './IaC/application',

        [Parameter(Mandatory = $false)]
        [switch]$AutoApprove
        )

    # Function to write colored output
    function Write-Step {
        param([string]$Message, [string]$Color = 'Cyan')
        Write-Host $Message -ForegroundColor $Color
    }

    function Write-SuccessMessage {
        param([string]$Message)
        Write-Host "[SUCCESS] $Message" -ForegroundColor Green
    }

    function Write-WarningMessage {
        param([string]$Message)
        Write-Host "[WARNING] $Message" -ForegroundColor Yellow
    }

    function Write-InfoMessage {
        param([string]$Message)
        Write-Host "[INFO] $Message" -ForegroundColor Cyan
    }

    function Write-ErrorMessage {
        param([string]$Message)
        Write-Host "[ERROR] $Message" -ForegroundColor Red
    }

    # Main execution
    Write-Step "===============================================================" "Magenta"
    Write-Step "    App Platform Maintenance (Local Simulation)" "Magenta"
    Write-Step "===============================================================" "Magenta"
    Write-Host ""

    # Step 1: Determine configuration
    Write-Step "Step 1: Determining configuration..." "Cyan"
    Write-Host ""

    $envPrefix = switch ($Environment) {
        'production' { 'Production' }
        'staging' { 'Staging' }
        'development' { 'Development' }
    }

    $maintenanceMode = 'false'
    $actionDesc = ''

    if ($Action -eq 'archive') {
        $maintenanceMode = $true
        $actionDesc = 'Enable maintenance mode (stop app instances)'
    } else {
        $maintenanceMode = $false
        $actionDesc = 'Disable maintenance mode (start app instances)'
    }

    Write-InfoMessage "Environment: $envPrefix"
    Write-InfoMessage "Action: $actionDesc"
    Write-InfoMessage "Maintenance Mode: $maintenanceMode"
    Write-Host ""

    # Step 2: Run Terraform Apply
    Write-Step "Step 2: Running Terraform apply..." "Cyan"
    Write-Host ""

    # Set environment variables
    $env:TF_VAR_env = $envPrefix
    $env:TF_VAR_maintenance_mode = $maintenanceMode

    # Build Terraform command
    $tfArgs = @(
        'apply',
        "-var=`"env=$envPrefix`"",
        "-var=`"maintenance_mode=$($maintenanceMode.ToString().ToLower())`""
    )

    if ($AutoApprove) {
        $tfArgs += '-auto-approve'
    }

    Write-InfoMessage "Working Directory: $WorkingDirectory"
    Write-InfoMessage "Terraform Command: terraform $($tfArgs -join ' ')"
    Write-Host ""

    # Verify Terraform is installed
    if (-not (Get-Command terraform -ErrorAction SilentlyContinue)) {
        Write-Error "Terraform CLI is not installed or not in PATH. Please install Terraform first."
        return
    }

    if (-not (Test-Path $WorkingDirectory)) {
        Write-Error "Working directory does not exist: $WorkingDirectory"
        return
    }

    Push-Location $WorkingDirectory

    try {
        Write-Step "Executing Terraform..." "Yellow"
        & terraform $tfArgs

        $exitCode = $LASTEXITCODE

        if ($exitCode -ne 0) {
            Write-ErrorMessage "Terraform apply failed with exit code: $exitCode"
            return
        }

        Write-SuccessMessage "Terraform apply completed successfully!"
        Write-Host ""
    }
    catch {
        Write-ErrorMessage "Error executing Terraform: $_"
        return
    }
    finally {
        Pop-Location
    }

    # Step 3: Summary
    Write-Step "===============================================================" "Magenta"
    Write-Step "    Operation Summary" "Magenta"
    Write-Step "===============================================================" "Magenta"
    Write-Host ""

    Write-InfoMessage "Action: $Action"
    Write-InfoMessage "Environment: $Environment"
    Write-InfoMessage "Description: $actionDesc"
    Write-Host ""

    if ($Action -eq 'archive') {
        Write-SuccessMessage "Maintenance mode enabled"
        Write-Host ""
        Write-InfoMessage "What happened:"
        Write-InfoMessage "- App instances stopped"
        Write-InfoMessage "- Billing for app instances stopped"
        Write-InfoMessage "- Database remains running (no data loss)"
        Write-InfoMessage "- App configuration preserved"
        Write-Host ""
        Write-InfoMessage "To restore:"
        Write-InfoMessage "Invoke-AppMaintenance -Action restore -Environment $Environment"
    } else {
        Write-SuccessMessage "Maintenance mode disabled"
        Write-Host ""
        Write-InfoMessage "What happened:"
        Write-InfoMessage "- App instances started"
        Write-InfoMessage "- App is now accessible"
        Write-InfoMessage "- Billing resumed"
        Write-Host ""
        Write-InfoMessage "ETA: App should be ready in 5-10 minutes"
    }

    Write-Host ""
    Write-Step "===============================================================" "Magenta"
    Write-SuccessMessage "Operation completed successfully!"
    Write-Step "===============================================================" "Magenta"
}

function Show-Usage {
    <#
    .SYNOPSIS
        Displays usage information for the App Maintenance functions.
    #>

    Write-Host @"

App Maintenance Script for Poultry Farm Manager

Available Function:
  Invoke-AppMaintenance - Toggle maintenance mode for the App Platform

Usage Examples:
  # Load the script (dot-source)
  . .\scripts\Invoke-AppMaintenance.ps1

  # Enable maintenance mode (stop app instances)
  Invoke-AppMaintenance -Action archive

  # Disable maintenance mode (start app instances)
  Invoke-AppMaintenance -Action restore

  # With auto-approve (skip confirmation)
  Invoke-AppMaintenance -Action archive -AutoApprove

  # Specify environment
  Invoke-AppMaintenance -Action restore -Environment production

What happens when archived:
  - App instances stop running
  - Billing for app instances stops
  - Database continues running (no data loss)
  - App configuration is preserved

What happens when restored:
  - App instances start running
  - App becomes accessible again (ETA: 5-10 minutes)
  - Billing resumes

Prerequisites:
  1. Terraform CLI installed
  2. DigitalOcean credentials configured
  3. Run from the project root directory

"@ -ForegroundColor Blue
}

Show-Usage

