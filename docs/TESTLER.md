# Prototip doğrulaması

Unity **6000.3.25f1**, Linux. 8 Ekim 2026: 101/101 EditMode testi başarılı (`Logs/exploration-tests.xml`). Gerçek geliştirme player'ında hareket/seçim ve tam tarım döngüsü kontrol edildi. Windows henüz denenmedi.

## Mantık testleri

Unity Test Runner'ın EditMode sekmesinde `Farmer.Tests` testlerini çalıştırabilir veya editör kapalıyken depo kökünde şu komutu kullanabilirsin:

```bash
mkdir -p Logs
UNITY_EDITOR="$HOME/Unity/Hub/Editor/6000.3.25f1/Editor/Unity"
"$UNITY_EDITOR" -batchmode -nographics -projectPath "$PWD" \
  -runTests -testPlatform EditMode -testFilter Farmer.Tests \
  -testResults "$PWD/Logs/equipment-watering-tests.xml" -logFile "$PWD/Logs/equipment-watering-tests.log"
```

Test komutuna `-quit` ekleme; Test Runner tamamlanınca çıkar. XML'de sonuç, test sayısı ve başarısız/atlanan testleri kontrol et. Sadece çıkış koduna bakma.

## Build ve gerçek player kontrolü

```bash
"$UNITY_EDITOR" -batchmode -quit -projectPath "$PWD" \
  -executeMethod Farmer.Editor.ProjectSetup.BuildLinux \
  -logFile "$PWD/Logs/equipment-watering-build.log"
./builds/Linux/Farmer.x86_64 -screen-fullscreen 0 -screen-width 1280 -screen-height 720 \
  --farmer-smoke-capture builds/QA/movement-check.png --farmer-check-controls \
  -logFile "$PWD/Logs/movement-player.log"
```

PNG için daha önce kullanılmamış bir yol seç. Oyunun penceresi odakta kalsın; otomatik kontrol sırasında klavye/fare kullanma. Player testi grafik oturumu gerektirir, `-batchmode` veya `-nographics` ekleme. Başarılı build'de `FARMER_BUILD_OK`; başarılı player kontrolünde çıkış 0, 11 `FARMER_CHECK_OK` ve `FARMER_PLAYER_SMOKE_OK` bulunur. `FARMER_CHECK_FAILED` veya exception varsa sonucu başarısız say.

Kontrol, Input System'e geçici sanal klavye/fare olayları göndererek normal hareket/seçim kodunu çalıştırır. Sınır ve sandık kontrollerine hazırlanırken karakter konumu test tarafından ayarlanır. Test cihazları sonunda kaldırılır. Normal uygulama açılışında bu otomasyon etkinleşmez; yayın build'inde kontrol kodu yoktur. Ekran görüntüsünü ayrıca incele: yanlış kadraj, eksik/pembe malzeme, görünmeyen seçim veya kesilmiş HUD otomatik sayısal kontrollerden kaçabilir.

## Kısa manuel deneme

1. `Farm.unity` sahnesini açıp Play'e bas veya Linux uygulamasını ek bayrak olmadan çalıştır.
2. WASD ve oklarla her yöne yürü; çapraz hareketin hızlanmadığını, kameranın dönmediğini gözle.
3. Sağdaki sandığa doğru yürü; içinden geçmemelisin. Arazi kenarına yürü; düşmemelisin.
4. Yeni oyunda hazır tarla yoktur. 5/çapa ikonuyla boş zemine sol tıkla toprak hazırla. Fareyi tarlada gezdir; tıklamadan hedef çerçevesi gelmeli. Uzak kare turuncu olmalı ve sol tık işlem yapmamalı.
5. Fare HUD üzerindeyken/pencere dışındayken hedef kaybolmalı; sol tık önceki karede işlem yapmamalı. 1/2/3 veya eşya çubuğu düğmeleri eldeki eşyayı değiştirmeli.
6. Pazar tezgâhına yaklaş, B veya düğmelerle tohum al. Paran ve tohum sayın doğru değişmeli.
7. 1 ile tohumu kuşan, yakın boş kareyi fareyle hedefle ve sol tıkla ek. Aynı eşyayla tekrar sol tık sulamamalı. 2 ile sulama kabını kuşan ve sol tıkla sula; toprak koyulaşmalı. 1 ile ikinci bir kareye ekip kuru bırak.
8. Yatağa yaklaş ve N ile sonraki 06:00'ya uyu. Yataktan uzakta N işlem yapmamalı. Saat normal akışta ilerlemeli; tam döngü 10 gerçek dakika. Sulanan ürün büyümeli, kuru ürün aynı aşamada kalmalı. Yeniden sulamadan üç kez geceyi geçir; üçüncü geceden sonra hasat hazır olmalı. Hasat sonrası tekrar ekilen bitki bir kez yeniden sulanmalı.
9. Sulama kabıyla olgun ürünü hasat edememelisin. 3 ile orağı kuşan ve sol tıkla hasat et; pazara gidip V ile sat ve kazançla tekrar tohum al.
10. Bir kareyi suladıktan sonra oyunu kapat/aç veya F9'a bas. Para, tohum, hasat envanteri, gün, bitki, sulama ve kuşanılan eşya korunmalı.

