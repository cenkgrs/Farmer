# Yazılımcı — proje hafızası ve devir

Son güncelleme: **7 Ekim 2026 — Europe/Istanbul**.

## Şu an nerede kaldık?

**Unity 6000.3.25f1 LTS (e1dba0a9aba4)** Linux makinesine kuruldu ve `Unity -version` ile doğrulandı. Resmi URP şablonunun kaynak dosyaları depoya alındı. 7 Ekim tarihinde aktif lisans doğrulandı; ilk paket importu, C# derlemesi ve başlangıç sahnesi üretimi başarıyla tamamlandı. Linux build ve görsel kontrol sonucu aşağıdaki doğrulama kaydında tutulur. **0.1 çekirdek prototipi Linux üzerinde tamamlandı:** hareket, kare seçimi, tohum satın alma, ekim/sulama, dört görünür büyüme aşaması, hasat/satış, gün ilerletme ve kayıt/yükleme çalışıyor. **Görsel ara aşama Linux üzerinde entegre edildi:** kullanıcının Tripo çiftçi/sulama kabı/orak/pazar modelleri ve Mixamo Idle/Walk animasyonları sahnede. Kullanıcı yeni görselleri beğendi; arkadaşına göndermek için Windows deneme paketi hazırlandı. Sulama/hasat için prosedürel alet hareketleri ve su akışı da eklendi; kullanıcının ters el tutuşu geri bildirimi düzeltilip son Linux player turunda 121 kontrol geçti. 0.2 dünya düzenleme adımı hazır: inşa alanı sınırı kaldırıldı, çapa ve yerleştirilebilir yatak eklendi, saat/gece–gündüz kullanıcı kararıyla 10 gerçek dakikalık döngüye geçti. Yeni kompakt ikon envanteri ve kaldırılan dünya etiketleri aşağıdaki güncel kayıtta açıklanır. Kullanıcının ilk oynanış geri bildirimi uygulandı: tıkla-seç adımı kaldırıldı, fare hedefi üzerinde eldeki eşya ile doğrudan sol tık etkileşimine geçildi. Sulama kabıyla sol tuşu basılı tutup kareler arasında gezdirilir; su sesi döngüsü eklenmiştir. Her ekimde tek sulama yeterli; hasat süresi üç gece olarak korundu. Karakter/iki alet/pazar teslim edilen modelleri kullanıyor; zemin, bitkiler, kamp ve tohum torbası hâlâ geçici. Windows x64 paketi Linux üzerinde derlendi; gerçek Windows açılış/oynanış doğrulaması açık.

Kullanıcı tek başına geliştirecek, Linux ve Windows bilgisayarlar arasında çalışacak. Python, C# ve Java deneyimi yüksek; oyun motoru deneyimi sınırlı ancak Unity'de Godot'tan daha deneyimli. Modelleri harici AI araçlarıyla üretebilir; biz prompt ve entegrasyon gereksinimlerini hazırlayacağız.

## Güncel çalışma yeri

Kullanıcı 7 Ekim 2026'da diğer bilgisayarda değişiklik yapmadığını belirtti ve bu Linux bilgisayarda devam edilmesini istedi. Önceki Windows'a devir önceliği kalktı. Git main/origin/main eşitliği kontrol edildi. Unity hesap girişi ve lisansı artık aktif; önceki Signing in sorunu yeniden üretilmedi ve kök nedenine ilişkin bir düzeltme iddia edilmiyor.

## Kesinleşen kararlar

