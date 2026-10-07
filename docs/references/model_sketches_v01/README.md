# İlk model seti — Tripo referansları

7 Ekim 2026. Kullanıcı 0.2 inşa sisteminden önce oyuncu, sulama kabı, orak ve pazar modellerini Tripo'da kendisi üretecek; karakterin Idle/Walk animasyonlarını Mixamo'dan teslim edecek. Bu paket yerleşik ImageGen ile üretilen **2D referanslardır**; 3D model veya doğrulanmış rig değildir. Tripo üretimi çalıştırılmadı, Tripo kredisi harcanmadı.

Her PNG tek model içerir. Tripo'ya her birini ayrı iş olarak yükle; dört görseli aynı modelin farklı açıları gibi birlikte yükleme. Eskizler görsel olarak incelendi: tam silüet, boş arka plan, karakterde önden T pozu ve ayrı aletler. Karakter elleri önden ince profilde göründüğü için üretilen mesh'te parmaklar ayrıca kontrol edilmeli.

## Dosyalar ve ilk deneme hedefleri

Ölçüler Unity importunda düzeltilecek yaklaşık hedeflerdir. Poligon sayıları ilk deneme bütçeleridir; üretimin garantisi değildir.

| Referans | Geçici boyut hedefi | Üçgen bütçesi | Doku |
| --- | --- | --- | --- |
| [Çiftçi](farmer_character_tpose_v01.png) | Taban–baş üstü yaklaşık 1,7 m; şapka ek yükseklik | 15.000 | 2K |
| [Sulama kabı](watering_can_v01.png) | Sap dahil yaklaşık 0,4 m yükseklik; uç dahil 0,55 m genişlik | 3.000 | 1K |
| [Orak](sickle_v01.png) | Yaklaşık 0,5 m toplam uzunluk | 2.000 | 1K |
| [Pazar](market_stall_v01.png) | Yaklaşık 2,4 m genişlik × 1,3 m derinlik × 2,4 m yükseklik | 12.000 | 2K |

Tüm modeller: FBX ve doku dosyaları tercih edilir; orijinal GLB varsa onu da koru. Karakter ve aletler ayrı dosyalar olmalı. Tripo'da karakteri bu aşamada iskeletsiz üret; iskelet/animasyon Mixamo'da eklenecek. Tezgahtaki havuçlar görsel dekorasyon önerisidir; oyuna yeni ürün veya satış mekaniği eklenmiş değildir.

## Tripo için kısa yönlendirmeler

Arayüz görselin yanında metin kabul ediyorsa kullan. Ana referans ilgili PNG'dir.

### Çiftçi
```text
Create one stylized game character matching the supplied image. Preserve the teal shirt, ochre overalls, straw hat and brown boots. Full-body symmetric T-pose, straight horizontal arms, empty open hands, separate fingers and thumbs, legs apart, clear gaps around limbs. Human biped proportions, clean joints suitable for later Mixamo auto-rigging. No skeleton or animations yet, no tools, backpack, ground plane or pedestal. Neutral hand-painted textures without baked directional shadows. Target 15000 triangles and 2K textures. Deliver FBX and textures; preserve the original source mesh.
```

### Sulama kabı
```text
Create one stylized watering can matching the supplied image. Preserve the blue-gray reservoir, open fill hole, overhead handle with wooden grip, connected spout and round sprinkler rose. Keep the handle opening clear and every part physically connected. No water, hands, ground plane or pedestal. Target 3000 triangles and 1K textures. Deliver FBX and textures.
```

### Orak
```text
Create one small handheld harvesting sickle matching the supplied image. Short warm wooden handle, metal ferrule, broad curved blue-gray blade, readable concave cutting edge. A sickle, not a long scythe. Give the blade believable thickness. No hands, crops, ground plane or pedestal. Target 2000 triangles and 1K textures. Deliver FBX and textures.
```

### Pazar
```text
Create one stylized freestanding farm market stall matching the supplied image. Warm wooden counter, four posts, cream and sage striped canopy, two shallow produce trays on the counter. Preserve open service space under the canopy. No vendor, building, text, extra ground objects or terrain base. Keep major wooden beams straight and coherent. Target 12000 triangles and 2K textures. Deliver FBX and textures.
```

## Mixamo teslimi

Önce karakteri yükleyip otomatik iskeleti oluştur. Dirsek, omuz, diz, el ve şapkanın animasyonda bozulup bozulmadığını izle. Mixamo özel karakter için FBX/OBJ/ZIP kabul eder; FBX içindeki dokular için Embed Media önerilir. [Adobe yükleme/rig rehberi](https://helpx.adobe.com/creative-cloud/help/mixamo-rigging-animation.html)

Aşağıdakiler bu proje için önerilen export ayarlarıdır; arayüzde mevcut karşılıklarını kullan:

1. **Ana karakter:** Mixamo'dan iskeletli T-Pose, FBX, **With Skin** → `farmer_rigged_tpose.fbx`.
2. **Idle:** aynı karakter üzerinde sade, aletsiz bekleme; FBX, **Without Skin**, 30 FPS, Keyframe Reduction: None → `farmer_idle.fbx`.
3. **Walk:** aynı karakter üzerinde normal ileri yürüme; varsa **In Place** açık. FBX, **Without Skin**, 30 FPS, Keyframe Reduction: None → `farmer_walk.fbx`.
4. Tripo'dan gelen karakter dokuları ve kaynak model; sulama kabı, orak ve pazar FBX/doku paketleri.

Yalnızca iki animasyon dosyası yeterli değildir; Mixamo'dan çıkan iskeletli ana model de gerekir. Walk yerinde yürüsün: dünyadaki hareketi mevcut oyuncu kodu yönetir. T-Pose indirmesi bulunamazsa Idle'ı With Skin indirip ayrıca belirt; model/iskelet o dosyadan alınabilir.

Animasyonların aynı iskeletle gelmesi gerekir. Unity'de Humanoid Avatar, Idle/Walk geçişleri ve eldeki alet bağlantıları kontrol edilecek. Sulama/hasat özel animasyonları bu teslimin kapsamı değildir.

## Entegrasyon öncesi kontrol

- Boyut, yön, pivot, UV ve URP malzemeleri; gerçek üçgen/doku boyutları.
- Karakterde birleşik parmak/kol/bacak, delik, şapka deformasyonu ve animasyon sırasında zemin kayması.
- Aletlerde tutuş noktası ve kamerada okunabilir boyut; pazarın çarpışması ve etkileşim alanı.
- Normal oyuncu kaydının korunması; önceki açık üç otomatik kontrol ayrıca çözülecek.
- Binary modeller eklenmeden önce Git LFS gereksinimi ve iki bilgisayardaki kurulum tekrar kontrol edilecek. Bu küçük 2D eskizler normal Git'te tutulur.

Tam ImageGen promptları: [generation_prompts.json](generation_prompts.json). Model üretiminde kullanılan Tripo sürümü/ayarları ve kullanım hakkı bilgilerini teslim sırasında ayrıca kaydet.
