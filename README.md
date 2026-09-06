# MailFlow

MailFlow; firma bilgilerini ve e-posta şablonlarını tek yerde yöneterek, seçilen firmalar için Gmail üzerinde PDF ekli taslaklar oluşturan ASP.NET Core MVC uygulamasıdır.

> Uygulama e-postayı otomatik olarak göndermez. Hazırlanan ileti Gmail hesabının **Taslaklar** klasörüne kaydedilir ve gönderilmeden önce kullanıcı tarafından gözden geçirilebilir.

## Özellikler

- Firma adı, LinkedIn adresi ve e-posta bilgisiyle firma kaydı oluşturma
- E-posta alan adını kullanarak mükerrer firma kaydını engelleme
- E-posta veya alan adına göre firma arama
- Konu, içerik ve en fazla 10 MB PDF eki içeren e-posta şablonları oluşturma
- Kayıtlı şablonları güncelleme ve silme
- Google OAuth ile Gmail hesabına bağlanma
- Seçilen firma ve şablon için Gmail taslağı oluşturma
- Taslak oluşturulan ve henüz işlenmemiş firmaları ayrı listelerde izleme
- Entity Framework Core migration'larını uygulama başlangıcında otomatik çalıştırma

## Kullanılan Teknolojiler

- .NET 8
- ASP.NET Core MVC
- Entity Framework Core 8
- SQL Server / LocalDB
- FluentValidation
- Google OAuth 2.0 ve Gmail API
- Bootstrap ve jQuery

## Proje Yapısı

```text
MailFlow.sln
├── MailFlow.Presentation/  # MVC arayüzü, doğrulamalar ve Gmail entegrasyonu
├── MailFlow.Business/      # İş kuralları, servisler ve modeller
├── MailFlow.DataAccess/    # EF Core DbContext, repository ve migration'lar
└── MailFlow.Entities/      # Veritabanı varlıkları
```

## Gereksinimler

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server veya SQL Server LocalDB
- Gmail taslağı özelliği için Gmail API etkinleştirilmiş bir Google Cloud projesi

Gmail bağlantısı yapılandırılmadan firma ve şablon yönetimi kullanılabilir; yalnızca Gmail taslağı oluşturma özelliği devre dışı kalır.

## Kurulum

Depoyu klonlayın ve proje dizinine geçin:

```bash
git clone https://github.com/gokhanguneyy/MailFlow.git
cd "MailFlow"
```

Bağımlılıkları yükleyin:

```bash
dotnet restore MailFlow.sln
```

`MailFlow.Presentation/appsettings.json` içindeki `DefaultConnection` değerini kendi SQL Server ortamınıza göre düzenleyin. Varsayılan geliştirme bağlantısı şöyledir:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=MailFlowDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

Uygulamayı çalıştırın:

```bash
dotnet run --project MailFlow.Presentation
```

Ardından tarayıcıda `https://localhost:7122` adresini açın. İlk çalıştırmada mevcut EF Core migration'ları otomatik olarak uygulanır ve `MailFlowDb` veritabanı oluşturulur.

Yerel HTTPS sertifikası için gerekirse şu komutu çalıştırın:

```bash
dotnet dev-certs https --trust
```

## Gmail API Yapılandırması

1. Google Cloud Console'da bir proje oluşturun.
2. **Gmail API** hizmetini etkinleştirin.
3. OAuth izin ekranını yapılandırın ve geliştirme aşamasında kullanacağınız hesabı test kullanıcısı olarak ekleyin.
4. **Web application** türünde bir OAuth 2.0 istemcisi oluşturun.
5. Yetkilendirilmiş yönlendirme URI'si olarak aşağıdaki adresi ekleyin:

```text
https://localhost:7122/signin-google
```

İstemci bilgilerini kaynak kontrolüne eklemeden .NET user-secrets ile kaydedin:

```bash
dotnet user-secrets set "Google:ClientId" "GOOGLE_CLIENT_ID" --project MailFlow.Presentation
dotnet user-secrets set "Google:ClientSecret" "GOOGLE_CLIENT_SECRET" --project MailFlow.Presentation
```

Uygulamayı yeniden başlattığınızda **Taslak Oluştur** ekranındaki **Gmail'e Bağlan** düğmesi kullanılabilir hale gelir. Uygulama yalnızca taslak oluşturmak için gereken `gmail.compose` iznini ister.

## Kullanım Akışı

1. Ana sayfadan firma adı, LinkedIn adresi ve e-posta bilgisiyle bir firma kaydedin.
2. **Mail Şablonu** sayfasında başlık, konu, e-posta metni ve PDF dosyası içeren bir şablon oluşturun.
3. **Taslak Oluştur** sayfasından Gmail hesabınıza bağlanın.
4. Bir şablon seçin ve ilgili firmanın yanındaki **Taslak Oluştur** düğmesine basın.
5. Oluşturulan iletiyi Gmail'in **Taslaklar** klasöründe kontrol edin ve hazır olduğunda gönderin.

Her firma alan adı için yalnızca bir taslak kaydı oluşturulur. Başarıyla işlenen firma, bekleyen firmalar listesinden çıkarak oluşturulan taslaklar listesine taşınır.

## Yapılandırma

| Anahtar | Açıklama | Zorunlu |
| --- | --- | --- |
| `ConnectionStrings:DefaultConnection` | SQL Server bağlantı dizesi | Evet |
| `Google:ClientId` | Google OAuth istemci kimliği | Gmail entegrasyonu için |
| `Google:ClientSecret` | Google OAuth istemci sırrı | Gmail entegrasyonu için |

Yüklenen PDF dosyaları geliştirme ortamında `MailFlow.Presentation/wwwroot/uploads/mail-sablonu` dizininde saklanır. Üretim ortamında OAuth sırlarını güvenli bir secret store içinde tutun; yüklenen belgeler için de erişim kontrollü kalıcı depolama kullanın.

## Derleme

```bash
dotnet build MailFlow.sln
```

