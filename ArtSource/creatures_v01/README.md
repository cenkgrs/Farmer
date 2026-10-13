# Kullanıcı model teslimi — 13 Ekim 2026

Tavşan, slime, kaya yengeci, geyik ve Defne GLB dosyaları özgün halde korunur. Kaynak araç sürümü belirtilmedi. Hash ve gerçek geometri ölçümleri `source_manifest.json` içinde.

| Model | Üçgen | Vertex | Unity ölçü XYZ (m) |
|---|---:|---:|---|
| rabbit | 9770 | 8151 | 0.415, 0.650, 0.496 |
| forest_slime | 9194 | 6188 | 0.941, 0.800, 0.885 |
| stone_beetle | 10798 | 9633 | 0.850, 0.650, 0.844 |
| deer | 11928 | 10321 | 0.564, 1.900, 1.685 |
| defne | 39980 | 32973 | 0.776, 1.700, 0.539 |

Beş model de tek mesh/malzeme, UV ve 4K basecolor/normal/metallic-roughness içeriyor. Hiçbirinde skin, iskelet veya animasyon yok. `tools/art/prepare_creatures.py` mevcut mining dönüşüm hattını kullanır; Unity'ye sol elli eksen/winding ve UV dönüşümü yapar, tabanı merkeze alır ve 1K çalışma dokuları çıkarır. Unity `CreatureArtSetup.Apply` mesh/materyal/prefab üretir, görsel ön kontrol için boş sahnede render alır. Sahnedeki kullanıcı düzenine dokunmaz. Ön yüz +Z'ye bakacak şekilde prefab içindeki model 180° döndürülmüştür. Normal klon/açılışta dönüşüm araçlarını çalıştırmak gerekmez; üretilmiş assetler sürümlenir.

Tavşan mevcut dolaşma/gözleme/kaçış, av ve kayıt mantığına bağlıdır. Slime ve kaya yengeci mevcut iki düşmanın görsellerini değiştirir; kimlikler, ödüller ve kayıt şeması korunur. Rigsiz teslim nedeniyle tavşanda gövde sıçraması; düşmanlarda squash, hamle, darbe sarsıntısı ve ölüm dönüşü kullanılır. Eklemli bacak/koşu animasyonu eklenmiş sayılmaz; rigli model/klip teslimi sonraki sanat işi. Geyik ve Defne de kullanıcı teslimlerine bağlandı; geyik av yapısını, Defne konuşma/oyuncuya dönme ve çarpışma davranışını korur. Defne rigsiz A-pose modeldir; konuşma/el/yürüme animasyonları yok. Defne 39.980 üçgenle önerilen bütçeyi aşar; tek NPC için özgün geometri korundu, mobil/çoklu NPC optimizasyonu doğrulanmadı.
