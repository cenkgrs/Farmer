# Kullanıcı teslimi — balta ve kütük, 8 Ekim 2026

Orijinal `axe 3d model.glb` ve `tree stump 3d model.glb` dosyaları sırasıyla `axe.glb` ve `tree_stump.glb` olarak değişmeden saklandı. SHA-256 ve kaynak doku boyutları `source_manifest.json` içinde. Kullanıcı harici AI aracıyla üretti; model sürümü/ayarları belirtilmedi, yeni üretim veya ücretli API çağrısı yapılmadı. Hak bilgisi kullanıcı teslimine dayanır; yeni lisans atanmadı.

- Balta: 15.792 üçgen / 9.727 vertex; tek mesh ve materyal. Önerilen eskiz bütçesini aşıyor; tek elde kullanılan bu sürümde geometri korunuyor, performans ölçümü iddiası yok.
- Kütük: 2.173 üçgen / 2.032 vertex; tek mesh ve materyal.
- İki kaynakta da 4K base color, normal ve metallic/roughness dokuları var. Çalışma dokuları 1K; kaynak 4K GLB içinde korunur.
- Blender 4.5.14 yerel dönüşümü: balta ana ekseni PCA ile dikleştirildi, kalın baş üst tarafa alındı, yükseklik 0,70 m. Kütük taban merkezli, 0,30 m yüksek ve yaklaşık 0,54 m geniş. `conversion.json` gerçek ölçü/sayım içerir.
- Unity prefabında balta sapının 0,10–0,18 m aralığı kavrama merkezi; kesici yüz ileri yönlü dikey savurma düzlemine alınır. Mevcut sağ el socket ve prosedürel kesme hareketi kullanılır.
- Kütük yalnızca toplanmış ağacın yerinde etkinleşir. Gövde/taç kapanır, eski collider kapanması korunur; kütükte collider yoktur. Kaydedilmiş kesilmiş ağaçlar da yeni görseli kullanır; kayıt v6 değişmedi.
- URP malzemeleri ilk teslim setiyle aynı beyaz BaseColor çarpanı ve 0,65 normal/smoothness ayarıyla oluşturuldu. Kaynağın 0,8 BaseColor çarpanı bu sanat yönüyle aynı şekilde beyaza normalize edilir.

## Tekrar üretim

Depo kökünden `python3 tools/art/prepare_exploration.py`, ardından `blender -b -t 2 --python tools/art/convert_exploration.py`. Unity batch giriş noktası `Farmer.Editor.ExplorationArtSetup.ApplyAndBuild` yalnızca bu iki materyal/prefabı üretir ve Linux build alır; ana Farm sahnesini tekrar üretmez. Normal klonda bu işlemler gerekmez; FBX, dokular, materyaller, prefablar ve meta dosyaları sürümlüdür.

## Doğrulama

Linux gerçek-player keşif kontrolleri; balta mesh/doku ve el bağlantısı, kütüğün görünmesi/boyutu/collider durumu, üç vuruşla odun ve kayıt sonrası kütük kontrolü içerir. Yakın çekimler `docs/screenshots/exploration-axe-grip.png` ve `exploration-axe-stump.png` içinde. Sonuçların güncel sayımı `YAZILIMCI.md` içinde. Windows bu tur derlenmedi.

Ağaç tacı/gövdesi, sandık ve yabani bitki henüz geçici geometri; diğer teslimler bekleniyor.
