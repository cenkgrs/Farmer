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

## 7 Ekim — kullanıcı tarafından seçilen görsel ara aşama

0.2 inşa sisteminden önce oyuncu, sulama kabı, orak ve pazar tezgâhı üretilecek. Kullanıcı Tripo modellerini ve Mixamo Idle/Walk animasyonlarını teslim edecek. [Dört ayrı eskiz, model promptları ve teslim listesi](references/model_sketches_v01/README.md) hazır. Bu sıra yukarıdaki ilk ağaç/turp örnek seti önerisinin önüne geçer. Karakterin rig/animasyon entegrasyonu bu küçük ara aşamaya dahildir; modeller gelmeden uyumluluk doğrulanmış sayılmaz.

## Teslim edilen ilk set — entegrasyon

7 Ekim: dört GLB ve iki Mixamo FBX sahneye bağlandı. [Kaynak, ölçü, dönüşüm ve teknik notlar](../ArtSource/tripo_v01/README.md). Karakter şapka dahil 1,85 m, sulama kabı 0,42 m, orak 0,50 m, pazar 2,40 m yüksekliğinde. Alet sapları import edilmiş geometri üzerinden hizalandı; Idle/Walk aynı Humanoid Avatar'ı kullanıyor. Kaynak GLB/FBX'ler korunur. Model seti ilk aşamada normal Git'te; LFS iki makine hazırlanmadan etkinleştirilmedi.


## İlk alet hareketleri

Sulama ve hasat mevcut Humanoid iskelet üzerinde `FarmerAnimator` ile prosedürel olarak hazırlanır; ek Mixamo FBX veya ücretli model üretimi kullanılmadı. Sağ el IK hedefi kolu, avatardan örneklenen parmak kemik dönüşleri kavramayı yönetir. Elin avuç eksenleri kemiklerden hesaplanır; sulama kabında aşağı, orakta yana dönük kavrama kullanılır. Tam HumanPose'u her kare yeniden uygulamak IK sonucunu bozduğu için yalnızca başlangıç parmak örneğinde kullanılır.

Sulama kabının eldeki kopyası 180° çevrilir; kaynak model/prefab değiştirilmez. Böylece ağız karakterin işlem yönüne bakar. Su çıkışı bu modelin ölçülen ağız konumuna göredir; kabı değiştirirken `FarmerAnimator` içindeki ağız noktası ve `FarmPresentation` içindeki yerel yön de doğrulanmalıdır. `WateringStream` mevcut sahnede kullanılan URP Particles/Unlit shader'ıyla yedi ince akış oluşturur; fizik veya tarla işlemi yürütmez.

Bu küçük prosedürel set tam gövdeli, sanatçı tarafından hazırlanmış sulama/hasat klibi değildir. İleride özel klip eklenirse kol IK/parmak katmanıyla çakışması kontrol edilmeli.

## Envanter görselleri — 7 Ekim 2026

Yedi ikon ve ahşap tepsi yerleşik ImageGen ile ayrı ayrı üretildi. Konseptteki alt envanter stil referansı olarak verildi. PNG dosyaları `Assets/_Farmer/Resources/InventoryArt` içinde; [üretim promptları ve ölçüler](art-sources/inventory-prompts.json), [entegrasyon notu](art-sources/README.md). Bu 2D arayüz varlıkları eldeki 3D alet modellerini değiştirmez.

## İlk oda yapı seti — 8 Ekim 2026

`wood_floor` 1×1 m ve 3 cm kalınlık; `wood_wall` / `wood_door` bir hücre kenarında 1 m genişlik ve 2,4 m yükseklik. Mevcut Wood/WoodTrim renkleriyle kontrollü tahta geometrisi; nihai boyanmış yapı seti değildir. Prefab kökü hücre merkezinin 0,5 m üstündedir; duvar/kapı yerel +Z kenarına bakar. Kapı `Hinge` alt nesnesiyle 90° açılır. AI ile yeni yapı modeli hazırlanırsa hücre/kenar ölçüsü, menteşe ve collider açıklığı korunmalı. `HouseVisibility` özgün malzemeleri değiştirmeden saydam kopyalar kullanır.

