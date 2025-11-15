# Poultry Farm Manager

![Build Status](https://github.com/caicedopluiss/poultry-farm-manager/workflows/CI%20Workflow/badge.svg)

## Overview

Poultry Farm Manager is a comprehensive solution for managing broiler chicken farms. It enables efficient control of daily tasks, financial management, inventory tracking, and monitoring of production and batch health. This system is directly related to my entrepreneurial project and poultry farm, aiming to optimize processes, support decision-making, and improve profitability through intuitive tools and detailed reports.

## Technologies

![.NET](https://img.shields.io/badge/.NET-512BD4?style=flat&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=flat&logo=csharp&logoColor=white)
![xUnit](https://img.shields.io/badge/xUnit-512BD4?style=flat&logo=xunit&logoColor=white)
![EF Core](https://img.shields.io/badge/ORM-EF%20Core-512BD4?style=flat&logo=entityframework&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-336791?style=flat&logo=postgresql&logoColor=white)
![React](https://img.shields.io/badge/React-61DAFB?style=flat&logo=react&logoColor=black)
![Vite](https://img.shields.io/badge/Vite-646CFF?style=flat&logo=vite&logoColor=white)
![TypeScript](https://img.shields.io/badge/TypeScript-3178C6?style=flat&logo=typescript&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-2496ED?style=flat&logo=docker&logoColor=white)
![Terraform](https://img.shields.io/badge/Terraform-623CE4?style=flat&logo=terraform&logoColor=white)
![Cloud](https://img.shields.io/badge/Cloud-DigitalOcean-0080FF)
![Infrastructure](https://img.shields.io/badge/Infrastructure-HCP%20Terraform-623CE4)

### Tools

![VS Code](https://img.shields.io/badge/IDE-VS%20Code-007ACC?style=flat&logo=visualstudiocode&logoColor=white)
![Git](https://img.shields.io/badge/Version%20Control-Git-F05032?style=flat&logo=git&logoColor=white)
![PowerShell](https://img.shields.io/badge/Scripting-PowerShell-5391FE?style=flat&logo=powershell&logoColor=white)
![GitHub Desktop](https://img.shields.io/badge/GitHub%20Desktop-24292F?style=flat&logo=github&logoColor=white)
![Docker Desktop](https://img.shields.io/badge/Containerization-Docker%20Desktop-2496ED?style=flat&logo=docker&logoColor=white)
![Postman](https://img.shields.io/badge/API%20Testing-Postman-FF6C37?style=flat&logo=postman&logoColor=white)

## Table of Contents

-   [Overview](#overview)
-   [Technologies](#technologies)
-   [How To guides](#how-to-guides)
    -   [Run solution locally with Docker Compose](#run-solution-locally-with-docker-compose)
    -   [Build and Run Docker images locally](#build-and-run-docker-images-locally)
        -   [Build and Run WebAPI Image](#build-and-run-webapi-image)
        -   [Build and Run WebApp Image](#build-and-run-webapp-image)
        -   [Build and Run Hybrid Image](#build-and-run-hybrid-image)
        -   [Build and Run Standalone Image](#build-and-run-standalone-image)
    -   [Using PowerShell Build Scripts](#using-powershell-build-scripts)
    -   [Debug WebAPI project using VS Code and Docker Compose](#debug-webapi-project-using-vs-code-and-docker-compose)
    -   [Deploy solution to DigitalOcean locally](#deploy-solution-to-digitalocean-locally)
    -   [Create a Release and Deploy it using GitHub Actions](#create-a-release-and-deploy-it-using-github-actions)
    -   [Create a new database migration](#create-a-new-database-migration)
-   [API Testing with Postman](#api-testing-with-postman)
-   [CI/CD and Automation Workflows](#cicd-and-automation-workflows)

## How To Guides

### Run solution locally with Docker Compose

-   Ensure you have [Docker](https://docs.docker.com/get-docker/) installed.
-   Navigate to the root directory of the solution.
-   Create a `.env` file with the following content:

```env
DB_PORT=5432
DB_NAME=<your_database_name>
DB_USER=<your_database_user>
DB_PASSWORD=<your_database_password>
```

see [.example.env](.example.env) for reference.

-   Run the following command to start the services:

```bash
docker-compose up --build
```

or, to use the hybrid image with a single image for both services:

```bash
docker-compose -f docker-compose-hybrid.yaml up --build
```

-   Access the WebApp at `http://localhost:8081` and the WebAPI at `http://localhost:8080`.

or, to use the standalone image with both services in a single container:

```bash
docker-compose -f docker-compose-standalone.yaml up --build
```

-   Access the WebApp and WebAPI at `http://localhost:80` or just `http://localhost`.

### Build and Run Docker Images Locally

-   Ensure you have [Docker](https://docs.docker.com/get-docker/) installed.

The solution handles four different images:

-   **WebAPI image** - Backend API service
-   **WebApp image** - Frontend application
-   **Hybrid image** - Combined WebAPI and WebApp for testing and cost savings purposes. Both services can be run from the same image in separate containers.
-   **Standalone image** - Combined WebAPI and WebApp with Nginx for testing and cost savings purposes. Both services run in a single container.

#### Build and Run WebAPI Image

1. Open a terminal and navigate to `./server/PoultryFarmManager.WebAPI/`
2. Build the image:

```bash
docker build -t <image_name>:<tag> ./../../ -f ./Dockerfile
```

3. Run the container:

```bash
docker run -d --name <container_name> -e ASPNETCORE_ENVIRONMENT=<env> -e ASPNETCORE_HTTP_PORTS=<port> -e ConnectionStrings__pfm="Host=<db_host>;Port=5432;Database=<db_name>;Username=<db_user>;Password=<db_password>" <image_name>:<tag>
```

---

#### Build and Run WebApp Image

1. Open a terminal and navigate to `./client/webapp/` directory
2. Build the image:

```bash
docker build -t <image_name>:<tag> --build-arg API_HOST_URL=<http://your.api_host.url> .
```

3. Run the container:

```bash
docker run -d --name <container_name> <image_name>:<tag>
```

> **Note:** Nginx image CMD will be used as the default CMD for this image (`nginx -g "daemon off;"`).

---

#### Build and Run Hybrid Image

1. Open a terminal in the root directory of the solution
2. Build the image (same build command as for [WebApp](#build-and-run-webapp-image)):

```bash
docker build -f Dockerfile.hybrid -t <image_name>:<tag> --build-arg API_HOST_URL=<http://your.api_host.url> .
```

> **Note:** This image also uses nginx default CMD.

3.  Create separate containers for WebAPI and WebApp from this single image:

**Run a container for WebAPI:**

```bash
docker run -d --name <container_name> -p <host_port>:80 -e ASPNETCORE_ENVIRONMENT=<env> -e ASPNETCORE_HTTP_PORTS=80 -e ConnectionStrings__pfm="Host=<db_host>;Port=5432;Database=<db_name>;Username=<db_user>;Password=<db_password>" <image_name>:<tag> dotnet /webapi/PoultryFarmManager.WebAPI.dll
```

> The last part overrides the default CMD to run the WebAPI with dotnet through port 80 instead of Nginx.

**Run a container for WebApp:**

```bash
docker run -d --name <container_name> -p <host_port>:80 <image_name>:<tag>
```

#### Build and Run Standalone Image

1. Open a terminal in the root directory of the solution
2. Build the image:

```bash
docker build -f Dockerfile.standalone -t <image_name>:<tag> .
```

> **Note:** This image CMD uses the custom `start-standalone.sh` script to start both Nginx and WebAPI services.

3. Run the container:

```bash
docker run -d --name <container_name> -p <host_port>:80 -e ASPNETCORE_ENVIRONMENT=<env> -e ConnectionStrings__pfm="Host=<db_host>;Port=5432;Database=<db_name>;Username=<db_user>;Password=<db_password>" <image_name>:<tag>
```

> Note: The Standalone image does not require the API_HOST_URL build argument since relative paths are handled by Nginx. Also, the WebAPI service runs on port 5000 internally, proxied by Nginx. This port is hardcoded and in case of changes, the `nginx-standalone.conf` and `Dockerfile.standalone` file must be updated accordingly.

### Using PowerShell Build Scripts

For a more streamlined approach to building and pushing Docker images locally, you can use the PowerShell utility scripts located in the `scripts/` directory. These scripts provide convenient functions to build all image types with customizable options.

**Prerequisites:**

-   Docker Desktop (running)
-   DigitalOcean CLI (`doctl`) for registry authentication
-   PowerShell 5.1+ or PowerShell Core 7+

**Quick Start:**

```powershell
# Load the script
. .\scripts\Build-DockerImages.ps1

> IMPORTANT: Make sure to run the script from the project root. So cd .. to the project root before running the above command.

# Build and push standalone image
Build-StandaloneImage -ImageName "pfm/standalone" -Tag "v1.0.0" -Push $true

# Build hybrid image
Build-HybridImage -ImageName "pfm/hybrid" -Tag "latest"

# Build individual service images
Build-WebApiImage -ImageName "pfm/webapi" -Tag "dev"
Build-WebAppImage -ImageName "pfm/webapp" -Tag "dev"
```

For detailed documentation on available functions, configuration options, and examples, see [Build Scripts Documentation](docs/BUILD_SCRIPTS.md).

### Debug WebAPI project using VS Code and Docker Compose

To debug the WebAPI service, do the following:

-   Ensure you have [Docker](https://docs.docker.com/get-docker/) installed.
-   Open the project in VS Code.
-   Open a terminal and navigate to `./server/PoultryFarmManager.WebAPI/`
-   Create a .env file with the following content:

```env
DB_PORT=5432
DB_NAME=<your_database_name>
DB_USER=<your_database_user>
DB_PASSWORD=<your_database_password>
```

see [.example.env](.example.env) for reference.

-   run the following command to start the database service:

```bash
docker-compose up --build
```

-   Set up the `pfm` connection string in `appsettings.json` file to point to the local database:

```json
{
    "ConnectionStrings": {
        "pfm": "Host=localhost;Port=5432;Database=<your_database_name>;Username=<your_database_user>;Password=<your_database_password>"
    }
}
```

> **IMPORTANT**: Do not track changes to this file, as it can contain sensitive information. Make sure to run `git update-index --assume-unchanged ./server/PoultryFarmManager.WebAPI/appsettings.json` to untrack changes, use `appsettings.example.json` instead to leave any configuration reference.

-   Now you can run the WebAPI project, either by using the VS Code debugger. Open the VS Code debugger panel, select the `.NET Core Launch (web) (webapi)` configuration, and start debugging.

-   Set a breakpoint in the code where you want to start debugging.

<br/>

> **Note**: Avoid running multiple docker compose instances that might conflict with each other. If you have previously run a docker-compose instance, make sure to stop it and remove any existing containers that might interfere with the debugging session.

### Deploy solution to DigitalOcean locally

-   Ensure you have [Terraform ~1.3](https://learn.hashicorp.com/tutorials/terraform/install-cli) installed.
-   Ensure you have [doctl](https://github.com/digitalocean/doctl/releases) installed.
-   Ensure you have the DigitalOcean API token. See [Creating DigitalOcean API Token](docs/WORKFLOWS.md#how-to-create-digitalocean-api-token).
-   Navigate to `./IaC/cloud/` directory.
-   Create a `terraform.tfvars` file with the following content:

```hcl
digitalocean_token = "<your_digitalocean_token>"
```

-   Create a `backend_override.tf` file to use local state instead of HCP Terraform Cloud:

```hcl
terraform {
  backend "local" {
    path = "terraform.tfstate"
  }
}
```

-   Initialize Terraform:

```bash
terraform init
```

-   Apply the Terraform configuration:

```bash
terraform apply --auto-approve
```

-   Build Docker images. [See here](#build-and-run-docker-images-locally) and push them to DigitalOcean registry by following these steps:

    -   Ensure you have [doctl](https://github.com/digitalocean/doctl/releases) installed.

    -   Ensure you have the DigitalOcean API token. See [Creating DigitalOcean API Token](docs/WORKFLOWS.md#how-to-create-digitalocean-api-token).

    -   Ensure you have created a DigitalOcean container registry. See [Creating a registry](https://www.digitalocean.com/docs/container-registry/how-to/create-registry/).

    ```bash
    doctl auth init
    ```

    For using context DO contexts, you can set up a context for your Digital Ocean account. This allows you to manage multiple accounts or configurations easily.

    ```bash
    doctl auth init --context <my-context>
    doctl auth list
    doctl auth switch --context <NAME>
    ```

    Use the registry login command to authenticate Docker with your registry:

    ```bash
    doctl registry login
    ```

    Use the docker tag command to tag your image with the fully qualified destination path:

    ```bash
    docker tag <my-image> registry.digitalocean.com/<my-registry>/<my-image>
    ```

    Use the docker push command to upload your image:

    ```bash
    docker push registry.digitalocean.com/<my-registry>/<my-image>
    ```

    _If you push a new image using an existing tag, the tag gets updated but the old image is still accessible by its digest and takes up space in your registry. To reduce your storage usage, you can delete the untagged images and then run garbage collection_

-   Navigate to `./IaC/application/` directory.

-   Create a `terraform.tfvars` file with the following content:

```hcl
digitalocean_token = "<your_digitalocean_token>"
api_base_path   = "<your_api_base_path>" # (i.e. /webapi)
```

-   Create a `backend_override.tf` file to use local state instead of HCP Terraform Cloud:

```hcl
terraform {
  backend "local" {
    path = "terraform.tfstate"
  }
}
```

-   Initialize Terraform:

```bash
terraform init
```

-   Apply the Terraform configuration:

```bash
terraform apply --auto-approve
```

> **Note**: All these files override the HCP Terraform Cloud backend for local development. They're already included in `.gitignore` to prevent accidental commits. Make sure to never commit sensitive information (.tfvar, state files and all local overrides).

<br/>

### Create a Release and Deploy it using GitHub Actions

> **Important**: Make sure to set the properly backend configuration for HCP Terraform in the `./IaC/cloud/backend.tf` and `./IaC/application/backend.tf` directories when deploying through GitHub Actions.

See the [Workflows Documentation](docs/WORKFLOWS.md#deploy-workflow) for detailed instructions on creating a release and deploying it using GitHub Actions.

-   Ensure you have configured workflow secrets and variables as described in the [Environment Variables and Secrets](docs/WORKFLOWS.md#environment-variables-and-secrets) section.

-   Run the [Release Workflow](docs/WORKFLOWS.md#release-workflow) from the `main` branch to create a new release.

-   Run the [Deploy Workflow](docs/WORKFLOWS.md#deploy-workflow) from the created tag branch to deploy the release.

> Note: Make sure to use semantic versioning for your release tags (e.g., v1.0.0, v1.1.0, v2.0.0).

### Create a new database migration

To create a new database migration for the WebAPI project, follow these steps:

-   Make sure you have EF Core CLI tools installed. You can install them globally using the command:

```bash
dotnet tool install --global dotnet-ef
```

To update the EF Core CLI tools to the latest version, use:

```bash
dotnet tool update --global dotnet-ef
```

> For more information, see the [official EF Core tools documentation](https://learn.microsoft.com/en-us/ef/core/cli/dotnet).

-   Ensure that the startup project (`PoultryFarmManager.WebAPI`) references the `Microsoft.EntityFrameworkCore.Design` package. This is required for design-time operations such as migrations.

To create a new migration (for example, the initial migration), run the following command from the `./server/PoultryFarmManager.WebAPI/` directory:

```bash
dotnet ef migrations add Initial --project ..\PoultryFarmManager.Infrastructure\PoultryFarmManager.Infrastructure.csproj --context AppDbContext
```

`Initial` is the name of the migration. You can change it as needed.

The `--project` option points to the Infrastructure project where the DbContext is defined.

The `--context` option specifies the DbContext to use. It is optinal since there is only one DbContext in the Infrastructure project, in case you have multiple DbContexts this flag is required.

The `--startup-project` option is not required since the command is run from the WebAPI project directory.

**Applying migrations:**

Migrations are automatically applied when you run the migrations job (`PoultryFarmManager.WebAPI` project with `migrate` argument). **It's mandatory to run this job before starting the WebAPI service** to ensure the database schema is up to date.

You can also apply migrations manually by running the following command from the `./server/PoultryFarmManager.WebAPI/` directory:

```bash
dotnet run migrate
```

This ensures your database is always up to date with the latest schema changes.

## API Testing with Postman

See the [Postman Documentation](docs/POSTMAN.md) for detailed instructions on how to test the Poultry Farm Manager API using Postman collections and environment files.

## CI/CD and Automation Workflows

See the [Workflows Documentation](docs/WORKFLOWS.md) for detailed instructions on the CI/CD and automation workflows set up for this project using GitHub Actions.