## Tarım döngüsünün otomatik player denemesi

```bash
./builds/Linux/Farmer.x86_64 -screen-fullscreen 0 -screen-width 1280 -screen-height 720 \
  --farmer-smoke-capture builds/QA/farming-check.png --farmer-check-controls --farmer-check-farming \
  -logFile "$PWD/Logs/farming-player.log"
```

`--farmer-check-farming`, pazar düğmesine gerçek UI tıklaması ve 1/2/3/B/N/V/F5/F9 tuş olaylarıyla satın alma, kuru bitkinin beklemesi, dört aşama, uzak hasadın engellenmesi, hasat, satış, yeniden yatırım ve kayıt yüklemeyi kontrol eder. Tarla işlemleri tek sol tıkla yapılır; E işlemez. WASD hareketi sırasında tıklama da denenir. Yanlış alet, HUD/pencere dışı tıklama, eşya düğmesi ve aynı karede imleç hareketi + sol tık da kontrol edilir. Ayrıca pazar ve her büyüme aşaması için ayrı PNG üretir; son görüntüde farklı yaşta dört bitki bırakır. Testte konum hazırlığı teleport ile yapılır; hareket/çarpışma ayrıca önceki kontrol setinde denenir.

**Her smoke capture çalıştırması ayrı geçici kayıt klasörü kullanır; oyuncunun normal çiftliğini yüklemez veya üzerine yazmaz.** Sonunda test kayıt klasörü temizlenir. Ekran görüntüleri ve günlükler `builds/QA` / `Logs` altında kalır. Başarı için 50 `FARMER_FARM_CHECK_OK`, `FARMER_FARMING_CHECKS_FINISHED`, `FARMER_PLAYER_SMOKE_OK` ve çıkış 0 gerekir; `FARMER_FARM_CHECK_FAILED` olmamalı. Ses kontrolü AudioSource oynatma durumunu doğrular; dinleme kalitesi değerlendirmesi değildir.

## Kayıt dosyası

Oyuncu kaydı Unity'nin `Application.persistentDataPath` klasöründe `farm-v1.json` dosyasıdır. Linux'ta genel konum `~/.config/unity3d/Cenk Gurses/Farmer/`; Windows'ta `%USERPROFILE%/AppData/LocalLow/Cenk Gurses/Farmer/`. İşletim sistemi doğrulaması Linux üzerinde yapıldı.

Her başarılı işlem ve normal çıkış kaydedilir; F5 tekrar kaydeder, F9 diskten yükler. Yeni dosya önce geçici dosyaya yazılır, sonra atomik değiştirme yapılır; önceki kayıt `.bak` olarak korunur. Ana kayıt bozuksa sağlam yedek açılır; bozuk dosya üzerine yazmadan önce `.corrupt-*` kopyası korunur. İkisi de okunamazsa sessizce yeni oyuna başlanmaz: işlemler durdurulur, dosyalar korunur ve ekranda hata gösterilir. Kaydetme izni/disk sorunu olursa mevcut oturum bellekte sürer; F5 ile yeniden dene.

