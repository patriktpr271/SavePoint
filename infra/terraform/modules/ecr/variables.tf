variable "project_name" {
  type = string
}

variable "repositories" {
  description = "Repository names (one per service)"
  type        = list(string)
}