## Çatı ve inşa ikonları — 8 Ekim devamı

Ahşap çatı 1×1 m düz paneldir; 2,4 m ince duvar veya 3 m blok duvar üstüne oturur. Yürünebilir üst kat değildir. Altı yapı ikonu ve sekiz gözlü tepsi yerleşik ImageGen ile üretildi: `Resources/ConstructionArt`; promptlar `docs/art-sources/construction-prompts.json`. Kaynak alfa korunur, Sprite sınırları şeffaf kenarları dışarıda bırakır.

## İlk kaynak geometrileri

8 Ekim keşif prototipi: ağaç 3 m civarı, dar gövde collider'ı; sandık yaklaşık 0,95×0,76×0,7 m, arkadan menteşeli kapak; yabani bitki 0,65 m. `ResourceView` geçici geometrileri; nihai Tripo modelleri olarak değerlendirilmez. Elde balta `FarmPresentation` altında kısa ahşap sap/metal baştır; Humanoid prosedürel kesme hareketini kullanır. Balta ikonu yerleşik ImageGen, `docs/art-sources/axe-prompt.json`. Nihai modellerde boyut, pivot, menteşe ve collider ayrımı korunmalı.


## Keşif kaynakları referansları — 8 Ekim 2026

Meşe, kütük, balta, keşif sandığı ve yabani tohum bitkisi için [eskizler, Tripo promptları ve teslim ölçüleri](references/exploration_sketches_v01/README.md) hazır. Önce meşe üretilecek. Bunlar 2D referanslardır; kullanıcıdan gelecek 3D dosyalar doğrulanmadan oyuna hazır sayılmaz.


## Balta/kütük teslimi — 8 Ekim 2026

Kullanıcının balta ve kesilmiş kütük modelleri oyuna entegre edildi. [Kaynak/dönüşüm kaydı](../ArtSource/exploration_v01/README.md): 0,70 m balta, 0,30 m kütük; 1K URP dokular. Balta sağ el kavramasına, kütük ağacın kesilmiş durumuna bağlıdır. Ağaç, sandık ve yabani bitki modelleri bekleniyor.


## 9 Ekim — orman seti ve balta kavraması

[Ağaç, keşif sandığı ve yabani bitki teslim/dönüşüm kaydı](../ArtSource/forest_v01/README.md). Ağaç alt gövde ve üst bölüm olarak ayrıldı; sandık kapağı ve iç yüzeyleri yerel olarak hazırlandı. Bitkinin metalik görünümü kaldırıldı. Kaynak GLB'ler ve 4K dokular korunur; oyunda 1K dokular kullanılır.

Balta tutuşunda yalnızca socket yakınlığı doğal el pozu için yeterli değildi: kullanıcı ters bilek ve açık avuç görünümünü reddetti. `FarmerAnimator` artık balta için Humanoid IK hedefi ile ithal bilek kemiğinin eksen farkını kalibre eder; ayrı parmak kavraması, sapın avuç içi ofseti ve yanda taşıma/vuruş/toparlanma pozu kullanır. Bu düzeltme sulama kabı/orak/çapanın mevcut bilek yönünü değiştirmez. Yakın çekim ve ön kol–bilek açı kontrolü birlikte değerlendirilir.

9 Ekim kullanıcı düzeltmesi: Trunk/Canopy ayrımı dosyalarda korunur fakat yakınlıkla taç gizleme kaldırıldı. Kesilmemiş ağaç her mesafede bütün görünür; kesilince iki bölüm de kapanır ve kütük görünür.

9 Ekim satın alınabilir mobilya seti: [dört ayrı eskiz, ölçü ve kısa model promptları](references/furniture_sketches_v01/README.md). İlk teslim önceliği ev sandığı; mevcut yatak korunur. Görseller 2D referanstır, 3D import henüz yapılmadı.

