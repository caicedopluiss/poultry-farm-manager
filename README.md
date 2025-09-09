# Poultry Farm Manager

![Build Status](https://github.com/caicedopluiss/poultry-farm-manager/workflows/CI%20Workflow/badge.svg)
![Infrastructure](https://img.shields.io/badge/Infrastructure-HCP%20Terraform-623CE4)
![Cloud](https://img.shields.io/badge/Cloud-DigitalOcean-0080FF)

## Tech Stack

![.NET](https://img.shields.io/badge/.NET-512BD4?style=flat&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=flat&logo=csharp&logoColor=white)
![React](https://img.shields.io/badge/React-61DAFB?style=flat&logo=react&logoColor=black)
![Vite](https://img.shields.io/badge/Vite-646CFF?style=flat&logo=vite&logoColor=white)
![TypeScript](https://img.shields.io/badge/TypeScript-3178C6?style=flat&logo=typescript&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-2496ED?style=flat&logo=docker&logoColor=white)
![Terraform](https://img.shields.io/badge/Terraform-623CE4?style=flat&logo=terraform&logoColor=white)

Poultry Farm Manager is a comprehensive solution for managing broiler chicken farms. It enables efficient control of daily tasks, financial management, inventory tracking, and monitoring of production and batch health. This system is directly related to my entrepreneurial project and poultry farm, aiming to optimize processes, support decision-making, and improve profitability through intuitive tools and detailed reports.

## Table of Contents

-   [Documentation](#documentation)
    -   [How to Build and Run Docker Images Locally](#how-to-build-and-run-docker-images-locally)
        -   [Build and Run WebAPI Image](#build-and-run-webapi-image)
        -   [Build and Run WebApp Image](#build-and-run-webapp-image)
        -   [Build and Run Hybrid Image](#build-and-run-hybrid-image)
    -   [Publish images to Digital Ocean registry](#publish-images-to-digital-ocean-registry)
        -   [Creating DigitalOcean API Token](#creating-digitalocean-api-token)
-   [Infrastructure as Code & CI/CD Pipeline](#infrastructure-as-code--cicd-pipeline)
    -   [Infrastructure Overview](#infrastructure-overview)
    -   [Prerequisites](#prerequisites)
    -   [Required Secrets Configuration](#required-secrets-configuration)
    -   [HCP Terraform Cloud Setup](#hcp-terraform-cloud-setup)
    -   [CI/CD Workflow Architecture](#cicd-workflow-architecture)
    -   [Terraform Actions Available](#terraform-actions-available)
    -   [Manual Workflow Execution](#manual-workflow-execution)
    -   [Terraform Outputs (JSON Format)](#terraform-outputs-json-format)
    -   [Local Development & Testing](#local-development--testing)
-   [🚀 Deployment Guide](#-deployment-guide)
    -   [📋 Deployment Architecture](#-deployment-architecture)
    -   [🔄 Step-by-Step Deployment Process](#-step-by-step-deployment-process)
        -   [Phase 1: Release Setup (Main Branch)](#phase-1-release-setup-main-branch)
        -   [Phase 2: Infrastructure Deployment (Tag Branch)](#phase-2-infrastructure-deployment-tag-branch)
    -   [🛡️ Safety Features](#️-safety-features)
    -   [🌟 Best Practices](#-best-practices)

## Documentation

### How to Build and Run Docker Images Locally

The solution handles three different images:

-   **WebAPI image** - Backend API service
-   **WebApp image** - Frontend application
-   **Hybrid image** - Combined WebAPI and WebApp for testing purposes and cost savings on cloud registry services

---

#### Build and Run WebAPI Image

1. Open a terminal and navigate to `./server/PoultryFarmManager.WebAPI/`
2. Build the image:

```console
docker build -t <image_name>:<tag> ./../../ -f ./Dockerfile
```

3. Run the container:

```console
docker run -d --name <container_name> -e ASPNETCORE_ENVIRONMENT=<env> -e ASPNETCORE_HTTP_PORTS=<port> <image_name>:<tag>
```

---

#### Build and Run WebApp Image

1. Open a terminal and navigate to `./client/webapp/` directory
2. Build the image:

```console
docker build -t <image_name>:<tag> --build-arg API_URL=<http://your.api.url/api> .
```

3. Run the container:

```console
docker run -d --name <container_name> <image_name>:<tag>
```

> **Note:** Nginx image CMD will be used as the default CMD for this image (`nginx -g "daemon off;"`).

---

#### Build and Run Hybrid Image

1. Open a terminal in the root directory of the solution
2. Build the image (same build command as for [WebApp](#build-and-run-webapp-image)):

```console
docker build -t <image_name>:<tag> --build-arg API_URL=<http://your.api.url/api> .
```

> **Note:** This image also uses nginx default CMD.

3.  Create separate containers for WebAPI and WebApp from this single image:

**Run a container for WebAPI:**

```console
docker run -d --name <container_name> -p <host_port>:80 -e ASPNETCORE_ENVIRONMENT=<env> -e ASPNETCORE_HTTP_PORTS=80 <image_name>:<tag> dotnet /webapi/PoultryFarmManager.WebAPI.dll
```

> The last part overrides the default CMD to run the WebAPI with dotnet through port 80 instead of Nginx.

**Run a container for WebApp:**

```console
docker run -d --name <container_name> -p <host_port>:80 <image_name>:<tag>
```

### Publish images to Digital Ocean registry

Install doctl and authenticate it with an API token (An API token can be created in the Digital Ocean control panel).

#### Creating DigitalOcean API Token:

1. Go to [DigitalOcean Control Panel](https://cloud.digitalocean.com/account/api/tokens)
2. Click "Generate New Token"
3. Name: `<some_name>`
4. Scopes: `Read` and `Write`
5. Copy the token immediately (it won't be shown again)

```console
doctl auth init
```

For using context DO contexts, you can set up a context for your Digital Ocean account. This allows you to manage multiple accounts or configurations easily.

```console
doctl auth init --context <my-context>
doctl auth list
doctl auth switch --context <NAME>
```

Use the registry login command to authenticate Docker with your registry:

```console
doctl registry login
```

Use the docker tag command to tag your image with the fully qualified destination path:

```console
docker tag <my-image> registry.digitalocean.com/<my-registry>/<my-image>
```

Use the docker push command to upload your image:

```console
docker push registry.digitalocean.com/<my-registry>/<my-image>
```

_If you push a new image using an existing tag, the tag gets updated but the old image is still accessible by its digest and takes up space in your registry. To reduce your storage usage, you can delete the untagged images and then run garbage collection_

---

## Infrastructure as Code & CI/CD Pipeline

This project uses **Infrastructure as Code (IaC)** with **Terraform** and **HCP Terraform Cloud** for managing cloud infrastructure on **DigitalOcean**, combined with **GitHub Actions** for automated CI/CD deployment.

### Infrastructure Overview

The infrastructure setup includes:

-   **DigitalOcean** for Cloud services
-   **HCP Terraform Cloud** for state management and plan/apply operations
-   **GitHub Actions** for automated build, test, and deployment workflows

### Prerequisites

Before setting up the infrastructure, ensure you have:

1. **DigitalOcean Account** with API access
2. **HCP Terraform Cloud Account**

### Required Secrets Configuration

Configure these secrets in your GitHub repository (`Settings > Secrets and variables > Actions`):

#### Repository Secrets:

| Secret Name                 | Description                   | Example Format      |
| --------------------------- | ----------------------------- | ------------------- |
| `DIGITALOCEAN_ACCESS_TOKEN` | DigitalOcean API token        | `dop_v1_abc123...`  |
| `TF_API_TOKEN`              | HCP Terraform Cloud API token | `abc123.atlasv1...` |

#### Repository Variables:

| Variable Name | Description                           | Example Value          |
| ------------- | ------------------------------------- | ---------------------- |
| `IMAGE_NAME`  | Docker image name for the application | `poultry-farm-manager` |

#### Creating HCP Terraform Cloud API Token:

1. Go to [HCP Terraform Cloud](https://app.terraform.io/)
2. Navigate to `User Settings > Tokens`
3. Click "Create an API token"
4. Description: `GitHub Actions Integration`
5. Copy the token value

### HCP Terraform Cloud Setup

#### 1. Create Organization and Workspace:

```bash
# Organization: your-org-name
# Workspace: poultry-farm-infrastructure
# Working Directory: IaC/cloud-services
```

#### 2. Configure Workspace Settings:

-   **Execution Mode**: `Remote`
-   **Terraform Version**: `~> 1.0`
-   **Auto Apply**: `Disabled` (we use manual apply via GitHub Actions)

#### 3. Set Workspace Variables:

| Variable Name        | Type        | Value           | Sensitive |
| -------------------- | ----------- | --------------- | --------- |
| `digitalocean_token` | Environment | `your-do-token` | ✅ Yes    |

### CI/CD Workflow Architecture

The deployment process follows a **branch-based strategy** with plan/apply separation:

#### Main Branch Workflow (`plan` action):

```
Build & Test → Terraform Format → Terraform Validate → Terraform Plan → Create Git Tag
```

#### Tag Branch Workflow (`deploy` action):

```
Build & Test → Terraform Format → Terraform Validate → Terraform Plan → Terraform Apply → Docker Build & Push → GitHub Release
```

#### Tag Branch Workflow (`destroy` action):

```
Terraform Destroy
```

### Terraform Actions Available

The reusable Terraform job supports these actions:

| Action     | Purpose                              | Dependencies     | Secrets Required |
| ---------- | ------------------------------------ | ---------------- | ---------------- |
| `format`   | Check and apply Terraform formatting | None             | ❌               |
| `validate` | Validate Terraform configuration     | `terraform init` | ✅               |
| `plan`     | Create execution plan                | `terraform init` | ✅               |
| `apply`    | Apply infrastructure changes         | Existing plan    | ✅               |
| `destroy`  | Destroy infrastructure               | `terraform init` | ✅               |

### Manual Workflow Execution

#### Planning Infrastructure (Main Branch):

```bash
# Trigger from GitHub Actions UI
Action: plan
Release Tag: v1.0.0
```

#### Deploying Infrastructure (Tag Branch):

```bash
# Switch to the created tag
git checkout v1.0.0

# Trigger deployment from GitHub Actions UI
Action: deploy
Is Pre-release: false
As Draft: false
Clean Registry: true
```

#### Destroying Infrastructure (Tag Branch):

```bash
# Trigger from GitHub Actions UI on tag branch
Action: destroy
```

### Terraform Outputs (JSON Format)

The Terraform infrastructure job returns all outputs in **JSON format** for maximum flexibility and reusability across different projects.

#### Output Structure:

```json
{
    "registry_endpoint": {
        "value": "registry.digitalocean.com/your-registry-name"
    }
}
```

#### Accessing Outputs in GitHub Workflows:

```yaml
# Example: Using registry endpoint in Docker build job
docker-build-push:
    needs: terraform-apply
    with:
        # Access specific output using fromJSON() function
        image_name: ${{ fromJSON(needs.terraform-apply.outputs.terraform_outputs).registry_endpoint.value }}/${{ vars.IMAGE_NAME }}
```

#### Adding New Outputs:

```hcl
# In your Terraform configuration
output "new_resource_endpoint" {
  description = "New resource endpoint"
  value       = digitalocean_some_resource.example.endpoint
}

# Automatically available in workflows as:
# ${{ fromJSON(needs.terraform-apply.outputs.terraform_outputs).new_resource_endpoint.value }}
```

### Local Development & Testing

For deployment locally, you need to configure the backend and variables properly:

##### 1. Create `backend_override.tf` file:

```hcl
# IaC/cloud-services/backend_override.tf
terraform {
  backend "local" {
    path = "terraform.tfstate"
  }
}
```

> **Note**: This file overrides the HCP Terraform Cloud backend for local development. It's already included in `.gitignore` to prevent accidental commits.

##### 2. Create `terraform.tfvars` file:

```hcl
# IaC/cloud-services/terraform.tfvars
digitalocean_token = "your-digitalocean-api-token-here"
```

> **⚠️ Important**: Never commit this file to version control as it contains sensitive credentials. It's already included in `.gitignore`.

#### Initialize Terraform and apply changes

```bash
terraform init
terraform apply
```

---

## 🚀 Deployment Guide

This project uses a **two-phase deployment system** for maximum safety and control. The deployment process is split into **Release Setup** and **Infrastructure Deployment** phases.

### 📋 Deployment Architecture

The CD workflow has been divided into two separate workflows:

#### 1️⃣ **Release Workflow** (`workflow_release.yaml`)

-   **Runs on**: `main` branch only
-   **Purpose**: Code validation and release preparation
-   **Output**: Creates Git tag and draft GitHub release

#### 2️⃣ **Deploy Workflow** (`workflow_deploy.yaml`)

-   **Runs on**: Tag branches only
-   **Purpose**: Infrastructure and application deployment
-   **Output**: Live infrastructure and published release

### 🔄 Step-by-Step Deployment Process

#### **Phase 1: Release Setup (Main Branch)**

1. **Navigate to GitHub Actions**

    - Go to your repository on GitHub
    - Click on **"Actions"** tab
    - Select **"Release Workflow"**

2. **Run Release Workflow**

    - Click **"Run workflow"**
    - Fill in the parameters:
        ```
        Release tag: v1.0.0 (or your desired version)
        Is pre-release: false
        Release notes: (optional custom notes)
        ```
    - Click **"Run workflow"**

3. **Wait for completion**
   The workflow will:
    - ✅ Run CI Workflow jobs
    - ✅ Validate Terraform configuration
    - ✅ Display infrastructure plan
    - ✅ Create Git tag
    - ✅ Create draft GitHub release

#### **Phase 2: Infrastructure Deployment (Tag Branch)**

4. **Navigate to GitHub Actions**

    - Go to **"Actions"** tab
    - Select **"Deploy Workflow"**
    - Ensure you're now on the tag branch or select it from the branch dropdown

5. **Run Deploy Workflow**

    - Click **"Run workflow"**
    - Verify you're on the tag branch
    - Fill in the parameters:
        ```
        Action: deploy
        Clean registry: true (recommended)
        ```
    - Click **"Run workflow"**

### 🛡️ Safety Features

#### **Branch Protection**

-   **Release Workflow**: Only runs on `main` branch
-   **Deploy Workflow**: Only runs on `tag` branches
-   **No accidental deploys**: Must explicitly switch branches

#### **Infrastructure Safety**

-   **Clean slate deployment**: Always destroy before apply
-   **Draft releases**: Releases start as drafts until deployment succeeds
-   **Validation first**: All tests pass before any deployment actions

#### **Rollback Capability**

To rollback to a previous version run Deploy Workflow with action `deploy` on a previous tag branch

#### **Required Secrets**

Ensure these are configured in GitHub repository settings:

-   `DIGITALOCEAN_ACCESS_TOKEN`
-   `TF_API_TOKEN`

#### **Required Variables**

-   `IMAGE_NAME`

### 🌟 Best Practices

1. **Always test on main first**: Run Release Workflow to validate before deployment
2. **Use semantic versioning**: `v1.0.0`, `v1.1.0`, `v2.0.0`
3. **Clean deployments**: Always use destroy + apply for consistent state
4. **Monitor logs**: Check GitHub Actions logs for any issues
5. **Tag protection**: Don't manually delete deployment tags

This deployment system provides maximum safety and control over your infrastructure and application releases! 🚀
