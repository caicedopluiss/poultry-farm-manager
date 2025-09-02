resource "digitalocean_container_registry" "personal" {
  name                   = var.registry_name
  subscription_tier_slug = var.registry_subscription_tier
  region                 = var.region
}

output "registry_endpoint" {
  description = "Endpoint URL of the container registry"
  value       = digitalocean_container_registry.personal.endpoint
}
