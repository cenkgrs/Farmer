# Yazılımcı — proje hafızası ve devir

Son güncelleme: **7 Ekim 2026 — Europe/Istanbul**.

## Şu an nerede kaldık?

**Unity 6000.3.25f1 LTS (e1dba0a9aba4)** Linux makinesine kuruldu ve `Unity -version` ile doğrulandı. Resmi URP şablonunun kaynak dosyaları depoya alındı. 7 Ekim tarihinde aktif lisans doğrulandı; ilk paket importu, C# derlemesi ve başlangıç sahnesi üretimi başarıyla tamamlandı. Linux build ve görsel kontrol sonucu aşağıdaki doğrulama kaydında tutulur. Aktif hedef 0.1; **karakter hareketi ve tarla karesi seçimi artık çalışıyor**. Sıradaki adım veri tanımlı turp/tohum/ekonomi döngüsü. 0.1 bütünü henüz tamamlanmadı.

Kullanıcı tek başına geliştirecek, Linux ve Windows bilgisayarlar arasında çalışacak. Python, C# ve Java deneyimi yüksek; oyun motoru deneyimi sınırlı ancak Unity'de Godot'tan daha deneyimli. Modelleri harici AI araçlarıyla üretebilir; biz prompt ve entegrasyon gereksinimlerini hazırlayacağız.

## Güncel çalışma yeri

Kullanıcı 7 Ekim 2026'da diğer bilgisayarda değişiklik yapmadığını belirtti ve bu Linux bilgisayarda devam edilmesini istedi. Önceki Windows'a devir önceliği kalktı. Git main/origin/main eşitliği kontrol edildi. Unity hesap girişi ve lisansı artık aktif; önceki Signing in sorunu yeniden üretilmedi ve kök nedenine ilişkin bir düzeltme iddia edilmiyor.

## Kesinleşen kararlar

- Sabit açılı izometrik kamera ve stilize 3D görsel dil.
- `docs/references/` altındaki iki konseptin sıcak, yumuşak, boyanmış hissi veren tasarım dili kullanıcı tarafından beğenildi. Kamera referansı izometrik görseldir.
- Çiftlik huzurlu; dışarıda basit moblar ve hafif risk bulunur.
- Pazardan çeşitli tohumlar alma, görünür büyüme, bakım, hasat, satış ve üretimde kullanım ana sistemdir.
- Ahşap blokları üst üste/yan yana yerleştirme; çit, kapı, çatı ve iç mekân eşyalarıyla oyuncunun kendi evini yapabilmesi gerekir.
- Sürüm sürüm geliştirme; kapsam `docs/YOL_HARITASI.md` içinde.
- Kaynak kodu, kararlar, asset rehberi ve devir bilgisi Git'te tutulacak.

## Öneriler ve açık kararlar

| Konu | Durum |
|---|---|
| Motor | Kullanıcının deneyimine göre Unity 6.3 LTS + C# + URP yönüne geçildi. |
| Tam editör yaması / paketler | Editör 6000.3.25f1 olarak sabitlendi. URP 17.3.0, Input System 1.20.0, Test Framework 1.6.0 ve uGUI 2.0.0 manifestte sabit. Kilit dosyası gerçek editör importuyla üretildi. Audio, Physics ve Screen Capture yerleşik modülleri 1.0.0 sürümüyle açıkça tanımlandı. |
| Geliştirme sistemleri | Linux ve Windows — kullanıcı tarafından belirtildi. |
| Yayın hedefi | İlk prototip masaüstü PC varsayımı; mağaza/diğer platformlar seçilmedi. |
| Uzak Git deposu | `origin`: `https://github.com/cenkgrs/Farmer.git`. Özel `cenkgrs/Farmer` deposu doğrulandı; ilk push başarılı, `main` dalı `origin/main` izliyor. |
| Eşzamanlı çalışma | Sırayla devir varsayımı; aynı anda çalışılırsa ayrı görev dalları kullanılacak. |
| Donanım | Linux: Ubuntu 24.04 x86_64/X11, RTX 4090, NVIDIA 595.91.07, yaklaşık 94 GiB RAM. Windows donanımı henüz bilinmiyor. |
| Asset üretim aracı | Kullanıcı harici AI aracı kullanacak; marka/araç henüz belirtilmedi. |

## Hazırlananlar

- `AGENTS.md`: yeni oturumların başlangıç ve devir kuralları.
- Bu dosya: proje durumu ve sıradaki adımlar.
- Oyun tasarımı, sürüm kabul ölçütleri, motor karşılaştırması, model teslim kuralları ve ilk promptlar.
- İki konsept görselinin depo içindeki kopyaları.
- Git ignore ve platformlar arası satır sonu kuralları.

## Teknik durum ve kontroller

