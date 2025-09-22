# PostgreSQL database cluster
resource "digitalocean_database_cluster" "pfm_postgres" {
  name       = "${var.app_code}-postgres"
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
