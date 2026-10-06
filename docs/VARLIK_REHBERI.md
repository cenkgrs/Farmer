# Görsel dil ve AI model üretim rehberi

## Sabit görsel hedef

Kullanıcının onayladığı iki konsept:

- [İzometrik — kamera ve sanat yönü](references/farm_isometric_concept.png)
- [Üçüncü şahıs — yalnızca sanat yönü](references/farm_third_person_concept.png)

Görseller konuşma içinde yerleşik ImageGen ile üretildi, özgün çıktılar kopyalanarak depoya alındı. Oyun içi görüntü veya doğrudan kullanılabilir 3D model değiller.

Stil: yumuşak/bevel verilmiş biçimler, sıcak ahşap ve krem sıva, terracotta çatı, doğal yeşiller, boyanmış hissi veren temiz yüzeyler. Küçük ölçekte okunaklı silüet ve renk farkı, çok ince detaydan önemlidir. Fotogerçekçi kir, aşırı yüzey gürültüsü ve keskin metalik görünüm hedeflenmez.

## Üretim yaklaşımı

İlk etapta bütün çiftliği tek mesh olarak üretme. Etkileşen parçalar ayrı asset olmalı: ağaç, toprak, ürün aşamaları, kapı, duvar, çatı ve mobilya. Yapının çatısı/duvarı iç mekân görünürlüğü için ayrı gizlenebilmelidir.

Karelere tam oturması gereken temel, küp, duvar ve çit gibi basit parçaları kontrollü geometriyle yapmak tercih edilir. AI sonucu kullanılırsa ölçü, pivot ve kenarlar düzeltilir. Organik ağaçlar, bitkiler, kayalar ve dekorasyonlar AI üretimi için ilk adaylardır.

Ölçü önerisi (henüz prototipte onaylanmadı): 1 dünya birimi = 1 metre; yapı hücresi 1 metre; karakter yaklaşık 1,7 metre. Duvar, kapı ve mobilya boyutları karakter geçişi denendikten sonra sabitlenir.

## Teslim biçimi

- Unity için tercih: FBX + ayrı doku dosyaları. Araç yalnızca GLB veriyorsa orijinali sakla; dönüşüm veya glTF import paketi entegrasyonda seçilir. [Unity biçim belgeleri](https://docs.unity3d.com/6000.3/Documentation/Manual/3D-formats.html)
- Asset adı: İngilizce küçük harf ve alt çizgi; örnek `tree_oak_small_v01.fbx`.
- Prop pivotu taban merkezinde; crop aşamalarının pivot/ölçek/ekim noktası aynı. Izgaraya oturan parçaların pivotu ayrıca standardize edilecek.
- Nötr aydınlatma; boyanmış dokularda güçlü yönlü ışık, yere düşen gölge veya AO'yu abartılı biçimde sabitleme.
- Tekrarlanabilir silüet, makul malzeme sayısı ve UV düzeni. İlk küçük prop için 1K dokuyla dene; kalite ihtiyacı ölçülmeden hepsini 4K üretme.
- Kullanılan araç/model, prompt, üretim tarihi, dosya sürümü ve kullanım haklarına dair kaynak notunu assetle birlikte kaydet.
- Animasyonlu karakter daha sonraki ayrı paket: rig, kemik uyumu ve animasyon listesi ayrıca tarif edilecek.

Prompttaki teknik istekler modelin gerçekten o ölçü/topolojiyle geldiğini garanti etmez. Importta geometri, boyut, malzeme, pivot ve performans kontrol edilir. Çarpışma şekilleri mümkün olduğunda ayrı basit geometrilerle kurulur.

## İlk küçük örnek seti

0.1 için tek ağaç ve turpun dört büyüme aşaması yeterli görsel denemedir. Karakter ve aletler davranış doğrulaması sırasında yer tutucu olabilir. Model listesini bu örnekler sahnede görülmeden büyütme.

### Ortak stil promptu

Her varlık promptunun başına ekle. AI aracı referans kabul ediyorsa izometrik konsepti yalnızca stil referansı olarak ver.

```text
Create one game-ready stylized 3D asset for a warm, cozy isometric farming and building game. Hand-painted visual style, soft beveled forms, readable silhouette from a fixed elevated camera, earthy colors and restrained surface detail. Match the supplied farm concept's art direction without copying the whole environment. Neutral lighting; no baked directional cast shadows. One isolated asset only, no display pedestal, no scene, no text or watermark. Prefer FBX with packaged texture files, clean UVs and a pivot at ground contact; GLB is acceptable as an interchange source if FBX export is unavailable. Keep the geometry simple enough for many repeated instances in a small farm scene.
```

### Küçük ağaç — `tree_oak_small_v01`

```text
A small stylized oak tree with a warm brown trunk and a broad rounded canopy made from a few readable clusters of green foliage. Slightly irregular natural shape, soft edges and subtle hand-painted color variation. Approximately 3 meters tall after import scaling. One tree only; no soil disk, grass patch, fruit, rocks, animals or surrounding landscape. Base-center pivot at the bottom of the trunk. The trunk should remain clearly visible from an elevated isometric camera. Separate trunk and foliage meshes if supported.
```

### Turp — her aşama ayrı dosya

Aynı bitkinin ayrı varyantları olarak üret; bir görselde dört bitkili diorama isteme. Tüm aşamalar aynı merkez ve ölçeği paylaşmalı. Bitki tek ekim hücresinde toprak yüzeyinden büyür.

| Dosya | Ortak prompta eklenecek tarif |
|---|---|
| `crop_radish_stage_01_v01` | `One tiny radish seedling, two small fresh green leaves, just emerging above the ground plane. No soil included. Approximately 8 cm tall.` |
| `crop_radish_stage_02_v01` | `One young radish plant, four slightly larger green leaves, an early compact leafy silhouette; root mostly below the ground plane. No soil included. Approximately 15 cm visible height.` |
| `crop_radish_stage_03_v01` | `One nearly mature radish plant, fuller green leaves and a small rounded red root shoulder beginning to show above the ground plane. No soil included. Approximately 22 cm visible height.` |
| `crop_radish_stage_04_v01` | `One harvest-ready radish plant, lush readable green leaves and a clearly visible plump red root shoulder, with the rest extending below the ground plane. No soil included. Approximately 25 cm visible height. Match the previous stages' palette and species.` |

Bu ölçüler ilk deneme içindir. Oyun kamerasında okunabilirlik için birlikte ayarlanabilir; dosyalar tek tek rastgele ölçeklenmemeli.

## Model geldiğinde kabul kontrolü

1. Dosya ve dokular açılıyor mu; nesne istenen tek asset mi?
2. Boyut, pivot ve dönüş doğru mu; zemine oturuyor mu?
3. Onaylı kamera mesafesinde silüet ve büyüme aşamaları okunuyor mu?
4. Geometri/malzeme sayısı ve doku boyutu sahnede çoklu kullanıma uygun mu?
5. Çarpışma/etkileşim geometrisi görsel modelden bağımsız tanımlanabiliyor mu?
6. Uygunsa asset kaydını, kaynak promptunu ve önizlemesini ekle; orijinal çıktıyı ezmeden sürümle.

## Git ve büyük dosyalar

Bu makinede Git LFS henüz kurulu değil; LFS attribute filtreleri etkin değil. Model/doku paketleri eklenmeden önce iki makinede LFS kurulumu ve remote desteği kontrol edilecek. Önerilen LFS hedefleri binary model ve büyük kaynak dosyalarıdır; metin sahneleri ve kod normal Git'te kalır. İki mevcut konsept PNG küçüktür ve normal Git'te tutulur.
