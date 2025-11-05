terraform {
  backend "s3" {
    bucket       = "epam-marathon-bucket"
    key          = "terraform.tfstate"
    region       = "eu-central-1"
    use_lockfile = true
    encrypt      = true
  }
}
