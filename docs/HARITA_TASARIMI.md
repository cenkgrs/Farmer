# Söğüt Vadisi — büyük harita taslağı v01

Durum: harita konsepti ve üretim planı. Henüz oynanabilir araziye uygulanmadı. Kullanıcının ayrı köy merkezi/esnaf kararı kesindir; isimler, ölçüler ve dükkân fiyat dengesi öneridir. Zoom bu tasarımdan ayrı olarak mevcut oyuna eklenir.

## Ölçek ve dolaşım

384×384 m ana vadi; 1 m yapı/tarla hücresi korunur. Tasarım koordinatları x/z: -192…192. Başlangıç açıklığı merkez (0,0), yaklaşık 48×48 m; oyuncuya hazır ev veya tarla verilmez. Çevrede başka uygun açıklıklara da yerleşilebilir. Harita zeminini tek dev AI modeli olarak üretmeyeceğiz; arazi/yollar/su Unity'de, tekrar kullanılan nesneler ayrı modellerden kurulacak.

Karakterin mevcut 3,8 m/sn hızıyla 100 m düz yol yaklaşık 26 sn; köye kıvrımlı yol 120–150 m, yaklaşık 32–40 sn tek yön. Bunlar koşmadan, engellere takılmadan teorik süreler; gerçek dolaşımda ölçülüp ayarlanacak. 10 dakikalık gün içinde pazara gidip dönmek ve kısa keşif yapmak mümkün olmalı. Büyük dünya için hız veya gün süresi sessizce değiştirilmeyecek.

| Bölge | Yaklaşık merkez | İşlev / görsel karakter |
|---|---|---|
| Başlangıç çayırı | (0,0) | Güneşli, boş, geniş yerleşim açıklığı; yakın su ve odun; hiçbir esnaf burada değil |
| Köy merkezi | (-55,95) | 64×56 m yerleşim; meydan, tohumcu, marangoz, 3 ev; gelecekte etkileşimli NPC alanları |
| Meşe açıklıkları | (-100,25) | Mevcut ağaç modeli, kaynak döngüsü, küçük barınak kurulabilen boşluklar |
| Eski bahçe | (-130,100) | Yıkık temel, birkaç eski ağaç; ileride başka çiftçi/hırsızlık sistemine dönüşebilir, henüz etkin değil |
| Çam korusu | (75,135) | Daha sık, serin orman; görüşü tamamen kapatmayan kümeler; gelecekte basit mob bölgesi |
| Taş kalıntılar | (135,145) | Uzakta görülen taş kemer, geniş yürüyüş zemini; keşif hedefi |
| Dere ve geçitler | x≈65–110 | Kuzeydoğudan güneye su hattı; köprü ve ikinci sığ geçit ile iki farklı dönüş rotası |
| Sazlık göleti | (110,-95) | Sakin kıyı yolu, çalılar ve ileride saz/kıyı bitkisi paketi; yüzme/balıkçılık bu tasarımın işlevsel kapsamı değil |
| Güney çayırları | (-55,-115) | İkinci yerleşim seçeneği, odun/bitki toplama ve açık manzaralar |

Koordinatlar şematik yerleşim önerisidir; ImageGen konsepti metrik veya birebir üretim planı değildir. Köy ve çiftlik arasında açık kırsal yol vardır; dükkânlar çiftliğin dibine dizilmeyecek.

## Köy meydanı

Yaklaşık 18×20 m açık meydan, ortada kuyu; en az 3 m dolaşım şeritleri. Tohumcu ve marangoz iki ayrı cephe kullanır. İlk model aynı shop prefabının renk/tabela varyasyonuyla iki esnafa yetebilir. Mevcut pazar tezgâhı meydanda ürün alış/satış noktası olarak yeniden kullanılır. Oyuncu çiftliğinde ücretsiz pazar noktası bulunmaz.

