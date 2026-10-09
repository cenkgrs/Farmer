# Kullanıcı teslimi — ağaç, keşif sandığı, yabani bitki

9 Ekim 2026. Kaynak dosyalar değiştirilmeden korundu:

| Kullanıcının dosyası | Depo kaynağı | Kaynak üçgen |
| --- | --- | --- |
| low-poly tree 3d model.glb | tree_oak.glb | 15.884 |
| wooden treasure chest 3d model.glb | treasure_chest.glb | 9.078 |
| stylized plant 3d model.glb | wild_plant.glb | 3.047 |

Her kaynak tek mesh, bir materyal ve gömülü 4K dokularla geldi. Model sürümü/üretim ayarları belirtilmedi. Hak bilgisi kullanıcı teslimine dayanır; yeni üçüncü taraf lisansı atanmadı. SHA-256, doku boyutları ve sayımlar `source_manifest.json` içinde. Yeni Tripo üretimi veya ücretli API çağrısı yapılmadı.

## Yerel dönüşüm

- `python3 tools/art/prepare_forest.py`: 1K çalışma dokuları; normal ve metallic/smoothness kanal dönüşümü önceki setle aynı. Kaynak 4K dokular GLB içinde korunur.
- `blender -b -t 2 --python tools/art/convert_forest.py`: Blender 4.5.14. Metre ölçeği, taban merkezi, UV koruyan parça ayrımı, FBX çıktısı.
- Ağaç 3 m boy, yaklaşık 2,81 × 2,18 m yayılım. Üst taç ve dallar 0,90 m düzleminden ayrılır; kesim yüzeyleri kapatılır. Yakınlıkta üst bölüm gizlenir, alt gövde ve collider kalır. İlk renk tabanlı ayırma boyanmış gövde detaylarını da deldiği için kullanılmadı. Çalışma mesh'i kapatılmış kesimlerle toplam 16.432 üçgendir.
- Sandık yaklaşık 0,95 × 0,66 × 0,643 m. Kapak 0,386 m düzleminde ayrıldı; gövdeye 1,5 cm iç duvar kalınlığı, kapağın altına kapalı yüzey eklendi. UV dikişi vertexleri birleştirilerek iç duvar taşmaları azaltıldı. Açılır kapak arka -Z kenarında döner. Dış yüzey kullanıcının dokusu; yeni iç yüzeyler mat koyu ahşap. Çalışma toplamı 17.228 üçgen (Body 13.236 + Lid 3.992).
- Bitki 0,65 m boy, yaklaşık 0,54 × 0,45 m. Kaynaktaki metalik altın parlaması kaldırıldı; ağaç ve bitki organik mat URP materyalleri kullanır. Sandık metal bantlarının dokusu korunur.
- Kaynaklar eskiz bütçelerini aşıyor; bu teslimde topoloji büyük ölçüde korunup gerekli kapanış yüzeyleri eklendi. Büyük orman performansı/LOD testi yapılmadı.

`Farmer.Editor.ForestArtSetup.ApplyAndBuild` yeni FBX'leri ve materyalleri yalnızca ilgili Resources/ExplorationArt prefablarına bağlar; mevcut Farm sahnesini yeniden üretmez. Üretilen FBX/doku/materyal/prefab/meta dosyaları Git'te olduğundan normal klonda dönüştürme gerekmiyor.

## Oyun davranışı

Ağaç kesilince önceden teslim edilen kütük görünür. Bitki toplanınca gizlenir. Sandık açılınca teslim edilen kapak dönerek açık kalır. Mevcut kaynak ödülleri, collider kuralları ve v6 kayıt korunur. Görsel ağaç kökleri dar geçiş collider'ından geniştir; her kök için ayrı fizik geometri eklenmedi.

Son gerçek-player doğrulama ve görsel kontrol kaydı `YAZILIMCI.md` içinde. Windows build ve performans profili ayrı açık işlerdir.
