

resource "digitalocean_project_resources" "pfm" {
  count = var.ignore_project ? 0 : 1

  project = data.digitalocean_project.pfm[0].id
  resources = [
    digitalocean_app.platform.urn,
    digitalocean_database_cluster.pfm_postgres.urn
  ]

  # Force replacement when app is replaced
  # This ensures clean project resource assignment after app recreation
  lifecycle {
    replace_triggered_by = [
      digitalocean_app.platform.id
    ]
  }
}

# Solution deployed as a single app with multiple services and routing rules
resource "digitalocean_app" "platform" {

  spec {
    name   = "${local.app_prefix}-platform"
    region = var.region

    # Maintenance mode: stops app instances to save costs while keeping configuration
    maintenance {
      enabled = var.maintenance_mode
      #   offline_page_url = "https://example.com/images/offline.png"
    }

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

      # CORS is not configured here because this is a standalone deployment where
      # the React frontend and the .NET API are served from the same container
      # behind nginx. They always share the same origin (DO URL or custom domain),
      # so the browser never issues a cross-origin request and CORS headers are
      # irrelevant in production. CORS origins for local development are set in
      # appsettings.json (localhost:5173 / localhost:4173).
      #
      # If the architecture changes (e.g. frontend deployed separately), uncomment
      # and adjust the following to allow the appropriate origins:
      #
      # dynamic "env" {
      #   for_each = var.domain_name != "" ? [1] : []
      #   content {
      #     key   = "Cors__AllowedOrigins__0"
      #     value = "https://${var.subdomain != "" ? "${var.subdomain}." : ""}${var.domain_name}"
      #   }
      # }
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

    dynamic "domain" {
      for_each = var.domain_name != "" ? [1] : []
      content {
        name     = "${var.subdomain}.${var.domain_name}"
        type     = "PRIMARY"
        wildcard = false
        zone     = var.domain_name
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

output "custom_domain_url" {
  description = "Custom domain URL (only set when domain_name variable is configured)"
  value       = var.domain_name != "" ? "https://${var.subdomain}.${var.domain_name}" : null
}


