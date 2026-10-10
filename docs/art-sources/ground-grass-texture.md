# Zemin çim dokusu

10 Ekim 2026 tarihinde kullanıcı tarafından `TCom_Ground_Grass03_2x2_2K_albedo.tif` adıyla teslim edildi.

- Kaynak: kullanıcı teslimi; özgün 2048×2048 RGB TIFF dosyası depo dışında korunuyor.
- Unity çalışma kopyası: `Assets/_Farmer/Art/Textures/ground/ground_grass03_albedo.jpg`.
- Dönüşüm: özgün boyut korunarak yüksek kaliteli JPEG (quality 95); alfa kanalı yok. Oyun paletinde kararmaması için çalışma kopyasına %125 parlaklık ve %82 doygunluk düzeltmesi uygulandı.
- Kullanım: `Assets/_Farmer/Materials/Ground.mat` üzerinde URP Lit Base Map.
- Ölçek: ilk 2 m/128×128 tekrar uzaktan kirli tek renge düştüğü için görsel değerlendirme sonrası 4 m/64×64 tekrar kullanılıyor.
- Malzeme: beyaz taban rengi, metallic 0, smoothness 0.04; mipmap, repeat, 8× anisotropic filtering ve sıkıştırmasız Unity importu açık.
- Lisans/kaynak mağaza bilgisi kullanıcı tesliminde bulunmadığından ayrıca doğrulanmadı. Dağıtım öncesinde kullanım hakkı kaydı tamamlanmalı.

Özgün TIFF dosyası dönüştürme sırasında değiştirilmedi veya silinmedi.
