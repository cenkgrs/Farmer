# Envanter görsel kaynakları

7 Ekim 2026; kullanıcı isteğiyle `imagegen` skill / **yerleşik ImageGen** kullanıldı. CLI/API fallback kullanılmadı. Stil referansı `docs/references/farm_isometric_concept.png` içindeki alt envanterdir.

- Üretim: yedi ayrı eşya ikonu ve tam yedi boş göz içeren ahşap çerçeve, her biri ayrı araç çağrısı.
- Kaynaklar: `Assets/_Farmer/Resources/InventoryArt/*.png`; özgün çıktılar değiştirilmeden kopyalandı, alfa korundu.
- [Tüm promptlar, dosyalar, boyutlar ve sınırlar](inventory-prompts.json).
- `InventoryIcon` yalnızca saydam kenar boşluklarını Sprite UV dikdörtgeniyle dışarıda bırakır. Çok düşük alfa gürültüsünün sınırı büyütmemesi için ölçümde alfa > 8 kullanıldı; kaynak resme piksel düzenlemesi yapılmadı. Normalleştirilmiş sınırlar Unity import ölçeklemesinde de çalışır.
- `FarmHud`: ahşap çerçeve 476×78; ikonlar en-boy oranını korur. Adetler, tıklama alanı, hover ve seçili gözün yarı saydam altın rengi ayrı UI öğeleridir.
- UI kaynaklarının `.meta` GUID değerlerini koru. Yeni boyut/ikon üretiminde Sprite sınırlarını ve oyun içi hizalamayı birlikte kontrol et.

Bu görüntüler oyun için üretilmiş 2D görsellerdir; dış stok görsel veya 3D model değildir. Görsel kalite Linux player ekran görüntüsünde değerlendirildi; özgün konseptin tüm sahne kalitesine ulaşıldığı iddia edilmez.

## İnşa barı — 8 Ekim 2026

Altı ayrı ikon ve sekiz gözlü çerçeve **yerleşik ImageGen** ile ayrı çağrılarda üretildi; CLI kullanılmadı. Orijinal RGBA PNG dosyaları `Assets/_Farmer/Resources/ConstructionArt` içinde. [Tam prompt seti](construction-prompts.json). `ConstructionArtwork` alfa > 128 ölçümüyle bulunan normalize Sprite sınırlarını kullanır; kaynak alfa/pikseller değiştirilmedi. Çerçeve 541×85 referans boyutunda, sekiz tıklama alanı 54×54; ilk altı dolu, ikisi boş. Miktarlar ve seçili yuva vurgusu ayrı UI öğeleridir. Bunlar 3D yapı modellerini değiştirmez.

## Balta — 8 Ekim keşif

Yerleşik ImageGen ile üretilen [balta promptu](axe-prompt.json) ve `Assets/_Farmer/Resources/InventoryArt/axe.png`; alfa korunur. Normal eşya barında balta eklenince mevcut sekiz yuvalı `ConstructionArt/construction_frame.png` arka planı kullanılır. İnşa ve eşya barları ayrı seçim durumlarını korur.