Kayıtlar Git'e girmez ve iki bilgisayar arasında otomatik taşınmaz. Önceki günlük sulama kayıtlarında büyümesi başlamış ürünün (`growth > 0`) daha önce sulandığı anlaşılır ve sulanmış olarak açılır. Şema sürümü 6; para, envanter, gün/dakika, dünya koordinatlı hazırlanmış toprak, odun ve yapı konum/dönüşlerini içerir. Dosya adı uyumluluk için `farm-v1.json` kalır. V1 geçişinde başlangıç odunu bir kez verilir; ilk v6 yazmada sağlam eski ana dosya `.pre-v6` olarak korunur. V1/V2 hazırlanmış tarlalar ve blokların dünya konumları korunur. Eski exe v6 kaydı desteklemez. Kuşanılan eşya yeni isteğe bağlı `equippedItem` alanında saklanır. Eski sürüm 1 kayıtlarında bu alan yoksa tohum seçilir; çapa/balta/orak/sulama kabı kalıcı envanterde bulunur. Karakter konumu saklanmaz; açılışta pazar yakınında başlarsın. Yeni dünya yerleşimi ilk açılışta otomatik kaydedilir. Aynı çiftliği iki uygulamada eşzamanlı açma.

## Basılı tutarak sulama ve ses

Son build üzerinde `--farmer-check-watering` eklenerek tarım/hareket kontrolleriyle birlikte çalıştırılır:

```bash
./builds/Linux/Farmer.x86_64 -screen-fullscreen 0 -screen-width 1280 -screen-height 720 \
  --farmer-smoke-capture builds/QA/hold-watering-check.png \
  --farmer-check-controls --farmer-check-farming --farmer-check-watering \
  -logFile "$PWD/Logs/hold-watering-player.log"
```

Bu kontrol yalnızca smoke capture'ın ayrı `FarmerQA` kayıt klasöründe geçici tarla kurar; önceki test durumunu sonunda geri yükler. Kontrol boyunca Unity içindeki mevcut fare/klavye cihazları kapatılır, yalnızca sanal test girdileri kullanılır; sonunda cihazlar yeniden açılır. İşletim sistemi pencere odağı yine korunmalıdır. Basılı tutarak iki kareyi sulama, aynı karede beklerken tekrar işlem/kayıt üretmeme, sesin başlaması, önceki 0.064 seviyesinden %30 ses artışı, sönümlenme ve sönümlenirken yeniden basma, HUD/pencere dışı/uzak hedefler, UI'dan başlayan sürükleme, eşya değiştirme, tek tıklık ekim/hasat ve devre dışı bırakma denenir. Odak kaybı callback'i açıkça tetiklenir; gerçek işletim sistemi pencere geçişi testi değildir. 27 `FARMER_WATER_CHECK_OK`, `FARMER_WATERING_CHECKS_FINISHED`, `FARMER_PLAYER_SMOKE_OK` ve çıkış 0 gerekir; `FARMER_WATER_CHECK_FAILED` olmamalı.

Manuel deneme: 2 ile sulama kabını al, iki yakın kuru ekili kare üzerinde sol tuşu bırakmadan gezdir. İkisi de koyulaşmalı. Tuşu bırakınca, arayüze gidince veya 1/3 ile eşya değiştirince sulama hemen durmalı; ses yaklaşık 0,7 saniyede azalıp bitmeli. Sönümlenme sırasında tekrar basmak sesi sıfırdan keserek başlatmamalı. Kaynak ses seviyesi 0.0832 (önceki 0.064 değerinden %30 yüksek). Alt-Tab ile de dene. Basılı tuşla tohum/orak sürüklemek ikinci bir kareye işlem yapmamalı. Ses gerçek sulama kabı kaydından işlenmiş, altı saniyelik CC0 WAV döngüsüdür; otomatik kontrol örneklerin boş olmadığını/taşmadığını ve AudioSource oynatma durumunu doğrular. Öznel ses kalitesi dinleme değerlendirmesi bekler.

## Modeller ve Idle/Walk — 7 Ekim

