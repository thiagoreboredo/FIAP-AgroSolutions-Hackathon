param (
    [Parameter(Mandatory=$true)]
    [string]$GithubToken
)

Write-Host "--- AgroSolutions: Iniciando Setup do Cluster ---" -ForegroundColor Cyan

# 1. Reset e Namespace
kubectl apply -f k8s/namespace.yaml

# 2. Autenticacao GHCR
Write-Host "Configurando acesso ao GitHub Container Registry..."
kubectl delete secret ghcr-secret -n agrosolutions --ignore-not-found
kubectl create secret docker-registry ghcr-secret `
  --docker-server=ghcr.io `
  --docker-username=thiagoreboredo `
  --docker-password=$GithubToken `
  --docker-email=thiagoreboredo@gmail.com `
  -n agrosolutions

kubectl patch serviceaccount default -p '{\"imagePullSecrets\": [{\"name\": \"ghcr-secret\"}]}' -n agrosolutions

# 3. Infraestrutura (Postgres e RabbitMQ)
Write-Host "Subindo Postgres e RabbitMQ..."
kubectl apply -f k8s/postgres/
kubectl apply -f k8s/rabbitmq/

# 4. Aguardar Postgres (Saude do cluster)
Write-Host "Aguardando PostgreSQL aceitar conexões..." -ForegroundColor Yellow
while ($true) {
    $log = kubectl logs -l app=postgres -n agrosolutions 2>$null
    if ($log -match "database system is ready to accept connections") { break }
    Start-Sleep -Seconds 3
}

# 5. Ingress Controller (NGINX)
Write-Host "Instalando NGINX Ingress Controller..."
kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.14.3/deploy/static/provider/cloud/deploy.yaml

# 6. Aguardar Ingress Controller (Saude do NGINX)
Write-Host "Aguardando o NGINX Ingress ficar pronto (isso pode levar 1 minuto)..."
Start-Sleep -Seconds 45 # Dá um tempo para o pod ser criado antes de dar o wait
kubectl wait --namespace ingress-nginx --for=condition=ready pod --selector=app.kubernetes.io/component=controller --timeout=120s

# 7. Microsserviços
Write-Host "Deploy de Microsservicos..." -ForegroundColor Green
kubectl apply -f k8s/identity-service/
kubectl apply -f k8s/property-service/
kubectl apply -f k8s/ingestion-service/
kubectl apply -f k8s/alert-service/
kubectl apply -f k8s/ingress.yaml

# Forçar reinício para garantir consistência
kubectl rollout restart deployment -n agrosolutions identity-service property-service ingestion-service alert-service

Write-Host "`n--- Setup Concluido! ---" -ForegroundColor Green
