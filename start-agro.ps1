param (
    [Parameter(Mandatory=$true)]
    [string]$GithubToken
)

Write-Host "--- 🌾 AgroSolutions: Iniciando Setup do Cluster ---" -ForegroundColor Cyan

# 1. Reset e Namespace
kubectl apply -f k8s/namespace.yaml

# 2. Autenticação GHCR
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

# 4. Aguardar Postgres (Saúde do cluster)
Write-Host "Aguardando PostgreSQL aceitar conexões..." -ForegroundColor Yellow
while ($true) {
    $log = kubectl logs -l app=postgres -n agrosolutions 2>$null
    if ($log -match "database system is ready to accept connections") { break }
    Start-Sleep -Seconds 3
}

# 5. Microsserviços
Write-Host "Deploy de Microsserviços..." -ForegroundColor Green
kubectl apply -f k8s/identity-service/
kubectl apply -f k8s/property-service/
kubectl apply -f k8s/ingestion-service/
kubectl apply -f k8s/alert-service/

# Forçar reinício para garantir consistência
kubectl rollout restart deployment -n agrosolutions identity-service property-service ingestion-service alert-service

Write-Host "`n--- ✅ Setup Concluído! ---" -ForegroundColor Green
Write-Host "Abra novos terminais e execute os túneis:" -ForegroundColor Cyan
Write-Host "Porta 5001: kubectl port-forward svc/identity-service 5001:80 -n agrosolutions"
Write-Host "Porta 5002: kubectl port-forward svc/property-service 5002:80 -n agrosolutions"
Write-Host "Porta 5003: kubectl port-forward svc/ingestion-service 5003:80 -n agrosolutions"