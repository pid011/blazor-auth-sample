# blazor-auth-sample

Blazor 기본 샘플을 활용하여 쿠버네티스 배포를 해보는 프로젝트입니다.

## 기술 스택

이 프로젝트는 **.NET Aspire**를 기반으로 하며, 다음 기술들을 사용합니다:

- **ASP.NET Core** (.NET 10.0): 웹 애플리케이션 프레임워크
- **Blazor**: 인터랙티브 웹 UI 구현
- **PostgreSQL**: 주 데이터베이스 (Aspire.Hosting.PostgreSQL)
- **Entity Framework Core**: ORM 및 데이터베이스 마이그레이션 (Npgsql EF Core Provider)
- **ASP.NET Core Identity**: 사용자 인증 및 권한 관리
- **.NET Aspire**: 클라우드 네이티브 앱 오케스트레이션 및 서비스 디스커버리

### 주요 프로젝트 구성

- **BlazorAuthSample.AppHost**: Aspire AppHost - 애플리케이션 오케스트레이션 및 PostgreSQL 컨테이너 관리
- **BlazorAuthSample**: Blazor Server 앱 - 메인 웹 애플리케이션 (Identity 인증 포함)
- **BlazorAuthSample.ServiceDefaults**: Aspire 서비스 기본 설정 및 공통 확장 메서드

## 프로젝트 구조

간단한 폴더 구조는 아래와 같습니다:

- `infra/`: 쿠버네티스 매니페스트, Helm 차트 또는 인프라 관련 메타데이터를 보관합니다. 클러스터 배포 및 환경별 설정이 이 디렉터리에 위치합니다.
- `src/`: 실제 서버/애플리케이션 소스 코드가 들어있습니다. Blazor 앱, 호스트, 서비스 설정 등이 포함됩니다.
