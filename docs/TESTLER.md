# Prototip doğrulaması

Unity **6000.3.25f1**, Linux. 7 Ekim 2026: 12 EditMode testi ve gerçek geliştirme player'ında 11 hareket/seçim kontrolü başarılı. Windows henüz denenmedi. Testler 0.1'in yalnızca hareket/kare seçimi adımını kapsar.

## Mantık testleri

Unity Test Runner'ın EditMode sekmesinde `Farmer.Tests` testlerini çalıştırabilir veya editör kapalıyken depo kökünde şu komutu kullanabilirsin:

```bash
mkdir -p Logs
UNITY_EDITOR="$HOME/Unity/Hub/Editor/6000.3.25f1/Editor/Unity"
"$UNITY_EDITOR" -batchmode -nographics -projectPath "$PWD" \
  -runTests -testPlatform EditMode -testFilter Farmer.Tests \
  -testResults "$PWD/Logs/movement-tests.xml" -logFile "$PWD/Logs/movement-tests.log"
```

Test komutuna `-quit` ekleme; Test Runner tamamlanınca çıkar. XML'de sonuç, test sayısı ve başarısız/atlanan testleri kontrol et. Sadece çıkış koduna bakma.

## Build ve gerçek player kontrolü

```bash
"$UNITY_EDITOR" -batchmode -quit -projectPath "$PWD" \
  -executeMethod Farmer.Editor.ProjectSetup.BuildLinux \
  -logFile "$PWD/Logs/movement-build.log"
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

Henüz ekim, sulama, hasat, pazar veya kayıt/yükleme bulunmuyor; bunların çalıştığı bu testlerden çıkarılamaz.
