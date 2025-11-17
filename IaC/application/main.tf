

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

    env {
      key   = "DB_SECRETS"
      type  = "SECRET"
      value = jsonencode(local.db_secrets)
    }

    service {
      name = "${local.app_prefix}-standalone"
      image {
        registry_type = "DOCR"
        registry      = data.digitalocean_container_registry.personal.name
        repository    = var.image_name
        tag           = var.image_tag
        deploy_on_push {
          enabled = false
        }
      }
      run_command        = "/bin/sh /start.sh migrate" # run migrations on startup
      instance_size_slug = "basic-xxs"                 # hardcoded for now, to prevent cost spikes
      instance_count     = 1
      http_port          = 80

      env {
        key   = "ASPNETCORE_ENVIRONMENT"
        value = var.env
      }
      env {
        key   = "ConnectionStrings__pfm"
        type  = "SECRET"
        value = "Host=${local.db_secrets.host};Port=${local.db_secrets.port};Database=${local.db_secrets.database};Username=${local.db_secrets.app_user};Password=${local.db_secrets.app_user_password};Pooling=true;Trust Server Certificate=true;"
      }
    }

    ingress {
      rule {
        component {
          name = "${local.app_prefix}-standalone"
        }

        match {
          path {
            prefix = "/"
          }
        }
      }
    }
  }

  # Explicit dependency to ensure database is created before the app
  depends_on = [
    digitalocean_database_cluster.pfm_postgres,
    digitalocean_database_db.pfm_db,
    digitalocean_database_user.app_user
  ]
}

output "platform_live_url" {
  description = "URL of the deployed platform"
  value       = digitalocean_app.platform.live_url
}

output "webapp_url" {
  description = "URL of the deployed Web Application"
  value       = digitalocean_app.platform.live_url
}
