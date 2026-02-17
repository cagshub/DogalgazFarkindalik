# CLAUDE.md — Doğalgaz Farkındalık Eğitim ve Değerlendirme Platformu

## 🎯 Proje Özeti

Bu proje, kullanıcıların doğalgaz güvenliği ve verimli kullanımına yönelik farkındalığını artırmak amacıyla geliştirilecek bir **eğitim ve değerlendirme platformu**dur. Eğitim videoları, interaktif simülasyonlar ve dinamik anketlerle kullanıcıların bilgi seviyesi ölçülür ve raporlanır.

Platform, **yaş grubu** (4–12, 13–65, 65+) ve **abonelik tipi** (Bireysel, Merkezi, Endüstriyel) bazında kişiselleştirilmiş içerikler sunar.

**Önemli:** Bu proje bir mülakat ödevi olarak hazırlanıyor. Kod kalitesi, mimari tutarlılık ve çalışır demo çok önemli.

---

## 🏗️ Mimari: Clean Architecture

Proje **Clean Architecture** prensiplerine uygun olarak 5 katmanlı yapıdadır. Bağımlılıklar daima dıştan içe doğru akar.

```
┌─────────────────────────────────────────────────────┐
│                    API + Web (UI)                    │  ← Sunum katmanı
├─────────────────────────────────────────────────────┤
│                   Infrastructure                     │  ← Teknik detaylar (EF Core, JWT, vb.)
├─────────────────────────────────────────────────────┤
│                    Application                       │  ← İş kuralları arayüzleri, DTO'lar
├─────────────────────────────────────────────────────┤
│                      Domain                          │  ← Entity'ler, Enum'lar (saf C#, bağımlılık yok)
└─────────────────────────────────────────────────────┘
```

### Solution Yapısı

```
DogalgazFarkindalik.sln
├── src/
│   ├── DogalgazFarkindalik.Domain/           # Entity, Enum, Interface (saf C#, dış bağımlılık yok)
│   │   ├── Common/BaseEntity.cs              # Tüm entity'lerin temel sınıfı (Id, CreatedAt, UpdatedAt)
│   │   ├── Entities/                         # User, UserProfile, Video, Simulation, Survey, Attempt, AuditLog vb.
│   │   ├── Enums/                            # AgeGroup, SubscriptionType, UserRole, QuestionType, ModuleType
│   │   └── Interfaces/                       # IRepository<T>, IUnitOfWork
│   │
│   ├── DogalgazFarkindalik.Application/      # DTO, Service Interface, Validator
│   │   ├── DTOs/                             # Auth/, Video/, Simulation/, Survey/, Report/ altklasörleri
│   │   ├── Interfaces/                       # IAuthService, IVideoService, ISimulationService, ISurveyService, IReportService, IJwtService
│   │   ├── Validators/                       # FluentValidation (RegisterValidator, LoginValidator)
│   │   └── DependencyInjection.cs            # AddApplication() extension method
│   │
│   ├── DogalgazFarkindalik.Infrastructure/   # EF Core, Repository, Service implementasyonları
│   │   ├── Data/AppDbContext.cs              # Tüm DbSet tanımları ve OnModelCreating yapılandırmaları
│   │   ├── Data/SeedData.cs                  # Demo verileri (admin, kullanıcı, videolar, simülasyon, anket)
│   │   ├── Repositories/                     # Generic Repository<T>, UnitOfWork
│   │   ├── Services/                         # AuthService, JwtService, VideoService, SimulationService
│   │   └── DependencyInjection.cs            # AddInfrastructure() extension method
│   │
│   ├── DogalgazFarkindalik.API/              # ASP.NET Core Web API
│   │   ├── Controllers/                      # AuthController, VideosController, SimulationsController
│   │   ├── Program.cs                        # JWT, Swagger, CORS, Seed yapılandırması
│   │   ├── Dockerfile
│   │   └── appsettings.json
│   │
│   └── DogalgazFarkindalik.Web/              # ASP.NET Core MVC (Razor)
│       ├── Controllers/HomeController.cs
│       ├── Views/                            # Shared/_Layout.cshtml, Home/Index.cshtml
│       ├── Dockerfile
│       └── appsettings.json
│
├── docker-compose.yml                        # PostgreSQL + API + Web
├── .gitignore
├── .dockerignore
└── README.md
```

