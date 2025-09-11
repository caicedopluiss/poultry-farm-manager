resource "digitalocean_project" "pfm" {
  name        = "Poultry Farm Manager"
  description = "Poultry Farm Manager Project"
  purpose     = "Web Application Hosting"
  is_default  = false
}
