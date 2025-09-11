data "digitalocean_container_registry" "personal" {
  name = var.registry_name
}

data "digitalocean_project" "pfm" {
  count = var.ignore_project ? 0 : 1

  name = var.project_name
}
