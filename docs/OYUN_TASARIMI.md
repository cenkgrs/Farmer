# Oyun tasarımı

Durum: Kullanıcıyla kabul edilen temel yön; sayısal değerler ve içerik isimleri prototipte ayarlanabilir.

## Deneyim

Huzurlu bir çiftlik kur; ürün yetiştir; kazancın ve topladığın malzemelerle evini/üretimini geliştir; hafif tehlikeli çevreye çıkarak yeni fırsatlar bul. Oyuncu kısa ve uzun hedeflerini kendi belirleyebilmeli. Sürükleyicilik merak, sahiplenme ve görünür ilerlemeden gelsin.

Kamera sabit açılı izometrik; sınırlı yakınlaştırma önerisi var. Görsel dil stilize 3D, sıcak toprak/ahşap renkleri, yumuşak formlar ve okunaklı ürün silüetleri. Orman daha serin gölgeli; çiftlik güvenli ve davetkâr. Onaylı referanslar `references/` dizinindedir.

## Temel döngü

Tohum al → toprağı hazırla → ek/sula → büyümeyi gör → hasat et → sat, işle veya üretim için sakla → çiftliği geliştir → keşfe hazırlan → kaynakla geri dön.

Tekrarlanan işleri ileride sulama/üretim sistemleri kolaylaştırmalı. Daha geniş tarla yalnızca daha çok tıklama anlamına gelmemeli. Gün uzunluğu oyuncunun dekorasyon ve keşifle oyalanmasına izin vermeli; süre henüz belirlenmedi.

## Çiftçilik

**7 Ekim kullanıcı geri bildirimiyle kesinleşen etkileşim:** Tarla karesini tıklayarak seçmek gerekmez. Fare o karenin üzerindeyken ve oyuncu yeterince yakındayken sol tık, eldeki eşyayı kullanır. Tohum ekim, sulama kabı sulama, orak hasat yapar; işlemler bitkinin durumuna bakarak otomatik başka bir işe dönüşmez. Eşyalar hızlı erişim çubuğundan veya 1/2/3/5 ile kuşanılır; karakterin elinde görünür. Tohum ve hasat bir sol tıkla tek işlem yapar. Sulama kabında sol tuş basılı tutulur; farenin geçtiği erişilebilir ekili kareler birer kez sulanır. Yakındaki tarla üzerinde hafif su dökme sesi döner. Tuş bırakma, eşya değiştirme veya hedeften çıkmada sulama hemen durur; ses yaklaşık 0,7 saniyede azalarak biter. Odak kaybı/pause/devre dışı bırakmada ses doğrudan susturulur. Son 7 Ekim ses geri bildirimiyle sentez yerine gerçek sulama kabı kaydına geçildi; kaynak ses seviyesi 0.064 değerinden %30 artırılarak 0.0832 yapıldı. Kaynak/lisans `audio-sources/README.md` içinde. Arayüz üzerinde başlayan sürükleme sulamayı başlatmaz. Alan etkisi, su doldurma ve alet dayanıklılığı yoktur.

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

0.1 için uygulanan geçici denge: turp tohumu 10 para, ürün satışı 18 para, hasat verimi 1, olgunlaşma ilk sulamadan sonraki üç gece. Başlangıç 60 para, sıfır tohum/ürün, sulama kabı, orak ve çapa. Dört aşama ekildiği gün filiz, ilk sulamadan bir gece sonra gelişen bitki, ikinci gece olgunlaşan bitki ve üçüncü gece hasada hazır bitkidir. Diğer ürünlerin değerleri henüz belirlenmedi.