- Sabit açılı izometrik kamera ve stilize 3D görsel dil.
- İnşa alanı veya yalnızca çiftlikte kurma kısıtı yok; aynı kurallar gelecekte düşman bölgelerine de taşınacak.
- Yeni tarlayı oyuncu çapayla hazırlar. Saat kendiliğinden ilerler; tam 24 oyun saati **10 gerçek dakika**. N yalnızca yatakta uyur, sonraki 06:00'ya atlar.
- Pazar/yatak üzerinde dünya yazısı yok; altta referanstaki ahşap çerçeveli kompakt ikon envanteri kullanılır, sürekli büyük tuş şeridi yok.
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
| Asset üretim aracı | Tripo: kullanıcı GLB modelleri teslim etti. Karakter animasyonları Mixamo; kullanıcı Idle/Walk FBX teslim etti. |

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
- Git LFS Linux'ta kurulu değil; Windows kurulumu doğrulanmadı. İki makine hazır olmadan filtre açılmadı. İlk kullanıcı model seti (en büyük dosya yaklaşık 16 MB) kaynaklarıyla normal Git'te tutuluyor. Çoğalan binary sürümler için LFS geçişi ayrı hazırlanmalı.
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
- `FarmGame.WateringActive` / `WateringChanged`: basılı sulama ve sunum sesi. UI üzerinde başlayan tıklama dünyaya sürüklenerek sulama başlatamaz; dünya üzerinde uzakta başlayan basılı tutma, hedef erişime girince başlayabilir. Geçici HUD/uzak hedefte sulama durur, ses 0,7 saniyede söner; aynı sulama hareketi geri dönünce sürer. Tuş bırakma, eşya değişimi, yükleme, odak kaybı, pause veya devre dışı bırakma hareketi iptal eder.
- `FarmPresentation`: Nokta tarafından kaydedilmiş CC0 sulama kabı sesinden hazırlanan altı saniyelik WAV döngüsü; ayrı looping AudioSource, **0.0832** kaynak sesi (önceki 0.064 değerinden %30 yüksek), 0,12 saniyelik yumuşak başlangıç, bırakmada 0,7 saniyelik eğrisel sönümlenme. Önceki sentetik damlacık üretimi kaldırıldı; gerçek kayıt kullanılıyor. Kaynak/lisans/işleme ayrıntısı `docs/audio-sources/README.md` içinde. Tek AudioSource, sönümlenirken yeniden basıldığında kesilip başlatılmaz. Odak/pause/devre dışı bırakmada anında Stop. Ses `Assets/_Farmer/Audio/watering_can_pour.wav` üzerinden sahneye bağlı; yeni paket yok.
- `FarmModel.EquippedItem` / `UseEquipped` kuralları sunumdan ayrıdır. `FarmGame.UseHovered` erişimi kontrol eder; `FarmSelection` yalnızca imleç hedefini tutar. `FarmHud` eşya çubuğu ve hedef bilgisi gösterir.
- Sulama kabı ve orak teslim edilen dokulu modelleri kullanır; avuç merkezini izleyen socket'e bağlanır. Tohum torbası geçici geometri olarak kaldı ve tohum sıfırsa gizlenir. Karakter 1,85 m; Humanoid Idle/Walk, root motion kapalı, yürüyüş ağırlığı gerçek yatay hareketten alınır. Sulama/hasat prosedürel kol/el hareketleriyle uygulanır; sağ el IK ve avatardan örneklenen kapalı parmak pozu kullanılır. Özel sulama/hasat FBX klibi teslim edilmedi.
- Kuşanılan eşya `FarmSnapshot.equippedItem` ile kaydedilir. Şema hâlâ sürüm 1; eski kayıtta bu alan yoksa tohum varsayılır. Para/tarla/envanter korunur, aletler başlangıç donanımı olarak bulunur.
- Tek sulama hasada kadar korunur; hiç sulanmayan ürün bekler, hasat sonrası yeni ekim yeniden bir kez sulanır. Süre üç gecedir. Eski kayıtta `growth > 0` olan ürün önceden sulanmış kabul edilir; ilerleme kaybolmaz.
- **44/44 EditMode testi geçti** (`Logs/equipment-watering-tests.xml`): eşya işlemleri, eski kayıt uyumluluğu, tek sulamanın kayıt sonrası sürmesi ve yeniden ekimde sıfırlanması dahil.
- Önceki eşya/tek sulama Linux build'i başarılı (`Logs/equipment-watering-build.log`). Gerçek player'da **11 hareket/hedef + 48 tarım kontrolü geçti**, çıkış 0 (`Logs/equipment-watering-player.log`). Döngüde hiç tarla tıklaması yok; tek sulamadan sonra yeniden sulanmadan üç gecede hasat doğrulandı; eşya düğmesi/1–2–3, yanlış alet, HUD üzerinde E, pencere dışı E, aynı görüntü karesinde fare hareketi + E ve kayıt/yükleme denendi.
- **Son sol tık doğrulaması:** Linux build ve 61 gerçek player kontrolü geçti; `Logs/click-build.log` / `Logs/click-player.log`. Arayüz yönlendirmeleri ve ekran görüntüleri güncellendi.
- Yeni eşya çubuğu, tohum torbası, sulama kabı ve orak oyun görüntülerinde incelendi. Güncel PNG'ler `docs/screenshots/farming-prototype.png` / `farming-market.png`. Kullanıcı sol tık etkileşimini ve son gerçek sulama sesini beğendi. Windows doğrulaması açık. Önceki üç başarısız otomatik kontrol, model entegrasyonunun ayrı grafik oturumundaki son denemesinde geçti; bu, önceki başarısız kayıtları başarıya dönüştürmez.

