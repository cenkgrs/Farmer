# Unity içinde haritayı düzenleme

Bu turdan sonra çevre yerleşimini kullanıcı yapacak. Şu anki geniş ve seyrek sahne konseptin tamamlanmış karşılığı değildir. Önce küçük bir köy çevresini bitirip yoğunluğu orada oturtmak, bütün araziyi birden doldurmaktan daha kolaydır.

## Sahneyi aç

1. Unity Hub'dan Farmer projesini **6000.3.25f1** ile aç.
2. Project panelinde `Assets/_Farmer/Scenes/Farm.unity` dosyasını çift tıkla.
3. **Play kapalıyken** çalış. Play sırasında yapılan sahne düzenlemeleri çıkınca geri alınır.
4. Üst menü **Farmer > Harita > Köy meydanına odaklan** seni köye götürür. **Model paletini seç** kullanılabilir sekiz prefabın klasörünü seçer.
5. Hierarchy'de `Valley First Slice` grubunu genişlet. Buradaki evler, kuyu, kayalar, çalılar ve çamlar sahnede düzenlenebilir.

**ValleyArtSetup.ApplyAndBuild veya ilk sahne kurulum araçlarını yeniden çalıştırma:** otomatik oluşturulan yerleşim senin düzenlemelerini siler. Normal derleme `ProjectSetup.BuildLinux` ile yapılır; sahneyi yeniden üretmez. `MapEditingTools.ApplyPavingAndBuild` tek seferlik zemin geçişidir, günlük harita aracı değildir.

## Bir modeli yerleştir

Project'teki `Assets/_Farmer/Resources/ValleyArt` klasöründen bir **prefabı Scene görünümüne** sürükle. FBX kaynak mesh yerine prefab kullan; malzeme ve mevcut çarpışma ayarları hazır gelir.

| Prefab | Kullanım |
|---|---|
| `village_shop` | Köy evi/dükkân dış cephesi; henüz girilebilir iç mekân veya satıcı değil |
| `village_well` | Meydan kuyusu |
| `pine_tree` | Dekor çamı; kesilebilir kaynak değil |
| `meadow_bush` | Dekor çalı; çarpışmasız |
| `granite_boulder` | Büyük kaya |
| `cliff_module` | Sınır/manzara kayalığı |
| `ruin_arch` | Keşif alanı kemeri |
| `wood_bridge` | Köprü görseli; çok yükseklikli yürüyüş henüz desteklenmiyor |

Nesneyi Hierarchy'de `Valley First Slice` altına taşı; istersen `Village`, `Forest`, `Rocks` adlı boş alt gruplar oluştur. Grupların Scale değeri (1,1,1) kalsın.

- **W:** taşı, **E:** döndür, **R:** ölçek. Scene araç çubuğunda **Global** ve **Pivot** seçmek ilk yerleşimde daha kolaydır.
- **F:** seçili nesneye odaklan. **Alt + sol sürükleme:** çevresinde dön; orta fare: gezin; tekerlek: yakınlaş. Scene kamera açısını değiştirmek oyunun Main Camera açısını değiştirmez.
- **Ctrl+D:** seçili nesneyi çoğalt. Inspector'da Transform değerlerini de doğrudan yazabilirsin.
- Tabanı yerde olan prefabların **Position Y=0** olsun. Eşit ölçek için X/Y/Z'ye aynı değer ver; örneğin .85 veya 1.1. Yalnız bir ekseni büyütmek ağaçları/evleri yamultur.
- Evleri çoğunlukla Y rotation 0/90/180/270 ile yerleştir. Ağaç/çalı/kayalarda Y dönüşünü değiştirip .8–1.2 ölçek aralığıyla tekrar hissini azalt.
- **Ctrl+S:** Farm sahnesini kaydet. Sonra Play'e bas, **Devam Et**, yürüyerek mesafeleri ve görüşü kontrol et. Play'den çıkınca düzenlemeye devam et.

## Mevcut pazar ve taş yol

