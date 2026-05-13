# Deploy SavePoint to AWS (Learner Lab)

This guide walks you through deploying the full SavePoint stack (backend, reviews, lookup, frontend, SQL Server) to AWS using Terraform + EKS + ECR + ALB. It is written for someone who has never deployed to AWS before.

You are running on **Windows + PowerShell** but the deploy scripts are bash (`.sh`). The easiest way to run them on Windows is **Git Bash** (it ships with Git for Windows). All commands below assume you opened a **Git Bash** terminal at the repository root.

---

## 1. What each tool does (one sentence each)

| Tool | Purpose |
|---|---|
| **Terraform** | Reads `.tf` files and creates the matching AWS resources (VPC, EKS, ECR, etc.). |
| **ECR** (Elastic Container Registry) | AWS-hosted Docker registry; we push our images here so EKS can pull them. |
| **EKS** (Elastic Kubernetes Service) | Managed Kubernetes cluster — AWS runs the control plane, you run the worker nodes (EC2). |
| **ALB** (Application Load Balancer) | Public HTTP entry point in front of the cluster; routes `/api`, `/hangfire`, `/` to the right Kubernetes service. |
| **EBS CSI driver** | Lets PersistentVolumeClaims in Kubernetes be backed by EBS disks (used by SQL Server). |

---

## 2. Get AWS Learner Lab credentials

Credentials expire every few hours. You **must** refresh them at the start of each session.

1. Open the Learner Lab in your browser (Canvas / Vocareum).
2. Click **Start Lab**. Wait for the dot next to "AWS" to turn green.
3. Click **AWS Details** (top of the lab).
4. Next to **AWS CLI**, click **Show**. You will see three lines that look like:

   ```
   aws_access_key_id=ASIA....
   aws_secret_access_key=....
   aws_session_token=....
   ```

5. In your **Git Bash** terminal, run (replace the values with what the portal showed you):

   ```bash
   export AWS_ACCESS_KEY_ID="ASIA..."
   export AWS_SECRET_ACCESS_KEY="..."
   export AWS_SESSION_TOKEN="..."
   export AWS_REGION="us-east-1"
   ```

6. Verify:

   ```bash
   aws sts get-caller-identity
   ```

   You should see a JSON response with `"Account"` and `"Arn"`. If you see an error, the credentials are wrong or expired — go back to step 4.

> **Common student mistake:** opening a new terminal window. Environment variables only live in the terminal you set them in. If you open a new Git Bash window, you must re-export them.

---

## 3. Find your LabRole ARN

The lab restricts creating IAM roles, but it provides one called **LabRole** that already has the permissions EKS needs. You must tell Terraform its ARN.

1. In the AWS Console (opened from the Learner Lab), go to **IAM** → **Roles**.
2. In the search box type `LabRole`.
3. Click on the role.
4. At the top of the page copy the **ARN**. It looks like:

   ```
   arn:aws:iam::123456789012:role/LabRole
   ```

5. Open `infra/terraform/terraform.tfvars` and paste the ARN into `lab_role_arn`.

---

## 4. Find your AWS account ID

You can read it three ways — pick whichever is easiest:

- Top-right corner of the AWS Console (the 12-digit number).
- Run: `aws sts get-caller-identity --query Account --output text`
- It's the number inside the LabRole ARN above (`123456789012` in the example).

You do **not** need to put this anywhere — the deploy script reads it automatically.

---

## 5. Prerequisites checklist

Open Git Bash and run each command. If any errors, install that tool.

| Tool | Check |
|---|---|
| AWS CLI | `aws --version` |
| Terraform | `terraform -version` (need 1.5+) |
| Docker | `docker version` (Docker Desktop must be running) |
| kubectl | `kubectl version --client` |
| Helm | `helm version` |
| Kustomize | `kustomize version` (standalone — not the one bundled in kubectl) |
| Python 3 | `python --version` (used by the deploy script to parse JSON) |