---

## 🔧 Teknoloji Yığını

| Teknoloji | Kullanım | Versiyon |
|-----------|----------|----------|
| .NET | Framework | 10 |
| ASP.NET Core Web API | REST API | 10 |
| ASP.NET Core MVC | Web UI (Razor Views) | 10 |
| Entity Framework Core | ORM (Code-First) | 10.x |
| PostgreSQL | Veritabanı | 16 |
| Npgsql | PostgreSQL EF Core provider | 10.x |
| JWT (System.IdentityModel.Tokens.Jwt) | Kimlik doğrulama | 8.x |
| BCrypt.Net-Next | Parola hash | 4.x |
| FluentValidation | Request doğrulama | 11.x |
| Swashbuckle.AspNetCore | Swagger/OpenAPI | 7.x |
| Docker + docker-compose | Konteynerizasyon | - |
| Bootstrap 5 + Bootstrap Icons | Frontend CSS | 5.3 |

---

## 📊 Veri Modeli

### Entity'ler

| Entity | Alanlar | İlişki |
|--------|---------|--------|
| **User** | Id, Email, PasswordHash, FullName, Role, CreatedAt | 1:1 UserProfile, 1:N SurveyResponse, Attempt, AuditLog |
| **UserProfile** | UserId, BirthDate, AgeGroup, SubscriptionType | N:1 User |
| **Video** | Id, Title, Description, Url, DurationSec, Tags, MinAgeGroup, SubscriptionFilter, IsPublished, ThumbnailUrl | Bağımsız |
| **Simulation** | Id, Title, Description, MinAgeGroup, SubscriptionFilter, IsPublished | 1:N SimulationQuestion |
| **SimulationQuestion** | Id, SimulationId, Text, ImageUrl, Order | N:1 Simulation, 1:N SimulationOption |
| **SimulationOption** | Id, QuestionId, Text, IsCorrect, Explanation | N:1 SimulationQuestion |
| **Survey** | Id, Title, Description, IsActive | 1:N SurveyQuestion, 1:N SurveyResponse |
| **SurveyQuestion** | Id, SurveyId, Text, Type, Weight, Order, AgeGroupFilter, SubscriptionFilter | N:1 Survey, 1:N SurveyOption |
| **SurveyOption** | Id, QuestionId, Text, Value | N:1 SurveyQuestion |
| **SurveyResponse** | Id, SurveyId, UserId, QuestionId, SelectedOptionId, NumericValue | N:1 Survey, User, Question |
| **Attempt** | Id, UserId, Module, ReferenceId, StartedAt, FinishedAt, Score | N:1 User |
| **AuditLog** | Id, UserId, Action, Entity, EntityId, Meta | N:1 User (nullable) |

### Enum'lar

```csharp
AgeGroup        → Child (4-12), Adult (13-65), Senior (65+)
SubscriptionType → Bireysel, Merkezi, Endustriyel
UserRole        → User, Editor, Admin
QuestionType    → SingleChoice, MultipleChoice, TrueFalse, Scale
ModuleType      → Video, Simulation, Survey
```

### Enum'ların veritabanında string olarak saklanması

Tüm enum alanları `HasConversion<string>()` ile string olarak saklanır. Bu AppDbContext.OnModelCreating içinde yapılandırılmıştır.

---

## 🔐 Kimlik Doğrulama ve Yetkilendirme

- **JWT** tabanlı kimlik doğrulama
- Access Token süresi: 15 dakika
- Refresh Token süresi: 7 gün
- Parola hashleme: BCrypt
- Role-based authorization: `[Authorize(Roles = "Admin")]`, `[Authorize(Roles = "Editor,Admin")]`

### Roller ve Yetkileri

| Rol | Video Görme | Simülasyon | Anket | İçerik Yönetimi | Raporlar |
|-----|:-----------:|:----------:|:-----:|:----------------:|:--------:|
| User | ✅ | ✅ | ✅ | ❌ | ❌ |
| Editor | ✅ | ✅ | ✅ | ✅ | ❌ |
| Admin | ✅ | ✅ | ✅ | ✅ | ✅ |

