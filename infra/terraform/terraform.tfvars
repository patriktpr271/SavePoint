# Fill in lab_role_arn before running `terraform plan`.
# How to find it:
#   1. Open the AWS Console from the Learner Lab portal.
#   2. Go to IAM > Roles, search for "LabRole".
#   3. Copy the Role ARN. It looks like: arn:aws:iam::123456789012:role/LabRole
lab_role_arn = "arn:aws:iam::380076695912:role/LabRole"

# Defaults below are Learner-Lab-friendly; only change if you know why.
aws_region             = "us-east-1"
project_name           = "savepoint"
environment            = "dev"
kubernetes_version     = "1.29"
eks_node_instance_type = "t3.medium"
eks_desired_nodes      = 2
eks_min_nodes          = 1
eks_max_nodes          = 3