- Başlangıçta kullanılabilir Git deposu yoktu; yerel `main` oluşturuldu.
- Yalnızca repo kimliği `Cenk Gürses <cenkgrs@gmail.com>` olarak ayarlandı. Global Git config değiştirilmedi. Kesin commit ve çalışma ağacı durumu için `git log -3 --oneline` ve `git status --short --branch` kullan.
- Origin doğrulandı; `d3ce1b4` ilk commit'i GitHub main dalına gönderildi. Diğer bilgisayardan klonlanabilir; o bilgisayarda klon/kurulum henüz doğrulanmadı.
- Unity CLI 1.0.0-beta.12 resmi betikle kuruldu. Editör: `~/Unity/Hub/Editor/6000.3.25f1/Editor/Unity`.
- Unity Hub CLI kurucusu ARM AppImage indirdi. Resmi Linux belgesindeki doğrudan x86_64 dosyası indirilerek mimari doğrulandı ve değiştirildi. Hub 3.22.2 penceresi X11 pencere ağacında doğrulandı. AppImage normal bağlama yolunda takıldığı için `--appimage-extract-and-run` kullanıldı; yerel masaüstü kısayolu ve unityhub:// bağlantı işleyicisi kaydedildi. Farmer depo kökü Hub proje listesinde. Hesap/lisans 7 Ekim kontrolünde aktif.
- `unity auth login` tarayıcı girişi kullanıcıdan istendi ancak zaman aşımına uğradı. Bu önceki denemenin sonucudur; 7 Ekim kontrolünde hesap ve lisans aktif.
- Git LFS kurulu değil. Henüz model yok; LFS filtreleri etkinleştirilmedi. İki küçük konsept PNG normal Git'te tutulacak.
- Resmi şablon denemesi lisans nedeniyle exit 198 ile durdu. Şablon kaynakları korundu; bu engel 7 Ekim aktif lisans kontrolüyle kalktı.
- `Assets/_Farmer/Editor/ProjectSetup.cs`: başlangıç sahnesi üretme, yapılandırma doğrulama ve Linux build araçları. Derlendi; sahne üretimi ve ValidateProject başarıyla çalıştı.
- Şablondan gelen SampleScene korunuyor; `Assets/_Farmer/Scenes/Farm.unity` üretildi ve build sahnesi olarak ayarlandı. Sabit ortografik kamera, 6×6 tarla, hareketli geçici karakter, çarpışmalı sandık, kare seçimi ve Türkçe kontrol/geri bildirim arayüzü içeriyor.
- Manifest: URP 17.3.0, Input System 1.20.0, Test Framework 1.6.0, uGUI 2.0.0. Kullanılmayan şablon paketleri çıkarıldı. Şablon kilidi Input System sürümüyle tutarsızdı; kaldırıldı. Gerçek importun ürettiği kilit dosyası artık depoda; manifestle birlikte korunmalı.
- Belge bağlantıları, asset/meta eşleri, benzersiz GUID değerleri, manifest JSON, tam editör sürümü ve Force Text/Visible Meta Files ayarları statik olarak kontrol edildi. Oyun içi doğrulama değildir.

## 7 Ekim 2026 doğrulama kaydı

- `unity license status --json`: hesap girişi ve aktif lisans doğrulandı.
- İlk import, C# derlemesi ve `CreateInitialScene` exit 0 ile tamamlandı; `FARMER_SETUP_OK` ve `FARMER_VALIDATION_OK` görüldü.
- `BuildLinux`: exit 0, `FARMER_BUILD_OK`. Çıktı `builds/Linux/Farmer.x86_64`; build ve günlükler Git dışında.
- Gerçek Linux player 1280×720 pencerede çalıştırıldı; exit 0, `FARMER_PLAYER_SMOKE_OK`. `builds/QA/farm-setup.png` incelendi: ortografik kadraj, yeşil zemin ve kahverengi tarla görünür, eksik/pembe malzeme yok. Bu kontrol editörde Play modu testi değildir.
- Geliştirme build'ine özel `--farmer-smoke-capture` seçeneği, PNG çıktısı ve çalıştırma kontrolü eklendi. Normal açılışı değiştirmez. Screen Capture ve Audio yerleşik modülleri manifestte açıkça tanımlandı.
- İlk build denemesinin oturumu kesildi; başarı kabul edilmedi. Kalıcı `Logs/setup-build.log` ile yeniden çalıştırılan build başarıyla tamamlandı.
- Build sonrası Unity kapanışında `.NET SDK/build-server` bulunamadı mesajı görüldü; build raporu ve çıkış kodu başarılı. Player'da ekran DPI bilgisi alınamadı uyarıları ve kapanışta motorun allocation raporu mevcut; sahne çalışmasını engellemedi. Uzun süreli performans/bellek testi yapılmadı.
- GUI editörünün ilk kullanım onayı sonrasında ana Unity penceresinin açıldığı doğrulandı. Kullanıcı otomatik test/build için editörü kapattı. Windows açılışı/build'i ve editör Play modu ayrıca denenmedi; oynanış Linux player üzerinde doğrulanıyor.