Ürün değerleri `Assets/_Farmer/Data/Turnip.asset` tanımındadır. Saat normal akışta ilerler; tam 24 oyun saati kullanıcı kararıyla **10 gerçek dakika** sürer. Gece yarısında gün ve sulanmış ürünlerin büyümesi bir kez ilerler. Yalnızca bir yatağın yanında N veya uyuma düğmesi saati sonraki 06:00'ya taşır; 00:00–05:59 arasında bu aynı günün sabahıdır ve yeniden büyüme vermez. Uyku zorunlu değildir; gece kendiliğinden sabaha döner. Odak kaybında zaman durur, oyun kapalıyken ilerlemez. Pazar tezgâhına yaklaşarak tohum alınır ve ürün satılır. Başarılı işlemler otomatik kaydedilir. Bu ilk denge oynanış geri bildirimiyle değişebilir.

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

### Dünya üzerinde serbest yerleşim ve çapa — 7 Ekim güncel karar

İnşa alanı veya yalnızca çiftlikte kurma kısıtı yoktur. Gelecekte düşman bölgelerinde de uygun zeminde yapı/barınak kurulabilir; düşmanlardan korunma, duvarların davranışı ve uyuma tehlikesi o bölgenin geliştirmesinde ele alınacak. Bu adım yeni düşman bölgesi veya savaş eklemez.

Mevcut prototipin düz 20×20 arazisindeki boş ve sağlam zemin kullanılabilir. Arazinin dışı, havada kalan yapılar, mevcut nesneler/oyuncu ve ekili ürünlerle çakışma engellenir. Dünya kayıtları alan kimliğine bağlı değildir; yeni bölgelerde zemin `WorldGround` ile tanımlanır. Prototipte zemin y=0, hücre 1 m, üst üste en fazla üç ahşap blok; farklı arazi yükseklikleri ve yaşanabilir üst kat henüz yoktur.

`4` inşa; alt inşa barındaki ikonlar veya `Q` ile blok/yatak/döşeme/duvar/kapı/çatı seçimi; sol tık yerleştir; `R` 90° döndür; tekerlek blok yüksekliği; sağ tık görünen yapıyı sök. `Esc` veya `1/2/3/5` tarıma döner. Grid sadece oyuncu çevresindeki bir hizalama yardımcısıdır, yerleşim bölgesi değildir. Yeşil/kırmızı önizleme ve yatay 3,5 m erişim korunur. Hacim blokları için alt destek gereklidir; üstünde blok olan alt blok sökülemez. Çatı ayrı parçadır: fareyle hedeflenen desteğe otomatik oturur; ince duvar/kapıda 2,4 m, üç blokluk duvarda 3 m; destekten aynı yükseklikte en fazla dört panel uzayabilir. Altı boş kalır. Son çatı desteği sökülemez veya taşınamaz. `M` taşıma modunda eşyayı sol tuşla tut, sürükle ve bırak; R döndürür, geçersiz bırakma/Esc işlemi kayıpsız iptal eder.

Yeni oyun 50 para, 24 odun ve yataksız başlar. Blok/çatı 2 odun; yatak pazardan 100 paraya satın alınır. 1×2 hücrelik yatağı yerleştirmek odun harcamaz, sökmek yatak eşyasını çantaya geri verir. Ahşap parçalarda sökme tam odun iadesidir. Pazar 20 paraya 10 odun, çanta sınırı 999. Bunlar geçici denge değerleridir. Yatak zemine kurulur ve 1,8 m çevresinde uyuma sağlar; başlangıçta dünyada ücretsiz yatak bulunmaz. Eski kayıttaki başlangıç yatağı taşınabilir parçaya dönüştürülür; eski konum doluysa yatak çantaya alınır. Yatak önizlemesi uyuma sağlamaz.

Yeni oyunda otomatik hazırlanmış tarla yoktur. `5` ile kuşanılan çapa, yakındaki boş toprağı sol tıkla ekime hazırlar. Ardından 1/2/3 ile aynı ekim/sulama/hasat döngüsü işler. Ekili kare tekrar çapalanmaz ve kaynak kaybetmez; hasat sonrası toprak ekilebilir kalır. Yapının altında veya sert engellerde çapa kullanılmaz; boş ekilebilir karenin üzerine yapı kurulabilir, ekili ürün önce hasat edilmelidir.