## Sıradaki somut işler

1. Kullanıcı yeni karakter/Idle/Walk, alet tutuşları ve pazar görünümünü beğendi. Windows arkadaş denemesinden gelecek açılış/oynanış geri bildirimini değerlendir. Entegrasyon ve 103 Linux player kontrolü tamamlandı; kaynak/dönüşüm ayrıntıları `ArtSource/tripo_v01/README.md` içinde. Prosedürel sulama/hasat hareketleri tamamlandı; özel FBX klipleri kullanılmıyor. Kaynak FBX'te Unity'nin attığı bir kesişen poligon, ileride sanat temizliği sırasında kontrol edilmeli.
2. **0.2 sonraki küçük adım:** serbest yerleşim/çapa, 10 dakikalık gece–gündüz ve kompakt envanter geri bildirimini al. Ardından kapı boşluğu/ince duvarlarla içine girilebilir küçük yapı denemesi. Yerleştirilebilir yatak uyutuyor; diğer mobilyalar, çatı gizleme ve depolama hâlâ açık. İnşa alanı sınırını veya otomatik hazır tarlayı geri getirme.
3. Windows x64 release paketinin Linux'tan derlemesi tamamlandı; gerçek Windows player açılışı/oynanışı ve Windows editöründe import/build ayrıca doğrulanmalı. Mevcut Farm sahnesini yeniden üretme; ValidateProject kullan.

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

**2026-10-07 — sulama sesi düzeltmesi:** Kullanıcı sesi aşırı yüksek ve fırtına yağmuruna benzer buldu; %80 azaltma, daha sakin su dökülmesi ve bırakmada kısa devam istedi. Kaynak kazancı 0.32 → 0.064, damlacık ağırlıklı yeni sentez ve 0,7 saniyelik sönümlenme uygulandı. Sulama/ekonomi/kayıt mantığı değişmedi. Linux build başarılı (`Logs/soft-watering-build.log`). İlk player denemesinde hareket/hedef/sulama kontrolleri başarısız oldu; başarılı sayılmadı. Eski pencere kapalıyken yeniden çalıştırılan tek player denemesinde **11 hareket + 50 tarım + 26 sulama/ses = 87 kontrol geçti**, çıkış 0 (`Logs/soft-watering-player-retry.log`). 0.064 kaynak seviyesi, bırakmada sesin hemen kesilmemesi, giderek azalması, yeniden basmayla sürmesi ve sonunda tamamen susması doğrulandı. Öznel ses tercihi kullanıcı dinlemesiyle değerlendirilecek. Önceki 44 model testi ses değişikliğinde yeniden çalıştırılmadı.

**2026-10-07 — gerçek sulama kaydı:** Kullanıcı önceki sesi hâlâ suya benzetmedi ve mevcut seviyeden %30 artış istedi. Kaynak kazancı 0.0832 yapıldı; prosedürel sentez kaldırılıp Nokta’nın CC0 sulama kabı kaydından 6 saniyelik işlenmiş WAV bağlandı. Önizleme kaynağı, lisansı ve işlemler `docs/audio-sources/README.md` içinde. 0,7 saniyelik yumuşak bitiş korundu. Son Linux build başarılı (`Logs/recorded-watering-build-isolated.log`, `FARMER_BUILD_OK`). İlk kayıt uyarlamasında 88 player kontrolü geçti; ardından WAV seviyesi dengelendi ve test hazırlığı düzeltildi. **Son build üzerindeki tam player denemesi başarılı sayılmadı:** 11 hareket, 48/50 tarım ve 26/27 sulama kontrolü geçti (`Logs/recorded-watering-player-isolated.log`, çıkış 1). Tarımda bir hover/dört aşama görünümü kontrolü; seste uzun bekleme sonundaki 0.0832 kazanç kontrolü başarısızdı (o anda `volume=0`, `focused=True`, `pouring=False`). Pencere odak değişimleri gözlendi; başarısızlıkların yalnızca bundan kaynaklandığı kesinleştirilmedi. Kayıt formatı, örnekler, döngü dikişi, basılı sürükleme, bırakma kuyruğu ve yeniden başlama kontrolleri geçti. Tek başına sulama testinde eksik geçici klasör oluşturulması düzeltildi; test sırasında gerçek Unity fare/klavye cihazları geçici kapatılıp sonunda yeniden açılıyor. Normal oyun girdileri değişmedi. Önceki 44 model testi yeniden çalıştırılmadı. Sıradaki doğrulama: kesintisiz odaklı player denemesi ve kullanıcı dinlemesi; Windows denenmedi.

