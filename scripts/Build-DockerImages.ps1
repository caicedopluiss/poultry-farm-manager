# PowerShell Script for Building and Pushing Docker Images
# This script provides functions to build and push Docker images for the Poultry Farm Manager project

# Default registry configuration
$DefaultRegistry = "registry.digitalocean.com/caicedopluiss"
$DefaultTag = "latest"

# Colors for output
$Green = [System.ConsoleColor]::Green
$Red = [System.ConsoleColor]::Red
$Yellow = [System.ConsoleColor]::Yellow
$Blue = [System.ConsoleColor]::Blue

function Write-ColorOutput {
    param(
        [string]$Message,
        [System.ConsoleColor]$Color = [System.ConsoleColor]::White
    )
    $currentColor = $Host.UI.RawUI.ForegroundColor
    $Host.UI.RawUI.ForegroundColor = $Color
    Write-Output $Message
    $Host.UI.RawUI.ForegroundColor = $currentColor
}

function Test-DockerRunning {
    try {
        docker version | Out-Null
        return $true
    }
    catch {
        Write-ColorOutput "Docker is not running or not installed. Please start Docker Desktop." $Red
        return $false
    }
}

function Test-RegistryLogin {
    param([string]$Registry)

    Write-ColorOutput "Checking Docker registry authentication..." $Blue

    # Try to authenticate with DigitalOcean registry
    $loginResult = docker login $Registry 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-ColorOutput "Not authenticated with registry: $Registry" $Red
        Write-ColorOutput "Please run: doctl registry login" $Yellow
        return $false
    }

    Write-ColorOutput "Registry authentication verified" $Green
    return $true
}

function Build-StandaloneImage {
    <#
    .SYNOPSIS
    Builds and pushes the standalone Docker image (nginx + WebAPI + WebApp)

    .DESCRIPTION
    This function builds the Dockerfile.standalone that contains nginx, WebAPI, and WebApp in a single container.

    .PARAMETER ImageName
    The name of the Docker image (without registry prefix)

    .PARAMETER Tag
    The tag for the Docker image (default: latest)

    .PARAMETER Registry
    The Docker registry URL (default: registry.digitalocean.com/caicedopluiss)

    .PARAMETER Push
    Whether to push the image to the registry (default: false)

    .EXAMPLE
    Build-StandaloneImage -ImageName "pfm-standalone" -Tag "v1.0.0"

    .EXAMPLE
    Build-StandaloneImage -ImageName "pfm-standalone" -Tag "latest" -Push $true
    #>
    param(
        [Parameter(Mandatory=$true)]
        [string]$ImageName,
        [string]$Tag = $DefaultTag,
        [string]$Registry = $DefaultRegistry,
        [bool]$Push = $false
    )

    if (-not (Test-DockerRunning)) { return }

    $FullImageName = "$Registry/$ImageName`:$Tag"
    $RootPath = Split-Path $PSScriptRoot -Parent
    $DockerfilePath = Join-Path $RootPath "Dockerfile.standalone"

    Write-ColorOutput "Building Standalone Image (nginx + WebAPI + WebApp)" $Blue
    Write-ColorOutput "Context: $RootPath" $Blue
    Write-ColorOutput "Dockerfile: $DockerfilePath" $Blue
    Write-ColorOutput "Image: $FullImageName" $Blue

    try {
        # Build the standalone image
        $buildArgs = @(
            "build",
            "-t", $FullImageName,
            "-f", $DockerfilePath,
            $RootPath
        )

        Write-ColorOutput "Building image..." $Yellow
        docker @buildArgs

        if ($LASTEXITCODE -ne 0) {
            Write-ColorOutput "Build failed for image: $FullImageName" $Red
            return $false
        }

        Write-ColorOutput "Successfully built: $FullImageName" $Green

        if ($Push) {
            if (-not (Test-RegistryLogin -Registry $Registry)) { return $false }

            Write-ColorOutput "Pushing image to registry..." $Yellow
            docker push $FullImageName

            if ($LASTEXITCODE -ne 0) {
                Write-ColorOutput "Push failed for image: $FullImageName" $Red
                return $false
            }

            Write-ColorOutput "Successfully pushed: $FullImageName" $Green
        }

        return $true
    }
    catch {
        Write-ColorOutput "Error building standalone image: $($_.Exception.Message)" $Red
        return $false
    }
}