Saat, odun, yatak/yapı koordinatları ve hazırlanmış toprak kayıt sürümü 5'te saklanır. V1/V2 kayıtlarındaki tarlalar ve blokların dünya konumları korunur; eski çiftlik bilerek silinmez. İlk v5 yazımından önce sağlam eski ana kayıt `.pre-v5` yedeğine alınır. Saat gündüz/gece ışıklarını yumuşak değiştirir; kalıcı işlem ve gece yarısına ek olarak 30 saniyede bir kaydedilir.

## Dünya ve savaş

İlk demo: çiftlik, küçük pazar ve orman/küçük maden girişi. Büyük açık dünya gerekmiyor.

Çiftlik başlangıçta güvenlidir; baskın yok. Ormanda iki basit düşman adayı: yavaş slime ve saldırısını önceden belli eden böcek. Basit saldırı ve kaçınma; çoğu karşılaşmadan uzaklaşmak mümkün. Tam envanter kaybı yerine hafif yenilgi bedeli hedeflenir; miktarı daha sonra belirlenir.

## Üretim ve ilerleme

Odun → yapı parçaları; keten → lif/kumaş → yatak/ekipman; maden → gelişmiş alet ve tezgâh. Her hasat veya keşif, görünür bir inşaat/üretim hedefine katkı sağlayabilmeli.

## Kapsam dışı ve açık alanlar

İlk demoda multiplayer, hayvancılık, mevsimler, karmaşık NPC ilişkileri, büyük açık dünya, tam otomasyon ve yaşanabilir üst katlar yok. Bunlar ileride değerlendirilebilir; vaat veya aktif iş değildir.

İlk fikirlerde geçen sihirli bitkiler/sis temizleme ve hava felaketleri temel kapsamda kabul edilmedi. Bunları sessizce oyunun ana mekaniğine dönüştürme.


## Oyun içi arayüz — güncel kullanıcı tercihi

Pazar ve yatağın üstünde dünya yazısı yoktur; yaklaşınca etkileşim paneli açılır. Alt arayüz referanstaki gibi ortalanmış ahşap çerçeveli küçük eşya gözlerinden oluşur: tohum, sulama kabı, orak, inşa, çapa, hasat ve odun. Tohum/hasat/odun miktarları ikon köşesinde görünür; seçili eşya altın renkle ayrılır. Sürekli geniş tuş listesi kaldırıldı. Ad/kısayol/kullanım açıklaması yalnızca ikonun üzerine gelince; inşa komutları inşa modundayken gösterilir. İşlem geri bildirimi kısa süreli görünür. Saat ve para sol üstte küçük karttadır. Bu hızlı envanter çubuğudur; genel çanta/sürükle-bırak/depolama sistemi değildir.

Kullanıcı düz vektör ikon ve tek renk kahverengi arka planı reddetti. Envanterde konseptle eşleşen ImageGen boyanmış eşya resimleri, ahşap damarları ve krem dokulu oyuklar kullanılır; miktar ve seçim vurgusu ayrı UI katmanıdır.

## İlk oda — 8 Ekim 2026

Döşeme (1 odun) 1×1 hücrenin tabanına, ince duvar (2 odun) ve kapı (3 odun) hücrenin kenarına oturur. R ile seçilen kenar döner. Komşu karenin karşı kenarı aynı yerdir; iki kez yerleştirilemez. Döşeme ile yatak aynı hücreyi paylaşır; duvar iki hücrelik yatağı ortadan bölemez. Döşeme üzerinde tarım yapılamaz. Duvar ve kapı 2,4 m yüksektir; bu parçalar şimdilik yalnızca zemin katına konur. Döşeme üst bloklara taşıyıcı değildir.

Yakındaki kapıya fareyi yöneltip F ile aç/kapat. Kapı kanadı 90° döner; oyuncu veya eşya açılma alanını kapatıyorsa işlem reddedilir. Kapının açık/kapalı hali kayıt v5'te saklanır. Sağ tık, imlecin gerçekten değdiği parçayı söker; aynı hücredeki döşeme/duvar/yatağı birlikte silmez. V1/V2/V3/V4 çiftlikler korunur; ilk v5 kayıttan önce eski sağlam ana dosya `.pre-v5` yedeği alır.

