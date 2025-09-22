resource "digitalocean_container_registry" "personal" {
  name                   = var.registry_name
  subscription_tier_slug = "starter"
  region                 = var.region
}

output "registry_endpoint" {
  description = "Endpoint URL of the container registry"
  value       = digitalocean_container_registry.personal.endpoint
}
