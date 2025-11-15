terraform {
  required_providers {
    digitalocean = {
      source  = "digitalocean/digitalocean"
      version = "~> 2.0"
    }
    postgresql = {
      source  = "cyrilgdn/postgresql"
      version = "~> 1.26.0"
    }
  }
}

provider "digitalocean" {
  token = var.digitalocean_token
}

provider "postgresql" {
  host            = local.db_secrets.host
  port            = local.db_secrets.port
  database        = local.db_secrets.database
  username        = local.db_secrets.admin_user
  password        = local.db_secrets.admin_user_password
  sslmode         = "require"
  connect_timeout = 15
  superuser       = false
}
