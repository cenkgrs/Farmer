# Oyun tasarımı

Durum: Kullanıcıyla kabul edilen temel yön; sayısal değerler ve içerik isimleri prototipte ayarlanabilir.

## Deneyim

Huzurlu bir çiftlik kur; ürün yetiştir; kazancın ve topladığın malzemelerle evini/üretimini geliştir; hafif tehlikeli çevreye çıkarak yeni fırsatlar bul. Oyuncu kısa ve uzun hedeflerini kendi belirleyebilmeli. Sürükleyicilik merak, sahiplenme ve görünür ilerlemeden gelsin.

Kamera sabit açılı izometrik; sınırlı yakınlaştırma önerisi var. Görsel dil stilize 3D, sıcak toprak/ahşap renkleri, yumuşak formlar ve okunaklı ürün silüetleri. Orman daha serin gölgeli; çiftlik güvenli ve davetkâr. Onaylı referanslar `references/` dizinindedir.

## Temel döngü

Tohum al → toprağı hazırla → ek/sula → büyümeyi gör → hasat et → sat, işle veya üretim için sakla → çiftliği geliştir → keşfe hazırlan → kaynakla geri dön.

Tekrarlanan işleri ileride sulama/üretim sistemleri kolaylaştırmalı. Daha geniş tarla yalnızca daha çok tıklama anlamına gelmemeli. Gün uzunluğu oyuncunun dekorasyon ve keşifle oyalanmasına izin vermeli; süre henüz belirlenmedi.

## Çiftçilik

**7 Ekim kullanıcı geri bildirimiyle kesinleşen etkileşim:** Tarla karesini tıklayarak seçmek gerekmez. Fare o karenin üzerindeyken ve oyuncu yeterince yakındayken sol tık, eldeki eşyayı kullanır. Tohum ekim, sulama kabı sulama, orak hasat yapar; işlemler bitkinin durumuna bakarak otomatik başka bir işe dönüşmez. Eşyalar hızlı erişim çubuğundan veya 1/2/3 ile kuşanılır; karakterin elinde görünür. Tohum ve hasat bir sol tıkla tek işlem yapar. Sulama kabında sol tuş basılı tutulur; farenin geçtiği erişilebilir ekili kareler birer kez sulanır. Yakındaki tarla üzerinde hafif su dökme sesi döner. Tuş bırakma, eşya değiştirme veya hedeften çıkmada sulama hemen durur; ses yaklaşık 0,7 saniyede azalarak biter. Odak kaybı/pause/devre dışı bırakmada ses doğrudan susturulur. Son 7 Ekim ses geri bildirimiyle sentez yerine gerçek sulama kabı kaydına geçildi; kaynak ses seviyesi 0.064 değerinden %30 artırılarak 0.0832 yapıldı. Kaynak/lisans `audio-sources/README.md` içinde. Arayüz üzerinde başlayan sürükleme sulamayı başlatmaz. Alan etkisi, su doldurma ve alet dayanıklılığı yoktur.

**Alet hareketleri:** Sularken kol öne uzanır, kap eğilir ve ağzından hedef kareye ince su akışı görünür. Tuş bırakıldığında kap 0,7 saniyede taşımaya döner; su görseli mevcut sesin sönümlenmesini izler. Başarılı hasatta orak 0,52 saniyelik hazırlanma/savurma/toparlanma hareketi yapar. Bunlar mevcut Idle/Walk üzerinde prosedürel kol/el hareketleridir; yeni FBX gerektirmez. Hareket kilidi veya ek bekleme süresi yoktur, işlem sonucu mevcut tıklama anında uygulanır. Oyuncu sularken yürüyebilir; karakter görseli işlem hedefine bakar. Eşya değişimi, kayıt yükleme, odak/pause ve devre dışı bırakma geçici hareketi temizler.

0.1 envanterinde sulama kabı ve orak birer kalıcı başlangıç eşyasıdır. Tohum adedi pazar alışverişinden gelir ve ekimde azalır. Bu prototipte genel çanta/taşıma sistemi yoktur; alet edinme/üretme ekonomisi sonraki tasarım işidir.

