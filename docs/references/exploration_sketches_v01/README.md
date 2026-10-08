# Keşif modelleri — eskiz ve Tripo teslim rehberi

8 Ekim 2026. Bunlar ImageGen ile üretilmiş 2D model referanslarıdır; oyundaki geçici geometrilerin yerine gelecek 3D modeller henüz üretilmedi. Her görseli ayrı bir üretimde kullan; beş farklı nesneyi aynı işin çoklu açıları olarak yükleme.

Önce **ağaç** üretip gönder. İzometrik kamerada yoğunluğunu, karakter ölçeğini ve yaprak örtmesini doğruladıktan sonra diğerlerini alabiliriz. İlk aşamada tek ağaç türü yeterli; yeni tür zorunluluğu yok.

| Model | Referans | Yaklaşık oyun ölçüsü | Önerilen üçgen hedefi |
| --- | --- | --- | --- |
| Meşe | [tree_v01.png](tree_v01.png) | 3,2 m boy, 2,5 m taç genişliği | 5–8 bin |
| Kesilmiş kütük | [stump_v01.png](stump_v01.png) | 0,3 m boy, 0,55 m taban | 1 bin |
| Balta | [axe_v01.png](axe_v01.png) | 0,7 m uzunluk | 1,5–2 bin |
| Keşif sandığı | [chest_v01.png](chest_v01.png) | 0,95 × 0,7 × 0,75 m | 3–4 bin |
| Yabani tohum bitkisi | [plant_v01.png](plant_v01.png) | 0,6 m boy, 0,5 m yayılım | 1–1,5 bin |

Ölçüler ve bütçeler üretim hedefidir, araç garantisi değildir. Görseller ölçülü teknik çizim değildir. 1K dokularla başlanabilir; ağaç için gerekirse 2K. Her yaprağı ayrı yüksek poligonlu geometriye dönüştürmek yerine okunaklı yaprak kümeleri kullan. Sandık referansında kalan kahverengi hale modelin parçası değildir; arka plan/geometri olarak üretilmemeli.

## Kopyalanabilir promptlar

### Meşe ağacı

```text
Create one game-ready stylized oak tree matching the reference silhouette and warm hand-painted farming-game style. Approximate height 3.2m, canopy width 2.5m. Warm brown branching trunk, irregular layered green foliage clusters. Keep a visible trunk and readable asymmetry. Use simplified foliage masses with painted leaf detail, not thousands of individually modeled leaves. If supported, keep trunk/branches and foliage as separately selectable meshes named Trunk and Canopy. Base-center pivot. No ground disk, grass, rocks, fruit, scenery or background. No baked external shadow. Target 5000–8000 triangles with textures. Export GLB.
```

### Kesilmiş kütük

```text
Create one stylized chopped oak stump matching the reference and the oak tree's warm brown bark. About 0.3m tall, base width 0.55m. Softly faceted bark, subtle flared roots, golden cut surface with painted growth rings. Base-center pivot. No axe, mushrooms, terrain, grass or other objects. Target about 1000 triangles, textured GLB.
```

### Balta

```text
Create one practical woodcutting axe matching the reference. Cozy stylized hand-painted game prop, softly beveled gray steel head and curved honey-brown wooden handle. Total length about 0.7m. Clear single cutting edge, restrained detail, no fantasy ornament. No hand, character, pedestal or background. Target 1500–2000 triangles, textured GLB. Keep handle thick enough for readable hand contact.
```

### Keşif sandığı

```text
Create one closed rustic wooden chest matching the reference object only. Ignore the brown background halo completely. Warm timber planks, dark iron straps, small brass clasp, softly beveled hand-painted game style. Dimensions approximately 0.95m wide, 0.7m deep, 0.75m tall. If supported, make Body and Lid separate meshes, with the lid able to rotate around a rear hinge; model the inside surfaces for opening. No padlock, coins, glow, ground or scenery. Target 3000–4000 triangles, textured GLB.
```

### Yabani tohum bitkisi

```text
Create one small stylized wild seed plant matching the reference: a cluster of broad green leaves and three sturdy stems with golden tan seed pods. Warm hand-painted farming-game style, chunky readable silhouette. About 0.6m tall and 0.5m wide. Simplify tiny detail for an isometric camera. Base-center pivot. No pot, dirt platform, grass patch, exposed roots, scenery or background. Target 1000–1500 triangles, textured GLB.
```

## Teslim ve kontrol

- GLB dosyasını gönder; varsa orijinal proje ve ayrı dokuları da sakla. FBX + dokular da kabul edilebilir. Bu nesneler için Mixamo/karakter rig'i gerekmiyor.
- Ağacın gövde/yaprak ve sandığın gövde/kapak ayrımı araçta mümkünse uygula. Tek mesh çıkarsa olduğu gibi gönder; ayrılabilirliği incelemeden açılma/gizleme için hazır kabul etmeyeceğiz.
- Önizlemede alt/arka tarafları kontrol et: delik, havada kopuk parça, erimiş sap, doldurulmuş sandık içi ve yanlış arka plan geometrisi olmamalı.
- Model gelince ölçek, pivot, mesh/parça ayrımı, materyal/doku, üçgen sayısı, collider ve gerçek oyun kamerasındaki görünüm doğrulanacak. Ağaç kesilme ve taç gizleme; sandık kapak açılması ayrıca kontrol edilecek.
- Bitki mevcut tohum toplama kaynağının görselidir; yeni ürün veya tarif eklemez.

ImageGen üretim promptları: [prompts.json](prompts.json). Görseller gözle incelendi; 3D import/oyun testi henüz yapılamaz. Bu teslim yalnızca referans ve dokümantasyondur.
