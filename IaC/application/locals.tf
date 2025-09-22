locals {
  env_map = {
    Development = "dev"
    Staging     = "stg"
    Production  = "prod"
  }
  app_prefix = "${var.app_code}${var.env == "Production" ? "" : "-${local.env_map[var.env]}"}"
  db_secrets = {
    host                = digitalocean_database_cluster.pfm_postgres.host
    port                = digitalocean_database_cluster.pfm_postgres.port
    database            = var.database_name
    admin_user          = data.digitalocean_database_user.admin_user.name
    admin_user_password = data.digitalocean_database_user.admin_user.password
    app_user            = digitalocean_database_user.app_user.name
    app_user_password   = digitalocean_database_user.app_user.password
  }
}
