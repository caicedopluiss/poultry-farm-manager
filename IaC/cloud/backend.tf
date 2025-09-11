terraform {
  required_version = ">= 1.13"

  # HCP Terraform Cloud Backend
  cloud {
    organization = "caicedopluiss"

    workspaces {
      name = "pfm-cloud"
    }
  }
}
