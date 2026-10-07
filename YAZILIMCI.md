# Yazılımcı — proje hafızası ve devir

Son güncelleme: **7 Ekim 2026 — Europe/Istanbul**.

## Şu an nerede kaldık?

**Unity 6000.3.25f1 LTS (e1dba0a9aba4)** Linux makinesine kuruldu ve `Unity -version` ile doğrulandı. Resmi URP şablonunun kaynak dosyaları depoya alındı. 7 Ekim tarihinde aktif lisans doğrulandı; ilk paket importu, C# derlemesi ve başlangıç sahnesi üretimi başarıyla tamamlandı. Linux build ve görsel kontrol sonucu aşağıdaki doğrulama kaydında tutulur. **0.1 çekirdek prototipi Linux üzerinde tamamlandı:** hareket, kare seçimi, tohum satın alma, ekim/sulama, dört görünür büyüme aşaması, hasat/satış, gün ilerletme ve kayıt/yükleme çalışıyor. Sıradaki geliştirme 0.2 modüler üs kurmanın ilk küçük adımı. Kullanıcının ilk oynanış geri bildirimi uygulandı: tıkla-seç adımı kaldırıldı, fare hedefi üzerinde eldeki eşya ile doğrudan sol tık etkileşimine geçildi. Sulama kabıyla sol tuşu basılı tutup kareler arasında gezdirilir; su sesi döngüsü eklenmiştir. Her ekimde tek sulama yeterli; hasat süresi üç gece olarak korundu. Görseller geçici; Windows doğrulaması açık.

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

## 0.1 ilk oynanabilir adım — hareket ve kare seçimi (tarihsel)

- WASD/ok tuşları, kameraya göre yön, çapraz hız sınırı, CharacterController çarpışması ve arazi kenarında kalma.
- 6×6 tarla; fareyle kare önizlemesi, 2.5 birim yakınlıkta sol tıkla seçim, uzak karede yaklaşma bildirimi, ESC/sağ tıkla temizleme. HUD üstündeki tıklamalar dünyaya geçmez. Dünya sınırı karakter merkezi için ±9.3; bunlar değiştirilebilir prototip değerleridir.
- `FarmGridLayout` ve `PlanarMovement` hesapları sunumdan ayrı. `PlayerMotor`, `FarmSelection`, `PrototypeHud` sahne bileşenleri; runtime ve EditMode test assembly'leri ayrıldı.
- `MovementPrototypeSetup.UpgradeScene` ilk kurulum sahnesine bir kez ekleme yapar; mevcut hareket prototipinin üzerine yazmayı reddeder. Sonraki oturumlarda tekrar çalıştırma; sahne ve GUID'ler depoda hazır.
- Karakter/şapka/sandık geçici geometrilerden oluşuyor. Onaylanan konsept görsel kalitesine ulaşıldığı iddia edilmiyor. Tarım/ekonomi ve kayıt sistemi aşağıdaki sonraki adımda eklendi. İnşaat henüz yok.
- **12/12 EditMode testi başarılı**, başarısız/atlanan test yok. Negatif koordinatlar, üst sınır, hücre merkezi dönüşümü, erişim mesafesi, çapraz hareket ve dünya sınırı kontrol edildi.
- Linux geliştirme build'i başarılı. `--farmer-check-controls` ile Input System'e sanal klavye/fare olayları gönderilen gerçek player denemesinde **11 kontrol başarılı**: odak, hareket/yön, sandık çarpışması, dünya sınırı, yakın kare, HUD engellemesi, ESC, uzak kare, ekran dışı hover ve sabit kamera. Çıkış 0 ve `FARMER_PLAYER_SMOKE_OK`.
- 1280×720 player görüntüsü görsel olarak incelendi; karakter, grid, seçim çerçevesi ve Türkçe HUD görünüyor. Bu adımın görüntüsü `docs/screenshots/movement-prototype.png` içinde. Kullanıcı bu adımın tamam olduğunu belirterek devam edilmesini istedi; aşağıdaki tarım döngüsünün manuel değerlendirmesi henüz yapılmadı. Test komutları `docs/TESTLER.md` içinde. Yerel günlükler `Logs/movement-*.log`, XML sonucu `Logs/movement-tests.xml`; Git'e girmez.

## 0.1 tarım ve ekonomi — ilk uygulama (etkileşim aşağıda güncellendi)