İşlevli tezgâh sahnedeki mevcut **Market** nesnesidir. Onu taşımak istiyorsan üst nesnesini taşı; silip sıradan bir tezgâh modeli koyma. `FarmGame` üzerindeki Market referansı o nesneye bağlıdır. Dekor evleri çoğaltmak yeni satıcı yaratmaz.

`Valley path` nesneleri yol dilimleridir. Köy meydanı yaklaşık **X=-55, Z=96**, mevcut pazar **X=-60, Z=88** konumundadır. Köy girişindeki taş dilimi veya meydanı Ctrl+D ile çoğaltıp X/Z boyutu ve Y dönüşüyle düzenleyebilirsin. İncecik zemin diliminin Y konumu/kalınlığını koru; zeminin içine girerse görüntü titreşir. `PavingTiling` bileşeni taş boyutunu dilim büyüklüğüne göre sabit tutar. İstersen Material alanına `Assets/_Farmer/Art/Materials/village_paving.mat` sürükle; kopyalanan taş diliminde bu zaten vardır.

## İlk tasarım denemesi: yalnızca meydan çevresi

1. Yaklaşık **25×25 m** bir alanı ele al. Kuyuyu merkezden hafif kaydır; üç cepheyi aynı çizgiye değil küçük farklı açılarla yerleştir.
2. Meydandan çiftliğe uzanan ana geçişi **3–4 m açık** bırak. Ev önlerine de yürünecek boşluk bırak.
3. Meydan dışını 3–5 ağaçlık kümelerle sınırla. Ağaçları eşit aralıklarla ızgara gibi dizme; aralara çalı ve 1–2 kaya koy.
4. Kamera tarafında daha alçak çalılar, arka tarafta yüksek ağaçlar kullan. Ağaç yaprakları yaklaşınca gizlenmez; karakterin görünmesini yerleşimle sağla.
5. Her birkaç kümeden sonra Play'de yürü. Güzel görünen ama geçilemeyen veya karakteri sürekli örten düzeni hemen düzelt.

Konseptteki doluluk yalnız ağaç sayısından gelmiyor: farklı zemin renkleri, küçük ot/çiçek kümeleri, kıyılar, su, yükseklik katmanları ve ışık birlikte çalışıyor. Mevcut modellerle köy/orman kompozisyonunu kurabiliriz; nehir, şelale ve arazi yüksekliklerini bu araçlarla hazırmış gibi kabul etmeyelim.

## Şimdiki teknik sınırlar

- Dünya tabanı 256×256 m; hareket merkezi yaklaşık X/Z ±127.3. Şimdilik tabanı veya hareket sınırını büyütme.
- Çapa/inşa hücreleri **Y=0 düzlemine** bağlı. Terrain ile dağ/yürünebilir yamaç boyamak henüz bu sistemlerle uyumlu değil. Kayalıklar şimdilik görsel sınırdır.
- Kullanıcının inşa ettiği ev/tarla ve kaynak sandıkları sahneye değil yerel kayıt dosyasına aittir; Edit modunda görünmeyebilir. Silme/sıfırlama yapmadan Play'de kontrol et.
- Mevcut çevre örneklerinde `ValleyScenery`, kayıtlı yapı/tarla ile çakışırsa dekoru gizler. Yeni yerleştirdiğin dekor prefabına Inspector > Add Component > **Valley Scenery** ekleyebilirsin. Eski evin ortasına büyük dekor koymamak yine en iyi çözümdür.
- Kesilebilir ağaçlar/yabani bitkiler/ödül sandıkları çalışma anında kayıtlı kaynak sistemi tarafından oluşturulur. Çam prefabı yerleştirmek otomatik olarak kesilebilir ağaç oluşturmaz.
- `Main Camera`, `FarmGame`, `WorldGround` ve oyuncu bileşenlerini dekor temizlerken silme.

## İki bilgisayara aktar

Sahneyi kaydet, Play'den çık. Git'te Farm.unity ve eklediğin assetlerle **.meta dosyalarını birlikte** commit/push et. Library/Temp/builds klasörleri paylaşılmaz. Diğer bilgisayarda Unity'yi açmadan önce pull al. Aynı sahneyi iki bilgisayarda eşzamanlı düzenlemeyelim.