Install pointers (Windows):
- Docker Desktop: <https://www.docker.com/products/docker-desktop>
- kubectl: `winget install -e --id Kubernetes.kubectl`
- helm: `winget install -e --id Helm.Helm`
- kustomize: `winget install -e --id Kubernetes.kustomize`

---

## 6. First-time backend setup (one-time per AWS account)

Terraform stores its state in an S3 bucket so it can survive across sessions. You must create this bucket **once**, manually, before running Terraform.

Pick a globally-unique bucket name. A good pattern:

```
savepoint-tfstate-<your-name>-<random-suffix>
```

In Git Bash, run (replace `BUCKET_NAME` with the name you chose):

```bash
BUCKET_NAME="savepoint-tfstate-yourname-abc123"

aws s3api create-bucket \
  --bucket "${BUCKET_NAME}" \
  --region us-east-1

aws s3api put-bucket-versioning \
  --bucket "${BUCKET_NAME}" \
  --versioning-configuration Status=Enabled
```

> **Bucket name rules:** lowercase letters/digits/hyphens only, no underscores, must be globally unique. If you get `BucketAlreadyExists`, pick a different suffix.

Now initialise Terraform, telling it which bucket to use:

```bash
cd infra/terraform

terraform init \
  -backend-config="bucket=${BUCKET_NAME}" \
  -backend-config="key=savepoint/dev/terraform.tfstate" \
  -backend-config="region=us-east-1"
```

You only need to run this `terraform init` again if:
- You wipe the `infra/terraform/.terraform` folder.
- You change the backend bucket.

---

## 7. Deploy — copy-paste commands in order

Assume you have:
1. Refreshed Learner Lab credentials (section 2).
2. Put your LabRole ARN into `infra/terraform/terraform.tfvars` (section 3).
3. Created the S3 backend bucket and run `terraform init` (section 6).

Then, from the repository root, in **Git Bash**:

```bash
# 1. Optional sanity check — see what Terraform will create
cd infra/terraform
terraform plan
cd ../..

# 2. Run the deploy script. This does: terraform apply, docker build/push to ECR,
#    kubectl apply, and waits for everything to be ready. Takes 15–25 minutes
#    on a first run; about 5 minutes on subsequent runs.
bash scripts/deploy-aws.sh
```

When the script finishes it prints something like:

```
ALB DNS:  k8s-savepoint-...elb.us-east-1.amazonaws.com
URL:      http://k8s-savepoint-...elb.us-east-1.amazonaws.com/
```

Open that URL in your browser. Wait up to 90 seconds after the script finishes — DNS for a freshly-created ALB takes a moment.

> **First-run gotcha:** the EKS control plane alone takes ~12 min to create. Don't panic if `terraform apply` looks stuck on `aws_eks_cluster.main: Still creating...`.

### Verify after deploy

```bash
kubectl get nodes
kubectl -n savepoint get pods
kubectl -n savepoint get svc
kubectl -n savepoint get ingress
aws ecr describe-repositories --region us-east-1
```

You should see 2 nodes, 5 running pods, the `savepoint-ingress` with a hostname, and 4 ECR repositories.

### Dry-run the manifests (without applying)

```bash
kubectl apply -k infra/k8s-aws/ --dry-run=client -o yaml | less
# or, for server-side validation (requires kubeconfig already set):
kubectl apply -k infra/k8s-aws/ --dry-run=server
```

---

## 8. How to access the application

1. After `bash scripts/deploy-aws.sh` completes, look for the **ALB DNS** line in the script output.
2. If you closed the terminal, re-read it any time with:

   ```bash
   kubectl -n savepoint get ingress savepoint-ingress \
     -o jsonpath='{.status.loadBalancer.ingress[0].hostname}{"\n"}'
   ```

3. Paste `http://<that-hostname>/` into your browser.

Routes:
- `/` → frontend
- `/api/...` → backend
- `/api/review/...` → reviews service
- `/api/lookup/...` → lookup service
- `/hangfire` → backend Hangfire UI

---

## 9. Common errors and how to fix each

### a. `ExpiredToken: The provided token has expired`
Your Learner Lab session expired. Go back to **section 2**, re-export the three credential variables, re-run the command.

