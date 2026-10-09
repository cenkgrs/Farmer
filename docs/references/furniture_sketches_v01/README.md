# Satın alınabilir mobilyalar — model referansları

9 Ekim 2026. Yerleşik ImageGen ile üretilmiş dört ayrı 2D referans; 3D model veya oyuna eklenmiş asset değildir. Görseller gözle incelendi. [Üretim promptları](prompts.json) kayıtlıdır.

Her resmi ayrı Tripo üretiminde kullan. İlk öncelik **depolama sandığı**. Keşif sandığından ayırt etmek için düz kapaklı ev sandığı tasarlandı. Ardından masa, sandalye ve ayaklı lamba. Yatak için mevcut model korunuyor; yeniden üretmek gerekmiyor.

| Model | İndirilecek referans | Yaklaşık ölçü (genişlik × derinlik × yükseklik) | Üçgen hedefi |
| --- | --- | --- | --- |
| Ev sandığı | [storage_chest_v01.png](storage_chest_v01.png) | 0,90 × 0,55 × 0,55 m | 2–4 bin |
| Masa | [wood_table_v01.png](wood_table_v01.png) | 1,60 × 0,80 × 0,78 m | 1–2 bin |
| Sandalye | [wood_chair_v01.png](wood_chair_v01.png) | 0,45 × 0,45 × 0,90 m; oturak 0,45 m | 1–2 bin |
| Ayaklı lamba | [floor_lantern_v01.png](floor_lantern_v01.png) | taban 0,38 × 0,38 m; toplam boy 1,65 m | 2–3 bin |

Ölçü ve üçgen sayıları üretim hedefidir; görseller ölçülü teknik çizim değildir ve araç sonucu garanti edilmez. 1K dokulu GLB yeterli başlangıçtır; daha yüksek çözünürlükte kaynak varsa koru. Ortak stil: sıcak bal rengi ahşap, yumuşak köşeler, sade kalın parçalar, boyanmış hissi veren dokular. Arka plan ve yere düşen gölge modele dönüşmemeli.

## Görselle birlikte kullanılacak kısa promptlar

### Sandık

```text
One stylized farmhouse storage chest matching this reference. Warm honey oak planks, flat CLOSED hinged lid, restrained dark iron bands, simple brass latch. 0.90m wide, 0.55m deep, 0.55m high. If supported, separate Body and Lid meshes, rear hinge, hollow interior with thickness and a finished lid underside. No padlock, treasure or other contents. Base-center pivot, front latch facing +Z, Y-up. Hand-painted textures, roughly 2000–4000 triangles. No ground or background geometry. Textured GLB.
```

Kapak/gövde ayrımını araç yapamıyorsa tek mesh çıktıyı gönder; ayrılabilirliğini burada kontrol edeceğiz. Kapalı modelin içinin dolu olup olmadığını ayrıca inceleyeceğiz. Açık/kapalı iki ayrı üretim istemiyoruz; tek tutarlı model yeterli.

### Masa

```text
One simple stylized farmhouse table matching this reference. Honey oak plank top, four sturdy wooden legs and a simple apron, softly beveled edges. 1.60m wide, 0.80m deep, 0.78m high. Bare tabletop, no props, chairs or cloth. Preserve open space beneath the table and distinct legs. Warm hand-painted textures, roughly 1000–2000 triangles, base-center pivot, Y-up. No ground or background geometry. Textured GLB.
```

### Sandalye

```text
One sturdy stylized farmhouse chair matching this reference. Honey oak plank seat, four legs, two broad horizontal backrest slats. About 0.45m wide and deep, 0.90m tall, seat at 0.45m. Preserve open gaps between legs and backrest slats. No cushion, table, decorations or ground. Soft bevels and warm hand-painted texture. Roughly 1000–2000 triangles, base-center pivot, front facing +Z, Y-up. Textured GLB.
```

### Ayaklı lamba

```text
One freestanding stylized farmhouse lantern stand matching this reference: sturdy honey-oak post on a broad square wooden base, short top arm, thick rigid hook, simple dark bronze lantern frame with solid warm amber panels. Height 1.65m, base about 0.38m square. Opaque panels, no transparent glass, no modeled flame, no chains, no light rays or external glow geometry. If supported give the amber panels their own material named LampGlow; actual light will be added in Unity. Warm hand-painted texture, roughly 2000–3000 triangles, base-center pivot, Y-up. No ground or background. Textured GLB.
```

Lambanın sarı yüzeyleri ayrı malzeme olursa gece ışığını açıp kapatmak kolaylaşır. Üretimde bunu yapamıyorsan modeli yine gönder. Gerçek aydınlatma, ışık menzili ve gölgeler Unity içinde ayarlanacak; görseldeki parlamayı ayrı geometri olarak üretme.

## Teslim kontrolü

- GLB ve varsa ayrı dokuları/orijinal üretimi sakla. Mixamo veya karakter rig'i gerekmiyor.
- Önizlemede alt/arka yüzleri de kontrol et: eksik ayak, kapalı boşluk, havada kopuk parça veya arka plan düzlemi olmamalı.
- Tek model = tek dosya. Ölçek/pivot/parça ayrımı araçta tam çıkmazsa burada düzeltilmeye uygunluğunu inceleyeceğiz.
- Teslimden sonra mesh, doku, ölçü, pivot, çarpışma ve izometrik oyun görünümü doğrulanmadan model hazır sayılmaz.

## Oynanış kararı

Sandık, masa, sandalye ve lamba dükkândan satın alınacak. Yatak mevcut satın alma sistemiyle devam edecek. Satın alınan mobilya çantaya girer, yerleştirince çantadan düşer, sökünce mobilya olarak geri gelir. Dolu sandık taşınırken içeriği korunmalı; sökme/toplama eşya kaybı veya çoğaltma yaratmamalı. Önce çanta ve kişisel depolama sandığı, ardından diğer mobilyalar uygulanacak. Fiyatlar henüz kesinleşmedi. Bu eskiz tesliminde oyun kodu değişmedi.