Bu parça çatısız, içine girilebilir oda iskeletidir. Çatı, sandık ve dekorasyon sonraki küçük adımlardır; 0.2 tamamlandı sayılmaz. Dünya arazi sınırları ve mevcut 10 dakikalık saat değişmez.

Kullanıcının 8 Ekim görsel düzeltmesi: yalnızca tepenin açık olması yetmez. Oyuncunun bulunduğu döşemeli odanın kameraya bakan ön ve yan duvarları, kapı/çerçeve dahil, yaklaşık %16 opaklığa yumuşak geçer. Arka duvarlar korunur; karakteri doğrudan örten diğer duvar parçaları da saydamlaşır. Odadan ve görüş engelinden çıkınca eski malzeme geri gelir. Collider ve kapı etkileşimi değişmez. Oda döşemeden bulunur; duvar/kapı kenarları komşu odaları ayırır.


## Çatı, taşıma ve inşa envanteri — 8 Ekim güncel karar

İnşa modunda normal eşya barının yerini **sekiz yuvalı inşa envanteri** alır. Altı parça ImageGen ikonuyla gösterilir: blok, yatak, döşeme, ince duvar, kapı, çatı. Son iki göz şimdilik boştur; parça sayısı arttığında kategoriler eklenecek. İkon köşesi yatak adedini veya odunla yapılabilir parça sayısını gösterir. Ayrı uzun parça listesi yoktur; taşıma M veya barın yanındaki düğmeyle açılır. İnşadan çıkış normal tarım barını geri getirir.

Döşemesiz, üst üste ahşap bloklardan yapılmış evler de iç mekân sayılır. Kameraya bakan blok sütunları ve iç mekânın üzerindeki çatı saydamlaşır; dışarı çıkıldığında opaklaşır. Tek hücrelik açıklık görsel oda sınırı olarak tanınabilir; bu fiziksel kapı veya güvenlik sağlamaz. Çok katlı ev, merdiven ve eğimli arazi halen kapsam dışıdır.

Basılı sulamada hedef kare değişince kol, karakter yönü ve su akışı yaklaşık 0,18 saniyelik yumuşatma ile yeni hedefe yönelir. Gerçek sulama işlemi imlecin o an gösterdiği erişilebilir kareye uygulanır; animasyon işlem geciktirmez.

Sonraki keşif adımı için kullanıcı fikirleri: çevrede para içeren sandık, ağaçtan odun, yabani bitkiden tohum ve başka çiftçinin hasadını alma. Hırsızlık yaptırımları daha sonraya bırakılır. Kullanıcı önce ev/taşıma/yatak işlerinin bitirilmesini seçti; bu tur keşif veya hırsızlık sistemi eklemez.


## İlk keşif — 8 Ekim uygulanmış güncel kararlar

Yeni dünyada 50 para ve kalıcı çapa/balta/orak/sulama kabı vardır; kılıç yoktur. Yeni dünya için rastgele seed üretilir, dış halkaya 24 ağaç/12 yabani tohum bitkisi/5 keşif sandığı dağıtılır. Sandıklar 30–65 para içerir. Yerleşim ve ödül ilk açılışta kaydedilir; aynı kayıt yeniden açıldığında konum/açılmış durum değişmez. Yalnızca yeni dünya yeni dağılım üretir. Kaynaklar bu adımda yeniden doğmaz.

6 veya balta ikonu ile kuşan, ağaca üç sol tık vur → 8 odun. 3/orakla yabani bitkiye sol tık → 2 turp tohumu. Sandığa herhangi bir aletle sol tık → kayıtlı para. 2,5 m mesafe ve arada engel olmaması gerekir; dolu çanta son kaynak vuruşunu tüketmez. Kısmi ağaç kesimi de kaydedilir. Başlangıç silahı yoktur; balta bu adımda savaş silahı değildir. Gerçek düşman riski savaş adımında eklenecek.