- `CropDefinition` ve `Data/Turnip.asset`: veri tanımlı ürün, fiyat, büyüme süresi, hasat miktarı ve dört prefab. Başlangıç 60 para; turp tohumu 10, satış 18 para, hasat 1 ürün, 3 sulanmış gece. Bunlar prototip denge değerleridir.
- `FarmModel`: sahne/girdi/dosya bağımlılığı olmayan C# kuralları. Ürün başına tohum/hasat envanteri, alım/satım, ekim, günlük sulama, büyüme, hasat ve doğrulamalı snapshot. Yeni ürün kuralları aynı modelle çalışır; mevcut oyuncu arayüzü 0.1 gereği tek üründür.
- `FarmGame`: yakınlık kontrollü pazar/kamp, seçili kare işlemi, otomatik kayıt. B: tohum al, V: hasadı sat, E: ek/sula/hasat, kamp yakınında N: ertesi gün. Sulanmayan ürün bekler; ölüm ve gerçek zamanlı bekleme yok.
- `FarmPresentation` ve `FarmHud`: dört görünür aşama, ıslak toprak, hasatta kısa ses ve +ürün bildirimi, para/envanter/gün, bağlama göre pazar/kamp panelleri ve işlem düğmesi. Pazar ve kamp geçici geometrilerdir. Filizler görsel kontrol sonrası büyütüldü.
- `FarmSaveStore`: sürüm 1 JSON, geçici dosyadan değiştirme, önceki sağlam kayıt yedeği, bozuk ana kayıttan yedeğe dönüş. İki dosya da bozuksa işlemler durur ve dosyalar korunur. Para, envanter, gün, ekili ürün/büyüme/sulama saklanır; karakter konumu saklanmaz. Kayıt yereldir, Git ile taşınmaz. Konum ve kurtarma ayrıntıları `docs/TESTLER.md` içinde.
- `FarmingPrototypeSetup.UpgradeScene` sahneye bir kez uygulandı; tekrar çalıştırma. `PolishPresentation` font ve prefab ölçeğini düzenlemek için kullanıldı. Güncel Farm sahnesi, veri, prefab ve meta dosyaları depoda hazır.

### Doğrulama

- **39/39 EditMode testi başarılı**, başarısız/atlanan yok: önceki 12 hareket/grid + 22 tarım mantığı + 5 dosya kayıt/kurtarma testi. XML: `Logs/farming-tests.xml`.
- Son Linux geliştirme build'i başarılı, `FARMER_BUILD_OK`; günlük `Logs/farming-build-final.log`.
- Gerçek Linux player'da **11 hareket/seçim + 41 tarım kontrolü başarılı**. Pazar düğmesine UI tıklaması ve B/E/N/V/F5/F9 tuş olaylarıyla tam döngü, kuru ürünün beklemesi, uzak hasat engeli, tekrar hasat engeli, yeniden yatırım ve kayıt/yükleme denendi. Çıkış 0, `FARMER_FARMING_CHECKS_FINISHED` ve `FARMER_PLAYER_SMOKE_OK`; günlük `Logs/farming-player-final.log`.
- Pazar, dört aşama ve son toplu görüntü incelendi; eksik/pembe malzeme veya kesilmiş HUD görülmedi. Güncel örnekler `docs/screenshots/farming-prototype.png` ve `docs/screenshots/farming-market.png`. Hasat sesinin oynatma durumu kontrol edildi; dinleme kalitesi değerlendirmesi yapılmadı.
- Smoke capture ayrı geçici kayıt kullanır ve normal oyuncu kaydını değiştirmez; sonunda test kayıtları temizlenir. Build, günlük ve QA ara çıktıları Git dışında.
- Windows, editör Play modu ve uzun süreli performans testi ayrıca yapılmadı. 0.2 inşaat, çoklu ürün seçimi, üretim ve moblar bu sürüme dahil değil.

## Güncel etkileşim — eldeki eşya ve doğrudan sol tık

Kullanıcı 7 Ekim'de önce kare seçme adımını kaldırmamızı, ardından WASD ile birlikte kullanım daha rahat olsun diye E yerine sol tık kullanmamızı istedi. Güncel davranış:

