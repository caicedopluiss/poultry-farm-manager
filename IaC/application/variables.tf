variable "digitalocean_token" {
  description = "DigitalOcean API Token"
  type        = string
  sensitive   = true
}

variable "api_base_path" {
  description = "API base path for routing/platform source directory"
  type        = string
}

variable "region" {
  description = "Region for the resources"
  type        = string
  default     = "nyc1"
}

variable "registry_name" {
  description = "Name of the container registry"
  type        = string
  default     = "caicedopluiss"
}

variable "project_name" {
  description = "Name of the DigitalOcean project"
  type        = string
  default     = "Poultry Farm Manager"
}

variable "image_name" {
  description = "Name of the Docker image"
  type        = string
  default     = "pfm/hybrid"
}

variable "image_tag" {
  description = "Tag of the Docker image to deploy"
  type        = string
  default     = "latest"
}

variable "env" {
  type        = string
  default     = "Development"
  description = "The environment for the application (e.g., Development, Staging, Production)"
  validation {
    condition     = contains(["Development", "Staging", "Production"], var.env)
    error_message = "Environment must be Development, Staging, or Production."
  }
}

variable "app_code" {
  description = "Application prefix identifier code"
  type        = string
  default     = "pfm"
}

variable "ignore_project" {
  description = "Set to true to ignore project resource assignment and avoid circular dependency with cloud module"
  type        = bool
  default     = false
}
variable "database_name" {
  description = "Name of the PostgreSQL database"
  type        = string
  default     = "PoultryFarmManager"
}

variable "database_version" {
  description = "PostgreSQL version"
  type        = string
  default     = "17"
}

variable "maintenance_mode" {
  description = "Enable maintenance mode to stop app instances (saves costs)"
  type        = bool
  default     = false
}

variable "domain_name" {
  description = "Root domain name to use as custom domain on DigitalOcean App Platform (e.g. example.com). Leave empty to skip custom domain setup."
  type        = string
  default     = ""
}

variable "subdomain" {
  description = "Subdomain prefix (e.g. 'pollos' results in pollos.example.com). Leave empty to use the apex domain. Only used when domain_name is set."
  type        = string
  default     = ""
  validation {
    condition     = var.subdomain == "" || !startswith(var.subdomain, ".") && !endswith(var.subdomain, ".")
    error_message = "subdomain must not start or end with a dot."
  }
}