**2026-10-07 — model eskizleri:** Kullanıcı inşa sisteminden önce oyuncu, sulama kabı, orak ve pazar için eskiz istedi; 3D üretimi Tripo’da ve Idle/Walk teslimini Mixamo’da kendisi yapacak. Yerleşik ImageGen ile dört ayrı PNG üretildi ve görsel olarak incelendi; `docs/references/model_sketches_v01/` içinde tam promptlar, yaklaşık ölçü/poligon hedefleri ve Mixamo teslim listesiyle saklandı. Karakter önden T pozunda ve elleri boş; aletler ayrı. Bunlar 2D referanslardır, hazır 3D/rig kabul edilmez. Tripo üretimi veya kredi harcaması yapılmadı. Bu oturumda oyun kodu değişmedi; build/test yeniden çalıştırılmadı. Kullanıcı son sulama sesini beğendi; önceki otomatik test eksikleri açık kalır.

**2026-10-07 — modeller ve Mixamo entegrasyonu:** Dört kullanıcı GLB'si ve iki FBX alındı. FBX'ler skin/iskelet içeriyor; GLB'lerden çıkarılan dokular URP'ye bağlandı. Statik modeller Blender 4.5.14 ile FBX'e dönüştürüldü; kaynaklar/hash manifesti korundu. Tek bilinçli paket değişikliği yerleşik Animation 1.0.0; editör ve diğer paket sürümleri aynı. FarmerAnimator Idle/Walk, sağ el socket/temel tutuş ve gerçek hareket hızını bağlar; pazarın görseli/çarpışması değişti, ekonomi/kayıt şeması değişmedi. İlk derlemede eksik Animation modülü saptanıp eklendi. Yakın çekimde alet eksen/tutuşu düzeltilip gerçek geometri üzerinden sap merkezleri ölçüldü. **Son Linux build başarılı** (`Logs/art-build-delivery.log`). **11 hareket + 50 tarım + 27 sulama + 15 model/animasyon = 103 gerçek player kontrolü geçti**, çıkış 0 (`Logs/art-player-delivery.log`). Xvfb ayrı grafik oturumunda test odağını masaüstünden ayırdı; önceki üç başarısız kontrol burada tekrar başarısız olmadı. Bu sonuç gerçek masaüstü GPU performans testi değildir. Genel görünüm, sulama kabı/orak tutuşu, yürüme ve pazar yakın çekimleri görsel incelendi; `docs/screenshots/art-*.png`. Meta/kaynak hash/paket kontrolleri geçti. Önceki 44 EditMode testi bu model entegrasyonunda yeniden çalıştırılmadı. Windows ve özel sulama/hasat animasyonu açık.


## Windows arkadaş denemesi — 7 Ekim 2026

