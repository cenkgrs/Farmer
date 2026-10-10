# Köy pazarı — arayüz eskizi v01

![Pazar arayüzü önerisi](market_ui_v01.png)

**Durum: kullanıcı 11 Ekim 2026 tarihinde tasarımı onayladı; oyuna aktarıldı.**
[Unity oyun içi görüntüsü](../../screenshots/market-ui-game.png) ve
[uygulama/asset kaydı](../../art-sources/market_ui_v01/README.md).

Ahşap çerçeve ve krem paneller; solda kategori seçimi, ortada ürün kartları,
sağda seçilen ürünün açıklaması/adedi/satın alma işlemi, altta satış bölümü.
Tohum, alet, malzeme, mobilya ve üretim mevcut oyun kapsamını temsil eder.
Görseldeki cüzdan ve çanta adetleri örnektir; satış şeridindeki sayılar fiyat
değildir. Oyun ekonomisi değişmedi. Adet seçici ve yeni düzen artık çalışır; tohum ve tarifler çoklu, alet/odun paketi/mobilyalar birer işlemle alınır.

Yerleşik ImageGen ile üretildi. Referanslar: `docs/screenshots/crop-market.png`
ve `docs/references/farm_isometric_concept.png`. Kullanılan tam [prompt](prompt.txt)
saklandı. Türkçe başlıklar, fiyatlar ve öğe düzeni görsel olarak incelendi.
Özgün PNG artık çalışma dokusu olarak da kullanılır; değişen alanlar ayrı UI öğeleridir.

Bu eskizden bağımsız uygulanan davranış: pazara yaklaşınca yalnız F ipucu
görünür, F paneli açar; F/Esc/× kapatır. Genel bağlamsal etkileşim tuşu F'dir;
kapı/sandık da F kullanır. Panel açıkken hareket ve dünya eylemleri engellenir.
