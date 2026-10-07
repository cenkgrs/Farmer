# Sürüm planı ve kabul ölçütleri

Tarihler/tahmini geliştirme süreleri henüz yok. Her sürüm oynanabilir bir adım olmalı. 0.1 çekirdek prototipi Linux üzerinde doğrulandı; 0.2 ilk ahşap blok adımı hazır, sürümün tamamı henüz bitmedi. Görseller geçici, Windows doğrulaması ayrı açık iş.

## 0.1 — Çekirdek prototip

Küçük arazi, sabit ortografik kamera, karakter hareketi, fareyle kare hedefleme, temel envanter/para, bir ürün, basit pazar etkileşimi ve oyun günü ilerlemesi.

Kabul:

- [x] Oyuncu hareket eder, hedeflenen toprak karesi anlaşılır, kamera dönmez. (7 Ekim: 12 mantık testi, Linux player giriş/çarpışma/seçim kontrolü ve ekran görüntüsü.)
- [x] Elde kuşanılan tohum/sulama kabı/orak, fare hedefi üzerinde sol tıkla kullanılır; önce kare seçme adımı yoktur.
- [x] Tohum satın alma → ekim → sulama → dört görünür büyüme aşaması → hasat → satış → yeniden tohum satın alma döngüsü tamamlanır.
- [x] Tohum ve para doğru eksilir; hasat tek kez alınır; negatif miktar/para oluşmaz.
- [x] Sulama kabıyla basılı tutarak erişilebilir kareler arasında sulama ve döngüsel su sesi; bırakma/arayüzde 0,7 saniyelik ses sönümlenmesi, odak kaybında susma.
- [x] Her ekimde tek sulama büyümeyi başlatır; üç gece süresi korunur. Hiç sulanmayan ürün bekler, bitki hemen ölmez.
- [x] Geliştirme sırasında gün ilerletme hızlandırılabilir; oyuncu akışında anlaşılır gün bitirme etkileşimi vardır.
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

Kaynak elde etme, kare/hacim/kenar yerleşimi, temel ahşap yapı seti, yatak ve sandık, kapı geçişi, çatı/duvar gizleme. Basit masa, sandalye ve lamba.

Kabul:

- [ ] Kaynaklar harcanarak tek katlı, içine girilebilir bir ev kurulabilir.
- [x] Ahşap bloklar yan yana/üst üste yerleşir; geçersiz çakışmalar önlenir.
- [ ] Önizleme, döndürme, yükseklik seçimi, taşıma ve sökme çalışır.
- [ ] Ev içinde karakter ve yerleştirilen eşya görülebilir.
- [ ] Yatakla günü bitirme ve sandık depolaması çalışır; sökme/taşıma eşya çoğaltmaz veya kaybetmez.
- [ ] Yerleştirilen yapılar ve depolama kayıt/yüklemede korunur.

7 Ekim ilk parça: 6×5 alanda 1 m ahşap blok, üç blok yüksekliği, önizleme/döndürme/yükseklik/sökme, odun maliyeti/iadesi, pazardan odun ve kayıt geçişi uygulandı. Taşıma şimdilik söküp yeniden kurmayla yapılır; ayrı taşıma aracı yok. Ev, kapı, çatı, mobilya ve depolama henüz yapılmadığından yukarıdaki birleşik ölçütler açık tutuldu.

Yaşanabilir ikinci kat bu sürüme dahil değildir.

## 0.3 — Çalışan çiftlik

Altı ürün, tekrar hasat, pazar çeşitliliği, basit işleme/tarifler, çit/bahçe kapısı/yollar.

Kabul:

- [ ] Ürünler kullanım ve büyüme davranışıyla farklılaşır; yeni ürün veri tanımıyla eklenebilir.
- [ ] Ürün satılabilir veya en az bir anlamlı üretim tarifinde tüketilebilir.
- [ ] Girdi/çıktı miktarları, yetersiz malzeme ve envanter kapasitesi tutarlıdır.
- [ ] İlk 10–20 oyun dakikasındaki gelir/gider örneği kaydedilir ve kilitlenme açısından incelenir; bu süre tasarım denemesidir.

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
