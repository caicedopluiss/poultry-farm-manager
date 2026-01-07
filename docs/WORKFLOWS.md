# GitHub Actions Workflows Documentation

This document provides comprehensive documentation for all GitHub Actions workflows in the Poultry Farm Manager project. Our CI/CD pipeline consists of three main workflows and several reusable job components.

## Table of Contents

-   [Overview](#overview)
-   [Main Workflows](#main-workflows)
    -   [CI Workflow](#ci-workflow)
    -   [Release Workflow](#release-workflow)
    -   [Deploy Workflow](#deploy-workflow)
-   [Reading terraform outputs in GitHub Actions](#reading-terraform-outputs-in-github-actions)
-   [Reusable Job Components](#reusable-job-components)
-   [Environment Variables and Secrets](#environment-variables-and-secrets)
-   [Usage Guidelines](#usage-guidelines)
-   [Workflows Dependencies](#workflows-dependencies)

## Overview

This project uses **Infrastructure as Code (IaC)** with **Terraform** and **HCP Terraform Cloud** for managing cloud infrastructure on **DigitalOcean**, combined with **GitHub Actions** for automated CI/CD deployment. The deployment process follows a **branch-based strategy** with a three-stage workflow architecture and workflow responsibilities separation.

### Workflow Responsibilities

| Workflow    | Purpose                                   | Trigger          | Branch Requirement |
| ----------- | ----------------------------------------- | ---------------- | ------------------ |
| **CI**      | Code validation, testing, format checking | Push, PR, Manual | Any branch         |
| **Release** | Create tags and releases                  | Manual           | `main` branch only |
| **Deploy**  | Infrastructure deployment                 | Manual           | Tag branches only  |

### Best Practices

1. **Always test on main first**: Run Release Workflow to validate before deployment
2. **Use semantic versioning**: `v1.0.0`, `v1.1.0`, `v2.0.0`
3. **Clean deployments**: Always use destroy + apply for consistent state
4. **Monitor logs**: Check GitHub Actions logs for any issues
5. **Tag protection**: Don't manually delete deployment tags
6. **Rollback Capability**: To rollback to a previous version run Deploy Workflow with action `deploy` on a previous tag branch

This deployment system provides maximum safety and control over your infrastructure and application releases! 🚀

## Main Workflows

### CI Workflow

**File**: `.github/workflows/workflow_ci.yaml`

#### Purpose

Validates code quality, runs tests, and checks Terraform configurations on every push or pull request.

#### Triggers

-   **Push**: To any branch
-   **Pull Request**: To `main` branch
-   **Manual**: With customizable parameters

#### Manual Trigger Parameters

-   `node_version` (default: "22"): Node.js version for frontend jobs
-   `dotnet_version` (default: "8.0.x"): .NET version for backend jobs
-   `run_tests` (default: true): Whether to run tests
-   `configuration` (default: "Release"): Build configuration

#### Jobs

1. **frontend-lint**: ESLint validation for React application
2. **tf-format-cloud**: Terraform formatting check for cloud infrastructure
3. **tf-format-application**: Terraform formatting check for application infrastructure
4. **tf-validate-cloud**: Terraform validation for cloud infrastructure
5. **tf-validate-application**: Terraform validation for application infrastructure
6. **frontend-build-and-test**: Build and test React application
7. **backend-build-and-test**: Build and test .NET WebAPI

---

### Release Workflow

**File**: `.github/workflows/workflow_cd_release.yaml`

#### Purpose

Creates Git tags and GitHub releases after validating the entire codebase and infrastructure plans.

#### Triggers

-   **Manual**: Only on `main` branch

#### Required Parameters

-   `release_tag`: Git tag to create (e.g., "v1.0.0")
-   `is_pre_release`: Whether this is a pre-release (default: false)

#### Key Features

-   **Branch Protection**: Only runs on `main` branch
-   **Infrastructure Planning**: Validates both cloud and application infrastructure
-   **Automatic Tag Creation**: Creates Git tag
-   **Release Creation**: Creates published GitHub release
-   **Next Steps Guidance**: Provides clear instructions for deployment

#### Outputs

-   Git tag created
-   GitHub release published
-   Clear instructions for next deployment steps

---

### Deploy Workflow

**File**: `.github/workflows/workflow_cd_deploy.yaml`

#### Purpose

Deploys or destroys infrastructure and applications to DigitalOcean.

#### Triggers

-   **Manual**: Only on tag branches

#### Required Parameters

-   `action`: Choose between "deploy" or "destroy"
-   `clean_registry`: Whether to clean untagged images (default: false)
-   `force_replace_resources`: Resources to force replace (newline-separated, optional)

#### Advanced Parameters

**`force_replace_resources`** - For troubleshooting stuck or failed deployments

This parameter allows you to force Terraform to destroy and recreate specific resources that are stuck, failed, or corrupted. Enter resource names one per line.

**What it does:**

1. Terraform destroys the specified resource in DigitalOcean
2. Creates a fresh replacement with the same configuration
3. Updates Terraform state automatically

**When to use:**

-   ✅ **Deployment stuck or failed** - Common after DigitalOcean maintenance windows
-   ✅ **App won't respond** - Resource is in an error state that normal apply can't fix
-   ✅ **Database connection issues** - Connection problems that persist after redeployment
-   ✅ **Configuration drift** - Resource was manually changed in DO console
-   ✅ **Health check failures** - App Platform health checks repeatedly fail
-   ✅ **Corrupted deployment** - Application won't start or load properly

**How to use:**

1. Go to **Actions** → **Deploy Workflow** → **Run workflow**
2. Select your release tag (e.g., `v1.0.0`)
3. Set `action` to `deploy`
4. In `force_replace_resources`, enter resource names one per line:
    ```
    digitalocean_app.platform
    ```
5. Click **Run workflow**

**Common scenarios:**

_Stuck app deployment (most common):_

```
digitalocean_app.platform
```

_Database permissions issue:_

```
postgresql_grant.app_user_tables
postgresql_default_privileges.app_user_future_tables
```

_Multiple resources at once:_

```
digitalocean_app.platform
postgresql_grant.app_user_tables
```

_Database needs recreation (⚠️ destroys data):_

```
digitalocean_database_cluster.pfm_postgres
```

**⚠️ Important Warnings:**

-   **Downtime**: Replacing resources causes temporary service interruption
-   **Data Loss**: Replacing database resources (`digitalocean_database_cluster.pfm_postgres`) will **permanently delete all data** unless you have backups
-   **Production Impact**: Use during maintenance windows for production environments
-   **Sequence and Dependencies**: While you choose which resources to replace, Terraform determines the actual replacement order based on its dependency graph (not strictly the order listed), and may replace some resources in parallel.

**Best practices:**

-   Start with just the app platform: `digitalocean_app.platform`
-   Only add database resources if absolutely necessary and backups exist
-   Document why you're force replacing in your deployment notes
-   Monitor the GitHub Actions logs during replacement
-   Verify the new resource is healthy after deployment

**Example use case:**
After a DigitalOcean maintenance window, your app deployment shows "Deploy" step with status "ERROR". Normal redeployment doesn't fix it. Solution:

1. Run Deploy Workflow on your current tag
2. Set `force_replace_resources` to `digitalocean_app.platform`
3. This destroys the stuck app and creates a fresh deployment

#### Jobs

##### Deploy Jobs

1. **check-context**: Validates tag branch context
2. **tf-apply-cloud**: Creates cloud infrastructure (registry, networking)
3. **get-tf-cloud-outputs**: Extracts infrastructure outputs (registry endpoint)
4. **docker-build-push**: Builds and pushes Docker images
5. **tf-apply-application**: Deploys application to DigitalOcean App Platform

##### Destroy Jobs

1. **check-context**: Validates tag branch context
2. **tf-destroy-application**: Destroys application infrastructure first
3. **tf-destroy-cloud**: Destroys cloud infrastructure last

#### Key Features

-   **Tag Branch Protection**: Only runs on tag branches
-   **Ordered Destruction**: Application destroyed before infrastructure
-   **Registry Management**: Automatic image cleanup option
-   **Dynamic Configuration**: Uses tag name for image versioning

<br/>

## Reading terraform outputs in GitHub Actions

You can read Terraform outputs in GitHub Actions jobs using the `terraform output` command. Here's a simple example of how to do this for getting a `registry_endpoint` output:

```yaml
run: |
    terraform init
    if terraform output registry_endpoint >/dev/null 2>&1; then
        REGISTRY_ENDPOINT=$(terraform output -raw registry_endpoint)
        echo "registry_endpoint=$REGISTRY_ENDPOINT" >> $GITHUB_OUTPUT
    else
        echo "registry_endpoint=N/A" >> $GITHUB_OUTPUT
        echo "⚠️ Registry endpoint not found"
    fi
```

## Reusable Job Components

### Terraform Infrastructure Job

**File**: `.github/workflows/job_terraform_infrastructure.yaml`

#### Purpose

Reusable job for all Terraform operations across different directories.

#### Inputs

-   `terraform_action`: Action to perform (format, validate, plan, apply, destroy)
-   `working_directory`: Terraform directory (IaC/cloud or IaC/application)
-   `env_vars`: Additional environment variables (key=value format, newline separated)

#### Supported Actions

-   **format**: Check and apply Terraform formatting
-   **validate**: Validate Terraform configuration
-   **plan**: Show Terraform execution plan
-   **apply**: Apply Terraform configuration
-   **destroy**: Destroy Terraform infrastructure

#### Environment Variable Handling

The job accepts additional environment variables in the format:

```yaml
env_vars: |
    TF_VAR_image_name=my-app
    TF_VAR_image_tag=v1.0.0
    TF_VAR_env=Production
```

### Docker Build and Push Job

**File**: `.github/workflows/job_docker_build_push.yaml`

#### Purpose

Builds and pushes Docker images to DigitalOcean Container Registry.

#### Inputs

-   `release_tag`: Git tag for image versioning
-   `image_name`: Full image name including registry
-   `clean_registry`: Whether to clean untagged images

#### Features

-   **Multi-tag Strategy**: Creates release, SHA, and latest tags
-   **Build Arguments**: Passes API_HOST_URL for frontend configuration
-   **Registry Cleanup**: Optional garbage collection for untagged images

### Frontend Jobs

#### Frontend Lint (`job_frontend_lint.yaml`)

-   ESLint validation for React application
-   Customizable Node.js version

#### Frontend Build and Test (`job_frontend_build_and_test.yaml`)

-   Vite build process
-   Optional test execution
-   Artifact generation

### Backend Job

#### Backend Build and Test (`job_backend_build_and_test.yaml`)

-   .NET WebAPI build and test
-   Customizable .NET version and configuration
-   Test result reporting

---

## Environment Variables and Secrets

The repository requires several secrets and variables to be set up in GitHub for the workflows to function correctly.

### Required Secrets

| Secret                      | Description               | Used In                          |
| --------------------------- | ------------------------- | -------------------------------- |
| `DIGITALOCEAN_ACCESS_TOKEN` | DigitalOcean API token    | Deploy, Terraform jobs           |
| `TF_API_TOKEN`              | HCP Terraform Cloud token | Release, Deploy, Terraform jobs  |
| `GITHUB_TOKEN`              | GitHub access token       | Release workflow (auto-provided) |

### Required Variables

| Variable            | Description                  | Example                |
| ------------------- | ---------------------------- | ---------------------- |
| `IMAGE_NAME`        | Docker image repository name | `poultry-farm-manager` |
| `API_HOST_URL_PROD` | Production API base path     | `/api`                 |

### Terraform Variables

The following Terraform variables are automatically set by workflows:

-   `TF_VAR_digitalocean_token`: DigitalOcean access token
-   `TF_VAR_image_name`: Docker image name
-   `TF_VAR_image_tag`: Git tag for image versioning
-   `TF_VAR_api_base_path`: API routing path
-   `TF_VAR_env`: Environment name (Production)

<br/>

## Usage Guidelines

### Development Workflow

1. **Development**

    ```bash
    # Create feature branch
    git checkout -b feature/new-feature

    # Make changes and push
    git push origin feature/new-feature

    # Create PR to main
    ```

    - CI workflow runs automatically
    - All checks must pass before merge

2. **Release Process**

    ```bash
    # Ensure you're on main branch
    git checkout main
    git pull origin main
    ```

    - Go to GitHub Actions → Release Workflow
    - Click "Run workflow"
    - Enter release tag (e.g., "v1.0.0")
    - Select if pre-release
    - Wait for completion

3. **Deployment**
    ```bash
    # Switch to the created tag
    git checkout v1.0.0
    ```
    - Go to GitHub Actions → Deploy Workflow
    - Click "Run workflow"
    - Select "deploy" action
    - Optionally enable registry cleanup
    - Wait for completion

#### Branch Strategy

-   **Feature branches**: For development work
-   **Main branch**: For release preparation
-   **Tag branches**: For deployment

#### Release Naming

-   Use semantic versioning: `v1.0.0`, `v1.0.1`, `v2.0.0`
-   Use pre-release for testing: `v1.0.0-beta.1`

#### Infrastructure Management

-   Always run Release workflow before Deploy workflow
-   Use destroy action carefully (it removes all infrastructure)
-   Monitor costs when deploying to production

<br/>

## Workflows Dependencies

### External Dependencies

-   **DigitalOcean**: Cloud provider for hosting.

#### How to create DigitalOcean API Token:

1. Go to [DigitalOcean Control Panel](https://cloud.digitalocean.com/account/api/tokens)
2. Click "Generate New Token"
3. Name: `<some_name>`
4. Scopes: `Read` and `Write`
5. Copy the token immediately (it won't be shown again)

see more in [DigitalOcean Docs](https://docs.digitalocean.com/reference/api/create-personal-access-token/)

-   **HCP Terraform Cloud**: State management and remote execution.

### HCP Terraform Cloud Setup

-   Sign in at [Terraform Cloud](https://app.terraform.io/)
-   Create a new organization (e.g., `your-org-name`)
-   Create a new project within the organization
-   Create a new API Driven workspace for each terraform IaC directory and repeat the following steps:
    -   Set Workspace settings:
        -   Execution Mode: `Remote`
        -   Terraform Version: `~> 1.13`
        -   Auto Apply: `Disabled` (we use manual apply via GitHub Actions)
        -   Working Directory: `IaC/cloud` or `IaC/application`
-   (Optional) Set the digitalocean_token variable. You can set it in the workspace variables or pass it from GitHub Actions as TF_VAR_digitalocean_token environment variable later too.
-   Create an API token at [User Settings](https://app.terraform.io/app/settings/tokens)
-   Copy the token for later use in GitHub Secrets
