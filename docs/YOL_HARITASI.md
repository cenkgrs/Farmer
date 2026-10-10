# Sürüm planı ve kabul ölçütleri

Tarihler/tahmini geliştirme süreleri henüz yok. Her sürüm oynanabilir bir adım olmalı. 0.1 çekirdeği doğrulandı. Kullanıcı 10 Ekim 2026'da 0.2 son kontrollerini tamam kabul edip 0.3'e geçiş istedi. 0.3 kaynak/kazma ve ilk üç ürün/tekrar hasat/sebze kasası adımı uygulandı; altı ürün hedefi ve çit/bahçe kapısı/yollar açık. Windows doğrulaması ayrı takip ediliyor.

## 0.1 — Çekirdek prototip

Küçük arazi, sabit ortografik kamera, karakter hareketi, fareyle kare hedefleme, temel envanter/para, bir ürün, basit pazar etkileşimi ve oyun günü ilerlemesi.

Kabul:

- [x] Oyuncu hareket eder, hedeflenen toprak karesi anlaşılır, kamera dönmez. (7 Ekim: 12 mantık testi, Linux player giriş/çarpışma/seçim kontrolü ve ekran görüntüsü.)
- [x] Elde kuşanılan tohum/sulama kabı/orak, fare hedefi üzerinde sol tıkla kullanılır; önce kare seçme adımı yoktur.
- [x] Tohum satın alma → ekim → sulama → dört görünür büyüme aşaması → hasat → satış → yeniden tohum satın alma döngüsü tamamlanır.
- [x] Tohum ve para doğru eksilir; hasat tek kez alınır; negatif miktar/para oluşmaz.
- [x] Sulama kabıyla basılı tutarak erişilebilir kareler arasında sulama ve döngüsel su sesi; bırakma/arayüzde 0,7 saniyelik ses sönümlenmesi, odak kaybında susma.
- [x] Her ekimde tek sulama büyümeyi başlatır; üç gece süresi korunur. Hiç sulanmayan ürün bekler, bitki hemen ölmez.
- [x] Saat kendiliğinden ilerler; 24 oyun saati 10 gerçek dakika. Gece/gündüz ışıkları değişir; yalnızca yatakta N ile sonraki sabah 06:00'ya uyunur.
- [x] Temel kayıt/yükleme ile para, envanter, gün ve tarla durumu korunur. Bu teknik gereksinim tasarım sürekliliği için önerilen kapsamdır.
- [x] Mantık kontrolleri ve kısa oyun içi deneme belgelenir.

İlk görsel kalite hedefi tek küçük köşede denenir; bütün model listesinin bitmesini beklemek gerekmez.

## 0.2 öncesi — İlk görsel set

7 Ekim kullanıcı kararı: oyuncu, sulama kabı, orak ve pazar Tripo’da üretilecek; oyuncu için Mixamo Idle/Walk eklenecek. Bu küçük ara aşama yeni oyun sistemi içermez.

- [x] Dört ayrı model eskizi ve teslim rehberi hazır: `references/model_sketches_v01/README.md`.
- [x] Kullanıcının dört GLB'si ve iki Mixamo FBX'i alındı; FBX'ler iskeletli karakteri de içeriyor.
- [x] Ölçek, geometri, dokular, Humanoid Idle/Walk ve temel alet tutuşu sahneye bağlandı; kaynak kusuru/teknik sınırlar `../ArtSource/tripo_v01/README.md` içinde.
- [x] Linux player görselleri incelendi; 103 kontrol geçti. Önceki açık üç kontrol ayrı grafik oturumunda geçti. Kullanıcı görselleri beğendi; Windows player çalıştırma kontrolü ayrıca açık.

- Sulama kabını eğme/su akışı ve kısa orak savurma prosedürel olarak eklendi; nihai kontrol kaydı `../YAZILIMCI.md` içinde.

## 0.2 — İlk evim

**10 Ekim kullanıcı kabulüyle tamam.** Kaynak toplama, serbest inşa, kapı/çatı ve
iç görünürlük, yatak, satın alınan mobilyalar, çanta/sandık, taşıma/sökme ve
kayıt/yükleme mevcut. Önceki teknik doğrulamalar ve platform sınırları
`YAZILIMCI.md` içindedir. Yaşanabilir ikinci kat bu sürüme dahil değildir.

