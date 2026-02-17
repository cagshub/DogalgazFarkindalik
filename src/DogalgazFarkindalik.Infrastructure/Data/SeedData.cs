using DogalgazFarkindalik.Domain.Entities;
using DogalgazFarkindalik.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DogalgazFarkindalik.Infrastructure.Data;

public static class SeedData
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Users.AnyAsync()) return;

        // Admin user
        var admin = new User
        {
            Email = "admin@dogalgaz.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            FullName = "Sistem Yöneticisi",
            Role = UserRole.Admin,
            IsEmailVerified = true
        };
        var adminProfile = new UserProfile
        {
            UserId = admin.Id,
            BirthDate = new DateTime(1985, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            AgeGroup = AgeGroup.Adult,
            SubscriptionType = SubscriptionType.Bireysel
        };

        // Test user
        var user = new User
        {
            Email = "kullanici@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("User1234!"),
            FullName = "Test Kullanıcısı",
            Role = UserRole.User,
            IsEmailVerified = true
        };
        var userProfile = new UserProfile
        {
            UserId = user.Id,
            BirthDate = new DateTime(1990, 6, 15, 0, 0, 0, DateTimeKind.Utc),
            AgeGroup = AgeGroup.Adult,
            SubscriptionType = SubscriptionType.Bireysel
        };

        context.Users.AddRange(admin, user);
        context.UserProfiles.AddRange(adminProfile, userProfile);

        // Videos
        var videos = new List<Video>
        {
            new() { Title = "Doğalgaz Nedir?", Description = "Doğalgazın temel özellikleri ve kullanım alanları.", Url = "/videos/dogalgaz-nedir.mp4", DurationSec = 180, Tags = "temel,giriş", MinAgeGroup = AgeGroup.Child, IsPublished = true, ThumbnailUrl = "/thumbnails/dogalgaz-nedir.jpg" },
            new() { Title = "Doğalgaz Kaçağı Nasıl Anlaşılır?", Description = "Doğalgaz kaçağının belirtileri ve yapılması gerekenler.", Url = "/videos/kacak-tespiti.mp4", DurationSec = 240, Tags = "güvenlik,kaçak", MinAgeGroup = AgeGroup.Adult, IsPublished = true, ThumbnailUrl = "/thumbnails/kacak-tespiti.jpg" },
            new() { Title = "Kombi Bakımı ve Güvenlik", Description = "Kombi kullanımı ve periyodik bakım önerileri.", Url = "/videos/kombi-bakim.mp4", DurationSec = 300, Tags = "kombi,bakım,güvenlik", MinAgeGroup = AgeGroup.Adult, SubscriptionFilter = SubscriptionType.Bireysel, IsPublished = true, ThumbnailUrl = "/thumbnails/kombi-bakim.jpg" },
            new() { Title = "Endüstriyel Doğalgaz Güvenliği", Description = "Fabrikalarda doğalgaz kullanımı ve güvenlik protokolleri.", Url = "/videos/endustriyel-guvenlik.mp4", DurationSec = 420, Tags = "endüstriyel,güvenlik", MinAgeGroup = AgeGroup.Adult, SubscriptionFilter = SubscriptionType.Endustriyel, IsPublished = true, ThumbnailUrl = "/thumbnails/endustriyel.jpg" },
            new() { Title = "Yaşlılar İçin Doğalgaz Güvenliği", Description = "65 yaş üstü bireyler için özel güvenlik önlemleri.", Url = "/videos/yasli-guvenlik.mp4", DurationSec = 200, Tags = "yaşlı,güvenlik", MinAgeGroup = AgeGroup.Senior, IsPublished = true, ThumbnailUrl = "/thumbnails/yasli-guvenlik.jpg" },
        };
        context.Videos.AddRange(videos);

        // Simulation
        var sim = new Simulation
        {
            Title = "Doğalgaz Kaçağı Senaryosu",
            Description = "Evde doğalgaz kaçağı olduğunda yapmanız gerekenleri test edin.",
            MinAgeGroup = AgeGroup.Adult,
            IsPublished = true
        };

        var q1 = new SimulationQuestion { SimulationId = sim.Id, Text = "Evde doğalgaz kokusu aldınız. İlk ne yapmalısınız?", Order = 1 };
        var q2 = new SimulationQuestion { SimulationId = sim.Id, Text = "Doğalgaz kaçağı şüphesinde aşağıdakilerden hangisini yapmamalısınız?", Order = 2 };
        var q3 = new SimulationQuestion { SimulationId = sim.Id, Text = "Doğalgaz vanasını kapatma yönü hangisidir?", Order = 3 };

        sim.Questions = new List<SimulationQuestion> { q1, q2, q3 };

        var q1Options = new List<SimulationOption>
        {
            new() { QuestionId = q1.Id, Text = "Pencereleri açın ve ortamı havalandırın", IsCorrect = true, Explanation = "Doğalgaz kaçağında ilk yapılması gereken ortamı havalandırmaktır." },
            new() { QuestionId = q1.Id, Text = "Işık düğmesini açın", IsCorrect = false, Explanation = "Elektrik anahtarlarına dokunmak kıvılcım oluşturabilir." },
            new() { QuestionId = q1.Id, Text = "Kibrit yakarak kontrol edin", IsCorrect = false, Explanation = "Açık alev kullanmak patlamaya neden olabilir." },
        };

        var q2Options = new List<SimulationOption>
        {
            new() { QuestionId = q2.Id, Text = "Pencereleri açmak", IsCorrect = false, Explanation = "Havalandırma doğru bir adımdır." },
            new() { QuestionId = q2.Id, Text = "Elektrik düğmesine basmak", IsCorrect = true, Explanation = "Elektrik anahtarları kıvılcım oluşturabilir, kesinlikle dokunulmamalıdır." },
            new() { QuestionId = q2.Id, Text = "187'yi aramak", IsCorrect = false, Explanation = "Doğalgaz acil hattı (187) aranmalıdır." },
        };

        var q3Options = new List<SimulationOption>
        {
            new() { QuestionId = q3.Id, Text = "Saat yönünde çevirme", IsCorrect = false, Explanation = "Saat yönünde çevirmek vanayı açar." },
            new() { QuestionId = q3.Id, Text = "Saat yönünün tersine çevirme", IsCorrect = true, Explanation = "Vanayı kapatmak için saat yönünün tersine çevirilir." },
            new() { QuestionId = q3.Id, Text = "Yukarı itme", IsCorrect = false, Explanation = "Doğalgaz vanaları çevirme mekanizmalıdır." },
        };

        context.Simulations.Add(sim);
        context.SimulationOptions.AddRange(q1Options);
        context.SimulationOptions.AddRange(q2Options);
        context.SimulationOptions.AddRange(q3Options);

        // Survey
        var survey = new Survey
        {
            Title = "Doğalgaz Farkındalık Anketi",
            Description = "Doğalgaz güvenliği hakkındaki bilgi düzeyinizi ölçün.",
            IsActive = true
        };

        var sq1 = new SurveyQuestion { SurveyId = survey.Id, Text = "Doğalgaz kaçağında hangi numarayı aramanız gerekir?", Type = QuestionType.SingleChoice, Weight = 20, Order = 1 };
        var sq2 = new SurveyQuestion { SurveyId = survey.Id, Text = "Kombinin yıllık bakımı yapılmalı mıdır?", Type = QuestionType.TrueFalse, Weight = 15, Order = 2 };
        var sq3 = new SurveyQuestion { SurveyId = survey.Id, Text = "Doğalgaz güvenliği konusundaki bilgi düzeyinizi nasıl değerlendirirsiniz?", Type = QuestionType.Scale, Weight = 10, Order = 3 };

        survey.Questions = new List<SurveyQuestion> { sq1, sq2, sq3 };

        var sq1Options = new List<SurveyOption>
        {
            new() { QuestionId = sq1.Id, Text = "110", Value = 0 },
            new() { QuestionId = sq1.Id, Text = "187", Value = 100 },
            new() { QuestionId = sq1.Id, Text = "112", Value = 0 },
            new() { QuestionId = sq1.Id, Text = "155", Value = 0 },
        };

        var sq2Options = new List<SurveyOption>
        {
            new() { QuestionId = sq2.Id, Text = "Evet", Value = 100 },
            new() { QuestionId = sq2.Id, Text = "Hayır", Value = 0 },
        };

        context.Surveys.Add(survey);
        context.SurveyOptions.AddRange(sq1Options);
        context.SurveyOptions.AddRange(sq2Options);

        // Tags
        var tagGuvenlik = new Tag { Name = "Guvenlik" };
        var tagTemel = new Tag { Name = "Temel" };
        var tagKombi = new Tag { Name = "Kombi" };
        var tagEndustriyel = new Tag { Name = "Endustriyel" };
        var tagYasli = new Tag { Name = "Yasli" };
        context.Tags.AddRange(tagGuvenlik, tagTemel, tagKombi, tagEndustriyel, tagYasli);

        // VideoTags
        context.VideoTags.AddRange(
            new VideoTag { VideoId = videos[0].Id, TagId = tagTemel.Id },
            new VideoTag { VideoId = videos[1].Id, TagId = tagGuvenlik.Id },
            new VideoTag { VideoId = videos[2].Id, TagId = tagKombi.Id },
            new VideoTag { VideoId = videos[2].Id, TagId = tagGuvenlik.Id },
            new VideoTag { VideoId = videos[3].Id, TagId = tagEndustriyel.Id },
            new VideoTag { VideoId = videos[3].Id, TagId = tagGuvenlik.Id },
            new VideoTag { VideoId = videos[4].Id, TagId = tagYasli.Id },
            new VideoTag { VideoId = videos[4].Id, TagId = tagGuvenlik.Id }
        );

        // ContentTargetingRules
        context.ContentTargetingRules.AddRange(
            new ContentTargetingRule
            {
                Module = ModuleType.Simulation,
                AgeGroup = AgeGroup.Senior,
                ScoreMultiplier = 1.2,
                IsActive = true,
                Description = "65+ yas grubu icin guvenlik sorularinda x1.2 carpan"
            },
            new ContentTargetingRule
            {
                Module = ModuleType.Survey,
                AgeGroup = AgeGroup.Senior,
                ScoreMultiplier = 1.2,
                IsActive = true,
                Description = "65+ yas grubu icin anket puanlamasinda x1.2 carpan"
            },
            new ContentTargetingRule
            {
                Module = ModuleType.Simulation,
                SubscriptionType = SubscriptionType.Endustriyel,
                ScoreMultiplier = 1.1,
                IsActive = true,
                Description = "Endustriyel abonelik icin x1.1 carpan"
            }
        );

        await context.SaveChangesAsync();
    }
}
