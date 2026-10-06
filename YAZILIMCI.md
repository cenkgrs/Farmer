# Yazılımcı — proje hafızası ve devir

Son güncelleme: **6 Ekim 2026 — Europe/Istanbul**.

## Şu an nerede kaldık?

**Unity 6000.3.25f1 LTS (e1dba0a9aba4)** Linux makinesine kuruldu ve `Unity -version` ile doğrulandı. Resmi URP şablonunun kaynak dosyaları depoya alındı. İlk editör çalıştırması **aktif lisans yok / exit 198** nedeniyle durdu. Paket importu, C# derlemesi, başlangıç sahnesi üretimi ve build henüz doğrulanmadı. Aktif hedef 0.1; şu an motor/proje kurulumu aşamasındayız.

Kullanıcı tek başına geliştirecek, Linux ve Windows bilgisayarlar arasında çalışacak. Python, C# ve Java deneyimi yüksek; oyun motoru deneyimi sınırlı ancak Unity'de Godot'tan daha deneyimli. Modelleri harici AI araçlarıyla üretebilir; biz prompt ve entegrasyon gereksinimlerini hazırlayacağız.

## Son devir: diğer bilgisayarda devam

Kullanıcı, Linux Hub girişinin **Signing in** aşamasında takılması üzerine bu makinedeki sorun giderme çalışmasını durdurup diğer bilgisayarda devam etmeyi istedi. Giriş sorununun kök nedeni doğrulanmadı; çözülmüş sayılmamalı. Linux kurulumunu yeniden denemek bir sonraki oturumun önceliği değildir.

Diğer bilgisayarda depoyu klonla veya temiz çalışma ağacında `git pull --ff-only` yap; önce bu dosyayı ve `docs/KURULUM.md` dosyasını oku. Unity Hub hesabı/lisansı ve **6000.3.25f1** editörü hazır olduktan sonra mevcut depo kökünü aç. İlk import ve C# derlemesini doğrula, üretilen `Packages/packages-lock.json` dosyasını kaydet. Ardından Farm sahnesini üret ve Play modunda kontrol et. `BuildLinux` yardımcısı Linux hedeflidir; Windows doğrulaması için Windows build hedefini kullan. Henüz oynanabilir oyun, başarılı import veya doğrulanmış build yok.

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
| Tam editör yaması / paketler | Editör 6000.3.25f1 olarak sabitlendi. URP 17.3.0, Input System 1.20.0, Test Framework 1.6.0 ve uGUI 2.0.0 manifestte sabit. Kilit dosyası ilk başarılı importta üretilecek. |
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
- Unity Hub CLI kurucusu ARM AppImage indirdi. Resmi Linux belgesindeki doğrudan x86_64 dosyası indirilerek mimari doğrulandı ve değiştirildi. Hub 3.22.2 penceresi X11 pencere ağacında doğrulandı. AppImage normal bağlama yolunda takıldığı için `--appimage-extract-and-run` kullanıldı; yerel masaüstü kısayolu ve unityhub:// bağlantı işleyicisi kaydedildi. Farmer depo kökü Hub proje listesinde. Kullanıcı hesap/lisans etkinleştirmesini tamamlayacak.
- `unity auth login` tarayıcı girişi kullanıcıdan istendi ancak zaman aşımına uğradı. Hesap/lisans henüz etkin değil.
- Git LFS kurulu değil. Henüz model yok; LFS filtreleri etkinleştirilmedi. İki küçük konsept PNG normal Git'te tutulacak.
- Resmi şablon denemesi lisans nedeniyle exit 198 ile durdu. Şablon kaynakları korundu; çalıştırılabilir oyun/build yok.
- `Assets/_Farmer/Editor/ProjectSetup.cs`: başlangıç sahnesi üretme, yapılandırma doğrulama ve Linux build araçları. Henüz derlenmedi/çalıştırılmadı.
- Şablondan gelen SampleScene mevcut; `Assets/_Farmer/Scenes/Farm.unity` lisans sonrasında üretilecek.
- Manifest: URP 17.3.0, Input System 1.20.0, Test Framework 1.6.0, uGUI 2.0.0. Kullanılmayan şablon paketleri çıkarıldı. Şablon kilidi Input System sürümüyle tutarsızdı; kaldırıldı. İlk başarılı importun ürettiği kilit dosyası commit edilmeli.
- Belge bağlantıları, asset/meta eşleri, benzersiz GUID değerleri, manifest JSON, tam editör sürümü ve Force Text/Visible Meta Files ayarları statik olarak kontrol edildi. Oyun içi doğrulama değildir.

## Sıradaki somut işler

1. Kullanıcının seçtiği diğer bilgisayarda devam et: depoyu eşitle, Unity Hub hesabı girişini ve hesaba uygun lisansı tamamla; tam editör sürümü 6000.3.25f1 olmalı.
2. Depo kökünü Unity 6000.3.25f1 ile aç; Package Manager çözümlemesini ve Console hatalarını kontrol et. `packages-lock.json` değişikliklerini gerçek importtan sonra kaydet.
3. `Farmer.Editor.ProjectSetup.CreateInitialScene` aracını bir kez çalıştır; `Farm.unity` mevcutsa üzerine yazmadan `ValidateProject` kullan.
4. Devam edilen bilgisayarda gerçek editör açılışı, o platformun masaüstü build'i ve sahnenin görsel kontrolünü tamamla. Şimdiye kadar bunların geçtiği iddia edilmedi.
5. Ardından 0.1 hareket/kare seçimi ve tek ürün döngüsüne geç. Diğer platformdaki doğrulamayı ayrı bir açık iş olarak tut.

## Son oturum kaydı

**2026-10-06:** İlk push sonrası kullanıcı kuruluma başlama talimatı verdi. Unity CLI ve 6000.3.25f1 editörü kuruldu; resmi şablon kaynakları alındı, sürüm sabitlendi ve başlangıç sahnesi için editör aracı yazıldı. Hesap/lisans eksikliği nedeniyle ilk import ve çalıştırma bekliyor. Git devir belgeleri ve Linux/Windows kurulum rehberi güncellendi.

**2026-10-06 — bilgisayar değişimi:** Linux Hub penceresi açıldı ancak kullanıcı Signing in ekranında takıldığını bildirdi. Sorun giderme kullanıcı isteğiyle durduruldu. Proje kaynakları `c34317d` commitinde mevcut; bu devir notu sonraki committe kaydedilir. Giriş bilgileri ve yerel Hub günlükleri depoya eklenmedi. Diğer bilgisayarda ilk import/derleme/sahne doğrulamasıyla devam edilecek.