## 0.3 — Çalışan çiftlik

Altı ürün, tekrar hasat, pazar çeşitliliği, basit işleme/tarifler, çit/bahçe kapısı/yollar.

Kabul:

- [x] Ürünler kullanım ve büyüme davranışıyla farklılaşır; yeni ürün veri tanımıyla eklenebilir.
- [x] Ürün satılabilir veya en az bir anlamlı üretim tarifinde tüketilebilir.
- [x] Girdi/çıktı miktarları, yetersiz malzeme ve envanter kapasitesi tutarlıdır.
- [x] İlk 10–20 oyun dakikasındaki gelir/gider örneği kaydedilir ve kilitlenme açısından incelenir; bu süre tasarım denemesidir.

10 Ekim: bu ölçütler ilk üç ürün (turp, havuç, domates) ve tek sebze kasası tarifiyle doğrulandı. **0.3 tamamlanmadı:** kalan üç ürün ve çit/bahçe kapısı/yollar açıktır. İlk 10–20 dakika [denge hesabı](DENGE_03.md) gerçek oyuncu oturumu ölçümü değildir; ilk mahsul gelirinin 20 dakikayı aştığı kaydedildi.

## 0.4 — Ormanın ötesi

Orman/maden girişi, kaynaklar, iki basit mob, saldırı/kaçınma, iyileştirme ve hafif yenilgi bedeli.

Kabul:

- [ ] Dışarı çıkmak kaynak/üretim hedefi sağlar; tehlike okunaklıdır.
- [ ] Çiftliğe dönmek mümkün ve anlamlıdır; çiftlik başlangıçta güvenli kalır.
- [ ] Düşman ve karakter çarpışmaları, hasar tekrarları ve yenilgi akışı doğrulanır.
- [ ] Çiftçiliğe dönmek için sürekli savaş zorunlu değildir.

## 0.5 — İlk demo

Yaklaşık bir saatlik başlangıç deneyimi hedefi, birleşik görsel dil, yönlendirme, ses, ayarlar, kayıt iyileştirmeleri ve paketleme.

Kabul:

- [ ] Temiz klondan belgelenen adımlarla açılır; kullanılan motor/SDK/paket sürümleri sabittir.
- [ ] Linux ve Windows geliştirme akışı denenir; yayınlanacak işletim sistemleri ayrıca seçilir.
- [ ] Oyuncu tarım, satış, ev kurma ve keşif döngüsünü dışarıdan sözlü yardım almadan tamamlar.
- [ ] Kamera engelleri, giriş/çıkışlar, kayıt/yükleme ve temel performans gerçek donanımda incelenir.
- [ ] Kalan sorunlar ve oynanış geri bildirimi kaydedilir; demo tamamlanmadan yeni büyük sistem eklenmez.

## Demo sonrası adaylar

Hayvancılık, mevsimler, sera/sulama gelişimi, üst katlar, daha fazla dekorasyon ve üretim. Sıra oyuncu deneyimine göre seçilecek.

Sıradaki somut adım: kullanıcı ev/taşıma/yatak sürümünü oynadıktan sonra keşif. Para sandıkları, ağaçtan odun ve yabani bitkiden tohum; başka çiftçinin hasadını alma fikri ve daha sonraki hırsızlık yaptırımları kayıtlıdır. Bu sürümde uygulanmadı.


8 Ekim keşif ara adımı tamamlanan kapsam: ağaçtan odun, başlangıç baltası, yabani bitkiden tohum ve her yeni dünyada rastgele sandık. Kaynaklar ve toplanma durumları v6 kayıtta kalır. 0.2'nin kaynak toplama kısmı ilerledi; kişisel depolama sandığı, diğer mobilyalar ve 0.4 savaş/düşman hâlâ açık. Kullanıcı başlangıçta kılıç istemiyor; kılıç edinme yolu henüz seçilmedi.

