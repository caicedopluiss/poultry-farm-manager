data "digitalocean_container_registry" "personal" {
  name = var.registry_name
}

data "digitalocean_project" "pfm" {
  name = "Poultry Farm Manager"
}
