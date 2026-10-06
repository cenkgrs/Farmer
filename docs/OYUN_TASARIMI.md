# Oyun tasarımı

Durum: Kullanıcıyla kabul edilen temel yön; sayısal değerler ve içerik isimleri prototipte ayarlanabilir.

## Deneyim

Huzurlu bir çiftlik kur; ürün yetiştir; kazancın ve topladığın malzemelerle evini/üretimini geliştir; hafif tehlikeli çevreye çıkarak yeni fırsatlar bul. Oyuncu kısa ve uzun hedeflerini kendi belirleyebilmeli. Sürükleyicilik merak, sahiplenme ve görünür ilerlemeden gelsin.

Kamera sabit açılı izometrik; sınırlı yakınlaştırma önerisi var. Görsel dil stilize 3D, sıcak toprak/ahşap renkleri, yumuşak formlar ve okunaklı ürün silüetleri. Orman daha serin gölgeli; çiftlik güvenli ve davetkâr. Onaylı referanslar `references/` dizinindedir.

## Temel döngü

Tohum al → toprağı hazırla → ek/sula → büyümeyi gör → hasat et → sat, işle veya üretim için sakla → çiftliği geliştir → keşfe hazırlan → kaynakla geri dön.

Tekrarlanan işleri ileride sulama/üretim sistemleri kolaylaştırmalı. Daha geniş tarla yalnızca daha çok tıklama anlamına gelmemeli. Gün uzunluğu oyuncunun dekorasyon ve keşifle oyalanmasına izin vermeli; süre henüz belirlenmedi.

## Çiftçilik

- Pazar temel tohumların güvenilir kaynağıdır; keşif ileride çeşitliliği artırabilir.
- Her üründe dört görünür aşama: filiz, gelişen bitki, olgunlaşan bitki, hasada hazır bitki.
- Sulanmış ve kuru toprak görsel olarak ayrılır. Başlangıçta sulamayı atlamak büyümeyi geciktirir; hemen ölüm/çürüme yoktur.
- Gelişme oyun zamanı üzerinden ilerler; gerçek dünyada bekleme zorunluluğu hedeflenmez.
- Hasat açık geri bildirim verir: animasyon, ses, ürün ve envanter değişimi.
- Satış, yeniden yatırım, yemek ve eşya üretimi arasında seçim vardır.

Önerilen ilk altılı:

| Ürün | Rol |
|---|---|
| Turp | Hızlı ilk gelir; 0.1'in tek ürünü için aday. |
| Patates | Yemek hammaddesi. |
| Domates | İlk büyümeden sonra tekrar ürün. |
| Buğday | Una işleme. |
| Keten | Lif/kumaş, eşya üretimi. |
| Şifalı ot | Keşif için iyileştirici üretimi. |

Süreler, fiyatlar, verimler ve tarif miktarları henüz kararlaştırılmadı. Kod içine dağılmış sabitler yerine düzenlenebilir tanımlar kullanılacak.

## Üs kurma

- Dünya kare tabanlı bir yerleşim sistemi kullanır. Ahşap küpler yan yana ve üst üste konabilir.
- İnşaat parçaları: temel, tam/yarım blok, direk, döşeme, duvar, pencere, kapı çerçevesi, kapı, çatı, çit, bahçe kapısı ve yollar.
- İnce duvar ve çitler hücre kenarına; küpler hücre hacmine oturur. Çakışma kontrolleri bu ayrımı korur.
- İç mekân seti: yatak, masa, sandalye, sandık ve lamba. Tezgâh gibi üretim eşyaları ilerlemeye bağlanır.
- Yerleşim önizlemesi geçerli/geçersiz alanı gösterir. Döndürme, taşıma, sökme ve yükseklik seçimi anlaşılır olmalı.
- İlk yaklaşım: sökülen parçanın inşa malzemeleri geri verilir; deneme/yeniden tasarlama cezalandırılmaz. İçinde eşya olan sandığın sökülmesinde eşya kaybı/çoğaltma engellenir.
- Yapının önünü kapatan duvar/çatı gerektiğinde gizlenir; iç dekorasyon sırasında seçili katın üstü görünmez.
- İlk ev tek katlıdır. Blokları üst üste koymak erken desteklenir; yaşanabilir üst kat, merdiven ve gelişmiş kat yönetimi demo sonrasına aittir.
- Izgara, kapı geçişi, karakter yüksekliği ve eşya boyutları birlikte prototiplenir. Geçici ölçüler kesin üretim standardı diye kabul edilmez.

## Dünya ve savaş

İlk demo: çiftlik, küçük pazar ve orman/küçük maden girişi. Büyük açık dünya gerekmiyor.

Çiftlik başlangıçta güvenlidir; baskın yok. Ormanda iki basit düşman adayı: yavaş slime ve saldırısını önceden belli eden böcek. Basit saldırı ve kaçınma; çoğu karşılaşmadan uzaklaşmak mümkün. Tam envanter kaybı yerine hafif yenilgi bedeli hedeflenir; miktarı daha sonra belirlenir.

## Üretim ve ilerleme

Odun → yapı parçaları; keten → lif/kumaş → yatak/ekipman; maden → gelişmiş alet ve tezgâh. Her hasat veya keşif, görünür bir inşaat/üretim hedefine katkı sağlayabilmeli.

## Kapsam dışı ve açık alanlar

İlk demoda multiplayer, hayvancılık, mevsimler, karmaşık NPC ilişkileri, büyük açık dünya, tam otomasyon ve yaşanabilir üst katlar yok. Bunlar ileride değerlendirilebilir; vaat veya aktif iş değildir.

İlk fikirlerde geçen sihirli bitkiler/sis temizleme ve hava felaketleri temel kapsamda kabul edilmedi. Bunları sessizce oyunun ana mekaniğine dönüştürme.
