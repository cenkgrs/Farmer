# Pazar arayüzü — uygulama varlıkları

Kullanıcı 11 Ekim 2026 tarihinde [eskizi](../../references/market_ui_v01/README.md)
onayladı ve birebir aktarılmasını istedi. `MarketHud` yerleşimi özgün 1672×941
koordinatlarında kurar. `Resources/MarketArt/approved.png` referans PNG'nin
byte düzeyinde aynı kopyasıdır. Çerçeve, başlık, düğmeler, tohum ve satış çizimleri
bu görselden UV bölgeleriyle kullanılır; yeni bir benzer çerçeve çizilmedi.

`clean.png` yerleşik ImageGen ile yalnız değişen metin/öğe alanlarının altını
hazırlamak için üretildi. [Tam prompt](clean-prompt.txt) ve kaynak referans depoda.
Özgün resim değiştirilmedi. İki doku 1672×941, mipmapsız, sıkıştırmasız ve NPOT
ölçeklemesi kapalı içe aktarılır; `MarketArtImport` bu ayarları korur.

Para, adet, seçili ürün bilgisi, toplam, tarif girdileri ve satış miktarları gerçek
modelden çizilir. Canlı metinler Liberation Serif Regular/Bold kullanır; font
rasterizasyonu ve değişen UI durumları nedeniyle tüm ekranın her pikseli aynı
olduğu iddia edilmez. Kaynak PNG tek katmandır, özgün font dosyası içermez.
Statik tohum fiyatı çizimi veriyle eşleşmediğinde canlı fiyatla değiştirilir.

Fontlar Liberation Serif 2.x, SIL OFL 1.1; değiştirilmeden uygulamayla dağıtılır.
[Lisans ve telif bildirimi](FONT_LICENSE.txt) depoda. Kaynak paket LibreOffice'un
bundled Liberation fontlarıdır. Font verisi Unity importer'da uygulamaya gömülür;
oyuncunun işletim sistemi fontlarına bağımlı değildir.

Ekran oranı korunur; 4:3 gibi oranlarda çevrede koyu boşluk bırakılır. Pazarın
arkası eskizdeki sabit, boyanmış köy illüstrasyonudur; canlı dünya blur efekti
değildir. Oyun saati pazar açıkken önceki davranışla ilerler. Kazma/mobilya
kategorileri mevcut oyun ikonlarını kullanır; boş katalog yerleri yeni ürün
uydurmaz. Ürün/tarifler veri tanımlarından, mobilyalar mevcut inşa kataloğundan
gelmeye devam eder. Tohum/üretim adetli, diğer işlemler tek alet/eşya/pakettir.

Görsel kontrol: referans çözünürlük, 1280×720, 1920×1080 ve 1024×768 Unity
Canvas renderları; pencerenin ekran içinde kalması ve en-boy oranı ayrıca test
edilir. Örnek [oyun görüntüsü](../../screenshots/market-ui-game.png).