function Build-HybridImage {
    <#
    .SYNOPSIS
    Builds and pushes the hybrid Docker image (WebAPI + WebApp combined)

    .DESCRIPTION
    This function builds the multi-stage Dockerfile that contains both the WebAPI and WebApp services.

    .PARAMETER ImageName
    The name of the Docker image (without registry prefix)

    .PARAMETER Tag
    The tag for the Docker image (default: latest)

    .PARAMETER Registry
    The Docker registry URL (default: registry.digitalocean.com/caicedopluiss)

    .PARAMETER ApiHostUrl
    The API host URL for the webapp build (default: /webapi)

    .PARAMETER Push
    Whether to push the image to the registry (default: false)

    .EXAMPLE
    Build-HybridImage -ImageName "pfm/hybrid" -Tag "v1.0.0"

    .EXAMPLE
    Build-HybridImage -ImageName "pfm/hybrid" -Tag "latest" -ApiHostUrl "/webapi" -Push $true
    #>
    param(
        [Parameter(Mandatory=$true)]
        [string]$ImageName,

        [string]$Tag = $DefaultTag,
        [string]$Registry = $DefaultRegistry,
        [string]$ApiHostUrl = "/webapi",
        [bool]$Push = $false
    )


    if (-not (Test-DockerRunning)) { return }

    $FullImageName = "$Registry/$ImageName`:$Tag"
    $RootPath = Split-Path $PSScriptRoot -Parent

    Write-ColorOutput "Building Hybrid Image (WebAPI + WebApp)" $Blue
    Write-ColorOutput "Context: $RootPath" $Blue
    Write-ColorOutput "Image: $FullImageName" $Blue
    Write-ColorOutput "API Host URL: $ApiHostUrl" $Blue

    try {
        # Build the hybrid image
        $buildArgs = @(
            "build",
            "-t", $FullImageName,
            "--build-arg", "API_HOST_URL=$ApiHostUrl",
            "-f", "Dockerfile",
            $RootPath
        )

        Write-ColorOutput "Building image..." $Yellow
        $buildResult = docker @buildArgs

        if ($LASTEXITCODE -ne 0) {
            Write-ColorOutput "Build failed for image: $FullImageName" $Red
            return $false
        }

        Write-ColorOutput "Successfully built: $FullImageName" $Green

        if ($Push) {
            if (-not (Test-RegistryLogin -Registry $Registry)) { return $false }

            Write-ColorOutput "Pushing image to registry..." $Yellow
            docker push $FullImageName

            if ($LASTEXITCODE -ne 0) {
                Write-ColorOutput "Push failed for image: $FullImageName" $Red
                return $false
            }

            Write-ColorOutput "Successfully pushed: $FullImageName" $Green
        }

        return $true
    }
    catch {
        Write-ColorOutput "Error building hybrid image: $($_.Exception.Message)" $Red
        return $false
    }
}


function Build-WebApiImage {
    <#
    .SYNOPSIS
    Builds and pushes the WebAPI Docker image

    .DESCRIPTION
    This function builds the standalone WebAPI Docker image from the server directory.

    .PARAMETER ImageName
    The name of the Docker image (without registry prefix)

    .PARAMETER Tag
    The tag for the Docker image (default: latest)

    .PARAMETER Registry
    The Docker registry URL (default: registry.digitalocean.com/caicedopluiss)

    .PARAMETER Push
    Whether to push the image to the registry (default: false)

    .EXAMPLE
    Build-WebApiImage -ImageName "pfm/webapi" -Tag "v1.0.0"

    .EXAMPLE
    Build-WebApiImage -ImageName "pfm/webapi" -Tag "latest" -Push $true
    #>
    param(
        [Parameter(Mandatory=$true)]
        [string]$ImageName,
        [string]$Tag = $DefaultTag,
        [string]$Registry = $DefaultRegistry,
        [bool]$Push = $false
    )

    if (-not (Test-DockerRunning)) { return }

    $FullImageName = "$Registry/$ImageName`:$Tag"
    $RootPath = Split-Path $PSScriptRoot -Parent
    $DockerfilePath = Join-Path $RootPath "server\PoultryFarmManager.WebAPI\Dockerfile"

    Write-ColorOutput "Building WebAPI Image" $Blue
    Write-ColorOutput "Context: $RootPath" $Blue
    Write-ColorOutput "Dockerfile: $DockerfilePath" $Blue
    Write-ColorOutput "Image: $FullImageName" $Blue

    try {
        # Build the WebAPI image
        $buildArgs = @(
            "build",
            "-t", $FullImageName,
            "-f", $DockerfilePath,
            $RootPath
        )

        Write-ColorOutput "Building image..." $Yellow
        docker @buildArgs

        if ($LASTEXITCODE -ne 0) {
            Write-ColorOutput "Build failed for image: $FullImageName" $Red
            return $false
        }

        Write-ColorOutput "Successfully built: $FullImageName" $Green

        if ($Push) {
            if (-not (Test-RegistryLogin -Registry $Registry)) { return $false }

            Write-ColorOutput "Pushing image to registry..." $Yellow
            docker push $FullImageName

            if ($LASTEXITCODE -ne 0) {
                Write-ColorOutput "Push failed for image: $FullImageName" $Red
                return $false
            }

            Write-ColorOutput "Successfully pushed: $FullImageName" $Green
        }

        return $true
    }
    catch {
        Write-ColorOutput "Error building WebAPI image: $($_.Exception.Message)" $Red
        return $false
    }
}

