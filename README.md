# Dogalgaz Farkindalik Egitim ve Degerlendirme Platformu

Dogalgaz guvenligi ve verimli kullanimina yonelik farkindalik egitim platformu. Kullanicilarin bilgi seviyesini egitim videolari, interaktif simulasyonlar ve dinamik anketlerle olcer ve raporlar.

Platform, **yas grubu** (4-12, 13-65, 65+) ve **abonelik tipi** (Bireysel, Merkezi, Endustriyel) bazinda kisisellestirilmis icerikler sunar.

## Teknoloji Yigini

| Teknoloji | Kullanim |
|-----------|----------|
| .NET 10 | Framework |
| ASP.NET Core Web API | REST API |
| ASP.NET Core MVC | Web UI (Razor Views) |
| Entity Framework Core | ORM (Code-First) |
| PostgreSQL 16 | Veritabani |
| JWT | Kimlik dogrulama |
| BCrypt | Parola hashleme |
| FluentValidation | Request dogrulama |
| Swagger/OpenAPI | API dokumantasyonu |
| Docker + docker-compose | Konteynerizasyon |
| Bootstrap 5 | Frontend CSS |

## Mimari: Clean Architecture

```
API + Web (UI)           <- Sunum katmani
Infrastructure           <- Teknik detaylar (EF Core, JWT, vb.)
Application              <- Is kurallari arayuzleri, DTO'lar
Domain                   <- Entity'ler, Enum'lar (saf C#)
```

### Proje Yapisi

```
src/
  DogalgazFarkindalik.Domain/          # Entity, Enum, Interface (saf C#, bagimliliksiz)
  DogalgazFarkindalik.Application/     # DTO, Service Interface, Validator
  DogalgazFarkindalik.Infrastructure/  # DbContext, Repository, Service implementasyonlari
  DogalgazFarkindalik.API/             # REST API Controller'lari, Middleware, Swagger
  DogalgazFarkindalik.Web/             # MVC Razor Views, Admin CMS paneli
```

## Hizli Baslangic

### On Kosullar

- Docker Desktop yuklu ve calisiyor

### Docker ile Calistirma (Onerilen)

```bash
docker-compose up -d
```

Uc konteyner ayaga kalkar:

| Servis | URL | Aciklama |
|--------|-----|----------|
| dogalgaz-db | localhost:5432 | PostgreSQL veritabani |
| dogalgaz-api | http://localhost:5000 | REST API + Swagger |
| dogalgaz-web | http://localhost:5001 | Web arayuzu |

- **Swagger UI:** http://localhost:5000/swagger
- **Web Arayuzu:** http://localhost:5001

### Lokal Gelistirme

```bash
# Restore ve build
dotnet restore DogalgazFarkindalik.sln
dotnet build DogalgazFarkindalik.sln

# PostgreSQL baglantisi gereklidir (appsettings.json'u guncelleyin)
# API calistir
dotnet run --project src/DogalgazFarkindalik.API

# Web calistir (ayri terminalde)
dotnet run --project src/DogalgazFarkindalik.Web
```

## Varsayilan Kullanicilar (Seed Data)

| Email | Sifre | Rol |
|-------|-------|-----|
| admin@dogalgaz.com | Admin123! | Admin |
| kullanici@test.com | User1234! | User |

### Seed Icerikler

- **5 egitim videosu** (farkli yas gruplari ve abonelik tipleri)
- **1 simulasyon** - "Dogalgaz Kacagi Senaryosu" (3 soru, her birinde 3 secenek)
- **1 anket** - "Dogalgaz Farkindalik Anketi" (3 soru: SingleChoice, TrueFalse, Scale)

## API Endpoint Haritasi

### Auth (Anonim)
| Metot | Endpoint | Aciklama |
|-------|----------|----------|
| POST | `/api/auth/register` | Yeni kullanici kaydi |
| POST | `/api/auth/login` | JWT token al |
| GET | `/api/auth/verify-email` | E-posta dogrula |
| POST | `/api/auth/resend-verification` | Dogrulama maili tekrar gonder |

### Videos
| Metot | Endpoint | Yetki | Aciklama |
|-------|----------|-------|----------|
| GET | `/api/videos` | Anonim | Video listesi (?ageGroup, ?subscriptionType) |
| GET | `/api/videos/{id}` | Anonim | Video detayi |
| POST | `/api/videos` | Editor,Admin | Video ekle |
| PUT | `/api/videos/{id}` | Editor,Admin | Video guncelle |
| DELETE | `/api/videos/{id}` | Admin | Video sil |

### Simulations
| Metot | Endpoint | Yetki | Aciklama |
|-------|----------|-------|----------|
| GET | `/api/simulations` | Anonim | Simulasyon listesi (?ageGroup, ?subscriptionType) |
| GET | `/api/simulations/{id}` | Anonim | Simulasyon detayi (sorularla) |
| POST | `/api/simulations/{id}/answers` | Authorize | Cevap gonder, puan al |
| POST | `/api/simulations` | Editor,Admin | Simulasyon olustur |
| PUT | `/api/simulations/{id}` | Editor,Admin | Simulasyon guncelle |
| DELETE | `/api/simulations/{id}` | Admin | Simulasyon sil |

