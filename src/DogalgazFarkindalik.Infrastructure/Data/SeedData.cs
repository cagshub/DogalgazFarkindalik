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
            new() { Title = "Doğalgaz Nedir?", Description = "Doğalgazın temel özellikleri ve kullanım alanları.", Url = "https://www.youtube.com/embed/WKPt02S_S14", DurationSec = 180, Tags = "temel,giriş", MinAgeGroup = AgeGroup.Child, IsPublished = true, ThumbnailUrl = "/thumbnails/dogalgaz-nedir.jpg" },
            new() { Title = "Doğalgaz Kaçağı Nasıl Anlaşılır?", Description = "Doğalgaz kaçağının belirtileri ve yapılması gerekenler.", Url = "https://www.youtube.com/embed/VnttXd0tiHA", DurationSec = 240, Tags = "güvenlik,kaçak", MinAgeGroup = AgeGroup.Adult, IsPublished = true, ThumbnailUrl = "/thumbnails/kacak-tespiti.jpg" },
            new() { Title = "Kombi Bakımı ve Güvenlik", Description = "Kombi kullanımı ve periyodik bakım önerileri.", Url = "https://www.youtube.com/embed/nbevTUOwH1s", DurationSec = 300, Tags = "kombi,bakım,güvenlik", MinAgeGroup = AgeGroup.Adult, SubscriptionFilter = SubscriptionType.Bireysel, IsPublished = true, ThumbnailUrl = "/thumbnails/kombi-bakim.jpg" },
            new() { Title = "Endüstriyel Doğalgaz Güvenliği", Description = "Fabrikalarda doğalgaz kullanımı ve güvenlik protokolleri.", Url = "https://www.youtube.com/embed/nM1CINsx2q8", DurationSec = 420, Tags = "endüstriyel,güvenlik", MinAgeGroup = AgeGroup.Adult, SubscriptionFilter = SubscriptionType.Endustriyel, IsPublished = true, ThumbnailUrl = "/thumbnails/endustriyel.jpg" },
            new() { Title = "Yaşlılar İçin Doğalgaz Güvenliği", Description = "65 yaş üstü bireyler için özel güvenlik önlemleri.", Url = "https://www.youtube.com/embed/nNalHgbPBW4", DurationSec = 200, Tags = "yaşlı,güvenlik", MinAgeGroup = AgeGroup.Senior, IsPublished = true, ThumbnailUrl = "/thumbnails/yasli-guvenlik.jpg" },
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

        // Simulation 2: Kombi Arızası
        var sim2 = new Simulation
        {
            Title = "Kombi Arızası ve Güvenlik",
            Description = "Kombi arızalandığında doğru müdahale yöntemlerini öğrenin.",
            MinAgeGroup = AgeGroup.Adult,
            SubscriptionFilter = SubscriptionType.Bireysel,
            IsPublished = true
        };

        var s2q1 = new SimulationQuestion { SimulationId = sim2.Id, Text = "Kombiden su damlıyor. İlk ne yapmalısınız?", Order = 1 };
        var s2q2 = new SimulationQuestion { SimulationId = sim2.Id, Text = "Kombi yanarken tuhaf bir ses geliyor. Ne yapmalısınız?", Order = 2 };
        var s2q3 = new SimulationQuestion { SimulationId = sim2.Id, Text = "Kombinin pilot ışığı sürekli sönüyor. Sebebi ne olabilir?", Order = 3 };
        var s2q4 = new SimulationQuestion { SimulationId = sim2.Id, Text = "Kombi kaç yılda bir bakım yapılmalıdır?", Order = 4 };

        sim2.Questions = new List<SimulationQuestion> { s2q1, s2q2, s2q3, s2q4 };

        context.Simulations.Add(sim2);
        context.SimulationOptions.AddRange(new List<SimulationOption>
        {
            new() { QuestionId = s2q1.Id, Text = "Kombiyi kapatıp yetkili servisi arayın", IsCorrect = true, Explanation = "Su kaçağında kombiyi kapatıp yetkili servis çağırmak en doğru adımdır." },
            new() { QuestionId = s2q1.Id, Text = "Kendiniz tamir etmeye çalışın", IsCorrect = false, Explanation = "Kombi tamiri uzmanlık gerektirir, kendiniz müdahale etmek tehlikelidir." },
            new() { QuestionId = s2q1.Id, Text = "Hiçbir şey yapmayın, kendiliğinden geçer", IsCorrect = false, Explanation = "Su kaçağı ciddi arızaların habercisi olabilir, ihmal edilmemelidir." },

            new() { QuestionId = s2q2.Id, Text = "Kombinin altındaki gaz vanasını kapatın ve servis çağırın", IsCorrect = true, Explanation = "Anormal sesler yanma sorununa işaret edebilir, gaz vanasını kapatıp servis çağırılmalıdır." },
            new() { QuestionId = s2q2.Id, Text = "Sesler normal, kullanmaya devam edin", IsCorrect = false, Explanation = "Tuhaf sesler bir arızanın habercisidir, görmezden gelinmemelidir." },
            new() { QuestionId = s2q2.Id, Text = "Kombiyi açıp içini kontrol edin", IsCorrect = false, Explanation = "Kombinin iç kısmı tehlikelidir, kesinlikle kendiniz açmamalısınız." },

            new() { QuestionId = s2q3.Id, Text = "Termokapl arızası veya hava akımı sorunu", IsCorrect = true, Explanation = "Pilot ışığının sürekli sönmesi genellikle termokapl arızası veya baca hava akımı sorunudur." },
            new() { QuestionId = s2q3.Id, Text = "Doğalgaz bitti", IsCorrect = false, Explanation = "Doğalgaz şebeke ile gelir, bitmesi söz konusu değildir." },
            new() { QuestionId = s2q3.Id, Text = "Elektrik kesintisi", IsCorrect = false, Explanation = "Pilot ışığı elektrikle değil, gaz alevi ile yanar." },

            new() { QuestionId = s2q4.Id, Text = "Her yıl düzenli bakım yapılmalıdır", IsCorrect = true, Explanation = "Kombiler yılda bir kez yetkili servis tarafından bakım yapılmalıdır." },
            new() { QuestionId = s2q4.Id, Text = "5 yılda bir yeterlidir", IsCorrect = false, Explanation = "5 yıl çok uzun bir süredir, her yıl bakım şarttır." },
            new() { QuestionId = s2q4.Id, Text = "Arıza olmadıkça bakıma gerek yoktur", IsCorrect = false, Explanation = "Önleyici bakım güvenlik açısından kritik öneme sahiptir." },
        });

        // Simulation 3: Çocuklar İçin
        var sim3 = new Simulation
        {
            Title = "Çocuklar İçin Doğalgaz Güvenliği",
            Description = "Çocukların doğalgaz tehlikelerini tanıması ve doğru davranması için eğlenceli senaryo.",
            MinAgeGroup = AgeGroup.Child,
            IsPublished = true
        };

        var s3q1 = new SimulationQuestion { SimulationId = sim3.Id, Text = "Evde garip bir koku aldığında ne yapmalısın?", Order = 1 };
        var s3q2 = new SimulationQuestion { SimulationId = sim3.Id, Text = "Mutfaktaki ocağa dokunmalı mısın?", Order = 2 };
        var s3q3 = new SimulationQuestion { SimulationId = sim3.Id, Text = "Doğalgaz tehlikesi olduğunda kimi aramalısın?", Order = 3 };

        sim3.Questions = new List<SimulationQuestion> { s3q1, s3q2, s3q3 };

        context.Simulations.Add(sim3);
        context.SimulationOptions.AddRange(new List<SimulationOption>
        {
            new() { QuestionId = s3q1.Id, Text = "Hemen bir büyüğe söyle ve evden çık", IsCorrect = true, Explanation = "Garip bir koku aldığında hemen bir büyüğe haber vermelisin." },
            new() { QuestionId = s3q1.Id, Text = "Kokuyu araştırmak için mutfağa git", IsCorrect = false, Explanation = "Tek başına araştırmak tehlikeli olabilir." },
            new() { QuestionId = s3q1.Id, Text = "Hiçbir şey yapma, oyuna devam et", IsCorrect = false, Explanation = "Garip kokular tehlike işareti olabilir, ihmal etmemelisin." },

            new() { QuestionId = s3q2.Id, Text = "Hayır, ocak çok sıcaktır ve tehlikelidir", IsCorrect = true, Explanation = "Ocak çok sıcak olabilir ve yanabilirsin. Asla dokunmamalısın." },
            new() { QuestionId = s3q2.Id, Text = "Evet, merak ettiğim için dokunabilirim", IsCorrect = false, Explanation = "Ocağa dokunmak yanıklara neden olabilir, çok tehlikelidir." },
            new() { QuestionId = s3q2.Id, Text = "Sadece kapalıyken dokunabilirim", IsCorrect = false, Explanation = "Kapalı olsa bile ocak sıcak olabilir, büyükler olmadan dokunmamalısın." },

            new() { QuestionId = s3q3.Id, Text = "Anne, baba veya bir büyüğü ara", IsCorrect = true, Explanation = "Tehlike anında hemen bir büyüğe haber vermelisin." },
            new() { QuestionId = s3q3.Id, Text = "Arkadaşlarımı arayıp yardım isteyebilirim", IsCorrect = false, Explanation = "Arkadaşların bu konuda yardım edemez, bir büyüğe söylemelisin." },
            new() { QuestionId = s3q3.Id, Text = "Kimseyi aramama gerek yok", IsCorrect = false, Explanation = "Tehlike anında mutlaka bir büyüğe haber vermelisin." },
        });

        // Simulation 4: Endüstriyel Acil Durum
        var sim4 = new Simulation
        {
            Title = "Endüstriyel Tesis Acil Durum Protokolü",
            Description = "Endüstriyel tesislerde doğalgaz kaçağı ve acil durum müdahale prosedürlerini test edin.",
            MinAgeGroup = AgeGroup.Adult,
            SubscriptionFilter = SubscriptionType.Endustriyel,
            IsPublished = true
        };

        var s4q1 = new SimulationQuestion { SimulationId = sim4.Id, Text = "Endüstriyel tesiste gaz alarmı çaldığında ilk ne yapılmalıdır?", Order = 1 };
        var s4q2 = new SimulationQuestion { SimulationId = sim4.Id, Text = "Gaz kaçağı tespit edildiğinde hangi ekipman kullanılmalıdır?", Order = 2 };
        var s4q3 = new SimulationQuestion { SimulationId = sim4.Id, Text = "Tahliye sırasında asansör kullanılmalı mıdır?", Order = 3 };
        var s4q4 = new SimulationQuestion { SimulationId = sim4.Id, Text = "Acil durum sonrası tesis ne zaman tekrar devreye alınabilir?", Order = 4 };

        sim4.Questions = new List<SimulationQuestion> { s4q1, s4q2, s4q3, s4q4 };

        context.Simulations.Add(sim4);
        context.SimulationOptions.AddRange(new List<SimulationOption>
        {
            new() { QuestionId = s4q1.Id, Text = "Acil durum planını devreye alıp tahliyeye başlayın", IsCorrect = true, Explanation = "Gaz alarmı çaldığında önceden belirlenmiş acil durum planı hemen devreye alınmalıdır." },
            new() { QuestionId = s4q1.Id, Text = "Alarmı kapatıp çalışmaya devam edin", IsCorrect = false, Explanation = "Alarmı görmezden gelmek can kayıplarına yol açabilir." },
            new() { QuestionId = s4q1.Id, Text = "Kaçak yerini bulmaya çalışın", IsCorrect = false, Explanation = "Önce tahliye yapılmalı, kaçak tespiti uzman ekipler tarafından yapılmalıdır." },

            new() { QuestionId = s4q2.Id, Text = "Gaz dedektörü ve kişisel koruyucu donanım (KKD)", IsCorrect = true, Explanation = "Gaz kaçağında portatif gaz dedektörü ve uygun KKD kullanılmalıdır." },
            new() { QuestionId = s4q2.Id, Text = "Standart iş eldiveni yeterlidir", IsCorrect = false, Explanation = "Gaz kaçağında standart eldiven koruma sağlamaz, özel KKD gerekir." },
            new() { QuestionId = s4q2.Id, Text = "Herhangi bir ekipmana gerek yoktur", IsCorrect = false, Explanation = "Gaz kaçağında ekipmansız müdahale hayati tehlike oluşturur." },

            new() { QuestionId = s4q3.Id, Text = "Hayır, asansör kesinlikle kullanılmamalıdır", IsCorrect = true, Explanation = "Acil durumlarda asansörler tehlike oluşturur, merdiven kullanılmalıdır." },
            new() { QuestionId = s4q3.Id, Text = "Evet, daha hızlı tahliye sağlar", IsCorrect = false, Explanation = "Asansörler acil durumlarda arızalanabilir ve can kaybına yol açabilir." },
            new() { QuestionId = s4q3.Id, Text = "Sadece üst katlardakiler kullanabilir", IsCorrect = false, Explanation = "Hiçbir durumda asansör kullanılmamalıdır." },

            new() { QuestionId = s4q4.Id, Text = "Yetkili ekip güvenlik kontrolünü tamamladıktan sonra", IsCorrect = true, Explanation = "Tesis ancak uzman ekibin güvenlik onayı verdikten sonra devreye alınabilir." },
            new() { QuestionId = s4q4.Id, Text = "Alarm sustuktan hemen sonra", IsCorrect = false, Explanation = "Alarmın susması güvenliğin sağlandığı anlamına gelmez." },
            new() { QuestionId = s4q4.Id, Text = "Ertesi gün", IsCorrect = false, Explanation = "Süre değil, güvenlik kontrolü belirleyicidir." },
        });

        // Survey 1 (mevcut)
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

        // Survey 2: Ev İçi Güvenlik Değerlendirmesi
        var survey2 = new Survey
        {
            Title = "Ev İçi Doğalgaz Güvenliği Değerlendirmesi",
            Description = "Evinizdeki doğalgaz güvenlik önlemlerini değerlendirin ve eksiklerinizi keşfedin.",
            IsActive = true
        };

        var s2sq1 = new SurveyQuestion { SurveyId = survey2.Id, Text = "Evinizde doğalgaz dedektörü var mı?", Type = QuestionType.TrueFalse, Weight = 25, Order = 1, AgeGroupFilter = AgeGroup.Adult, SubscriptionFilter = SubscriptionType.Bireysel };
        var s2sq2 = new SurveyQuestion { SurveyId = survey2.Id, Text = "Doğalgaz vanasının yerini biliyor musunuz?", Type = QuestionType.TrueFalse, Weight = 20, Order = 2 };
        var s2sq3 = new SurveyQuestion { SurveyId = survey2.Id, Text = "Son 1 yılda kombi bakımı yaptırdınız mı?", Type = QuestionType.SingleChoice, Weight = 25, Order = 3, SubscriptionFilter = SubscriptionType.Bireysel };
        var s2sq4 = new SurveyQuestion { SurveyId = survey2.Id, Text = "Evinizde kaç doğalgaz cihazı bulunuyor?", Type = QuestionType.SingleChoice, Weight = 10, Order = 4 };
        var s2sq5 = new SurveyQuestion { SurveyId = survey2.Id, Text = "Ev içi doğalgaz güvenliğinizi nasıl değerlendirirsiniz?", Type = QuestionType.Scale, Weight = 20, Order = 5 };

        survey2.Questions = new List<SurveyQuestion> { s2sq1, s2sq2, s2sq3, s2sq4, s2sq5 };

        context.Surveys.Add(survey2);
        context.SurveyOptions.AddRange(new List<SurveyOption>
        {
            new() { QuestionId = s2sq1.Id, Text = "Evet", Value = 100 },
            new() { QuestionId = s2sq1.Id, Text = "Hayır", Value = 0 },

            new() { QuestionId = s2sq2.Id, Text = "Evet", Value = 100 },
            new() { QuestionId = s2sq2.Id, Text = "Hayır", Value = 0 },

            new() { QuestionId = s2sq3.Id, Text = "Evet, düzenli yaptırıyorum", Value = 100 },
            new() { QuestionId = s2sq3.Id, Text = "Hayır, hiç yaptırmadım", Value = 0 },
            new() { QuestionId = s2sq3.Id, Text = "Hatırlamıyorum", Value = 25 },

            new() { QuestionId = s2sq4.Id, Text = "1 (sadece kombi)", Value = 30 },
            new() { QuestionId = s2sq4.Id, Text = "2-3 (kombi + ocak)", Value = 60 },
            new() { QuestionId = s2sq4.Id, Text = "4 ve üzeri", Value = 100 },
        });

        // Survey 3: Endüstriyel Bilgi Testi
        var survey3 = new Survey
        {
            Title = "Endüstriyel Doğalgaz Bilgi Testi",
            Description = "Endüstriyel tesislerde doğalgaz güvenliği bilginizi test edin.",
            IsActive = true
        };

        var s3sq1 = new SurveyQuestion { SurveyId = survey3.Id, Text = "İş yerinizde gaz algılama sistemi mevcut mu?", Type = QuestionType.TrueFalse, Weight = 20, Order = 1, SubscriptionFilter = SubscriptionType.Endustriyel };
        var s3sq2 = new SurveyQuestion { SurveyId = survey3.Id, Text = "Doğalgaz basınç regülatörünün görevi nedir?", Type = QuestionType.SingleChoice, Weight = 25, Order = 2 };
        var s3sq3 = new SurveyQuestion { SurveyId = survey3.Id, Text = "Endüstriyel tesislerde gaz kaçağı risk değerlendirmesi ne sıklıkla yapılmalıdır?", Type = QuestionType.SingleChoice, Weight = 30, Order = 3 };
        var s3sq4 = new SurveyQuestion { SurveyId = survey3.Id, Text = "Doğalgaz patlamasının alt patlama sınırı (LEL) yüzde kaçtır?", Type = QuestionType.SingleChoice, Weight = 25, Order = 4 };

        survey3.Questions = new List<SurveyQuestion> { s3sq1, s3sq2, s3sq3, s3sq4 };

        context.Surveys.Add(survey3);
        context.SurveyOptions.AddRange(new List<SurveyOption>
        {
            new() { QuestionId = s3sq1.Id, Text = "Evet", Value = 100 },
            new() { QuestionId = s3sq1.Id, Text = "Hayır", Value = 0 },

            new() { QuestionId = s3sq2.Id, Text = "Gaz basıncını kullanım seviyesine düşürmek", Value = 100 },
            new() { QuestionId = s3sq2.Id, Text = "Gazı ısıtmak", Value = 0 },
            new() { QuestionId = s3sq2.Id, Text = "Gaz akışını ölçmek", Value = 25 },

            new() { QuestionId = s3sq3.Id, Text = "Yılda en az bir kez", Value = 100 },
            new() { QuestionId = s3sq3.Id, Text = "5 yılda bir", Value = 20 },
            new() { QuestionId = s3sq3.Id, Text = "Sadece kaza sonrası", Value = 0 },
            new() { QuestionId = s3sq3.Id, Text = "Ayda bir", Value = 80 },

            new() { QuestionId = s3sq4.Id, Text = "%5", Value = 100 },
            new() { QuestionId = s3sq4.Id, Text = "%15", Value = 0 },
            new() { QuestionId = s3sq4.Id, Text = "%25", Value = 0 },
            new() { QuestionId = s3sq4.Id, Text = "%1", Value = 0 },
        });

        // Survey 4: Çocuklar İçin
        var survey4 = new Survey
        {
            Title = "Çocuklar İçin Doğalgaz Bilgi Anketi",
            Description = "Doğalgaz güvenliği hakkında neler bildiğini test et!",
            IsActive = true
        };

        var s4sq1 = new SurveyQuestion { SurveyId = survey4.Id, Text = "Doğalgaz nasıl kokar?", Type = QuestionType.SingleChoice, Weight = 30, Order = 1, AgeGroupFilter = AgeGroup.Child };
        var s4sq2 = new SurveyQuestion { SurveyId = survey4.Id, Text = "Mutfaktaki ocağa dokunmalı mısın?", Type = QuestionType.TrueFalse, Weight = 35, Order = 2, AgeGroupFilter = AgeGroup.Child };
        var s4sq3 = new SurveyQuestion { SurveyId = survey4.Id, Text = "Doğalgaz tehlikesi olduğunda ne yapmalısın?", Type = QuestionType.SingleChoice, Weight = 35, Order = 3, AgeGroupFilter = AgeGroup.Child };

        survey4.Questions = new List<SurveyQuestion> { s4sq1, s4sq2, s4sq3 };

        context.Surveys.Add(survey4);
        context.SurveyOptions.AddRange(new List<SurveyOption>
        {
            new() { QuestionId = s4sq1.Id, Text = "Çürük yumurta gibi kokar", Value = 100 },
            new() { QuestionId = s4sq1.Id, Text = "Çiçek gibi kokar", Value = 0 },
            new() { QuestionId = s4sq1.Id, Text = "Hiç kokusu yoktur", Value = 0 },

            new() { QuestionId = s4sq2.Id, Text = "Hayır, asla dokunmamalıyım", Value = 100 },
            new() { QuestionId = s4sq2.Id, Text = "Evet, dokunabilirim", Value = 0 },

            new() { QuestionId = s4sq3.Id, Text = "Hemen bir büyüğe söyle ve evden uzaklaş", Value = 100 },
            new() { QuestionId = s4sq3.Id, Text = "Kendi başına çözmeye çalış", Value = 0 },
            new() { QuestionId = s4sq3.Id, Text = "Odana git ve bekle", Value = 0 },
        });

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