function Build-WebAppImage {
    <#
    .SYNOPSIS
    Builds and pushes the WebApp Docker image

    .DESCRIPTION
    This function builds the standalone WebApp Docker image from the client directory.

    .PARAMETER ImageName
    The name of the Docker image (without registry prefix)

    .PARAMETER Tag
    The tag for the Docker image (default: latest)

    .PARAMETER Registry
    The Docker registry URL (default: registry.digitalocean.com/caicedopluiss)

    .PARAMETER ApiHostUrl
    The API host URL for the webapp build (default: /api)

    .PARAMETER Push
    Whether to push the image to the registry (default: false)

    .EXAMPLE
    Build-WebAppImage -ImageName "pfm/webapp" -Tag "v1.0.0"

    .EXAMPLE
    Build-WebAppImage -ImageName "pfm/webapp" -Tag "latest" -ApiHostUrl "https://api.example.com" -Push $true
    #>
    param(
        [Parameter(Mandatory=$true)]
        [string]$ImageName,

        [string]$Tag = $DefaultTag,
        [string]$Registry = $DefaultRegistry,
        [string]$ApiHostUrl = "/api",
        [bool]$Push = $false
    )

    if (-not (Test-DockerRunning)) { return }

    $FullImageName = "$Registry/$ImageName`:$Tag"
    $WebAppPath = Join-Path (Split-Path $PSScriptRoot -Parent) "client\webapp"
    $DockerfilePath = Join-Path $WebAppPath "Dockerfile"

    Write-ColorOutput "Building WebApp Image" $Blue
    Write-ColorOutput "Context: $WebAppPath" $Blue
    Write-ColorOutput "Dockerfile: $DockerfilePath" $Blue
    Write-ColorOutput "Image: $FullImageName" $Blue
    Write-ColorOutput "API Host URL: $ApiHostUrl" $Blue

    try {
        # Build the WebApp image
        $buildArgs = @(
            "build",
            "-t", $FullImageName,
            "--build-arg", "API_HOST_URL=$ApiHostUrl",
            "-f", $DockerfilePath,
            $WebAppPath
        )

        Write-ColorOutput "Building image..." $Yellow
        docker @buildArgs

        if ($LASTEXITCODE -ne 0) {
            Write-ColorOutput "Build failed for image: $FullImageName" $Red
            return $false
        }

        Write-ColorOutput "Successfully built: $FullImageName" $Green

        if ($Push) {
            if (-not (Test-RegistryLogin -Registry $Registry)) { return $false }

            Write-ColorOutput "Pushing image to registry..." $Yellow
            docker push $FullImageName

            if ($LASTEXITCODE -ne 0) {
                Write-ColorOutput "Push failed for image: $FullImageName" $Red
                return $false
            }

            Write-ColorOutput "Successfully pushed: $FullImageName" $Green
        }

        return $true
    }
    catch {
        Write-ColorOutput "Error building WebApp image: $($_.Exception.Message)" $Red
        return $false
    }
}

# Helper function to show usage examples
function Show-Usage {
    Write-ColorOutput @"
Docker Image Build Script for Poultry Farm Manager

Available Functions:
  Build-StandaloneImage - Builds the standalone Docker image (nginx + WebAPI + WebApp) - RECOMMENDED
  Build-HybridImage     - Builds the hybrid image (WebAPI + WebApp with ingress routing)
  Build-WebApiImage     - Builds the standalone WebAPI image
  Build-WebAppImage     - Builds the standalone WebApp image

Usage Examples:
  # Build specific image with custom tag
  Build-HybridImage -ImageName "pfm/hybrid" -Tag "v1.2.3"

  # Build without pushing to registry
  Build-WebApiImage -ImageName "pfm/webapi" -Tag "dev" -Push $false

  # Build with custom API URL
  Build-WebAppImage -ImageName "pfm/webapp" -ApiHostUrl "https://api.myapp.com"

Prerequisites:
  1. Docker Desktop must be running
  2. Authenticate with DigitalOcean registry: doctl registry login
  3. Run from the project root or scripts directory

"@ $Blue
}

Show-Usage
