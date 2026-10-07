# Prototip doğrulaması

Unity **6000.3.25f1**, Linux. 7 Ekim 2026: 39 EditMode testi başarılı. Gerçek geliştirme player'ında hareket/seçim ve tam tarım döngüsü kontrol edildi. Windows henüz denenmedi.

## Mantık testleri

Unity Test Runner'ın EditMode sekmesinde `Farmer.Tests` testlerini çalıştırabilir veya editör kapalıyken depo kökünde şu komutu kullanabilirsin:

```bash
mkdir -p Logs
UNITY_EDITOR="$HOME/Unity/Hub/Editor/6000.3.25f1/Editor/Unity"
"$UNITY_EDITOR" -batchmode -nographics -projectPath "$PWD" \
  -runTests -testPlatform EditMode -testFilter Farmer.Tests \
  -testResults "$PWD/Logs/farming-tests.xml" -logFile "$PWD/Logs/farming-tests.log"
```

Test komutuna `-quit` ekleme; Test Runner tamamlanınca çıkar. XML'de sonuç, test sayısı ve başarısız/atlanan testleri kontrol et. Sadece çıkış koduna bakma.

## Build ve gerçek player kontrolü

```bash
"$UNITY_EDITOR" -batchmode -quit -projectPath "$PWD" \
  -executeMethod Farmer.Editor.ProjectSetup.BuildLinux \
  -logFile "$PWD/Logs/farming-build.log"
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
4. Fareyi tarlada gezdir. Yakındaki kareyi seç; seçili çerçeve kalmalı. Uzak kareye tıklayınca yaklaşma bildirimi gelmeli.
5. HUD'a tıklamak seçimi değiştirmemeli. ESC ve sağ tık seçimi temizlemeli. Fareyi pencere dışına taşımak önizlemeyi temizlemeli.
6. Pazar tezgâhına yaklaş, B veya düğmelerle tohum al. Paran ve tohum sayın doğru değişmeli.
7. Yakın bir boş kare seç; E ile ek ve tekrar E ile sula. Toprak koyulaşmalı. İkinci bir kareye ekip sulamadan bırak.
8. Kamp minderine yaklaş ve N ile günü bitir. Sulanan ürün büyümeli, kuru ürün aynı aşamada kalmalı. Her gün yeniden sula; üçüncü sulanmış geceden sonra hasat hazır olmalı.
9. E ile hasat et, pazara gidip V ile sat ve kazançla tekrar tohum al.
10. Bir kareyi suladıktan sonra oyunu kapat/aç veya F9'a bas. Para, tohum, hasat envanteri, gün, bitki ve sulama durumu korunmalı.

## Tarım döngüsünün otomatik player denemesi

```bash
./builds/Linux/Farmer.x86_64 -screen-fullscreen 0 -screen-width 1280 -screen-height 720 \
  --farmer-smoke-capture builds/QA/farming-check.png --farmer-check-controls --farmer-check-farming \
  -logFile "$PWD/Logs/farming-player.log"
```

`--farmer-check-farming`, pazar düğmesine gerçek UI tıklaması ve B/E/N/V/F5/F9 tuş olaylarıyla satın alma, kuru bitkinin beklemesi, dört aşama, uzak hasadın engellenmesi, hasat, satış, yeniden yatırım ve kayıt yüklemeyi kontrol eder. Ayrıca pazar ve her büyüme aşaması için ayrı PNG üretir; son görüntüde farklı yaşta dört bitki bırakır. Testte konum hazırlığı teleport ile yapılır; hareket/çarpışma ayrıca önceki kontrol setinde denenir.

**Her smoke capture çalıştırması ayrı geçici kayıt klasörü kullanır; oyuncunun normal çiftliğini yüklemez veya üzerine yazmaz.** Sonunda test kayıt klasörü temizlenir. Ekran görüntüleri ve günlükler `builds/QA` / `Logs` altında kalır. Başarı için 41 `FARMER_FARM_CHECK_OK`, `FARMER_FARMING_CHECKS_FINISHED`, `FARMER_PLAYER_SMOKE_OK` ve çıkış 0 gerekir; `FARMER_FARM_CHECK_FAILED` olmamalı. Ses kontrolü AudioSource oynatma durumunu doğrular; dinleme kalitesi değerlendirmesi değildir.

## Kayıt dosyası

Oyuncu kaydı Unity'nin `Application.persistentDataPath` klasöründe `farm-v1.json` dosyasıdır. Linux'ta genel konum `~/.config/unity3d/Cenk Gurses/Farmer/`; Windows'ta `%USERPROFILE%/AppData/LocalLow/Cenk Gurses/Farmer/`. İşletim sistemi doğrulaması Linux üzerinde yapıldı.

Her başarılı işlem ve normal çıkış kaydedilir; F5 tekrar kaydeder, F9 diskten yükler. Yeni dosya önce geçici dosyaya yazılır, sonra atomik değiştirme yapılır; önceki kayıt `.bak` olarak korunur. Ana kayıt bozuksa sağlam yedek açılır; bozuk dosya üzerine yazmadan önce `.corrupt-*` kopyası korunur. İkisi de okunamazsa sessizce yeni oyuna başlanmaz: işlemler durdurulur, dosyalar korunur ve ekranda hata gösterilir. Kaydetme izni/disk sorunu olursa mevcut oturum bellekte sürer; F5 ile yeniden dene.

Kayıtlar Git'e girmez ve iki bilgisayar arasında otomatik taşınmaz. Şema sürümü 1; para, envanter, gün ve tarla durumunu içerir. Karakter konumu saklanmaz; açılışta pazar yakınında başlarsın. Kayıt yeniyken tohum almak ilk otomatik kaydı oluşturur. Aynı çiftliği iki uygulamada eşzamanlı açma.