---

## 🌐 API Endpoint Haritası

### Auth
```
POST /api/auth/register          → Yeni kullanıcı kaydı (Anonim)
POST /api/auth/login             → JWT token al (Anonim)
POST /api/auth/refresh           → Token yenile (Anonim, refresh token ile)
```

### Videos
```
GET    /api/videos                → Video listesi (?ageGroup=Adult&subscriptionType=Bireysel)
GET    /api/videos/{id}           → Video detayı
POST   /api/videos                → Video ekle [Editor+]
PUT    /api/videos/{id}           → Video güncelle [Editor+]
DELETE /api/videos/{id}           → Video sil [Admin]
```

### Simulations
```
GET    /api/simulations           → Simülasyon listesi (?ageGroup, ?subscriptionType)
GET    /api/simulations/{id}      → Simülasyon detayı (sorularla birlikte)
POST   /api/simulations/{id}/answers → Cevap gönder, puan al [Authorize]
```

### Surveys
```
GET    /api/surveys/active        → Aktif anketler (?ageGroup, ?subscriptionType) [Authorize]
GET    /api/surveys/{id}          → Anket detayı [Authorize]
POST   /api/surveys/{id}/responses → Anket yanıtı kaydet [Authorize]
```

### Reports
```
GET    /api/reports/summary       → Genel özet raporu [Admin]
GET    /api/reports/by-segment    → Yaş/abonelik kırılımlı rapor [Admin]
```

### Admin CMS
```
CRUD   /api/admin/videos          → Video yönetimi [Editor+]
CRUD   /api/admin/simulations     → Simülasyon yönetimi [Editor+]
CRUD   /api/admin/surveys         → Anket yönetimi [Editor+]
```

---

## 📐 Puanlama Mantığı

```
Nihai Puan = (Doğru Cevap Puanları Toplamı / Toplam Ağırlık) × 100 × Segment Çarpanı
```

- Soru ağırlıkları (weight) toplamı 100 olacak şekilde normalize edilir
- Yaş grubu ve abonelik tipine göre çarpan uygulanır (örn. 65+ güvenlik sorularında ×1.2)
- Simülasyon ve anket sonuçları Attempt tablosunda saklanır

---

## 🐳 Docker Yapılandırması

### docker-compose.yml Servisleri

| Servis | Image | Port | Açıklama |
|--------|-------|------|----------|
| dogalgaz-db | postgres:16 | 5432 | PostgreSQL veritabanı |
| dogalgaz-api | Custom (.NET 10) | 5000 | REST API |
| dogalgaz-web | Custom (.NET 10) | 5001 | MVC Web arayüzü |

### Çalıştırma
```bash
docker-compose up -d
```

### Ortam Değişkenleri
```
ConnectionStrings__Default = Host=dogalgaz-db;Port=5432;Database=dogalgaz_db;Username=postgres;Password=postgres123
Jwt__Secret = BuCokGizliBirAnahtarOlmaliEnAz32Karakter!!
Jwt__Issuer = DogalgazFarkindalik
Jwt__Audience = DogalgazFarkindalik
Jwt__ExpiryMinutes = 15
```

---

## 🌱 Seed Data (Demo Verileri)

Uygulama ilk çalıştığında otomatik olarak şu veriler oluşturulur:

### Kullanıcılar
| Email | Şifre | Rol |
|-------|-------|-----|
| admin@dogalgaz.com | Admin123! | Admin |
| kullanici@test.com | User1234! | User |

### İçerikler
- **5 eğitim videosu** (farklı yaş grupları ve abonelik tipleri için)
- **1 simülasyon** ("Doğalgaz Kaçağı Senaryosu") — 3 soru, her birinde 3 seçenek
- **1 anket** ("Doğalgaz Farkındalık Anketi") — 3 soru (SingleChoice, TrueFalse, Scale)

---

## ⚙️ Geliştirme Ortamı

- **OS:** Windows
- **IDE:** Visual Studio 2026
- **.NET:** 10
- **Docker Desktop** yüklü ve çalışır durumda

---

## 📋 GÖREV LİSTESİ — Adım Adım Yapılacaklar

