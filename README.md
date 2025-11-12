# blazor-auth-sample

Blazor Server 샘플 애플리케이션을 .NET Aspire 기반으로 구성하고, Kubernetes에 배포하는 과정을 실습하는 프로젝트입니다.

## 기술 스택

- **ASP.NET Core** (.NET 10.0)
- **Blazor Server** + **ASP.NET Core Identity**
- **Entity Framework Core** (Npgsql Provider)
- **PostgreSQL** (Aspire.Hosting.PostgreSQL)
- **.NET Aspire** (AppHost, ServiceDefaults)
- **Kubernetes** + **Nginx Ingress Controller**

## 솔루션 구성

- **BlazorAuthSample**: Blazor Server 앱과 Identity UI가 포함된 웹 애플리케이션
- **BlazorAuthSample.AppHost**: Aspire AppHost. 개발 환경에서 PostgreSQL 컨테이너와 앱 오케스트레이션을 담당
- **BlazorAuthSample.ServiceDefaults**: 공통 서비스 확장 및 기본 구성
- **infra/k8s**: 프로덕션 환경 배포를 위한 Kubernetes 매니페스트 모음

## 폴더 구조

- `src/` — 애플리케이션 코드 (Blazor 앱, AppHost, ServiceDefaults)
- `infra/` — Kubernetes 매니페스트와 인프라 관련 자료

## Kubernetes 배포 가이드

### 사전 준비

1. Kubernetes 클러스터 v1.19 이상
2. `kubectl` CLI
3. Nginx Ingress Controller (아래 명령 참고)

```powershell
# PowerShell

kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.9.5/deploy/static/provider/cloud/deploy.yaml
```

```bash
# Bash

kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.9.5/deploy/static/provider/cloud/deploy.yaml
```

### 1. 환경 변수와 Secret 설정

- `infra/k8s/configmap.yaml`에서 `aspnetcore-environment` 값을 원하는 환경으로 설정합니다.
- 데이터베이스 연결 문자열은 Secret으로 관리합니다.

```powershell
# PowerShell

$connectionString = "Host=<DB URL>;Port=5432;Username=postgres;Password=<패스워드>;Database=postgres;"
kubectl delete secret blazor-auth-sample-secret -n default --ignore-not-found
kubectl create secret generic blazor-auth-sample-secret `
  --from-literal=connection-string=$connectionString
```

```bash
# Bash

connectionString="Host=<DB URL>;Port=5432;Username=postgres;Password=<패스워드>;Database=postgres;"
kubectl delete secret blazor-auth-sample-secret -n default --ignore-not-found
kubectl create secret generic blazor-auth-sample-secret \
  --from-literal="connection-string=${connectionString}"
```

Secret 값을 확인하려면 다음을 사용하세요.

```powershell
# PowerShell

kubectl get secret blazor-auth-sample-secret -o jsonpath='{.data.connection-string}' | ForEach-Object {
  [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($_))
}
```

```bash
# Bash

kubectl get secret blazor-auth-sample-secret -o jsonpath='{.data.connection-string}' | base64 --decode; echo
```

### 2. 매니페스트 적용

`infra/k8s` 디렉터리에는 `configmap`, `deployment`, `service`, `ingress`가 정의되어 있습니다. Kustomize로 한 번에 적용할 수 있습니다.

```powershell
# PowerShell

kubectl apply -k infra/k8s
```

```bash
# Bash

kubectl apply -k infra/k8s
```

특정 리소스를 다시 적용하려면 `-f` 옵션과 파일 경로를 사용합니다.

```powershell
# PowerShell

kubectl apply -f infra/k8s/configmap.yaml
kubectl apply -f infra/k8s/deployment.yaml
kubectl apply -f infra/k8s/service.yaml
kubectl apply -f infra/k8s/ingress.yaml
```

```bash
# Bash

kubectl apply -f infra/k8s/configmap.yaml
kubectl apply -f infra/k8s/deployment.yaml
kubectl apply -f infra/k8s/service.yaml
kubectl apply -f infra/k8s/ingress.yaml
```

### 3. 배포 상태 확인

```powershell
# PowerShell