Arazi 56×56 m, merkez çiftlik aynı konumda; dış halkada kaynaklar arasında yürünebilir aralık bulunur. İzometrik açı değişmez; kamera çiftlikten uzaklaşırken takip eder. Mevcut yapı/çapa/10 dakikalık saat kuralları sürer. Kayıt v6, eski sağlam kayıt `.pre-v6` yedeğiyle korunur. Normal envanter sekiz göze çıktı; inşa barı kendi sekiz yuvasını korur.

9 Ekim görsel kararı: oyuncu ağaca yaklaşınca yaprakları veya üst gövdesi gizlenmez. Ağaç yalnızca kesildiğinde kütüğe dönüşür. Bu karar ev duvarlarının/çatısının iç mekân saydamlığını değiştirmez.


## Satın alınan mobilyalar — 9 Ekim kararı

Kişisel depolama sandığı, masa, sandalye ve lamba dükkândan satın alınır; yatak da mevcut satın alma düzenini korur. Mobilya çantada bir eşyadır: yerleştirme adedi tüketir, sökme mobilyayı geri verir; odun veya para iadesine dönüşmez. Dolu sandığı taşıma içeriği korur; sökme/toplama kayıp veya çoğaltma yaratmamalıdır. Fiyatlar geçici denge değerleriyle belirlenecek, henüz kesinleşmedi.

Önce Tab ile çanta ve kişisel sandık, sonra masa/sandalye/zemine konan ayaklı lamba. Alt sekiz yuva hızlı erişim barı olarak kalır. Çanta ve sandık arasında sürükle-bırak, Shift+tık ile yığın aktarımı hedefleniyor. Bu bölüm onaylı sıradaki kapsamdır; henüz uygulanmış özellik listesi değildir. [Model eskizleri ve teslim rehberi](references/furniture_sketches_v01/README.md).


### 9 Ekim uygulanan mobilya ve depolama adımı

Dükkân: depolama sandığı 80, masa 60, sandalye 25, ayaklı lamba 45 para; yatak 100 para. Bunlar ilk oynanış için geçici denge değerleridir. Mobilya odun harcamaz. İnşa barı sekiz yuva olarak kalır; **Yapı / Mobilya** sekmeleriyle kategori seçilir. Masa ve yatak iki hücre, diğer mobilyalar tek hücre kaplar; zemine/döşeme üstüne konur, üst yapı taşıyıcısı değildir.

**Tab** çantayı açar. Yakındaki kişisel sandığı hedefleyip **F** ile çanta/sandık paneli açılır. Tık bir adet, Shift+tık tüm yığın, karşı panele sürükle-bırak tüm yığını aktarır. Geçersiz bırakma kayıpsız iptaldir. Çalışma aletleri kalıcıdır ve sandığa bırakılamaz; odun, tohum, hasat ve satın alınan mobilyalar saklanabilir. Sandık 16 farklı eşya yığını tutar; yığın sınırı 999. Bu ilk çanta görünümü mevcut eşya türlerini gösterir; serbest yuva düzenleme veya ağırlık sistemi eklenmedi.

Dolu sandık **M** ile içeriğiyle taşınır. Çantaya geri toplamak için önce boşaltılır; dolu sandığı sökmek reddedilir ve M bilgisi gösterilir. F9/yükleme veya erişim kaybı açık sandık panelini kapatır. Panel açıkken hareket, tarım ve inşa girdileri engellenir; saat akmaya devam eder.

Lamba 18:00–19:00 arasında yanar, 06:00–07:00 arasında söner. Masa/sandalye yerleştirilebilir mobilyadır; oturma animasyonu yok. Sandık bu adımda kapalı modelle depolama panelini açar; kapak animasyonu yok. Kayıt v7, eski kayıtlar ilk yazımdan önce `.pre-v7` ile korunur.

## İç mekân ve karakter merkezli kamera — 9 Ekim