Aşağıdaki görevler sırasıyla tamamlanmalıdır. Her adımı tamamladıktan sonra build ve çalıştırma testi yap.

### Faz 1: Proje İskeleti ve Veritabanı (Gün 1–2)

- [ ] **1.1** — Solution ve 5 proje oluştur (Domain, Application, Infrastructure, API, Web)
- [ ] **1.2** — Proje referanslarını ekle:
  - Application → Domain
  - Infrastructure → Domain + Application
  - API → Application + Infrastructure
  - Web → Application
- [ ] **1.3** — NuGet paketlerini yükle (yukarıdaki teknoloji tablosuna göre)
- [ ] **1.4** — Domain katmanını oluştur: BaseEntity, tüm Entity'ler, Enum'lar, IRepository<T>, IUnitOfWork
- [ ] **1.5** — Application katmanını oluştur: Tüm DTO'lar, Service Interface'leri, Validator'lar, DependencyInjection.cs
- [ ] **1.6** — Infrastructure katmanını oluştur: AppDbContext (tüm entity ilişkileri ve enum conversion'lar), Generic Repository, UnitOfWork, DependencyInjection.cs
- [ ] **1.7** — API Program.cs oluştur: JWT yapılandırması, Swagger (Bearer token destekli), CORS, DI registrations
- [ ] **1.8** — docker-compose.yml oluştur (PostgreSQL + API + Web)
- [ ] **1.9** — İlk EF Core migration oluştur: `dotnet ef migrations add InitialCreate --project src/DogalgazFarkindalik.Infrastructure --startup-project src/DogalgazFarkindalik.API`
- [ ] **1.10** — SeedData.cs oluştur ve Program.cs'de çağır (auto-migrate + seed)
- [ ] **1.11** — `docker-compose up -d` ile sistemi ayağa kaldır ve veritabanının oluştuğunu doğrula

### Faz 2: Auth Modülü (Gün 3)

- [ ] **2.1** — JwtService implementasyonu (GenerateAccessToken, GenerateRefreshToken)
- [ ] **2.2** — AuthService implementasyonu (Register, Login) — BCrypt ile parola hash, yaş grubu otomatik hesaplama
- [ ] **2.3** — AuthController (POST register, POST login)
- [ ] **2.4** — Swagger'dan test et: register → login → token al → korumalı endpoint'e eriş

### Faz 3: Core API (Gün 4)

- [ ] **3.1** — VideoService implementasyonu (CRUD + yaş/abonelik filtreleme)
- [ ] **3.2** — VideosController (GET listele, GET detay, POST ekle, PUT güncelle, DELETE sil)
- [ ] **3.3** — SimulationService implementasyonu (listeleme, detay, cevap gönderme + puanlama)
- [ ] **3.4** — SimulationsController (GET listele, GET detay, POST answers)
- [ ] **3.5** — SurveyService implementasyonu (aktif anketler, detay, yanıt kaydetme)
- [ ] **3.6** — SurveysController (GET active, GET detay, POST responses)
- [ ] **3.7** — Tüm endpoint'leri Swagger'dan test et

### Faz 4: Web UI (Gün 5)

- [ ] **4.1** — Shared/_Layout.cshtml: Bootstrap 5 navbar, footer, genel stil
- [ ] **4.2** — Home/Index: Hero section, 3 özellik kartı, istatistikler
- [ ] **4.3** — Auth sayfaları: Login ve Register formları (API'ye HttpClient ile istek)
- [ ] **4.4** — Videos/Index: API'den video listesi çek, kart grid'i göster, yaş/abonelik filtreleme
- [ ] **4.5** — Simulations/Index: Simülasyon listesi
- [ ] **4.6** — Simulations/Detail: Soru-cevap akışı, sonuç gösterimi
- [ ] **4.7** — Surveys/Index: Aktif anketler
- [ ] **4.8** — Surveys/Detail: Anket formu, gönderim

### Faz 5: Raporlama ve Admin (Gün 6)

- [ ] **5.1** — ReportService implementasyonu (özet rapor, segment bazlı rapor)
- [ ] **5.2** — ReportsController (GET summary, GET by-segment)
- [ ] **5.3** — Admin dashboard sayfası: Toplam kullanıcı, ortalama puan, segment dağılımları
- [ ] **5.4** — Admin CMS: Video/Simülasyon/Anket CRUD arayüzleri
- [ ] **5.5** — AuditLog middleware: Kritik işlemleri otomatik logla

### Faz 6: Son Rötuşlar (Gün 7)

- [ ] **6.1** — Swagger açıklamalarını tamamla (tüm endpoint'lere XML comment)
- [ ] **6.2** — Hata yönetimi: Global exception middleware
- [ ] **6.3** — Rate limiting ekle
- [ ] **6.4** — Dockerfile'ları doğrula, docker-compose ile tam test
- [ ] **6.5** — README.md güncelle

---

## 🚨 Önemli Kurallar ve Dikkat Edilecekler

### Mimari Kurallar
1. **Domain katmanı HİÇBİR dış bağımlılığa sahip olmamalı** — sadece saf C#
2. **Application katmanı sadece Domain'e bağımlı** — Infrastructure'a referans vermemeli
3. **Bağımlılık enjeksiyonu** her zaman interface üzerinden yapılmalı
4. **Entity'ler doğrudan API'den dönmemeli** — her zaman DTO kullan

### Kod Standartları
1. Tüm dosyalar **file-scoped namespace** kullansın (`namespace X;`)
2. **Record type** kullanımı DTO'lar için tercih edilsin
3. **Async/await** pattern her yerde kullanılsın
4. **CancellationToken** tüm async metotlara parametre olarak geçilsin
5. Türkçe karakterler dosya adlarında KULLANILMASIN, sadece içeriklerde kullanılsın

### Veritabanı Kuralları
1. Tüm enum alanları `HasConversion<string>()` ile saklanacak
2. Cascade delete ilişkilere göre ayarlanacak (SurveyResponse → Question: Restrict)
3. User.Email üzerinde unique index olacak
4. Migration'lar Infrastructure projesinde, startup API projesinden çalıştırılacak

### Docker Kuralları
1. API ve Web ayrı Dockerfile'lara sahip
2. docker-compose'da healthcheck ile db hazır olana kadar API bekleyecek
3. Ortam değişkenleri appsettings.json'u override edecek

### Güvenlik Kuralları
1. Parolalar BCrypt ile hashlenecek
2. JWT secret en az 32 karakter olacak
3. Swagger'da Bearer token desteği olacak
4. CORS sadece Web projesinin origin'ine izin verecek
5. Admin endpoint'leri `[Authorize(Roles = "Admin")]` ile korunacak

---

## 🧪 Test Senaryoları (Demo için)

### Senaryo 1: Kullanıcı Kaydı ve Giriş
1. POST /api/auth/register ile yeni kullanıcı oluştur
2. POST /api/auth/login ile token al
3. Token ile korumalı endpoint'e eriş

### Senaryo 2: İçerik Filtreleme
1. GET /api/videos?ageGroup=Adult&subscriptionType=Bireysel → filtrelenmiş liste
2. GET /api/videos?ageGroup=Child → çocuklara uygun videolar
3. GET /api/videos?subscriptionType=Endustriyel → endüstriyel içerikler

### Senaryo 3: Simülasyon Tamamlama
1. GET /api/simulations → liste
2. GET /api/simulations/{id} → sorular
3. POST /api/simulations/{id}/answers → cevapları gönder, puan ve detaylı sonuç al

### Senaryo 4: Admin Rapor
1. Admin token ile giriş yap
2. GET /api/reports/summary → toplam kullanıcı, ortalama puan
3. GET /api/reports/by-segment → yaş ve abonelik kırılımları

---

## 📂 Mevcut Dosyalar

Bu proje bir zip dosyası olarak sağlanmıştır. Zip'i açtıktan sonra tüm dosyalar yukarıdaki yapıya uygun şekilde yerleşmiştir. `dotnet restore` ve `dotnet build` ile başlayabilirsin.

İlk iş olarak:
1. `dotnet restore DogalgazFarkindalik.sln`
2. `dotnet build DogalgazFarkindalik.sln`
3. Hata varsa düzelt
4. Migration oluştur
5. docker-compose ile ayağa kaldır

Başarılar!
