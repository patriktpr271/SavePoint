variable "cluster_name" {
  type = string
}

variable "kubernetes_version" {
  type = string
}

variable "lab_role_arn" {
  description = "LabRole ARN, used for BOTH the EKS cluster role and the node group role"
  type        = string
}

variable "subnet_ids" {
  description = "All subnets the EKS control plane may use (public + private)"
  type        = list(string)
}

variable "node_subnet_ids" {
  description = "Subnets where worker nodes are placed (private)"
  type        = list(string)
}

variable "node_instance_type" {
  type = string
}

variable "desired_size" {
  type = number
}

variable "min_size" {
  type = number
}

variable "max_size" {
  type = number
}