Son Linux build: `Logs/art-build-delivery.log`. Son player: `Logs/art-player-delivery.log`, çıkış 0; **11 hareket + 50 tarım + 27 sulama + 15 art = 103 kontrol başarılı**. Eski üç başarısız giriş/sulama kontrolü bu ayrı grafik oturumunda geçti. Önceki 44 EditMode sonucu tarihsel olarak korunur; bu adımda tekrar çalıştırılmadı.

`--farmer-check-art`: Humanoid Avatar, root motion kapalı olması, skinned mesh/doku, iki Mixamo klibi, dururken/yürürken geçiş, yatay kayma olmaması, el socket'i, aletlerin gerçek geometri ve dik boyutu, pazar malzeme/çarpışması. 15 `FARMER_ART_CHECK_OK` ve `FARMER_ART_CHECKS_FINISHED` gerekir. Ekran görüntüleri karakter/alet, yürüyüş ve pazar yakın çekimlerini de içerir; sayısal testler görsel incelemenin yerine geçmez.

Masaüstü odağına müdahale etmeden Linux testi için Xvfb ve xwininfo kuruluysa:

```bash
python3 tools/art/run_player_checks.py --name art-check-unique
```

Çalıştırıcı yazılım OpenGL kullanır; normal masaüstü GPU performansına dair sonuç çıkarılmaz. Kaynak FBX'te tek kesişen poligon import uyarısı kaydedildi; bu kaynak kusuru düzeltilmiş sayılmıyor. Windows import/build ayrı doğrulanmalı.


## Windows deneme paketi — 7 Ekim

Windows x64 Mono release build Linux'ta üretildi. Son doğrulama:
`Logs/windows-playtest-strict-build.log`, çıkış 0, `FARMER_WINDOWS_BUILD_OK`;
StrictMode açık, raporda hata yok. İlk denemedeki UberPost shader hatası son
derlemede tekrarlanmadı; eksik üç varyant yeniden derlendi.
`tools/build/package_windows.py` PE x64 başlığı/runtime dosyaları ve ZIP CRC
kontrollerini geçti. 168 dosya, 52,3 MiB; SHA-256 devir notundadır.

Windows'ta gerçek çalıştırma henüz denenmedi. ZIP tamamen çıkarılıp EXE açılmalı;
manuel deneme listesindeki hareket, pazar, ekim/sulama/hasat ve kayıt döngüsü
Windows'ta tekrarlanmalı. Hata varsa `BENI_OKU.txt` içindeki Player.log konumu
kullanılmalı. Bu paketleme işi için oyun mantık testleri tekrar çalıştırılmadı.


## Prosedürel alet hareketleri ve el tutuşu — 7 Ekim

Son Linux build `Logs/tool-animation-final-build.log`; gerçek player tekrarında
`Logs/tool-animation-delivery-retry.log`, çıkış 0 ve **121 kontrol başarılı**.
Önceki 103 kontrole `--farmer-check-tool-animation` ile 18 kontrol eklenir.
İlk tam son-build açılışında native/Mono SIGSEGV oluştu, testler başlamadı;
aynı build'in temiz tekrarında üremedi. Kök neden çözülmüş sayılmaz.

```bash
python3 tools/art/run_player_checks.py --name tool-animation-unique
# Yalnızca alet hareketleri:
python3 tools/art/run_player_checks.py --name tool-animation-only-unique --checks tool-animation
```

Xvfb PATH üzerinde değilse `--xvfb <Xvfb-executable>` ekle. Başarı için
18 `FARMER_TOOL_ANIMATION_CHECK_OK`, `FARMER_TOOL_ANIMATION_CHECKS_FINISHED`
ve `FARMER_PLAYER_SMOKE_OK` gerekir. Yeni grup sadece izole QA kaydında çalışır;
normal kullanıcı çiftliğini değiştirmez. Yakın çekim kamera konumu test sonunda
eski haline döner. `docs/screenshots/tool-*.png` son görsel kontrol sonuçlarıdır.

Manuel: 2 ile kabı kuşan; basılı sularken WASD ile yürü ve hedefi değiştir.
Kabın ağzı suyla birlikte hedefe yönelmeli, parmaklar sapı kavramalı. Bırakınca
0,7 saniyede normale dönmeli. 3 ile olgun ürüne tıkla; kısa savurma bir kez olmalı.
Eşya değişimi ve F9 eski hareketi taşımamalı. Gerçek Alt-Tab da ayrıca denenmeli.


