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
