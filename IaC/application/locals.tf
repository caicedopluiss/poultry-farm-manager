locals {
  env_map = {
    Development = "dev"
    Staging     = "stg"
    Production  = "prod"
  }
  app_prefix = "${var.app_code}${var.env == "Production" ? "" : "-${local.env_map[var.env]}"}"
}