## Ahşap blok inşası — güncel doğrulama

66/66 EditMode testi geçti (`Logs/building-tests-final.xml`); ilk turdaki v1 geçiş hatası düzeltildi. Son Linux build `Logs/building-player-build.log`; tam player turu `Logs/building-regression.log`, çıkış 0, 143/143 kontrol (önceki 121 + yeni 22 inşa). Görseller ayrıca incelendi; Windows sonucu değildir.

```bash
python3 tools/art/run_player_checks.py --name building-check-unique --checks building
```

Yerel Xvfb PATH üzerinde değilse `--xvfb builds/Tools/xvfb/runtime/usr/bin/Xvfb` ekle; başka bilgisayarda kurulu Xvfb kullan. Tüm gruplar için `--checks` seçeneğini çıkar. İnşa grubunda 22 `FARMER_BUILDING_CHECK_OK`, `FARMER_BUILDING_CHECKS_FINISHED`, `FARMER_PLAYER_SMOKE_OK`, sıfır hata ve çıkış 0 gerekir.

Manuel deneme: sağdaki alana yaklaş, 4 ile aç, yeşil önizlemede iki komşu blok koy. R ile döndür; tekerleği yukarı alıp birinin üstüne blok koy. Altı boş yer kırmızı olmalı. Önce üstteki bloğu sağ tıkla sök; odun iadesini izle. Oyuncunun içine/uzak noktaya veya arayüz üstünden yerleştirmeyi dene; kaynak harcanmamalı. F9 veya kapat/aç ile konum/dönüş/odun korunmalı. Pazarda odun al; 1/2/3 ile tarıma dön. Kapı/çatı/iç mekân bu adımın kabul ölçütü değil.

## Serbest dünya, çapa, saat ve boyanmış envanter

```bash
python3 tools/art/run_player_checks.py --xvfb builds/Tools/xvfb/runtime/usr/bin/Xvfb --name world-check --checks world
# Tüm gruplar (yeni ve benzersiz çıktı adı kullan):
python3 tools/art/run_player_checks.py --xvfb builds/Tools/xvfb/runtime/usr/bin/Xvfb --name complete-check
```

Xvfb yolu makineye özgü yerel kurulumdur; binary Git içinde değildir. Çalıştırıcı ayrı ekran ve geçici kayıt açar; masaüstündeki oyuncu kaydına dokunmaz. Eski tarım grupları kendi hazırlığında 36 kare açar; normal yeni oyunun hazır tarlası yoktur. Saat, ilgisiz eski testler için dondurulur; dünya grubu otomatik ilerlemeyi ve gece yarısını ayrıca açarak dener.

Dünya grubu: yeni tarla yokluğu, yedi envanter ikonu ve dünya etiketlerinin kaldırılması, 10 dakika ayarı, boş zemine ekim reddi, çapa/tek basış, tarım, eski alan dışında yapı, engelde çapa reddi, 1×2 yatak/uyku, gün/gece ışıkları, otomatik saat/gece yarısı ve kayıt/yükleme. Kayıt denemesinde saat farkı veya yatak/görsel eksikliği ayrıntılı loglanır.

Manuel görsel kontrol: yedi resim ahşap gözlerin içinde taşmadan görünmeli, seçili göz dokuyu kapatmayan altın vurguyla ayrılmalı; sayaçlar okunmalı. İkon tıklaması dünyaya işlem göndermemeli. Gece okunabilir kalmalı; yeni hazırlanmış tarla ve yatak normal kayıt/yüklemede korunmalı.

Son sonuç: `Logs/painted-inventory-delivery.log`, **168/168 geçti**, çıkış 0. Build `Logs/painted-inventory-final-build.log` başarılı. Saatin JSON round-trip testi 1e-9 dakika toleranslı; diğer kayıt alanları birebir karşılaştırılır. 76/76 EditMode sonucu `Logs/world-tests-final.xml`.


## İlk oda ve saydam ön cephe — 8 Ekim 2026

```bash
python3 tools/art/run_player_checks.py --xvfb builds/Tools/xvfb/runtime/usr/bin/Xvfb --name house-check-unique --checks house
```