- Kullanıcı mevcut görselleri beğendi ve Windows kullanan arkadaşına göndermek için EXE istedi. Unity **6000.3.25f1 Windows Build Support (Mono)** resmi Unity CLI üzerinden kuruldu; editör/paket sürümleri değişmedi.
- `ProjectSetup.BuildWindows`: Windows x64, Mono, release; `StrictMode` ve raporda sıfır hata gereksinimi. Başarı işareti `FARMER_WINDOWS_BUILD_OK`. Oynanış kodu ve görsel ayarlar değişmedi.
- İlk build exit 0 bildirse de UberPost shader derleyicisinde üç varyant hatası vardı (`Logs/windows-playtest-build.log`); bu çıktı teslim edilmedi. `-job-worker-count 4` ile StrictMode tekrarında exit 0 ve başarılı rapor alındı (`Logs/windows-playtest-strict-build.log`); eksik üç varyant derlendi, UberPost toplam 433 programa ulaştı, shader hatası/IPC exception tekrarlanmadı. Kök neden kesinleştirilmedi. Lisans token yenileme mesajı ve kapanıştaki .NET build-server mesajı build'i engellemedi.
- Teslim: `builds/Releases/Farmer-0.1.0-Windows-x64.zip` — **52,3 MiB**, 168 dosya. SHA-256: `660bf6b9cb464870b4c7b75327d1197f8abfd6108f43954d3d102c4374a4709e`. Windows x64 PE başlığı, gerekli EXE/DLL/Data/Mono dosyaları ve ZIP CRC doğrulandı. D3D12 runtime dahil, Burst debug klasörü hariç. `BENI_OKU.txt` Türkçe kontroller/ilk hasat/kayıt/günlük yolunu içerir.
- `tools/build/package_windows.py` yeniden paketleme aracı; `docs/KURULUM.md` komutları içerir. Paket ve build/log dosyaları Git dışında, yalnızca bu makinede; diğer bilgisayarda yeniden üretilmeli veya ZIP ayrıca aktarılmalı.
- **Gerçek Windows cihazında çalıştırma yapılmadı.** Arkadaş denemesinde açılış, görüntü/ses, tam tarım döngüsü ve kapatıp açınca kayıt kontrol edilmeli. Önceki Linux 103 player/44 tarihsel EditMode sonucu bu adımda yeniden koşulmadı; bu iş derleme/paketleme değişikliği.
- Yerel Unity aktif build hedefi Windows'a geçti; bir sonraki Linux batch build için `-buildTarget Linux64` kullan. Sıradaki özellik 0.2 ilk ahşap yerleşim; özel sulama/hasat animasyonları hâlâ açık.


## Prosedürel sulama/hasat ve tutuş düzeltmesi — 7 Ekim 2026

- Kullanıcı yeni animasyon dosyası vermeden sulama ve hasat hareketlerini hazırlamamızı istedi. `FarmerAnimator` mevcut Idle/Walk üstüne sağ kol/el IK uygular; kabı 0,18 saniyede eğme, basılı duruş, 0,7 saniyede indirme ve 0,52 saniyelik orak savuruşu. İşlemler tıklama anında olur; yeni cooldown, hareket kilidi veya kayıt şeması yok.
- `WateringStream`: kabın ağzından hedef kareye yedi ince akış; güç mevcut 0.0832 ses kaynağının sönümlenmesini izler. Ağız ileri baksın diye kabın yalnızca eldeki kopyası çevrilir; kaynak/prefab dokunulmadı. `PlayerMotor.FacingTarget` görseli işlem yönüne çevirir; hareket/çarpışma WASD ile sürer.
- İlk yakın çekimde kullanıcı avucun ters, tutuşun garip olduğunu belirtti. El kavrama ekseni kemiklerden hesaplanıp kabın/orak sapının yönüne ayrıldı. HumanPose'u her kare yeniden uygulamak yerine başlangıçta yalnızca parmak pozu örneklenir; IK sonrası sadece parmakların yerel dönüşleri kapanır. Son kontrol işaret parmağında yaklaşık 70° kıvrılmayı doğruladı; yakın çekimler `docs/screenshots/tool-watering.png`, `tool-grip.png`, `tool-harvest.png`.
- Son Linux build başarılı (`Logs/tool-animation-final-build.log`, çıkış 0, `FARMER_BUILD_OK`). İlk tam son-build test açılışında Mono/native SIGSEGV oldu; kontroller başlamadı ve başarılı sayılmadı (`Logs/tool-animation-delivery.log`). Hata ayıklayıcı takıldı; test çalıştırıcısının kendi süreç grubunu ve Xvfb'yi zaman aşımında temizlemesi düzeltildi. Kök neden belirlenmedi.
- **Aynı son build'in temiz tekrarında 121/121 gerçek player kontrolü geçti:** 11 hareket + 50 tarım + 27 sulama/ses + 15 model + 18 alet hareketi (`Logs/tool-animation-delivery-retry.log`, çıkış 0, `FARMER_PLAYER_SMOKE_OK`). Yeni grup parmak kavrama, kabın eğimi, root kaymaması, yürürken sulama, çoklu kare, ses/su kuyruğu, eşya değiştirme, tek hasat, geçersiz işlem, yükleme, odak callback'i ve disable durumlarını kapsar. Son sulama/hasat/tutuş görüntüleri incelendi. Gerçek Alt-Tab/performance testi değildir. Önceki 44 EditMode testi model/kayıt değişmediği için yeniden çalıştırılmadı.
- Çalıştırıcı `--checks tool-animation` ile sadece yeni grubu çalıştırabilir; varsayılan tüm gruplardır. Geçici Xvfb önceki yolunda yoktu, yerel `builds/Tools/xvfb/runtime/usr/bin/Xvfb` hazırlandı (`--xvfb` ile verilebilir); binary Git'e girmez.
- Paket/editör sürümleri aynı. Yerel aktif hedef yeniden Linux. Önceki Windows ZIP (`c9c5ad7` dönemindeki paket) bu animasyonları içermez; Windows build/player bu değişiklikten sonra yeniden denenmedi. Sonraki adım kullanıcının hareket hissi değerlendirmesi ve 0.2 ilk ahşap yerleşim.

