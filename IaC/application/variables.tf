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
  default     = "poultryfarmmanager/hybrid"
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

variable "instance_size_slug" {
  description = "Size of the application instance"
  type        = string
  default     = "basic-xxs"
  # Options: basic-xxs, basic-xs, basic-s, professional-xxs, professional-xs, professional-s
  # https://www.digitalocean.com/community/questions/app-platform-instance_size_slug-options
}
