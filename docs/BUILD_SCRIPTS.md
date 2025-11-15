# Docker Image Build Scripts

This directory contains PowerShell scripts to build, run, and manage Docker images for the Poultry Farm Manager project.

> **Note**: Make sure to run the script from the project root.

## Prerequisites

1. **Docker Desktop** - Must be running
2. **DigitalOcean CLI (doctl)** - For registry authentication. Install from [doctl releases](https://github.com/digitalocean/doctl/releases).
3. **PowerShell** - Windows PowerShell 5.1+ or PowerShell Core 7+

## Setup

### 1. Authenticate with DigitalOcean Registry

```powershell
# Authenticate with DigitalOcean
doctl auth init

# Login to container registry
doctl registry login
```

### 2. Load the Build Script

```powershell
# From the scripts directory
. .\Build-DockerImages.ps1

# Or from project root
. .\scripts\Build-DockerImages.ps1
```

## Available Functions

### Build-StandaloneImage

Builds the standalone Docker image containing nginx, WebAPI, and WebApp services.

```powershell
# Build with default settings
Build-StandaloneImage -ImageName "pfm/standalone"

# Build with custom tag
Build-StandaloneImage -ImageName "pfm/standalone" -Tag "v1.0.0"

# Build with custom API URL
Build-StandaloneImage -ImageName "pfm/standalone" -Tag "latest" -ApiHostUrl "/api/v1"

# Build pushing to registry
Build-StandaloneImage -ImageName "pfm/standalone" -Tag "dev" -Push $true
```

### Build-HybridImage

Builds the multi-stage Docker image containing both WebAPI and WebApp services.

```powershell
# Build with default settings
Build-HybridImage -ImageName "pfm/hybrid"

# Build with custom tag
Build-HybridImage -ImageName "pfm/hybrid" -Tag "v1.0.0"

# Build with custom API URL
Build-HybridImage -ImageName "pfm/hybrid" -Tag "latest" -ApiHostUrl "/api/v1"

# Build pushing to registry
Build-HybridImage -ImageName "pfm/hybrid" -Tag "dev" -Push $true
```

### Build-WebApiImage

Builds the standalone WebAPI Docker image.

```powershell
# Build with default settings
Build-WebApiImage -ImageName "pfm/webapi"

# Build with custom tag
Build-WebApiImage -ImageName "pfm/webapi" -Tag "v1.0.0"

# Build pushing
Build-WebApiImage -ImageName "pfm/webapi" -Tag "dev" -Push $true
```

### Build-WebAppImage

Builds the standalone WebApp Docker image.

```powershell
# Build with default settings
Build-WebAppImage -ImageName "pfm/webapp"

# Build with custom API URL
Build-WebAppImage -ImageName "pfm/webapp" -ApiHostUrl "https://api.example.com"

# Build with custom tag
Build-WebAppImage -ImageName "pfm/webapp" -Tag "v1.0.0"
```

## Configuration

### Default Settings

-   **Registry**: `registry.digitalocean.com/caicedopluiss`
-   **Tag**: `latest`
-   **API Host URL**: `/webapi`
-   **Push**: `false`

### Custom Configuration

```powershell
# Build with specific configuration
Build-HybridImage `
    -ImageName "pfm/hybrid" `
    -Tag "staging" `
    -Registry "my-registry.com" `
    -ApiHostUrl "https://staging-api.example.com" `
    -Push $true
```

If you encounter permission issues, run PowerShell as Administrator or check Docker Desktop permissions.

## Help

Run `Show-Usage` after loading the script to see inline help and examples.

```powershell
Show-Usage
```
