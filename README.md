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

## Overview

Poultry Farm Manager is a comprehensive solution for managing broiler chicken farms. It enables efficient control of daily tasks, financial management, inventory tracking, and monitoring of production and batch health. This system is directly related to my entrepreneurial project and poultry farm, aiming to optimize processes, support decision-making, and improve profitability through intuitive tools and detailed reports.

## Table of Contents

-   [Overview](#overview)
-   [How To guides](#how-to-guides)
    -   [Build and Run Docker images locally](#build-and-run-docker-images-locally)
    -   [Publish images to DigitalOcean Container Registry](#publish-images-to-digitalocean-container-registry)
-   [Deploy solution to DigitalOcean locally](#deploy-solution-to-digitalocean-locally)
-   [Create a Release and Deploy it](#create-a-release-and-deploy-it)

## How To Guides

### Build and Run Docker Images Locally

The solution handles three different images:

-   **WebAPI image** - Backend API service
-   **WebApp image** - Frontend application
-   **Hybrid image** - Combined WebAPI and WebApp for testing purposes and cost savings on cloud registry services

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
docker build -t <image_name>:<tag> --build-arg API_HOST_URL=<http://your.api_host.url> .
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
docker build -t <image_name>:<tag> --build-arg API_HOST_URL=<http://your.api_host.url> .
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

### Publish images to DigitalOcean Container Registry

-   Ensure you have [doctl](https://github.com/digitalocean/doctl/releases) installed.

-   Ensure you have the DigitalOcean API token. See [Creating DigitalOcean API Token](docs/WORKFLOWS.md#how-to-create-digitalocean-api-token).

-   Ensure you have created a DigitalOcean container registry. See [Creating a registry](https://www.digitalocean.com/docs/container-registry/how-to/create-registry/).

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

## Deploy solution to DigitalOcean locally

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

```console
terraform init
```

-   Apply the Terraform configuration:

```console
terraform apply --auto-approve
```

-   Build and push the Docker image to DigitalOcean registry. See [Publish images to DigitalOcean registry](#publish-images-to-digitalocean-container-registry).
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

```console
terraform init
```

-   Apply the Terraform configuration:

```console
terraform apply --auto-approve
```

> **Note**: All these files override the HCP Terraform Cloud backend for local development. They're already included in `.gitignore` to prevent accidental commits. Make sure to never commit sensitive information (.tfvar, state files and all local overrides).

<br/>

### Create a Release and Deploy it

> **Important**: Make sure to set the properly backend configuration for HCP Terraform in the `./IaC/cloud/backend.tf` and `./IaC/application/backend.tf` directories when deploying through GitHub Actions.

See the [Workflows Documentation](docs/WORKFLOWS.md#deploy-workflow) for detailed instructions on creating a release and deploying it using GitHub Actions.

-   Ensure you have configured workflow secrets and variables as described in the [Environment Variables and Secrets](docs/WORKFLOWS.md#environment-variables-and-secrets) section.

-   Run the [Release Workflow](docs/WORKFLOWS.md#release-workflow) from the `main` branch to create a new release.

-   Run the [Deploy Workflow](docs/WORKFLOWS.md#deploy-workflow) from the created tag branch to deploy the release.

> Note: Make sure to use semantic versioning for your release tags (e.g., v1.0.0, v1.1.0, v2.0.0).

```

```
