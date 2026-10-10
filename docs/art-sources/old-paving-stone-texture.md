# Ana yol taş dokusu

10 Ekim 2026 tarihinde kullanıcı tarafından `TCom_OldPavingStone_2K_albedo.tif` adıyla teslim edildi.

- Kaynak: kullanıcı teslimi; özgün 2048×2048 RGB TIFF dosyası depo dışında korunuyor.
- Unity çalışma kopyası: `Assets/_Farmer/Art/Textures/old_paving_stone_albedo.jpg`.
- Dönüşüm: özgün boyut korunarak yüksek kaliteli JPEG (quality 95); alfa kanalı yok. İlk sarı/foto-gerçekçi görünüm geri bildirimi sonrasında çalışma kopyası %88 parlaklık ve %55 doygunlukla nötrleştirildi.
- Kullanım: `Assets/_Farmer/Art/Materials/valley_path.mat` üzerinde URP Lit Base Map.
- Kapsam: başlangıç alanından köy girişine uzanan ana yol. Köy meydanının ayrı `village_paving` malzemesi değiştirilmedi.
- Geometri: eski beş dikdörtgen segment silinmeden renderer'ları kapatıldı; yerlerine Catmull–Rom eğrisiyle üretilen tek bir `Main road surface` örgüsü kullanılıyor. UV mesafe boyunca kesintisizdir; virajlarda parça dikişi yoktur.
- Ölçek: yol genişliği 3 m, taş dokusu uzunluk boyunca her 3 metrede bir tekrar eder.
- Malzeme: hafif soğuk beyaz taban rengi, metallic 0, smoothness 0.035; mipmap, repeat, 8× anisotropic filtering ve sıkıştırmasız Unity importu açık.
- Lisans/kaynak mağaza bilgisi kullanıcı tesliminde bulunmadığından ayrıca doğrulanmadı. Dağıtım öncesinde kullanım hakkı kaydı tamamlanmalı.

Özgün TIFF dosyası dönüştürme sırasında değiştirilmedi veya silinmedi.
