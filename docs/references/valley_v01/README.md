# Vadi ve köy — Tripo model üretim paketi

[Harita konsepti](valley_concept.png) · [Harita tasarım planı](../../HARITA_TASARIMI.md)

Görseller yerleşik ImageGen ile ayrı ayrı üretildi ve incelendi. Tam promptlar `generation_manifest.json` içinde; haritanın ilk üretim promptu `master-original-prompt.txt`. Bunlar 2D referanslardır, hazır 3D modeller değildir. Harita görselini bütün olarak Tripo'ya vermeyin; aşağıdaki tek nesneli görselleri kullanın.

## İlk teslim sırası

| Öncelik | Referans / dosya adı | Hedef ölçü | Üçgen hedefi | Doku |
|---|---|---|---|---|
| 1 | [Dükkân](village_shop.png) → village_shop.glb | 5×4 m taban, 4,5 m yüksek | 8–15 bin | 2K |
| 2 | [Kaya](granite_boulder.png) → granite_boulder.glb | 1,6×1,2×1 m | 0,5–1,5 bin | 1K |
| 3 | [Çalı](meadow_bush.png) → meadow_bush.glb | 0,9×0,9×0,7 m | 0,5–1,5 bin | 1K |
| 4 | [Çam](pine_tree.png) → pine_tree.glb | 5 m yüksek, taç 2,5–3 m | 2–4 bin | 1K |
| 5 | [Köprü](wood_bridge.png) → wood_bridge.glb | 4 m uzunluk, 2 m genişlik, korkuluk 0,9 m | 3–6 bin | 2K |
| 6 | [Kuyu](village_well.png) → village_well.glb | 1,5 m çap, 2 m toplam yüksek | 2–4 bin | 1K |
| 7 | [Kayalık modülü](cliff_module.png) → cliff_module.glb | 4×2 m taban, 2 m yüksek | 1–3 bin | 2K |
| 8 | [Taş kemer](ruin_arch.png) → ruin_arch.glb | 3 m geniş, 3,3 m yüksek; geçiş 1,6 m | 2–4 bin | 2K |

Hepsini beklemek gerekmiyor. İlk dükkân + kaya + çalı ile köy/çiftlik yolu kurulmaya başlanabilir. Bunlar üretim hedefidir; Tripo'nun gerçek çıktısı teslimde ölçülür. Görselde oranlar ölçü cetveli değildir; gerekirse Unity/Blender'da normalize edeceğiz.

## Ortak kısa prompt eki

`Match the supplied reference. Cozy hand-painted low-poly 3D farming game asset, warm natural colors, softly beveled chunky shapes. One isolated object, no ground, no people, no extra props. Clean UVs, embedded base color, normal and roughness textures. Export textured GLB. No rig or animation.`

Aşağıdaki nesne cümlesine bu eki ekle; referans resmini de yükle.

### Dükkân

`Small rustic village shop exterior, cream plaster and honey-brown timber frame, terracotta gable roof, wooden door, blank hanging sign, green-and-cream canopy and empty front sales counter. Footprint 5 by 4 meters, height 4.5 meters. Closed exterior shell.`

İlk sürümde satış dış tezgâhtan yapılır, içeri girilebilir dükkân varsayılmıyor. Çatı/gövde/kapıyı ayrı mesh üretebiliyorsan iyi; olmuyorsa tek GLB gönder, kontrol edeceğiz. Tabela yazısız olsun; esnaf kimliğini oyunda ekleyeceğiz. Köy evlerinin ilk varyasyonları bu gövdeden yerel olarak hazırlanabilir; üç yeni ev modeli şu an gerekmiyor.

### Kaya

`One rounded irregular blue-grey granite boulder, broad low-poly facets, sparse moss on top, flat bottom, 1.6 meters wide and 1 meter tall. Solid closed geometry.`

### Çalı

`One compact olive-green bush made of three rounded leaf masses, broad readable leaves, small hidden woody base. Width 0.9 meters and height 0.7 meters. No berries, no flowers, no individual thin leaf cards.`

### Çam

`One stylized pine tree, 5 meters tall, exposed brown trunk and chunky tiered dark green needle foliage. Solid broad foliage masses, no individual needles. Trunk and canopy as separate meshes if possible.`

Kesim için gövde/taç ayrımı tercih edilir; yakınlaşınca yaprakları gizlemeyeceğiz. Kütük modelimiz mevcut, yeniden üretme.

### Köprü

`One straight flat wooden footbridge, 4 meters long and 2 meters wide, horizontal plank deck, simple post-and-rail guards on both sides, four short legs underneath. No stairs, arch, terrain or river.`

Yürüyüş hattı düz ve boş olmalı; giriş/çıkışta enine korkuluk bulunmamalı. Daha geniş dere kesitinde bu modül uç uca uzatılabilir. Yürüme collider'ını ben ekleyeceğim.

### Kuyu

`One rustic circular stone well, diameter 1.5 meters, two wooden posts and a crossbeam with spindle, one wooden bucket suspended above the opening. Total height 2 meters. No roof or ground.`

Şimdilik meydan dekoru; su doldurma mekaniği bu modele bağlanmış değil. Kovanın ayrı mesh olması ilerisi için yararlı, animasyon gerekmiyor.

### Kayalık

`One modular cliff section, 4 meters wide, 2 meters deep and 2 meters high. Broad warm-grey rock faces, closed back and bottom, mostly flat top with thin muted grassy cap. No caves or plants.`

### Kemer

`One weathered freestanding stone arch, 3 meters wide and 3.3 meters high, clear opening 1.6 meters wide. Chunky blue-grey blocks, sparse moss. No attached walls, door, stairs or terrain.`

## Teslim kontrolü

- Dokular gömülü GLB tercih edilir; kaynak yüksek çözünürlüklü dosyayı da koru. Rig/Mixamo gerekmez.
- Beyaz arka plan, gölge düzlemi veya görsel çerçevesi mesh'e dönüşmemeli.
- Sap/ince çubuk yerine burada önemli nokta: köprü ve kemer geçişi açık; ağaç gövdesi bağlı; bina altında delik/ters yüzey yok.
- Dekor harici mesh yok; materyal sayısını mümkünse 1–2 tut. Taş/ahşap plastik veya ayna gibi parlamasın.
- Ölçü/pivot hassasiyetini teslimde ben doğrularım. NPC, yüzme, maden veya dükkân içi sistemleri bu modellerle otomatik gelmez.

## Sonraya bırakılan görsel tamamlayıcılar

Saz kümesi, söğüt ağacı, meyve ağacı, kırık taş duvar ve köy ev varyantları ikinci pakettir. Bunlar ana konsepti birebir zenginleştirir fakat ilk oynanabilir harita kesiti için sekiz modelin üzerine hemen üretim yükü getirmiyoruz. Çim/çiçek serpiştirme, zemin, yol, su ve şelale efektlerini Unity tarafında hazırlayacağız. Mevcut meşe, kütük, pazar, keşif sandığı ve yabani bitki yeniden kullanılacak.