- **1:** turp tohumu; **2:** sulama kabı; **3:** orak. Alt eşya çubuğundan da kuşanılır. Alınan tohum adedi burada gösterilir; sulama kabı ve orak kalıcı başlangıç envanter eşyalarıdır. Genel çanta/alet satın alma sistemi henüz yok.
- Fare yakındaki kare üzerindeyken **sol tık** eldeki eşyayı doğrudan kullanır. Önce seçim adımı yoktur; E artık tarla işlemi yapmaz. Yanlış alet başka bir işe dönüşmez; tekrar ekim/sulama/hasat kaynak çoğaltmaz.
- HUD/pencere dışı/uzak karede işlem engellenir. Sol tık sırasında fare konumu yeniden okunur; aynı görüntü karesinde eski hedefe yanlış işlem yapılmaz. Tohum ve orak tek basışta tek işlem yapar. Sulama kabında sol tuş basılıyken erişilebilir kareler sulanır; zaten sulanmış kare tekrar işlem/kayıt üretmez. Alan etkisi yoktur.
- `FarmGame.WateringActive` / `WateringChanged`: basılı sulama ve sunum sesi. UI üzerinde başlayan tıklama dünyaya sürüklenerek sulama başlatamaz; dünya üzerinde uzakta başlayan basılı tutma, hedef erişime girince başlayabilir. Geçici HUD/uzak hedefte ses durur; aynı sulama hareketi geri dönünce sürer. Tuş bırakma, eşya değişimi, yükleme, odak kaybı, pause veya devre dışı bırakma hareketi iptal eder.
- `FarmPresentation`: özgün prosedürel, dört saniyelik su dökme döngüsü; ayrı looping AudioSource, yumuşak ses başlangıcı, sulama durunca Stop. Dış ses varlığı veya paket eklenmedi.
- `FarmModel.EquippedItem` / `UseEquipped` kuralları sunumdan ayrıdır. `FarmGame.UseHovered` erişimi kontrol eder; `FarmSelection` yalnızca imleç hedefini tutar. `FarmHud` eşya çubuğu ve hedef bilgisi gösterir.
- Tohum torbası, sulama kabı ve orak karakterin elinde geçici geometriyle görünür. Tohum sayısı sıfırsa elde torba gösterilmez. Modeller nihai sanat varlığı değildir.
- Kuşanılan eşya `FarmSnapshot.equippedItem` ile kaydedilir. Şema hâlâ sürüm 1; eski kayıtta bu alan yoksa tohum varsayılır. Para/tarla/envanter korunur, aletler başlangıç donanımı olarak bulunur.
- Tek sulama hasada kadar korunur; hiç sulanmayan ürün bekler, hasat sonrası yeni ekim yeniden bir kez sulanır. Süre üç gecedir. Eski kayıtta `growth > 0` olan ürün önceden sulanmış kabul edilir; ilerleme kaybolmaz.
- **44/44 EditMode testi geçti** (`Logs/equipment-watering-tests.xml`): eşya işlemleri, eski kayıt uyumluluğu, tek sulamanın kayıt sonrası sürmesi ve yeniden ekimde sıfırlanması dahil.
- Önceki eşya/tek sulama Linux build'i başarılı (`Logs/equipment-watering-build.log`). Gerçek player'da **11 hareket/hedef + 48 tarım kontrolü geçti**, çıkış 0 (`Logs/equipment-watering-player.log`). Döngüde hiç tarla tıklaması yok; tek sulamadan sonra yeniden sulanmadan üç gecede hasat doğrulandı; eşya düğmesi/1–2–3, yanlış alet, HUD üzerinde E, pencere dışı E, aynı görüntü karesinde fare hareketi + E ve kayıt/yükleme denendi.
- **Son sol tık doğrulaması:** Linux build ve 61 gerçek player kontrolü geçti; `Logs/click-build.log` / `Logs/click-player.log`. Arayüz yönlendirmeleri ve ekran görüntüleri güncellendi.
- Yeni eşya çubuğu, tohum torbası, sulama kabı ve orak oyun görüntülerinde incelendi. Güncel PNG'ler `docs/screenshots/farming-prototype.png` / `farming-market.png`. Kullanıcı sol tık etkileşimini beğendi; son basılı sulama/ses hissi değerlendirmesi ve Windows doğrulaması bekliyor.

## Sıradaki somut işler

1. Yeni eşya/hover etkileşimini kullanıcıyla değerlendir. Bir sonraki kapsam 0.2; ek kullanım sorunları bildirilirse önce bunları gider.
2. **0.2 ilk adım:** veri tanımlı ahşap blok, kare/hacim yerleşim modeli, geçerli/geçersiz önizleme, yan yana/üst üste koyma ve oyuncuyla çakışmayı engelleme. Kaynak harcaması ve kayıt şemasını birlikte ele al; mevcut tarla kayıtlarını korumadan şema değiştirme. 0.2'nin tamamını tek seferde yapmaya çalışma.
3. Windows'ta aynı editör sürümüyle açılış ve build ayrıca doğrulanmalı. Mevcut Farm sahnesini yeniden üretme; ValidateProject kullan.

## Son oturum kaydı

**2026-10-06:** İlk push sonrası kullanıcı kuruluma başlama talimatı verdi. Unity CLI ve 6000.3.25f1 editörü kuruldu; resmi şablon kaynakları alındı, sürüm sabitlendi ve başlangıç sahnesi için editör aracı yazıldı. Hesap/lisans eksikliği nedeniyle ilk import ve çalıştırma bekliyor. Git devir belgeleri ve Linux/Windows kurulum rehberi güncellendi.

