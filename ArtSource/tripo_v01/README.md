# Farmer — kullanıcı teslimi, 7 Ekim 2026

Kullanıcının ürettiği dört Tripo GLB ve iki Mixamo FBX bu görsel ara aşamanın kaynaklarıdır. Kaynak dosya adları ve SHA-256 değerleri `source_manifest.json` içindedir. GLB'ler burada değişmeden korunur; orijinal Mixamo FBX'ler `Assets/_Farmer/Art/Models/farmer_idle.fbx` ve `farmer_walk.fbx` olarak korunur. Ayrı bir T-Pose teslimine gerek kalmadı: iki FBX de aynı karakterin skin/iskeletini içeriyor. Tripo model sürümü ve üretim ayarları teslimde belirtilmedi; uydurulmadı. Kaynak hakları kullanıcı teslimine dayanır; bu dosyalara yeni bir üçüncü taraf lisansı atanmamıştır.

## İnceleme

- Karakter GLB: 35.819 üçgen, 28.568 vertex, bir UV ve bir malzeme; iskelet yok.
- Mixamo FBX'ler: 35.820 üçgen (Blender tessellation), 28.633 vertex, 65 kemik, 52 ağırlık grubu; UV var, malzeme/doku yok. Orijinal GLB'nin dokusu aynı karaktere uygulandı.
- Idle: 30 FPS, 1–299 kare, Unity'de yaklaşık 9,933 saniye. Walk: 30 FPS, 1–27 kare, yaklaşık 0,867 saniye.
- Sulama kabı: 6.222 üçgen. Orak: 4.912 üçgen. Pazar: 14.016 üçgen. Bunlar önceki prompt hedeflerinden farklı gerçek sayımlardır; bu ilk sahnede tekil kullanılan modellerin topolojisi korunmuştur.
- Her GLB içinde 4096×4096 base color, normal ve roughness/metallic dokuları bulunur. Karakter/pazar için 2K, aletler için 1K çalışma dokuları çıkarıldı. Kaynak 4K görüntüler GLB içinde korunur.
- Unity, her Mixamo FBX'te kendiyle kesişen bir poligonu attığını bildirir. İlk yakın çekimde belirgin bir yüzey boşluğu görülmedi; kaynak kusurunun onarıldığı iddia edilmez. Son sanat temizliğinde yeniden kontrol edilmeli.

`inspection.json`: Blender 4.5.14 LTS ham inceleme çıktısı. `prop_conversion.json`: statik dönüşümün üçgen/ölçek bilgileri.

## Yerel dönüşüm

Üretim için yeni Tripo çağrısı veya ücretli işlem yapılmadı. Kullanılan araçlar: Blender 4.5.14 LTS, Python 3 + Pillow 11.0.0, Unity 6000.3.25f1.

Depo kökünden:

```bash
python3 tools/art/prepare_models.py
blender -b --python tools/art/convert_props.py
```

İlk betik GLB içindeki dokuları çıkarıp boyutlandırır. glTF metallic-roughness G=roughness/B=metallic kanalları, Unity için R=metallic/A=1-roughness biçiminde paketlenir. Base color sRGB, normal ve metallic/smoothness linear import edilir. İkinci betik yalnızca statik modelleri taban merkezine alır, metre ölçeğine getirir, FBX'e çevirir. Mesh topolojisi, UV ve orijinal kaynaklar korunur. Son model yükseklikleri: sulama kabı 0,42 m, orak 0,50 m, pazar 2,40 m. Karakter sahnede şapka dahil 1,85 m olarak ayarlanmıştır.

`ArtImportSetup.ImportAndInspect`: Unity import ayarları ve URP malzemeleri. `ArtSceneSetup.Apply`: yalnızca ilk entegrasyon için; zaten entegre sahnede tekrar çalıştırılmaz. Normal klon/açılışta bu araçların hiçbirini çalıştırmak gerekmez; üretilmiş FBX, materyal, prefab, Animator ve sahne Git'tedir.

## Animasyon ve aletler

Idle ana Humanoid Avatar'ı oluşturur; Walk aynı Avatar'ı kullanır. İki klip loop, root hareketi bake edilmiş; runtime root motion kapalı. Idle/Walk BlendTree, CharacterController'ın gerçekten katettiği yatay mesafeden beslenir. Pazar ve tarlanın oyun kuralları değişmez.

Sağ el IK ile gövdenin önünde tutulur, parmaklarda hafif procedural kavrama uygulanır. Alet socket'i animasyonlu avuç merkezini izler; kabın dik durması için oyuncunun yönüne göre döner. Bu, yeni sulama/hasat animasyonu değildir: teslim edilen Idle/Walk üzerinde temel tutuş düzenlemesidir. Tohum torbası ve bitkiler hâlâ geçici modellerdir.

## Git kararı

Linux'ta Git LFS kurulu değil; Windows LFS kurulumu doğrulanamadı. İki makine hazır olmadan LFS filtreleri açılmadı. Bu ilk set normal Git'te tutuluyor (en büyük dosya yaklaşık 16 MB); model/doku sürümleri çoğaldığında LFS geçişi iki makine birlikte hazırlanarak ele alınmalı. Kullanıcının Downloads klasöründeki orijinaller değiştirilmedi.