- Pazar temel tohumların güvenilir kaynağıdır; keşif ileride çeşitliliği artırabilir.
- Her üründe dört görünür aşama: filiz, gelişen bitki, olgunlaşan bitki, hasada hazır bitki.
- Her ekimden sonra **bir kez sulamak yeterlidir**; sulama hasada kadar korunur. Büyüme süresi değişmez. Hiç sulanmayan bitki bekler; ölüm/çürüme yoktur. Hasat sonrası yeni ekim yeniden bir sulama ister. Sulanmış ve kuru toprak görsel olarak ayrılır.
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

0.1 için uygulanan geçici denge: turp tohumu 10 para, ürün satışı 18 para, hasat verimi 1, olgunlaşma ilk sulamadan sonraki üç gece. Başlangıç 60 para, sıfır tohum/ürün ve iki başlangıç aleti. Dört aşama ekildiği gün filiz, ilk sulamadan bir gece sonra gelişen bitki, ikinci gece olgunlaşan bitki ve üçüncü gece hasada hazır bitkidir. Diğer ürünlerin değerleri henüz belirlenmedi.

Ürün değerleri `Assets/_Farmer/Data/Turnip.asset` tanımındadır. Gün yalnızca kampta N veya dinlenme düğmesiyle ilerler; gerçek zamanlı süre sınırı yoktur. Pazar tezgâhına yaklaşarak tohum alınır ve ürün satılır. Başarılı işlemler otomatik kaydedilir. Bu ilk denge oynanış geri bildirimiyle değişebilir.

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

### 0.2 ilk yerleşim denemesi

Uygulanan geçici değerler: tarlanın sağındaki 6×5 inşa alanında 1 m ahşap bloklar, en fazla üç blok yüksekliği. Başlangıç 24 odun; bir blok 2 odun, sökme tam iade. Pazardan 20 paraya 10 odun alınır; odun üst sınırı 999. Bunlar nihai ekonomi dengesi değildir; kaynak toplama daha sonra ele alınacak.

`4` veya alt çubuk inşa modunu açar. Sol tık yerleştirir, `R` 90° döndürür, tekerlek yüksekliği seçer, sağ tık görünen bloğu söker; `Esc` veya `1/2/3` tarıma döner. Önizleme yeşil/kırmızıyla geçerliliği gösterir. Yerleştirme yatayda 3,5 m erişim ister; oyuncu/nesne çakışması ve havada desteksiz blok engellenir. Üstünde blok olan alt blok sökülemez. Basılı sol tık tekrar tekrar blok üretmez.

Yapı ve odun mevcut çiftlikle birlikte kaydedilir; eski çiftlikler korunarak kayıt sürümü 2'ye taşınır. Henüz çit, kapı, çatı, eşya veya duvar gizleme yoktur.

## Dünya ve savaş

İlk demo: çiftlik, küçük pazar ve orman/küçük maden girişi. Büyük açık dünya gerekmiyor.

Çiftlik başlangıçta güvenlidir; baskın yok. Ormanda iki basit düşman adayı: yavaş slime ve saldırısını önceden belli eden böcek. Basit saldırı ve kaçınma; çoğu karşılaşmadan uzaklaşmak mümkün. Tam envanter kaybı yerine hafif yenilgi bedeli hedeflenir; miktarı daha sonra belirlenir.

## Üretim ve ilerleme

Odun → yapı parçaları; keten → lif/kumaş → yatak/ekipman; maden → gelişmiş alet ve tezgâh. Her hasat veya keşif, görünür bir inşaat/üretim hedefine katkı sağlayabilmeli.

## Kapsam dışı ve açık alanlar

İlk demoda multiplayer, hayvancılık, mevsimler, karmaşık NPC ilişkileri, büyük açık dünya, tam otomasyon ve yaşanabilir üst katlar yok. Bunlar ileride değerlendirilebilir; vaat veya aktif iş değildir.

İlk fikirlerde geçen sihirli bitkiler/sis temizleme ve hava felaketleri temel kapsamda kabul edilmedi. Bunları sessizce oyunun ana mekaniğine dönüştürme.
