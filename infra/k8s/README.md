# SavePoint local Kubernetes manifests

## What these files are
- `namespace.yaml`: creates the `savepoint` namespace.
- `secret.yaml`: local development secrets (SQL SA password and backend connection string).
- `db.yaml`: SQL Server StatefulSet + Service.
- `backend.yaml`: backend Deployment + Service.
- `frontend.yaml`: frontend Deployment + Service.
- `ingress.yaml`: host routing for frontend, API, and Hangfire.
- `kustomization.yaml`: lets you deploy everything with one command.

## Prerequisites
- Docker Desktop running.
- `minikube` and `kubectl` available in PATH.

## Build images (from repository root)
```powershell
docker build -t savepoint-backend:phase2 .\SavePointBackend
docker build -t savepoint-frontend:phase2 --build-arg VITE_API_BASE_URL=http://savepoint.local .\SavePointFrontend
```

## Start minikube and load images
```powershell
minikube start --driver=docker
minikube addons enable ingress
minikube image load savepoint-backend:phase2
minikube image load savepoint-frontend:phase2
```

## Deploy manifests
```powershell
kubectl apply -k .\infra\k8s
kubectl get pods -n savepoint
kubectl get svc -n savepoint
kubectl get ingress -n savepoint
```

## Expose ingress host on Windows
1. Run `minikube tunnel` in a dedicated terminal.
2. Edit hosts file as Administrator: `C:\Windows\System32\drivers\etc\hosts`
3. Add line:
```text
127.0.0.1 savepoint.local
```

## Test URLs
- `http://savepoint.local`
- `http://savepoint.local/hangfire`
- `http://savepoint.local/api/auth/me`

## Reset only Kubernetes resources (keeps minikube)
```powershell
kubectl delete -k .\infra\k8s
```

## Full minikube reset
```powershell
minikube delete
```
