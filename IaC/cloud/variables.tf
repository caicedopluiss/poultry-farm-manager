variable "digitalocean_token" {
  description = "DigitalOcean API Token"
  type        = string
  sensitive   = true
}

variable "region" {
  description = "Region for the resources"
  type        = string
  default     = "nyc3"
}

variable "registry_name" {
  description = "Name of the container registry"
  type        = string
  default     = "caicedopluiss"
}

variable "registry_subscription_tier" {
  description = "Container registry subscription tier"
  type        = string
  default     = "starter"  # starter (free), basic, professional

  validation {
    condition     = contains(["starter", "basic", "professional"], var.registry_subscription_tier)
    error_message = "Registry subscription tier must be starter, basic, or professional."
  }
}
