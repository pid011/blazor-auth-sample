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

## 빠른 시작 가이드

이 프로젝트를 처음부터 배포하려면 다음 순서대로 진행하세요.

### 1. 리포지토리 클론

```powershell
# PowerShell

git clone https://github.com/pid011/blazor-auth-sample.git
cd blazor-auth-sample
```

```bash
# Bash

git clone https://github.com/pid011/blazor-auth-sample.git
cd blazor-auth-sample
```

### 2. 데이터베이스 준비

PostgreSQL 데이터베이스를 준비합니다. 로컬 PostgreSQL, 클라우드 매니지드 서비스(Aiven, Supabase 등) 모두 사용 가능합니다.

### 3. EF Bundle 다운로드 및 마이그레이션 적용

릴리즈에 포함된 EF Core 번들(efbundle)을 사용해 데이터베이스에 마이그레이션을 적용합니다. 번들은 self-contained 실행 파일로, .NET SDK 없이도 실행 가능합니다.

릴리즈 페이지: [https://github.com/pid011/blazor-auth-sample/releases](https://github.com/pid011/blazor-auth-sample/releases)

#### 3-1) efbundle 다운로드

**GitHub CLI 사용** (권장)

[GitHub CLI](https://cli.github.com/)가 설치되어 있어야 합니다. 설치되지 않은 경우 위 링크에서 설치하거나, 아래 웹페이지 방법을 사용하세요.

```powershell
# PowerShell (Windows)

# 최신 릴리즈에서 Windows용 번들만 다운로드
gh release download --pattern "efbundle-win-x64.exe"
```

```bash
# Bash (Linux)

# 최신 릴리즈에서 Linux용 번들만 다운로드
gh release download --pattern "efbundle-linux-x64"

chmod +x efbundle-linux-x64
```

##### 웹페이지에서 다운로드

GitHub CLI가 없는 경우 [릴리즈 페이지](https://github.com/pid011/blazor-auth-sample/releases)에서 직접 다운로드할 수 있습니다:

1. 최신 릴리즈로 이동
2. Assets 섹션에서 `efbundle-win-x64.exe` (Windows) 또는 `efbundle-linux-x64` (Linux) 다운로드
3. 다운로드한 파일을 프로젝트 폴더에 저장

#### 3-2) 연결 문자열로 마이그레이션 적용

PostgreSQL(Npgsql) 예시를 기준으로 합니다. 환경에 맞게 값을 바꿔주세요.

```powershell
# PowerShell (Windows)

$CS = "Host=<호스트>;Port=5432;Username=<유저>;Password=<비밀번호>;Database=<DB>;"

./efbundle-win-x64.exe --connection "$CS"
```

```bash
# Bash (Linux)

CS="Host=<호스트>;Port=5432;Username=<유저>;Password=<비밀번호>;Database=<DB>;"

./efbundle-linux-x64 --connection "$CS"
```

성공 시 적용된 마이그레이션 목록이 출력됩니다.

### 4. Kubernetes 사전 준비

1. Kubernetes 클러스터 v1.19 이상
   - 로컬 개발: [Minikube](https://minikube.sigs.k8s.io/), [Docker Desktop Kubernetes](https://docs.docker.com/desktop/kubernetes/), [kind](https://kind.sigs.k8s.io/) 등 사용 가능
   - 클라우드: GKE, EKS, AKS 등
2. `kubectl` CLI 설치
3. Nginx Ingress Controller 설치 (아래 명령 참고)

```powershell
# PowerShell

kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.14.0/deploy/static/provider/cloud/deploy.yaml
```

```bash
# Bash

kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.14.0/deploy/static/provider/cloud/deploy.yaml
```

### 5. Kubernetes Secret 생성

데이터베이스 연결 문자열을 Secret으로 생성합니다. 3-2단계에서 사용한 것과 동일한 연결 문자열을 사용하세요.

```powershell
# PowerShell

$connectionString = "Host=<호스트>;Port=<포트>;Username=<유저네임>;Password=<패스워드>;Database=<데이터베이스>;"

kubectl delete secret blazor-auth-sample-secret -n default --ignore-not-found

kubectl create secret generic blazor-auth-sample-secret `
  --from-literal=connection-string=$connectionString
```

```bash
# Bash

connectionString="Host=<호스트>;Port=<포트>;Username=<유저네임>;Password=<패스워드>;Database=<데이터베이스>;"

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

### 6. ConfigMap 설정 (선택사항)

`infra/k8s/configmap.yaml`에서 `aspnetcore-environment` 값을 원하는 환경으로 설정할 수 있습니다. 기본값은 `Production`입니다.

### 7. Kubernetes 리소스 배포

`infra/k8s` 디렉터리에는 `configmap`, `deployment`, `service`, `ingress`가 정의되어 있습니다. Kustomize로 한 번에 적용할 수 있습니다.

```powershell
# PowerShell

kubectl apply -k infra/k8s
```

```bash
# Bash

kubectl apply -k infra/k8s
```

### 8. 배포 상태 확인

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

### 9. 애플리케이션 접속

Ingress에 설정된 도메인 또는 IP 주소로 애플리케이션에 접속할 수 있습니다. `kubectl get ingress blazor-auth-sample` 명령어로 Ingress 정보를 확인하세요.

## 고급 관리

### 특정 리소스 재적용

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

### 스케일링 및 업데이트

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
kubectl rollout status deployment/blazor-auth-sample

# 필요 시 롤백
kubectl rollout undo deployment/blazor-auth-sample
```

### Pod 재시작

동일한 이미지 태그(예: `latest`)로 새 버전을 배포한 경우 또는 ConfigMap/Secret 변경 후 Pod를 재시작해야 할 때 사용합니다. imagePullPolicy가 `Always`로 설정되어 있어서 항상 이미지를 새로 받습니다.

```powershell
# PowerShell

# Deployment 재시작 (모든 Pod 순차적으로 재시작)
kubectl rollout restart deployment/blazor-auth-sample

# 재시작 상태 확인
kubectl rollout status deployment/blazor-auth-sample
```

```bash
# Bash

# Deployment 재시작 (모든 Pod 순차적으로 재시작)
kubectl rollout restart deployment/blazor-auth-sample

# 재시작 상태 확인
kubectl rollout status deployment/blazor-auth-sample
```

### 리소스 삭제

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

## 추가 설정

### TLS/SSL 구성

Let's Encrypt 기반 TLS를 구성하려면 `cert-manager`를 설치한 뒤 `ingress.yaml`의 TLS 섹션을 활성화하고 인증서 Secret을 참조하도록 설정합니다.

```powershell
# PowerShell

kubectl apply -f https://github.com/cert-manager/cert-manager/releases/download/v1.19.1/cert-manager.yaml
```

```bash
# Bash

kubectl apply -f https://github.com/cert-manager/cert-manager/releases/download/v1.19.1/cert-manager.yaml
```

## 트러블슈팅

다음 명령어를 사용하여 배포 문제를 진단할 수 있습니다.

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
