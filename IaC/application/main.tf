

resource "digitalocean_project_resources" "pfm" {
  count = var.ignore_project ? 0 : 1

  project = data.digitalocean_project.pfm[0].id
  resources = [
    digitalocean_app.platform.urn
  ]
}


# Solution deployed as a single app with multiple services and routing rules
resource "digitalocean_app" "platform" {
  spec {
    name   = "${local.app_prefix}-platform"
    region = var.region

    service {
      name = "${local.app_prefix}-webapi"
      image {
        registry_type = "DOCR"
        registry      = data.digitalocean_container_registry.personal.name
        repository    = var.image_name
        tag           = var.image_tag
        deploy_on_push {
          enabled = false
        }
      }
      source_dir         = var.api_base_path
      run_command        = "dotnet /webapi/PoultryFarmManager.WebAPI.dll"
      instance_size_slug = "basic-xxs" #hardcoded for now, to prevent cost spikes
      instance_count     = 1
      env {
        key   = "ASPNETCORE_ENVIRONMENT"
        value = var.env
      }
      env {
        key   = "ASPNETCORE_HTTP_PORTS"
        value = "80"
      }
      http_port = 80
    }

    service {
      name = "${local.app_prefix}-webapp"
      image {
        registry_type = "DOCR"
        registry      = data.digitalocean_container_registry.personal.name
        repository    = var.image_name
        tag           = var.image_tag
        deploy_on_push {
          enabled = false
        }
      }

      instance_size_slug = "basic-xxs" #hardcoded for now, to prevent cost spikes
      instance_count     = 1
      http_port          = 80
    }

    ingress {
      rule {
        component {
          name = "${local.app_prefix}-webapi"
        }
        match {
          path {
            prefix = var.api_base_path
          }
        }
      }

      rule {
        component {
          name = "${local.app_prefix}-webapp"
        }

        match {
          path {
            prefix = "/"
          }
        }
      }
    }
  }
}

output "platform_live_url" {
  description = "URL of the deployed platform"
  value       = digitalocean_app.platform.live_url
}

output "webapi_url" {
  description = "URL of the deployed Web API"
  value       = "${digitalocean_app.platform.live_url}${var.api_base_path}"
}

output "webapp_url" {
  description = "URL of the deployed Web Application"
  value       = digitalocean_app.platform.live_url
}