**2026-10-06 — bilgisayar değişimi:** Linux Hub penceresi açıldı ancak kullanıcı Signing in ekranında takıldığını bildirdi. Sorun giderme kullanıcı isteğiyle durduruldu. Proje kaynakları `c34317d` commitinde mevcut; bu devir notu sonraki committe kaydedilir. Giriş bilgileri ve yerel Hub günlükleri depoya eklenmedi. Diğer bilgisayarda ilk import/derleme/sahne doğrulamasıyla devam edilecek.

**2026-10-07:** Kullanıcı diğer bilgisayarda işlem yapmadığını belirtti; Linux üzerinde devam edildi. Aktif lisansla paketler çözüldü, Farm sahnesi üretildi, Linux geliştirme build'i ve gerçek player ekran görüntüsü doğrulandı. Sonraki adım 0.1 hareket/kare seçimi.

**2026-10-07 — hareket prototipi:** Kullanıcının devam talimatıyla karakter hareketi, çarpışma, 6×6 grid, erişim kontrollü kare seçimi ve HUD eklendi. 12 EditMode testi ve 11 gerçek player kontrolü geçti; Linux build ve ekran görüntüsü doğrulandı. Sonraki özellik tarım/ekonomi döngüsü.

**2026-10-07 — tarım prototipi:** Kullanıcının devam talimatıyla 0.1 tarım/ekonomi, gün ve kalıcı kayıt döngüsü tamamlandı. 39 EditMode testi ve 52 gerçek player kontrolü geçti; son Linux build ve görseller doğrulandı. Devir için kod, prefablar, sahne, testler ve belgeler birlikte commitlenir; kesin commit/push durumu Git üzerinden kontrol edilmeli. Sıradaki geliştirme 0.2 ilk ahşap yerleşim adımı.

**2026-10-07 — etkileşim geri bildirimi:** Kullanıcının isteğiyle tıkla-seç + bağlama göre işlem yapısı kaldırıldı. 1/2/3 ve eşya çubuğuyla kuşanılan tohum/sulama kabı/orak, fare hedefindeki yakın kareye E ile uygulanıyor. Eşya adımı önce 42 mantık testi ve 61 player kontrolüyle doğrulandı; aşağıdaki tek sulama değişikliği sonrasında güncel sonuç 44 mantık testi ve 59 player kontrolüdür.

**2026-10-07 — tek sulama geri bildirimi:** Kullanıcı aynı adımda günlük sulama gereğini kaldırmamızı istedi. Her ekimde bir sulama yeterli olacak şekilde model, kayıt uyumluluğu ve arayüz güncellendi; olgunlaşma üç gece olarak korundu. Son Linux build, 44 mantık testi ve 59 player kontrolü başarılı.

**2026-10-07 — sol tık geri bildirimi:** Kullanıcı WASD ile E kullanmanın yorucu olduğunu bildirdi. Ekim/sulama/hasat doğrudan sol tıka taşındı; 1/2/3 eşya seçimi ve tek sulama/üç gece kuralı korundu. Arayüz ve pencere dışı tıklamalar tarla işlemi yapmaz. Linux build başarılı (`Logs/click-build.log`); gerçek player'da 11 hareket/hedef ve 50 tarım kontrolü başarılı, çıkış 0 (`Logs/click-player.log`). W ile yürürken tek tıkla ekim, E'nin etkisiz olması, yanlış alet, arayüz/pencere dışı tıklama ve tam tarım döngüsü denendi. Güncel ekran görüntüsü incelendi. Model/kayıt kuralları değişmediği için 44 EditMode testi bu adımda tekrar çalıştırılmadı; önceki geçerli sonuç yukarıdadır.

**2026-10-07 — basılı sulama ve ses:** Kullanıcı sulama kabıyla sol tuşu basılı tutup fareyi gezdirerek sürekli sulama ve arkada sulama sesi istedi. Ekim/hasat tek tıklık kaldı. `WateringSmokeChecks` izole QA kaydında çoklu kare, tekrar işlem/kayıt engeli, ses başlama/durma, UI sürükleme, eşya değişimi, tek tıklık ekim/hasat, odak callback'i ve uzaktan başlama senaryolarını doğrular. Linux build başarılı (`Logs/hold-watering-build-verified.log`); son player çıkış 0, **11 hareket + 50 tarım + 20 sulama/ses = 81 kontrol başarılı** (`Logs/hold-watering-player-final.log`). Ses örneklerinin boş olmadığı/taşmadığı ve döngü oynatma/durdurma durumları doğrulandı. Gerçek Alt-Tab ve öznel ses kalitesi dinleme değerlendirmesi yapılmadı; odak callback'i otomatik tetiklendi. Güncel HUD ekran görüntüsü incelendi. Model/kayıt kuralları değişmediği için önceki 44 EditMode testi bu oturumda tekrar çalıştırılmadı.