9 Ekim sıradaki onaylı adım: Tab çantası + satın alınabilir kişisel depolama sandığı, ardından dükkândan alınan masa/sandalye/ayaklı lamba. Satın alma→çanta→yerleştirme→eşya olarak geri toplama; dolu sandıkta içerik korunumu. Kullanıcı için model eskizleri hazır, bu mekanikler henüz uygulanmadı.

9 Ekim mobilya uygulaması: satın alınan sandık/masa/sandalye/ayaklı lamba, Tab çantası, 16 yığınlı kişisel sandık ve v7 içerik kaydı eklendi. Teslim doğrulamaları YAZILIMCI.md içinde. Oturma ve sandık kapak animasyonu bu adımda yok; büyük yerleşim performansı ve Windows doğrulaması açık.

9 Ekim kamera geri bildirimi: karakter merkezli sürekli takip, oda içinde yakın ölçek/inşa modunda geniş ölçek ve eksik döşemeden bağımsız oda saydamlığı. Doğrulama sonuçları YAZILIMCI.md içinde.

9 Ekim kullanıcı büyük harita tasarımını istedi: 384×384 m vadi ana konsepti, ayrı köy meydanı/esnaf ve ilk 128×128 m kesitle uygulama önerisi `HARITA_TASARIMI.md` içinde. Bu bir tasarım teslimi; mevcut dünyayı büyütme, pazar taşıma, NPC ve savaş bu tur uygulanmadı. İlk yeni model önceliği dükkân/kaya/çalı. Manuel zoom mevcut oyunda eklendi.

9 Ekim sekiz model tesliminden sonra ilk köy/yol kesiti uygulandı: 256 m taban, yaklaşık 128 m koridor, ayrı pazar, 3 cephe/kuyu/çevre modelleri. Tam vadi hedefi 384 m olarak duruyor; su/köprü yerleşimi, yeni dekoru kaynak sistemine bağlama ve esnaf/NPC sonraki işler. Kaynak/geometri doğrulaması ArtSource/valley_v01, oyun testi sonuçları YAZILIMCI.md içinde.

9 Ekim kullanıcı geri bildirimi: ev içi sandık hedefleme, hızlı zoom, sürekli yönergelerin kaldırılması, taş köy zemini, Devam Et/menü/tam ekran bu tur kapsamına alındı. Harita dizilimini kullanıcı Unity'de mevcut modellerle yapacak; otomatik genişletme/doldurma şu an sıradaki iş değil. Rehber: UNITY_HARITA_DUZENLEME.md. Sonraki somut adım kullanıcının ilk köy düzenini beraber değerlendirmek; çok yükseklikli arazi ve su sistemini hazır saymamak.


## 10 Ekim — 0.3 için kullanıcının seçtiği ilk adım

- Turpun dört büyüme aşamasını %20 küçültme.
- Kesilebilir ağaç hedefini 168'e çıkarma; mevcut kaynak durumlarını koruma.
- 48 kırılabilir kaya hedefi; pazardan satın alınan kazma, başlangıçta kazma yok.
- Taş toplama, çanta/sandık aktarımı ve v8 kalıcı kayıt; ilk geçişte `.pre-v8` yedeği.
- Kazma/kaya model eskizlerini kullanıcıya verme; gelen modelleri doğrulayıp bağlama.

Yeni ürünler, tekrar hasat ve işleme tarifleri 0.3'ün sonraki parçalarıdır.
Taş kullanımı henüz seçilmedi. Eski tarihli “sıradaki” notlar tarihsel kayıttır;
güncel öncelik bu bölüm ve YAZILIMCI.md'nin son kaydıdır.

## 10 Ekim — model teslimi ve ürün genişletmesi

Kazma/kaya kullanıcı GLB modellerine bağlandı. Kullanıcı havuç + tekrar hasat veren domates ve sebze kasası seçti. Ürün seçimi, pazar fiyatları, veri tanımlı tarif, çanta sayfaları ve v9 kayıt geçişi uygulandı. Yeni bitkiler geçici geometridir. Taş tarifi eklenmedi. Güncel değerler DENGE_03.md; kontroller YAZILIMCI.md son kaydındadır.
