# PostgreSQL database cluster
resource "digitalocean_database_cluster" "pfm_postgres" {
  name       = "${local.app_prefix}-postgres"
  engine     = "pg"
  version    = var.database_version
  size       = "db-s-1vcpu-1gb"
  region     = var.region
  node_count = 1
}

data "digitalocean_database_user" "admin_user" {
  cluster_id = digitalocean_database_cluster.pfm_postgres.id
  name       = "doadmin"
}

resource "digitalocean_database_db" "pfm_db" {
  cluster_id = digitalocean_database_cluster.pfm_postgres.id
  name       = var.database_name
}

# Database user
resource "digitalocean_database_user" "app_user" {
  cluster_id = digitalocean_database_cluster.pfm_postgres.id
  name       = "app_user"
}

# Grant permissions to app_user on all tables
resource "postgresql_grant" "app_user_tables" {
  database    = local.db_secrets.database
  role        = local.db_secrets.app_user
  schema      = "public"
  object_type = "table"
  privileges  = ["SELECT", "INSERT", "UPDATE", "DELETE", "TRUNCATE"]

  depends_on = [digitalocean_app.platform]
}

# Grant default privileges on future tables created by admin user
resource "postgresql_default_privileges" "app_user_future_tables" {
  database    = local.db_secrets.database
  role        = local.db_secrets.app_user
  schema      = "public"
  owner       = local.db_secrets.admin_user
  object_type = "table"
  privileges  = ["SELECT", "INSERT", "UPDATE", "DELETE", "TRUNCATE"]

  depends_on = [digitalocean_app.platform]
}