# Pod 상태
kubectl get pods -l app=blazor-auth-sample

# Service 정보
kubectl get svc blazor-auth-sample

# Ingress 경로
kubectl get ingress blazor-auth-sample

# 로그 스트리밍
$pod = kubectl get pods -l app=blazor-auth-sample -o jsonpath='{.items[0].metadata.name}'
kubectl logs -f $pod --tail=100 --timestamps
```

```bash
# Bash

# Pod 상태
kubectl get pods -l app=blazor-auth-sample

# Service 정보
kubectl get svc blazor-auth-sample

# Ingress 경로
kubectl get ingress blazor-auth-sample

# 로그 스트리밍
pod=$(kubectl get pods -l app=blazor-auth-sample -o jsonpath='{.items[0].metadata.name}')
kubectl logs -f "${pod}" --tail=100 --timestamps
```

### 스케일링과 롤아웃

```powershell
# PowerShell

# 수평 확장
kubectl scale deployment blazor-auth-sample --replicas=3

# 새 이미지로 업데이트
kubectl set image deployment/blazor-auth-sample `
  blazor-auth-sample=ghcr.io/pid011/blazor-auth-sample:v2.0

# 배포 상태 확인
kubectl rollout status deployment/blazor-auth-sample

# 필요 시 롤백
kubectl rollout undo deployment/blazor-auth-sample
```

```bash
# Bash

# 수평 확장
kubectl scale deployment blazor-auth-sample --replicas=3

# 새 이미지로 업데이트
kubectl set image deployment/blazor-auth-sample \
  blazor-auth-sample=ghcr.io/pid011/blazor-auth-sample:v2.0

# 배포 상태 확인
kubectl rollout status deployment-blazor-auth-sample

# 필요 시 롤백
kubectl rollout undo deployment-blazor-auth-sample
```

### 삭제

```powershell
# PowerShell

kubectl delete -k infra/k8s
kubectl delete secret blazor-auth-sample-secret --ignore-not-found
```

```bash
# Bash

kubectl delete -k infra/k8s
kubectl delete secret blazor-auth-sample-secret --ignore-not-found
```

개별 리소스를 삭제하려면 `kubectl delete -f <파일>`을 사용할 수 있습니다.

### TLS/SSL

Let's Encrypt 기반 TLS를 구성하려면 `cert-manager`를 설치한 뒤 `ingress.yaml`의 TLS 섹션을 활성화하고 인증서 Secret을 참조하도록 설정합니다.

```powershell
# PowerShell

kubectl apply -f https://github.com/cert-manager/cert-manager/releases/download/v1.13.2/cert-manager.yaml
```

```bash
# Bash

kubectl apply -f https://github.com/cert-manager/cert-manager/releases/download/v1.13.2/cert-manager.yaml
```

### 트러블슈팅 체크리스트

```powershell
# PowerShell

# Pod 이벤트 및 로그 확인
kubectl describe pod -l app=blazor-auth-sample
kubectl logs -l app=blazor-auth-sample

# Ingress 문제 확인
kubectl describe ingress blazor-auth-sample
kubectl logs -n ingress-nginx -l app.kubernetes.io/name=ingress-nginx

# Secret에 저장된 연결 문자열 확인
kubectl get secret blazor-auth-sample-secret -o jsonpath='{.data.connection-string}' | ForEach-Object {
    [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($_))
}
```

```bash
# Bash

# Pod 이벤트 및 로그 확인
kubectl describe pod -l app=blazor-auth-sample
kubectl logs -l app=blazor-auth-sample

# Ingress 문제 확인
kubectl describe ingress blazor-auth-sample
kubectl logs -n ingress-nginx -l app.kubernetes.io/name=ingress-nginx

# Secret에 저장된 연결 문자열 확인
kubectl get secret blazor-auth-sample-secret -o jsonpath='{.data.connection-string}' | base64 --decode; echo
```