### Surveys
| Metot | Endpoint | Yetki | Aciklama |
|-------|----------|-------|----------|
| GET | `/api/surveys/active` | Authorize | Aktif anketler (?ageGroup, ?subscriptionType) |
| GET | `/api/surveys/{id}` | Authorize | Anket detayi |
| POST | `/api/surveys/{id}/responses` | Authorize | Anket yaniti kaydet |
| POST | `/api/surveys` | Editor,Admin | Anket olustur |
| PUT | `/api/surveys/{id}` | Editor,Admin | Anket guncelle |
| DELETE | `/api/surveys/{id}` | Admin | Anket sil |

### Reports (Admin)
| Metot | Endpoint | Aciklama |
|-------|----------|----------|
| GET | `/api/reports/summary` | Genel ozet raporu |
| GET | `/api/reports/by-segment` | Segment bazli rapor |

## Roller ve Yetkileri

| Rol | Video | Simulasyon | Anket | Icerik Yonetimi | Raporlar |
|-----|:-----:|:----------:|:-----:|:---------------:|:--------:|
| User | + | + | + | - | - |
| Editor | + | + | + | + | - |
| Admin | + | + | + | + | + |

## Web Arayuzu Sayfalari

| Sayfa | URL | Aciklama |
|-------|-----|----------|
| Ana Sayfa | `/` | Hero section, ozellik kartlari |
| Giris | `/Auth/Login` | Kullanici girisi |
| Kayit | `/Auth/Register` | Yeni kayit |
| Videolar | `/Videos` | Video listesi (filtreleme destekli) |
| Simulasyonlar | `/Simulations` | Simulasyon listesi ve detay |
| Anketler | `/Surveys` | Anket listesi ve yanit formu |
| Admin Panel | `/Admin/Dashboard` | Istatistikler ve CMS yonetimi |
| Video Yonetimi | `/Admin/Videos` | Video CRUD |
| Simulasyon Yonetimi | `/Admin/Simulations` | Simulasyon CRUD |
| Anket Yonetimi | `/Admin/Surveys` | Anket CRUD |

## Guvenlik

- **JWT** tabanli kimlik dogrulama (15 dk access token, 7 gun refresh token)
- **BCrypt** ile parola hashleme
- **Role-based authorization** (User, Editor, Admin)
- **Rate limiting** (100 istek/dakika per IP)
- **CORS** sadece Web projesinin origin'ine izin verir
- **Global exception middleware** hata yonetimi
- **AuditLog middleware** kritik islemleri loglar
- **E-posta dogrulama** kayit sirasinda

## Puanlama Mantigi

```
Nihai Puan = (Dogru Cevap Puanlari Toplami / Toplam Agirlik) x 100
```

- Simulasyon: Her dogru cevap esit puan
- Anket: Soru agirliklari (weight) ile puanlama
- Sonuclar `Attempt` tablosunda saklanir

## Ortam Degiskenleri

```
ConnectionStrings__Default = Host=dogalgaz-db;Port=5432;Database=dogalgaz_db;Username=postgres;Password=postgres123
Jwt__Secret = BuCokGizliBirAnahtarOlmaliEnAz32Karakter!!
Jwt__Issuer = DogalgazFarkindalik
Jwt__Audience = DogalgazFarkindalik
Jwt__ExpiryMinutes = 15
```

## Test Senaryolari

### Senaryo 1: Kullanici Kaydi ve Giris
1. `POST /api/auth/register` ile yeni kullanici olustur
2. E-posta dogrulamasini tamamla
3. `POST /api/auth/login` ile JWT token al
4. Token ile korumali endpoint'e eris

### Senaryo 2: Icerik Filtreleme
1. `GET /api/videos?ageGroup=Adult&subscriptionType=Bireysel` - filtrelenmis liste
2. `GET /api/videos?ageGroup=Child` - cocuklara uygun videolar

### Senaryo 3: Simulasyon Tamamlama
1. `GET /api/simulations` - liste
2. `GET /api/simulations/{id}` - sorular
3. `POST /api/simulations/{id}/answers` - cevaplari gonder, puan al

### Senaryo 4: Admin CMS
1. Admin token ile giris yap
2. Admin Panel > Video/Simulasyon/Anket yonetimi
3. Icerik ekle, duzenle, sil
4. Raporlari incele

## Docker Komutlari

```bash
# Baslat
docker-compose up -d

# Durdur
docker-compose down

# Yeniden olustur
docker-compose up -d --build

# Loglari gor
docker-compose logs -f dogalgaz-api
docker-compose logs -f dogalgaz-web
```