## 0.1 ilk oynanabilir adım — hareket ve kare seçimi

- WASD/ok tuşları, kameraya göre yön, çapraz hız sınırı, CharacterController çarpışması ve arazi kenarında kalma.
- 6×6 tarla; fareyle kare önizlemesi, 2.5 birim yakınlıkta sol tıkla seçim, uzak karede yaklaşma bildirimi, ESC/sağ tıkla temizleme. HUD üstündeki tıklamalar dünyaya geçmez. Dünya sınırı karakter merkezi için ±9.3; bunlar değiştirilebilir prototip değerleridir.
- `FarmGridLayout` ve `PlanarMovement` hesapları sunumdan ayrı. `PlayerMotor`, `FarmSelection`, `PrototypeHud` sahne bileşenleri; runtime ve EditMode test assembly'leri ayrıldı.
- `MovementPrototypeSetup.UpgradeScene` ilk kurulum sahnesine bir kez ekleme yapar; mevcut hareket prototipinin üzerine yazmayı reddeder. Sonraki oturumlarda tekrar çalıştırma; sahne ve GUID'ler depoda hazır.
- Karakter/şapka/sandık geçici geometrilerden oluşuyor. Onaylanan konsept görsel kalitesine ulaşıldığı iddia edilmiyor. Ürün, pazar, para, envanter, gün, kayıt/yükleme ve inşaat henüz yok.
- **12/12 EditMode testi başarılı**, başarısız/atlanan test yok. Negatif koordinatlar, üst sınır, hücre merkezi dönüşümü, erişim mesafesi, çapraz hareket ve dünya sınırı kontrol edildi.
- Linux geliştirme build'i başarılı. `--farmer-check-controls` ile Input System'e sanal klavye/fare olayları gönderilen gerçek player denemesinde **11 kontrol başarılı**: odak, hareket/yön, sandık çarpışması, dünya sınırı, yakın kare, HUD engellemesi, ESC, uzak kare, ekran dışı hover ve sabit kamera. Çıkış 0 ve `FARMER_PLAYER_SMOKE_OK`.
- 1280×720 player görüntüsü görsel olarak incelendi; karakter, grid, seçim çerçevesi ve Türkçe HUD görünüyor. Güncel görüntü `docs/screenshots/movement-prototype.png` içinde. Kontroller otomatik; kullanıcıyla manuel oynanış hissi değerlendirmesi henüz yapılmadı. Test komutları `docs/TESTLER.md` içinde. Yerel günlükler `Logs/movement-*.log`, XML sonucu `Logs/movement-tests.xml`; Git'e girmez.

## Sıradaki somut işler

1. Veri tanımlı tek ürün (turp), tohum satın alma, envanter/para, ekim/sulama/büyüme/hasat/satış döngüsünü ekle; ekonomi ve ürün kurallarını görüntüden ayır.
2. Gün ilerletme ve kayıt/yükleme ekle; yol haritasındaki kalan 0.1 kabul ölçütlerini test et.
3. Windows'ta aynı editör sürümüyle açılış ve build ayrıca doğrulanmalı. Mevcut Farm sahnesini yeniden üretme; ValidateProject kullan.

## Son oturum kaydı

**2026-10-06:** İlk push sonrası kullanıcı kuruluma başlama talimatı verdi. Unity CLI ve 6000.3.25f1 editörü kuruldu; resmi şablon kaynakları alındı, sürüm sabitlendi ve başlangıç sahnesi için editör aracı yazıldı. Hesap/lisans eksikliği nedeniyle ilk import ve çalıştırma bekliyor. Git devir belgeleri ve Linux/Windows kurulum rehberi güncellendi.

**2026-10-06 — bilgisayar değişimi:** Linux Hub penceresi açıldı ancak kullanıcı Signing in ekranında takıldığını bildirdi. Sorun giderme kullanıcı isteğiyle durduruldu. Proje kaynakları `c34317d` commitinde mevcut; bu devir notu sonraki committe kaydedilir. Giriş bilgileri ve yerel Hub günlükleri depoya eklenmedi. Diğer bilgisayarda ilk import/derleme/sahne doğrulamasıyla devam edilecek.

**2026-10-07:** Kullanıcı diğer bilgisayarda işlem yapmadığını belirtti; Linux üzerinde devam edildi. Aktif lisansla paketler çözüldü, Farm sahnesi üretildi, Linux geliştirme build'i ve gerçek player ekran görüntüsü doğrulandı. Sonraki adım 0.1 hareket/kare seçimi.

**2026-10-07 — hareket prototipi:** Kullanıcının devam talimatıyla karakter hareketi, çarpışma, 6×6 grid, erişim kontrollü kare seçimi ve HUD eklendi. 12 EditMode testi ve 11 gerçek player kontrolü geçti; Linux build ve ekran görüntüsü doğrulandı. Sonraki özellik tarım/ekonomi döngüsü.
