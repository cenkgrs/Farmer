# Envanter görsel kaynakları

7 Ekim 2026; kullanıcı isteğiyle `imagegen` skill / **yerleşik ImageGen** kullanıldı. CLI/API fallback kullanılmadı. Stil referansı `docs/references/farm_isometric_concept.png` içindeki alt envanterdir.

- Üretim: yedi ayrı eşya ikonu ve tam yedi boş göz içeren ahşap çerçeve, her biri ayrı araç çağrısı.
- Kaynaklar: `Assets/_Farmer/Resources/InventoryArt/*.png`; özgün çıktılar değiştirilmeden kopyalandı, alfa korundu.
- [Tüm promptlar, dosyalar, boyutlar ve sınırlar](inventory-prompts.json).
- `InventoryIcon` yalnızca saydam kenar boşluklarını Sprite UV dikdörtgeniyle dışarıda bırakır. Çok düşük alfa gürültüsünün sınırı büyütmemesi için ölçümde alfa > 8 kullanıldı; kaynak resme piksel düzenlemesi yapılmadı. Normalleştirilmiş sınırlar Unity import ölçeklemesinde de çalışır.
- `FarmHud`: ahşap çerçeve 476×78; ikonlar en-boy oranını korur. Adetler, tıklama alanı, hover ve seçili gözün yarı saydam altın rengi ayrı UI öğeleridir.
- UI kaynaklarının `.meta` GUID değerlerini koru. Yeni boyut/ikon üretiminde Sprite sınırlarını ve oyun içi hizalamayı birlikte kontrol et.

Bu görüntüler oyun için üretilmiş 2D görsellerdir; dış stok görsel veya 3D model değildir. Görsel kalite Linux player ekran görüntüsünde değerlendirildi; özgün konseptin tüm sahne kalitesine ulaşıldığı iddia edilmez.