9 Ekim dört mobilya GLB teslimi: [kaynak/dönüşüm kaydı](../ArtSource/furniture_v01/README.md). Özgün GLB ve 4K doku kaynakları korundu, Unity çalışma dokuları 1K. Masa iki hücreye uygun döndürüldü/ölçeklendi. Yeni dört inşa ikonu yerleşik ImageGen ile şeffaf üretildi; [promptlar](art-sources/furniture-icon-prompts.json). Sandık bu sürümde tek kapalı mesh; lamba gerçek Unity ışığı kullanır.


9 Ekim çapa/orak: baltanın kalibre bilek/parmak kavraması iki alete de uygulandı, yakın çekimler incelendi. [Yeni çapa eskizi ve teslim ölçütleri](references/hoe_v01/README.md); GLB bekleniyor.

9 Ekim çapa teslimi entegre edildi: [kaynak ve dönüşüm raporu](../ArtSource/hoe_v01/README.md). Mevcut avuç kavramasını kullanır; geçici geometri kaldırıldı.

9 Ekim büyük vadi/köy: [harita konsepti ve sekiz ayrı model eskizi](references/valley_v01/README.md). Dükkân, kaya, çalı, çam, köprü, kuyu, kayalık, kemer; ölçü/üçgen hedefleri ve kısa üretim promptları aynı rehberde. Yerleşik ImageGen kullanıldı; 3D modeller kullanıcı tarafından üretilecek. Mevcut varlıklar yeniden kullanılır.

9 Ekim vadi GLB teslimi: [sekiz modelin kaynak/dönüşüm raporu](../ArtSource/valley_v01/README.md). Çalı alternatif v02 referansı da saklandı. Çam ve çalı sadeleştirmede siluet bozulduğu için kaynak geometriyi kullanır. Köprü prefabı var, nehir geçişi henüz yok.


10 Ekim 0.3 madencilik: [kazma ve kırılabilir kaya eskizleri, ölçüler ve model promptları](references/mining_v01/README.md). Kazma 0,80 m, kaya 1,2×0,8×0,9 m; GLB/FBX+dokular. Kaynak dosyası geldikten sonra ölçü, doku, taban ve tutuş kontrol edilir. Yeni ağaç modeli gerekmiyor; mevcut meşe/kütük kullanılır. Yeni kazma/taş envanter ikonları yerleşik ImageGen ile şeffaf üretildi; promptlar `art-sources/mining-icons.json` içinde.

10 Ekim kazma/kaya teslimi entegre edildi: [kaynak, hash, ölçüler ve dönüşüm](../ArtSource/mining_v01/README.md). Yeni motor/import paketi kurulmadı; native mesh/prefab kullanılıyor. Havuç/domates geçici dört aşamalı geometri, sebze kasası geçici ikon modeli; `CropExpansionSetup.Apply` bu assetleri üretir, sahneyi değiştirmez. Nihai bitki GLB'leri teslim edilmedi.


[Pazar arayüz eskizi v01](references/market_ui_v01/README.md): yerleşik ImageGen
ile mevcut oyun ve onaylı çiftlik konsepti referans alınarak üretildi. Prompt
aynı klasörde. Kullanıcı 11 Ekimde onayladı; [çalışma dokuları, canlı metin fontları ve aktarım kaydı](art-sources/market_ui_v01/README.md) hazır.


11 Ekim bahçe parçaları: `GardenSetup.Apply` native Unity prefab/veri üretir,
sahneye yazmaz. Çit/kapı 1 m kenar modülü, yaklaşık 1 m yükseklik; kök hücre
merkezinde, kenar local Z=+0,5, zemin local Y=−0,5. Kapı `Hinge` çocuğu
local (−0,40, −0,43, +0,50), açılma Y=−90°. Taş yol 0,96×0,96 m, dokuz
pahlı taş; taş mesh'i 24 üçgen (modül 216 üçgen), yüzey yaklaşık 5 cm.
Görsel geometri ve bağımsız kutu collider'ları ayrı tutulur. Malzemeler
sıcak mat ahşap/koyu metal/üç doğal taş rengi. İkonlar bu prefabların 256 px
şeffaf renderlarıdır. Bunlar harici GLB teslimi değildir; ileride GLB ile
birebir değiştirilirken bu ölçü, kök ve menteşe sözleşmesi korunmalıdır.