- Normal Linux player RTX 4090 üzerinde açıldı; 1280×720 Farmer penceresi doğrulandı (`Logs/tool-animation-play-session.log`). Bu açılışta hata/exception görülmedi; kullanıcı denemesi için açık bırakıldı. Uzun süreli oynanış testi değildir.


## 0.2 ilk parça — Ahşap bloklarla inşa, 7 Ekim 2026

- Kullanıcı alet hareketlerini beğendi ve devam etmemizi istedi. 6×5 inşa alanı tarlanın sağında; 1 m bloklar ve üç blok yüksekliği. `4`/alt düğme inşa, sol tık yerleştir, `R` döndür, tekerlek yükseklik, sağ tık sök, `Esc` veya `1/2/3` çık. Yeşil/kırmızı önizleme; yatay erişim 3,5 m, oyuncu/nesne çakışması ve desteksiz üst blok engeli. Üstünde blok varken alttaki sökülemez. İnşa sırasında tarım tıklamaları ve eldeki alet görseli kapanır.
- `BuildDefinition`/`Data/WoodBlock.asset` ve ahşap prefab veri tanımlıdır. `BuildingModel` saf yerleşim/odun kuralları, `BuildController` fare/önizleme/grid/fizik sunumu. İlk görsel geçici tahta geometrisidir; yeni harici asset/paket gerekmedi. `BuildingPrototypeSetup.ApplyAndBuild` mevcut sahneye bir kez uygulandı; güncel sahne/prefabları yeniden üretme, sonraki build için `ProjectSetup.BuildLinux` kullan.
- Başlangıç 24 odun, blok 2 odun, sökmede tam iade. Pazar düğmesinde 20 paraya 10 odun, üst sınır 999. Mevcut çiftliğe ilk geçişte 24 odun verilir; yeniden yükleme odunu yenilemez. Kaynak toplama ve nihai denge bu adımda yok.
- Kayıt sürümü 2: aynı `farm-v1.json` yolu korunur, yapı konum/dönüş ve odun da saklanır. V1 çiftlik/gün/envanter/kuşanılan eşya korunur; ilk v2 yazmadan önce sağlam v1 ana dosya `.pre-v2` kopyasıyla tutulur. `.bak` ve bozuk dosya koruması devam eder. Eski exe v2 kaydı açamaz; iki farklı sürümü aynı kayıt üzerinde sırayla çalıştırma.
- İlk EditMode turunda 65/66 geçti; v1 içindeki null sınıfın JsonUtility tarafından boş nesneye dönüştürülmesiyle geçiş testi başarısız oldu (`Logs/building-tests.xml`). Geçiş şema sürümüne göre düzeltildi. Sonuç **66/66 geçti**, başarısız/atlanan yok (`Logs/building-tests-final.xml`). Yerleşim/geri iade/kaynak korunumu, bozuk yapı reddi, özel veri tanımı, v1 geçişi ve disk yedekten kurtarma kapsanır.
- Linux build başarılı (`Logs/building-player-build.log`, çıkış 0, `FARMER_BUILD_OK`). Önce yalnız inşa 22/22 geçti (`Logs/building-first.log`), ardından **143/143 gerçek player kontrolü geçti**: 11 hareket + 50 tarım + 27 sulama/ses + 15 model + 18 alet hareketi + 22 inşa (`Logs/building-regression.log`, çıkış 0, `FARMER_PLAYER_SMOKE_OK`). Xvfb/yazılım OpenGL; gerçek masaüstü GPU performans testi değil. Önizleme, üst üste blok ve pazar HUD görüntüleri incelendi; `docs/screenshots/building-preview.png` ve `building-stacked.png`.
- İnşa testinde gerçek fare/tuş olaylarıyla yerleştirme/döndürme/kat seçimi/sökme/yükleme, basılı tuşun tekrarlamaması, oyuncu ve nesne çakışması, erişim ve UI engeli, tarımla ayrım ve odun alışverişi doğrulandı. QA kayıtları oyuncunun dosyasından ayrıdır.
- Bu yalnızca 0.2'nin ilk parçasıdır. Ayrı taşıma aracı, ince duvar/çit/kapı/çatı/mobilya, depolama ve duvar gizleme yok; söküp yeniden yerleştirme mümkündür. Windows bu değişiklikten sonra yeniden derlenmedi/oynanmadı; mevcut eski Windows ZIP inşa veya son alet animasyonlarını içermez.