İlk uygulamada mevcut alışveriş işlevleri köye taşınır. Ayrı esnaf katalogları/çalışma saatleri ve NPC konuşma-görev sistemleri sonraki ayrı adımlardır. Meydanda kapı önü ve tezgâh arkası NPC durma noktaları, 1,5–2 m etkileşim boşluğu ve evlere ulaşan yollar ayrılır. NPC modeli şu an istenmiyor; rig/animasyon gereksinimini sistem yaklaşınca belirleyeceğiz.

## Keşif akışı

- Kısa döngü: başlangıç → meşe açıklığı → köy → dere kıyısı → başlangıç.
- Uzun döngü: köy → eski bahçe → çam korusu → taş kalıntı → kuzey geçidi → gölet → köprü → başlangıç.
- Uzak hedefler kemer, şelale ve köy çatılarıyla okunur. Patikalar 2–3 m geniş, keskin çıkmazlar yerine alternatif dönüş yolları.
- Sandık yerleri harita çiziminde sabit ödül noktaları değildir. Yeni dünya başına rastgele seçim; aynı kayıtta yer/ödül/açılma durumu korunur. Yol, su, ev, ürün ve oyuncu yapılarıyla çakışmayan aday alanlar kullanılır.
- Risk görsel yoğunlukla artar, ancak mob/savaş bu tur eklenmez. Çam korusunda da uygun zeminde ev kurma hakkı devam eder; görünmez bölge yasağı yok.

## Teknik uygulanma sırası

1. Önce 128×128 m başlangıç–köy yol kesiti: ölçülü düz zemin, köy meydanı, mevcut ağaç/pazar ve temel yeni kayalar. Hareket süresi ve kamera okunurluğu denenir.
2. Sonra 384×384 m vadiye genişleme: orman kümeleri, açıklıklar, iki rota, su/çarpışma sınırları ve köprü.
3. Ayrı paket: çam korusu, kemer, kıyı dekoru; ardından NPC ve tehlike sistemlerinin kendi oynanabilir adımları.

Mevcut inşa/çapa kodu y=0 düz zemine dayanıyor. İlk geçilebilir arazi ve köprü yürüyüş yüzeyi bu düzleme oturacak. Konseptteki yüksek kayalık/şelale manzara sınırı olarak kullanılır; çok yükseklikli yürünebilir arazi, yamaçta inşa ve köprü altında geçiş ayrıca mühendislik gerektirir. Görsel konsept bu özellikler uygulanmış demek değildir.

Eski kullanıcı yapıları ve tarlaları taşınmadan korunmalı. Gerçek harita uygulamasından önce kayıt dünya-sürümü ve çakışma politikası hazırlanmalı; yeni dekor eski yapı/tarla hücrelerini ezmemeli. Mevcut kayıtları tasarım görseline uydurmak için sıfırlamayacağız.

## Performans / görsel kabul

Tek dev mesh yok. 32–64 m yerleşim parçaları, tekrarlı prefab ve paylaşılan materyaller; yoğun bitkilerde collider yok, ağaçlarda dar gövde collider'ı. Çam/kaya gibi tekrarlanan varlıklara gerekirse LOD ve culling hazırlanır. Hedef bütçeler model rehberinde; gerçek teslim ölçülmeden hazır sayılmaz.

Sabit izometrik açı ve oyuncu merkezleme korunur. Ağaç tacı yaklaşınca gizlenmez. Yolun kamera tarafındaki ağaç sıklığı azaltılarak görünürlük sağlanır. Ev görüş engelleri mevcut saydamlık sistemini kullanır.

Kabul: köy çiftlikten belirgin ayrı; boş başlangıç; tek bir zorunlu güzergâh yok; keşif sandıkları yeni dünyada değişiyor; mevcut kayıtta yapılar/ürünler korunuyor; zoom uçlarında yol/hedef okunuyor; orman yoğunluğunda gerçek Linux/Windows performansı ölçülüyor. Bunlar harita uygulamasının gelecek kabul ölçütleridir, bu tasarım turunda geçtiği iddia edilmez.