Kamera her konumda karakterin gövde merkezini ekran ortasında tutar; dış dünyada kenara yaklaşma eşiği veya gecikmeli kayma yoktur. Sabit izometrik dönüş korunur. Kapalı odaya girince oda ölçülerine uygun yakın ölçeğe yumuşak geçer; inşa modunda ve dışarı çıkınca eski geniş ölçeğe döner. Yakın görünümde de merkez karakterdir, oda merkezi değildir.

Oda tespiti döşeme bütünlüğüne bağlı değildir: blok duvar, ince duvar ve kapı sınırları kullanılır. İçeride odanın üstündeki çatılar, kameraya bakan çevre duvarları ve oda içini örten yapı parçaları saydamlaşır. Çarpışma ve arka duvar görünümü korunur. Döşemesi eksik veya toprak zeminli kapalı ev de aynı davranışı kullanır; yalnızca açık bir döşeme platformu yakın kamera başlatmaz.

9 Ekim manuel zoom: normal oyunda fare tekerleği yakınlaştırır/uzaklaştırır; inşa modunda Ctrl+tekerlek zoom, düz tekerlek blok yüksekliği olarak kalır. HUD üstünde ve çanta/sandık açıkken zoom engellenir. İç/dış/inşa ölçek tercihleri oturum boyunca ayrı tutulur; kayıt dosyasına yazılmaz. Karakter merkezde ve açı sabit kalır.

9 Ekim büyük harita kararı: pazar/esnaf oyuncunun çiftliğinde olmayacak; ayrı köy merkezi ve meydanda toplanacak. Etkileşimli karakterler sonraki adım. [Vadi tasarımı](HARITA_TASARIMI.md) ve [ilk sekiz model paketi](references/valley_v01/README.md) hazır; mevcut küçük oyun sahnesi bu tur büyük haritaya dönüşmedi.

9 Ekim ilk köy kesiti uygulanması: pazar çiftlikten (-60,88) köy noktasına taşındı; mevcut alışveriş kataloğu korunuyor. Köydeki üç yeni cephe şu an dekor; ayrı esnaf/NPC değil. Yeni yol, kuyu ve çevre dekoru eklendi, tam vadi/nehir henüz tamamlanmadı. Ayrıntılar HARITA_TASARIMI.md uygulama bölümünde.

## 9 Ekim — menü ve daha sessiz arayüz

Oyun açılış menüsünden başlar; kayıt varsa **Devam Et**, ilk açılışta **Başla**. Kayıt sıfırlayan Yeni Oyun düğmesi bu adımda yok. Esc normal oyunda menüyü açar; çanta/inşa açıksa önce ilgili görünümü kapatır. Menüde dünya saati ve oyuncu girdisi durur; Devam Et geri döndürür. Tam Ekran düğmesi veya F11 pencere/kenarlıksız tam ekran arasında geçer. Menüde Kaydet ve Çık vardır; kayıt başarısızsa çıkış durdurulur.

Dünya hedeflerine sürekli çıkan tuş/alet talimatları ve rutin işlem bildirimleri kaldırıldı. Eşya barında yalnız imleçle üzerine gelinen eşyanın kısa adı/tuşu görünür; alışveriş ve depolama arayüzü işlevleri korunur. Tutorial ayrı tasarlanacak. Tekerlek hem Linux birimlik hem 120 değerli girdilerle hızlı, kısa yumuşatmalı zoom yapar. Saydam duvar/çatı sandık hedefleme ışınını kesmez; oyuncuyla sandık arasındaki gerçek duvar ve mesafe kontrolü sürer.

Köy meydanı/giriş yolunda taş döşeme. Kullanıcı geniş haritanın seyrekliğini ve konseptten uzaklığını kabul etmedi; çevre dizilimini Unity'de kendisi yapacak. Otomatik yerleşim yeniden üretilmeyecek. [Proje özelinde harita düzenleme rehberi](UNITY_HARITA_DUZENLEME.md).