`--checks house`: UI üzerinden döşeme, duvar ve kapı yerleştirme; kaynak maliyeti; kapalı kapıda gerçek CharacterController çarpışması, açılma bölgesinde oyuncu varken ret, F ile aç/kapat, açık kapıdan geçiş, duvar engeli, kayıt/yükleme, döşeme üzerinde çapa reddi, sağ tıkla yalnızca hedef duvarın sökülmesi. 3×3 oda çevresi model kurallarıyla test fixture olarak hazırlanır, **yatak normal önizleme ve fare tıklamasıyla** içine konur. Uyuma/oda yükleme, öndeki iki cephenin saydamlaşması, arka duvarların opak kalması, saydam collider'ların etkin kalması ve dışarı çıkınca geri opaklaşma da kontrol edilir. Oyuncunun normal kaydı değişmez.

86 mantık testi katmanlı yerleşim, karşı kenardan çift yapı kurmanın reddi, yatağı bölen duvarın her iki sırada reddi, tabanın üst bloklara destek olmaması, döşeme altında ekimde tohum korunması, kapı durumu ve V3→V4 geçişini kapsar.

Manuel: 4 ile inşa, Q ile döşeme/duvar/kapı/yatak seç, R ile kenar/dönüş değiştir. Bir oda kur, kapı önünden biraz yana çekilip F ile aç ve içeri yürü. Ön/yan duvarlar yaklaşık %16 opaklığa düşerken arka cephe belirgin kalmalı; yatağı ve oyuncuyu görmelisin. Saydam duvara yürüyerek içinden geçilemediğini dene. Dışarı çıkınca duvarlar geri gelmeli. F5/F9 kapı ve oda durumunu korumalı.

Saydamlık kontrolü gerçek URP Linux build üzerinde yapıldı; Windows ve uzun süreli büyük dünya performansı bu adımda denenmedi. Çatı henüz yoktur.

Son teslim: **86/86 EditMode**, **194/194 player**, hatasız Linux build; günlükler `house-final-tests.xml`, `house-final-build.log`, `house-final-delivery.log`. İçeride ve dışarıda alınan iki görüntü incelendi.


## Çatı, taşıma, yatak ekonomisi ve inşa barı

`--farmer-check-roof` grubu: gerçek yeni oyun 50 para/yataksız, N ile yataksız uyuyamama, yatak satın alma, sekiz inşa yuvası/altı üretilmiş ikon, ikon seçimi/boş yuva UI engeli, yatağı tutup taşıma, UI/Esc iptal, döşemesiz blok ev saydamlığı, altı boş çatı kurulumu, iç/dış çatı görünürlüğü ve v5 yükleme. Normal save kullanılmaz. Eski tarım test fixture'ı 60 para ve açık test Camp kullanır; üretim açılışı bu değildir. `--farmer-check-tool-animation` ayrıca sulama hedefinin kareler arasında kademeli ilerleyip yeni merkeze oturmasını doğrular.

Manuel: 4 ile sekiz gözlü bara geç. Yatak varsa yatak ikonunu seçip kur; M ile tut ve yeni konuma bırak, R ile döndür. Boş son iki yuva işlem yapmamalı. Çatı ikonunda yükseklik hedef desteğe otomatik oturur: ince duvar/kapı için 2,4 m, blok duvar için 3 m; duvar yanından başlayıp iç boşluk üzerine uzat. İçeri girince ön/yan bloklar ve çatı saydamlaşmalı. Esc normal yedi eşya barını geri getirmeli. 2 ve basılı sol tuşla kareler arasında gezdirirken kol/su yönü yumuşak değişmeli.

8 Ekim tam tur: **219/219 player kontrolü geçti**, çıkış 0 (`Logs/roof-inventory-final.log`). Model testleri **93/93** (`Logs/roof-final-tests.xml`). Kaynak/meta eşleri ve GUID benzersizliği denetlendi; Windows bu tur test edilmedi.


## Keşif / v6

