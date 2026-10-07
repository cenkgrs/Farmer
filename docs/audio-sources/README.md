# Sulama kabı kaydı

- Eser: **Watering Sound**, Nokta, 21 Mart 2022.
- Kaynak: https://freesound.org/people/Nokta/sounds/625061/
- Lisans: **CC0 1.0 Universal** — https://creativecommons.org/publicdomain/zero/1.0/
- Kaynak açıklaması: sulama kabıyla yere su dökme kaydı.
- Alınan dosya: herkese açık yüksek kaliteli MP3 önizlemesi, https://cdn.freesound.org/previews/625/625061_13598887-hq.mp3
- Kontrol/indirme: 7 Ekim 2026. Kaynak sayfasında CC0 lisansı doğrulandı. Atıf zorunlu olmasa da kaynak bilgisi korunur.
- `nokta_watering_625061_preview.mp3`: indirilen önizleme değişmeden saklanır; orijinal yüklenen dosya olduğu iddia edilmez.
- Kaynak SHA-256: `db60e49b8795c2a7b1ddf246cedf1277e1db4c779b581283f2924ffc57b72d0f`.
- Oyun çıktısı: `Assets/_Farmer/Audio/watering_can_pour.wav`, 6 saniye, mono, 22050 Hz, PCM16; yaklaşık 265 KB.
- Çıktı SHA-256: `fcecab8bf11877274f46853a3d93b1a28c07c36ec620b7c3addc2088069f8827`.

## Yapılan düzenleme

FFmpeg ile 70 Hz high-pass / 6500 Hz low-pass, mono ve 22050 Hz dönüşümü. 7,5–13,5 saniye arası alındı; 7,35–7,5 saniyelik ön bölüm kullanılarak sondaki 150 ms doğrusal crossfade yapıldı. DC ortalaması çıkarıldı. RMS 0.052 hedefiyle kazanç uygulandı, yalnızca ani sıçramaları yumuşatmak için `0.8 * tanh(sample / 0.8)` sınırlaması yapıldı ve RMS tekrar 0.052 seviyesine dengelendi. Son tepe 0.75922, RMS 0.052. Bu seviye önceki sentezin yaklaşık RMS değerine yakın seçilerek yeni kaydın fazladan sessiz kalması önlendi. Sonuç PCM16 WAV olarak yazıldı. Perde/hız değiştirilmedi; sentetik ton eklenmedi.

Unity importu: PCM, Decompress On Load, Force To Mono, Preserve Sample Rate. Farm sahnesindeki `FarmPresentation.wateringSound` bu asseti referanslar. Asset paylaşımlıdır; çalışma sonunda Destroy edilmez. `WateringAudioSetup.Apply` import/bağlantı aracı yalnızca bu alanı günceller, sahneyi yeniden üretmez.

Kaynak kazancı 0.064 → **0.0832** (%30 artış). Kayıt önceki sentezden farklı olduğu için bu değer algılanan ses yüksekliğinin tam %30 değiştiği iddiası değildir. Mevcut 0,12 saniye açılış ve 0,7 saniye kapanış zarfı korunur. Sesin oyun içindeki hissi kullanıcı dinlemesiyle değerlendirilir.
