# 0.3 — kazma ve kırılabilir kaya model teslimi

Yerleşik ImageGen ile 10 Ekim 2026'da hazırlanan 2D referanslardır. 3D model
sayılmazlar. Önce kazma, ardından kaya üretilebilir. Her görseli ayrı üretimde
kullan; iki nesneyi tek sahne/model olarak üretme. Yeni ağaç modeli gerekmiyor.

| Dosya | Ölçü | Geometri hedefi | Pivot |
|---|---|---|---|
| pickaxe_v01.png | toplam 0,80 m boy, 0,45 m baş genişliği; sap yaklaşık 3–4 cm kalın | 2–5 bin üçgen | sap alt ucu; kavrama sap ortasında |
| stone_outcrop_v01.png | 1,2 m genişlik, 0,8 m boy, yaklaşık 0,9 m derinlik | 1–3 bin üçgen | taban merkezi |

Stil: sıcak, stilize 3D; boyanmış yüzey, yumuşak kenarlar, belirgin siluet.
Tercih FBX + dokular; **GLB de olur**. Tek 1K albedo/PBR seti yeterli; kaynak
modeli mümkünse ayrıca koru. Bir dünya birimi bir metre. +Y yukarı; kazma sapı
Y ekseninde, başı X boyunca. Arka plan, yer diski, gölge ve yazı modele girmesin.
Rig/animasyon gerekmez. Kaya tek bağlı kütle olabilir; kırılmış varyant şart değil.

## Kazma için kopyalanacak prompt

```text
Create one game-ready stylized rustic pickaxe matching the attached reference.
Warm brown straight wooden handle with a thick dark iron head: one curved pointed
end and one short blunt wedge. Soft bevels and subtle hand-painted surfaces,
readable from an isometric farming-game camera. Total height 0.80 m, head width
0.45 m, handle diameter 0.035 m. Handle along +Y, head along X. Clear grip around
the middle of the shaft. 2,000–5,000 triangles, clean UVs, one 1024px texture set,
neutral lighting without baked cast shadows. Pivot at the bottom of the handle.
One isolated object only. No hands, straps, ground, pedestal, text or extra props.
Export GLB or FBX with textures. No rig or animation required.
```

## Kaya için kopyalanacak prompt

```text
Create one game-ready stylized mineable stone outcrop matching the attached
reference. One connected broad rock mass with three fused chunky lobes, a low
flat bottom, warm gray granite and two simple shallow fissures. Soft beveled
facets, restrained hand-painted variation, clear silhouette from an elevated
isometric camera. Dimensions 1.2 m wide, 0.8 m high, approximately 0.9 m deep.
1,000–3,000 triangles, clean UVs, one 1024px texture set, pivot at bottom center,
+Y up. Neutral lighting with no baked cast shadows. No crystals, metal ore,
plants, scattered fragments, ground disk, pedestal, text or additional objects.
Export GLB or FBX with textures. No rig or animation required.
```

## Geldiğinde kontrol edilecekler

Gerçek boyut ve üçgen sayısı; UV/doku bağlantıları; tabana oturuş; kazma sapının
avuçta kavranabilirliği ve başın vuruş yönü; kamera mesafesinde siluet;
malzemenin aşırı metalik/parlak olmaması. Üretim aracına yazılan hedefler teslimin
bunlara uyduğunu garanti etmez. Dosyalar doğrulanıp ölçeklenmeden nihai oyun
varlığı sayılmayacak.