`--farmer-check-exploration` veya `run_player_checks.py --checks exploration`: yeni dünya alet/nesne sayısı, orman kamera takibi ve zemin, yanlış alet/UI/duvar/mesafe engelleri, üç vuruşta odun, kısmi kesim kayıt, yok olan kaynak collider'ı, yabani tohum, sandık ödülü, yeniden yüklemede tek seferlik ödül, dışarıda inşa modu. Mantık testleri 101/101 (`Logs/exploration-tests.xml`). Eski fixture testlerinde kamera takibi kapalıdır; yeni keşif grubu gerçek takip bileşenini açıp kontrol eder.

Sınırlı CPU örneği: `taskset -c 0,3,5,9 nice -n 10 python3 tools/art/run_player_checks.py --xvfb builds/Tools/xvfb/runtime/usr/bin/Xvfb --name benzersiz-ad`. Script kendi test process grubunu kapanışta temizler, oyuncunun normal oyununu hedeflemez. 360 s üst süre ve Xvfb bağlantı beklemesi vardır. Ekran ve Mono debugger arızası görülürse test başarılı sayılmaz.

8 Ekim keşif teslimi: 101/101 EditMode. Genel oyun turunda 237/239; iki kısa ses kuyruğu kontrolü düşük test FPS'inde geç örnekleniyordu (üç kare 0,565 s). İlk bırakma karesinde örnekleme düzeltildi; son hedefli tur 47/47 (27 ses + 20 keşif), exit 0 / FARMER_PLAYER_SMOKE_OK (`Logs/exploration-delivery.log`). Diğer 212 kontrol genel turda geçti; tüm tur bu test düzeltmesinden sonra tekrar koşulmadı.


## 9 Ekim — ağaç görünürlüğü / otomatik çatı yüksekliği

`--checks roof exploration`: çatıyı yeniden seçip tekerlek kullanmadan ince duvar/kapı üzerinde 2,4 m, blok duvar yanında 3 m önizleme ve gerçek sol tık yerleştirme. İzometrik ışında arkadaki alçak duvar öndeki yüksek desteğin hedefini çalmamalı. Ağaç yaklaşınca/uzaklaşınca bütün kalmalı, yalnız kesildiğinde kütüğe dönüşmeli.

Son doğrulama: 15/15 ilgili EditMode (`roof-auto-final-tests.xml`), 66/66 player (30 çatı + 36 keşif, `roof-tree-delivery.log`), çıkış 0 ve hata yok. Son build `roof-depth-build.log`. Önceki yüksek/alçak destek seçimi başarısızlıkları YAZILIMCI.md içinde kayıtlıdır; son tekrar bunları kapattı. Windows bu tur denenmedi.


## 9 Ekim — mobilya ve depolama

118/118 EditMode testi geçti (`Logs/furniture-tests.xml`). Linux build başarılı.
Gerçek player: `python3 tools/art/run_player_checks.py --name furniture-regression --checks controls house roof storage --xvfb builds/Tools/xvfb/runtime/usr/bin/Xvfb`; çıkış 0 ve `FARMER_PLAYER_SMOKE_OK`.
Satın alma, Yapı/Mobilya sekmeleri, yerleştirme, Tab çantası, F sandığı, tekli/Shift-tüm yığın ve sürükle-bırak aktarımı, dışarı bırakmada iptal, kayıt/yükleme, dolu sandığı M ile taşıma, uzak erişimin reddi ve gece lambası doğrulandı.
Sandık kapak/oturma animasyonları yok; Windows doğrulanmadı. Görseller: `screenshots/furniture-storage.png`, `furniture-day.png`, `furniture-night.png`.

## 10 Ekim — alet taşırken kol salınımı

`--farmer-check-tool-animation` mevcut alet hareketlerine ek olarak sulama
kabı/orak/çapa/baltayla yürüyüşte iki elin karaktere göre ileri–geri hareketini
1,2 saniye boyunca örnekler. Sağ el yayı 3,5 cm'den büyük ve serbest sol elin
%80'inden küçük olmalı; alet socket'i bütün yürüyüş boyunca bileğe 12 cm'den
fazla uzaklaşmamalı. İzole `FarmerQA` kaydı kullanılır. Sulama/savurma/yeniden
kuşanma ve tutuş kontrolleri aynı turda devam eder.
