# Prototip doğrulaması

Unity **6000.3.25f1**, Linux. 7 Ekim 2026: 44 EditMode testi başarılı. Gerçek geliştirme player'ında hareket/seçim ve tam tarım döngüsü kontrol edildi. Windows henüz denenmedi.

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
4. Fareyi tarlada gezdir; tıklamadan hedef çerçevesi gelmeli. Uzak kare turuncu olmalı ve E işlem yapmamalı.
5. Fare HUD üzerindeyken/pencere dışındayken hedef kaybolmalı; E önceki karede işlem yapmamalı. 1/2/3 veya eşya çubuğu düğmeleri eldeki eşyayı değiştirmeli.
6. Pazar tezgâhına yaklaş, B veya düğmelerle tohum al. Paran ve tohum sayın doğru değişmeli.
7. 1 ile tohumu kuşan, yakın boş kareyi fareyle hedefle ve E ile ek. Aynı eşyayla tekrar E sulamamalı. 2 ile sulama kabını kuşan ve E ile sula; toprak koyulaşmalı. 1 ile ikinci bir kareye ekip kuru bırak.
8. Kamp minderine yaklaş ve N ile günü bitir. Sulanan ürün büyümeli, kuru ürün aynı aşamada kalmalı. Yeniden sulamadan üç kez geceyi geçir; üçüncü geceden sonra hasat hazır olmalı. Hasat sonrası tekrar ekilen bitki bir kez yeniden sulanmalı.
9. Sulama kabıyla olgun ürünü hasat edememelisin. 3 ile orağı kuşan ve E ile hasat et; pazara gidip V ile sat ve kazançla tekrar tohum al.
10. Bir kareyi suladıktan sonra oyunu kapat/aç veya F9'a bas. Para, tohum, hasat envanteri, gün, bitki, sulama ve kuşanılan eşya korunmalı.

## Tarım döngüsünün otomatik player denemesi

```bash
./builds/Linux/Farmer.x86_64 -screen-fullscreen 0 -screen-width 1280 -screen-height 720 \
  --farmer-smoke-capture builds/QA/farming-check.png --farmer-check-controls --farmer-check-farming \
  -logFile "$PWD/Logs/farming-player.log"
```

`--farmer-check-farming`, pazar düğmesine gerçek UI tıklaması ve 1/2/3/B/E/N/V/F5/F9 tuş olaylarıyla satın alma, kuru bitkinin beklemesi, dört aşama, uzak hasadın engellenmesi, hasat, satış, yeniden yatırım ve kayıt yüklemeyi kontrol eder. Tarla işlemleri öncesinde fare tıklaması gönderilmez. Yanlış alet, HUD/pencere dışı E, eşya düğmesi ve aynı karede imleç hareketi + E de kontrol edilir. Ayrıca pazar ve her büyüme aşaması için ayrı PNG üretir; son görüntüde farklı yaşta dört bitki bırakır. Testte konum hazırlığı teleport ile yapılır; hareket/çarpışma ayrıca önceki kontrol setinde denenir.

**Her smoke capture çalıştırması ayrı geçici kayıt klasörü kullanır; oyuncunun normal çiftliğini yüklemez veya üzerine yazmaz.** Sonunda test kayıt klasörü temizlenir. Ekran görüntüleri ve günlükler `builds/QA` / `Logs` altında kalır. Başarı için 48 `FARMER_FARM_CHECK_OK`, `FARMER_FARMING_CHECKS_FINISHED`, `FARMER_PLAYER_SMOKE_OK` ve çıkış 0 gerekir; `FARMER_FARM_CHECK_FAILED` olmamalı. Ses kontrolü AudioSource oynatma durumunu doğrular; dinleme kalitesi değerlendirmesi değildir.

## Kayıt dosyası

Oyuncu kaydı Unity'nin `Application.persistentDataPath` klasöründe `farm-v1.json` dosyasıdır. Linux'ta genel konum `~/.config/unity3d/Cenk Gurses/Farmer/`; Windows'ta `%USERPROFILE%/AppData/LocalLow/Cenk Gurses/Farmer/`. İşletim sistemi doğrulaması Linux üzerinde yapıldı.

Her başarılı işlem ve normal çıkış kaydedilir; F5 tekrar kaydeder, F9 diskten yükler. Yeni dosya önce geçici dosyaya yazılır, sonra atomik değiştirme yapılır; önceki kayıt `.bak` olarak korunur. Ana kayıt bozuksa sağlam yedek açılır; bozuk dosya üzerine yazmadan önce `.corrupt-*` kopyası korunur. İkisi de okunamazsa sessizce yeni oyuna başlanmaz: işlemler durdurulur, dosyalar korunur ve ekranda hata gösterilir. Kaydetme izni/disk sorunu olursa mevcut oturum bellekte sürer; F5 ile yeniden dene.

Kayıtlar Git'e girmez ve iki bilgisayar arasında otomatik taşınmaz. Önceki günlük sulama kayıtlarında büyümesi başlamış ürünün (`growth > 0`) daha önce sulandığı anlaşılır ve sulanmış olarak açılır. Şema sürümü 1; para, envanter, gün ve tarla durumunu içerir. Kuşanılan eşya yeni isteğe bağlı `equippedItem` alanında saklanır. Eski sürüm 1 kayıtlarında bu alan yoksa tohum seçilir; iki kalıcı alet envanterde bulunur. Karakter konumu saklanmaz; açılışta pazar yakınında başlarsın. Kayıt yeniyken tohum almak ilk otomatik kaydı oluşturur. Aynı çiftliği iki uygulamada eşzamanlı açma.
