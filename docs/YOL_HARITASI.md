# Sürüm planı ve kabul ölçütleri

Tarihler/tahmini geliştirme süreleri henüz yok. Her sürüm oynanabilir bir adım olmalı. 0.1 kapsamında hareket ve kare seçimi doğrulandı; kalan özelliklerin durumu aşağıda.

## 0.1 — Çekirdek prototip

Küçük arazi, sabit ortografik kamera, karakter hareketi, kare seçimi, temel envanter/para, bir ürün, basit pazar etkileşimi ve oyun günü ilerlemesi.

Kabul:

- [x] Oyuncu hareket eder, seçilen toprak karesi anlaşılır, kamera dönmez. (7 Ekim: 12 mantık testi, Linux player giriş/çarpışma/seçim kontrolü ve ekran görüntüsü.)
- [ ] Tohum satın alma → ekim → sulama → dört görünür büyüme aşaması → hasat → satış → yeniden tohum satın alma döngüsü tamamlanır.
- [ ] Tohum ve para doğru eksilir; hasat tek kez alınır; negatif miktar/para oluşmaz.
- [ ] Sulanmayan gün büyüme gecikir; bitki hemen ölmez.
- [ ] Geliştirme sırasında gün ilerletme hızlandırılabilir; oyuncu akışında anlaşılır gün bitirme etkileşimi vardır.
- [ ] Temel kayıt/yükleme ile para, envanter, gün ve tarla durumu korunur. Bu teknik gereksinim tasarım sürekliliği için önerilen kapsamdır.
- [ ] Mantık kontrolleri ve kısa oyun içi deneme belgelenir.

İlk görsel kalite hedefi tek küçük köşede denenir; bütün model listesinin bitmesini beklemek gerekmez.

## 0.2 — İlk evim

Kaynak elde etme, kare/hacim/kenar yerleşimi, temel ahşap yapı seti, yatak ve sandık, kapı geçişi, çatı/duvar gizleme. Basit masa, sandalye ve lamba.

Kabul:

- [ ] Kaynaklar harcanarak tek katlı, içine girilebilir bir ev kurulabilir.
- [ ] Ahşap bloklar yan yana/üst üste yerleşir; geçersiz çakışmalar önlenir.
- [ ] Önizleme, döndürme, yükseklik seçimi, taşıma ve sökme çalışır.
- [ ] Ev içinde karakter ve yerleştirilen eşya görülebilir.
- [ ] Yatakla günü bitirme ve sandık depolaması çalışır; sökme/taşıma eşya çoğaltmaz veya kaybetmez.
- [ ] Yerleştirilen yapılar ve depolama kayıt/yüklemede korunur.

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
