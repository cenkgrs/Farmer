# 0.3 — altı ürün ve iki tarif dengesi

11 Ekim 2026. Bunlar ilk uygulama değerleridir; uzun oynanış değerlendirmesi yapılmadı. Tam gün 10 gerçek dakika ve ilk saat 06:00. Her ekimde bir sulama yeterli; domatesin tekrar hasadı da bu sulamayı korur. Büyüme gece yarısında ilerler.

| Ürün | Tohum fiyatı | İlk büyüme | Verim | Birim satış | Tekrar |
|---|---:|---:|---:|---:|---|
| Turp | 10 | 3 gece | 1 | 18 | Yeniden ekim |
| Havuç | 14 | 4 gece | 1 | 27 | Yeniden ekim |
| Domates | 24 | 5 gece | 2 | 12 | 2 gecede 2 ürün |
| Marul | 6 | 2 gece | 1 | 10 | Yeniden ekim |
| Buğday | 12 | 3 gece | 3 | 7 | Yeniden ekim |
| Balkabağı | 40 | 8 gece | 1 | 85 | Yeniden ekim |

Pazarda 1 turp + 1 havuç + 2 domates + 2 odun → 1 sebze kasası. Kasa 90 paraya satılır, çanta ve kişisel sandıkta saklanır. Girdileri ayrı satmak 69 para getirir; pazardan alınan iki odunun maliyeti 4 para olduğundan kasalama 17 para ek değer sağlar. Kasa bir yerleştirilebilir mobilya değildir. Taş bu tarifte harcanmaz; döşenebilir taş yol bir kare için 2 taş kullanır.

## İlk üç ürünle önceki rota (10 Ekim)

Bu hesap yürüyüş/ekim sürelerini sıfır kabul eder, gerçek oyuncu oturumu ölçümü değildir. Başlangıç 50 para / 24 odun / yataksız. Birer tohum türü almak 48 para; kalan 2. Üçünü 06:00'da ekip sulamak:

- 10 gerçek dakikada gün 2, 06:00: her ürün bir gece ilerlemiş; satış geliri 0, cüzdan 2.
- 20 gerçek dakikada gün 3, 06:00: her ürün iki gece ilerlemiş; satış geliri hâlâ 0, cüzdan 2.
- 27,5 dakika: üçüncü gece, ilk turp hazır. Satılırsa cüzdan 20; kasaya saklanırsa 2.
- 37,5 dakika: havuç hazır. Ayrı turp+havuç satışı toplam cüzdanı 47 yapar.
- 47,5 dakika: domates hazır. Üç ürünü kasaya ayıran rota 2 odun harcar ve satış sonrası 92 paraya ulaşır; başlangıç tohum giderine göre 42 net para artışı ve yerinde kalan domates vardır.
- 67,5 dakika: domates 2 ürün daha verir; yeni tohum veya sulama gideri yok.

**Bu rotanın gözlemi:** İlk 20 dakikada mahsul geliri yok. Oyuncu bu sürede ağaç, kalıcı para sandıkları ve yabani turp tohumlarını toplayabilir. Düşük para tarımı tamamen kilitlemez; sulanmış bitki kendiliğinden ilerler. Fakat ilk bekleme uzundur. Mevcut turp süresi ve gün uzunluğu kullanıcı kararı olduğu için bu tur sessizce kısaltılmadı. İlk oyuncu değerlendirmesinde başlangıç akışı ve yatağa ulaşma süresi ele alınmalı.

Yatak 100 para olduğundan bu örnekte tek kasa satışı yatağa hemen yetmez. Keşif geliri veya sonraki satış gerekir. Kazma 80 para; sebze kasası geliriyle alınabilir. Tüm tohumu satın alıp bitkileri bilerek sökmek gelir rotasını geciktirir; çapa sağ tık sökme tohum/ürün iadesi sağlamaz.

## Yeni başlangıç seçeneği ve uzun vadeli ürün

Marul küçük bütçeyle hızlı dönüş, buğday toplu hasat/üretim, balkabağı uzun
bekleme karşılığı yüksek tek hasat için eklendi. Eski üç ürünün dengesi değişmedi.

50 parayla 2 marul + 1 buğday tohumu 24 para tutar; 26 para kalır. Hemen 06:00'da
ekmek/sulamak varsayımıyla 17,5 gerçek dakikada iki maruldan 20 para gelir ve
cüzdan 46 olur. Buğday 27,5 dakikada 3 × 7 = 21 para getirir; cüzdan 67 olur.
İlk 20 dakikada artık mahsul geliri mümkündür. Bu süreler otomatik günün
hesabıdır; yürüyüş/işlem süresi, uyuma ve keşif geliri dahil değildir.

Balkabağı başlangıç 50 paranın 40'ını bağlar ve hemen ekilirse ilk hasadı
77,5 gerçek dakikada gelir. Tek ürün 85 para, tohum sonrası fark 45 paradır.
Pazar süreyi ve hasat verimini gösterir; pahalı tohumu almak zorunlu değildir.

Pazarda **1 marul + 3 buğday + 1 balkabağı + 2 odun → 1 hasat sepeti**.
Satış 145 para. Girdileri ayrı satmak 10 + 21 + 85 = 116 para; iki satın
alınmış odunun maliyeti 4 para kabulüyle işleme 25 para ek değer sağlar.
Bu üç ekimin toplam tohum gideri 58; 145 − 58 − 4 = 83 para net değer.
Sepet depolanabilir/satılabilir, yerleştirilebilir mobilya veya yemek etkisi değildir.

## Uygulama ve kalan değerlendirme

Altı ürün ve iki tarif katalogda. Bahçe yapıları önceki adımda uygulandı.
Marulun iki gecelik takvimi görsel aşamaları 0 → 1 → 3 olarak kullanır;
dört prefab yuvası korunur, olgunluk veya verim kazanmak için aşama sayısına
bağımlılık yoktur. Her ekim tek sulama ister; yalnız domates tekrar hasat verir.

Yeni bitkiler native geçici geometri; pazar tohum paketleri kullanıcının isteğiyle
ImageGen çizimleridir. Nihai bitki GLB teslimleri ve gerçek oyuncuyla uzun
süreli denge değerlendirmesi açık. Bu belge gerçek oyuncu süre ölçümü değildir.