### b. `AccessDenied: ... is not authorized to perform: iam:CreateRole`
Some part of Terraform tried to create an IAM role. In Learner Lab that is blocked. Make sure your `terraform.tfvars` `lab_role_arn` is set correctly — every IAM role this stack uses is the LabRole. If you customized the code and added an `aws_iam_role` block, remove it.

### c. `docker push` returns `denied` or `no basic auth credentials`
The Docker → ECR login expired. The deploy script logs in for you, but if you ran `docker push` by hand, run:

```bash
aws ecr get-login-password --region "${AWS_REGION}" \
  | docker login --username AWS --password-stdin \
    "${AWS_ACCOUNT_ID}.dkr.ecr.${AWS_REGION}.amazonaws.com"
```

### d. `Error from server (Forbidden) ... User "..." cannot list resource ...`
You ran `aws eks update-kubeconfig` from a different IAM principal than the one that created the cluster. Re-export the **same** Learner Lab credentials you used during `terraform apply`, then re-run:

```bash
aws eks update-kubeconfig --region us-east-1 --name savepoint-dev
```

### e. Pods stuck `Pending` with event `0/2 nodes are available: ... pod has unbound immediate PersistentVolumeClaim`
The EBS CSI driver isn't ready or the `ebs-gp3` StorageClass is missing. Check:

```bash
kubectl -n kube-system get pods | grep ebs
kubectl get storageclass
```

`ebs-gp3` should show `(default)`. If not, re-run `terraform apply` (the `kubernetes_storage_class_v1` resource will be re-created).

### f. ALB ingress shows no hostname after 5 minutes
Look at the load balancer controller logs:

```bash
kubectl -n kube-system logs deployment/aws-load-balancer-controller --tail=100
```

Common cause: subnet tags missing. The networking module sets them automatically — if you edited it, ensure public subnets have `kubernetes.io/role/elb=1` and the `kubernetes.io/cluster/<cluster-name>=shared` tag.

### g. `terraform apply` says `Error: configuring Terraform AWS Provider: no valid credential sources`
You forgot the three `export AWS_...` lines in this terminal. See section 2.

### h. `terraform destroy` hangs on VPC or subnet
The ALB Controller didn't clean up its load balancer in time. Run:

```bash
aws elbv2 describe-load-balancers --region us-east-1
# Find any with "k8s-savepoint" in the name and delete:
aws elbv2 delete-load-balancer --load-balancer-arn <arn>
```

Then re-run `terraform destroy`.

---

## 10. Tear down everything

```bash
bash scripts/teardown-aws.sh
# Type "destroy" when prompted.
```

This deletes:
- All Kubernetes resources (including the SQL Server volume — data is lost).
- The EKS cluster, node group, EC2 instances.
- VPC, subnets, NAT gateway, IGW, EIP.
- All four ECR repositories and every image pushed to them.

It does **not** delete:
- The S3 backend bucket (delete it manually if you don't need it anymore).
- The LabRole (you don't own it; the lab does).

---

## 11. Resuming work after a session restart

Learner Lab sessions expire. To resume:

1. Re-start the lab in the portal and re-export the three credentials (section 2).
2. Re-point kubectl: `aws eks update-kubeconfig --region us-east-1 --name savepoint-dev`
3. Verify: `kubectl get nodes`

The infrastructure stays up across sessions as long as you don't run `terraform destroy`. Learner Lab leaves resources running, **but** the lab account has a budget limit — keep an eye on the budget bar in the lab portal. NAT Gateway and EKS each cost about ~\$0.10/hr.

> **Save money tip:** when not actively demoing, scale the node group to 0 to avoid EC2 costs:
>
> ```bash
> aws eks update-nodegroup-config --cluster-name savepoint-dev \
>   --nodegroup-name savepoint-dev-ng \
>   --scaling-config minSize=0,maxSize=3,desiredSize=0 \
>   --region us-east-1
> ```
>
> Scale back to 2 before re-deploying. (You still pay for the EKS control plane and NAT Gateway, but no EC2.)