## Güncel dünya, çapa, saat ve envanter — 7 Ekim 2026

- Kullanıcı inşa alanı sınırını kaldırdı; çiftlik dışında/düşman bölgelerinde de aynı yapı kurallarının işlemesini istedi. Mevcut düz arazi üzerindeki uygun zeminlerde serbest kurulum var; düşman bölgeleri/savaş/sığınak güvenliği henüz yapılmadı. `WorldGround` zemin uygunluğunu belirler, çiftlik veya inşa bölgesi kimliği yok. Prototipin 20×20 arazisi, y=0 düz zemin ve üç blok yüksekliği korundu; farklı arazi yükseklikleri/yaşanabilir üst kat bu adımda yok.
- `BuildingModel` dünya koordinatlı seyrek yapı ve doluluk kümesi kullanır. Eski v2 yerel blok x/z değerleri +(3,-7) ile dünya konumuna taşınır. 1×2 hücrelik yatak dönüşe göre iki hücreyi işgal eder, 8 odun harcar ve sökünce geri verir; zemine konur, üstüne blok konmaz. `4` inşa, `Q` blok/yatak, `R` dönüş, tekerlek yükseklik, sağ tık sök. Başlangıç yatağı da gerçek `Bed` etkileşimidir. Yatak önizlemesi uyuma izni vermez.
- `FarmModel` yalnızca hazırlanmış kareleri tutar; yeni oyunda PlotCount=0. `5`/çapa ikonu ve sol tık boş toprağı hazırlar; 1/2/3 ile mevcut ekim/sulama/hasat devam eder. Çapa kalıcı başlangıç aleti, basit geometrili geçici model ve kısa prosedürel hareket içerir. Tarlalar hasattan sonra kalır; engelin altına tarla açılamaz, ekili ürünün üzerine yapı kurulamaz. Tek sulama ve üç gece kuralı korundu.
- `DayNightCycle`: **24 oyun saati = 10 gerçek dakika**, sahne ve varsayılan kod ayarı 10. İlk 20 dakika önerisi kullanıcı tarafından 10'a indirildi. Saat odaktayken ilerler; oyun kapalıyken/odak dışında ilerlemez. Gece yarısında tarih ve büyüme bir kez ilerler. N yalnızca aktif yatağa 1,8 m yakınken sonraki 06:00'ya uyutur; gece yarısından sonra aynı sabaha atlar ve ikinci büyüme vermez. Şafak/gündüz/alacakaranlık/gece ışık, ortam ve arka plan renkleri yumuşak değişir; ilk çok karanlık gece görünümü açıldı.
- Kayıt **v3**, dosya adı hâlâ `farm-v1.json`. V1/V2 tarla/ekin/para/alet korunur; eski hazırlanmış 36 kare silinmez. Dünya kare koordinatları, yapılar, odun ve dakika saklanır; ilk yükseltmede sağlam ana kayıt `.pre-v3` olarak korunur. `.bak` kurtarma sürer; eski exe v3 açamaz. Zaman 30 saniyede bir, gece yarısı ve mevcut başarılı işlemlerde kaydedilir. Karakter konumu hâlâ kaydedilmez. Koruma sınırları: 10.000 toprak + 10.000 yapı, koordinat ±100.000, dosya 16 MiB; bunlar oyun içi inşa alanı değildir.
- Arayüz geri bildirimi uygulandı: PAZAR/YATAK dünya TextMesh yazıları kaldırıldı. Eski tam genişlikte alt şerit yerine 476×78 (1280×720 referansta) ahşap çerçeveli yedi ikon: tohum, sulama kabı, orak, inşa, çapa, turp, odun. Seçili slot altın, miktarlar köşede; eşya bilgisi hover ile, inşa kontrolleri yalnızca ilgili modda. Kısa bildirim 3,5 saniye görünür. Saat/para küçük sol üst kartta, hedef bilgisi yalnızca gerektiğinde. Bu çubuk mevcut envanteri gösterir; sürükle-bırak/genel çanta/sandık sistemi eklenmedi. Kullanıcı ilk vektör ikonları ve düz kahverengi çerçeveyi reddetti. Son sürüm `imagegen` skill ve yerleşik ImageGen ile üretilmiş yedi boyanmış PNG ikon ve ahşap damarlı yedi gözlü çerçeve kullanır. `InventoryIcon` görselleri önbelleğe alınmış Sprite olarak yükler; kaynak PNG alfa kanalları korunur, şeffaf kenar boşluğu Sprite rect ile dışarıda bırakılır. Kaynaklar `Assets/_Farmer/Resources/InventoryArt`, tüm promptlar ve referans `docs/art-sources/inventory-prompts.json` içinde. Slotların düz dolgusu kaldırıldı; seçili eşyanın yarı şeffaf altın vurgusu dokuyu örtmez.
- `WorldPrototypeSetup.ApplyAndBuild` sahneye zemin/yatak/ışık bileşenlerini ve yatak prefab/verisini ekledi; ilk çalıştırma silinmiş tabela referansı nedeniyle başarısızdı (`Logs/world-setup-build.log`). Referans filtrelenip tekrar başarılı oldu. `ApplyHudAndBuild` dünya yazılarını kaldırdı. Güncel sahneyi yeniden üretme; normal `ProjectSetup.BuildLinux` kullan.
- Son model doğrulaması: **76/76 EditMode geçti**, başarısız/atlanan yok (`Logs/world-tests-final.xml`). İlk dünya testi 22/22, UI öncesi tam tur 166/166 geçti. İlk UI turlarında 167/168 geçti; ayrıntılı log, tek farkın JsonUtility double saatin son basamağını yuvarlaması olduğunu gösterdi (`2.5770584672689895 → 2.57705846726899`, `Logs/painted-inventory-final.log`). Testte yalnızca saat için 1e-9 dakika tolerans kullanıldı, tüm diğer kayıt alanlarının birebir karşılaştırması korundu. Oyun kayıt mantığında veri kaybı saptanmadı. QA normal oyuncu kaydından ayrıdır; eski tarım testleri yalnızca test hazırlığında 36 kareyi yeni çapa kurallarıyla açar, normal yeni oyun hazır tarla üretmez.
- Paket/editör sürümleri sabit. Windows bu adımda derlenmedi/oynanmadı; mevcut Windows ZIP bu değişiklikleri içermez.

- Nihai ImageGen sürümü: Linux build başarılı (`Logs/painted-inventory-final-build.log`); **168/168 gerçek player kontrolü geçti**, çıkış 0, `FARMER_PLAYER_SMOKE_OK` (`Logs/painted-inventory-delivery.log`). Dağılım: 11 hareket + 50 tarım + 27 sulama/ses + 15 model + 18 alet + 22 inşa + 25 dünya/arayüz. Boyanmış envanter gündüz görüntüsünde incelendi. Asset/meta eşleri ve GUID benzersizliği doğru, `git diff --check` temiz. Model sonucu 76/76; sadece görsel/test karşılaştırması değiştiğinden yeniden koşulmadı.
- Kullanıcı çıkmadan önce commit/push ve oyunu açma istedi. Görseller/promptlar repo içinde; build ve QA günlükleri yerel. Sonraki somut iş: kullanıcının yeni envanter değerlendirmesi, sonra 0.2 ince duvar/kapı/çatı ve iç mekân görünürlüğü. Windows paketi yeniden üretilmedi.
