# Görev ve NPC konuşması hazırlama

Görev içeriği C# dosyalarında değil, Unity Project penceresindeki assetlerde düzenlenir. İlk sistem ürün/eşya teslim görevlerini destekler; düşman öldürme, belli yere gitme gibi farklı hedef türleri henüz yoktur.

## Hazır örnekler

- `Assets/_Farmer/Resources/Npcs/huseyin.asset`: üç görevlik pazar zinciri.
- `Assets/_Farmer/Resources/Npcs/defne.asset`: bağımsız Akşam Sofrası görevi.
- `Assets/_Farmer/Data/Quests`: dört görevin hedef, ödül ve konuşma dosyaları.

Bir asseti seç; sağdaki Inspector alanlarını düzenle. `Ctrl/Cmd+S` ile kaydet. Runtime değişiklikleri Play modunu yeniden açınca yüklenir; Play sırasında yapılan asset değişiklikleri kalıcı olabildiğinden normal düzenleme modunu kullan.

## Yeni görev

1. `Assets/_Farmer/Data/Quests` klasöründe sağ tık → Create → Farmer → Quest.
2. `Id`: benzersiz, kalıcı ASCII kimlik; örneğin `defne_forest_supplies`. Kayıtlar bu kimliği kullanır; yayımlanan kimliği değiştirme veya başka görev için tekrar kullanma.
3. `Title` ve `Description`: Q ekranındaki ad/açıklama.
4. `Required Items`: teslim edilecek eşyalar. Her satırın `Item Id`, oyuncuya gösterilen `Display Name` ve `Count` alanını doldur.
5. `Reward Money`: para ödülü. `Reward Items`: sıfır veya daha fazla eşya ödülü. Para ve eşya birlikte verilebilir.
6. Konuşmaları aşağıdaki beş aşamada yaz. Her aşamaya istediğin sayıda satır ekleyebilirsin.
7. Görevi ilgili NPC'nin `Quests` listesine sürükle. Listede yukarıdan aşağıya sıra belirlenir; önceki görev teslim edilmeden sonraki alınamaz. Aynı görev assetini iki NPC'ye bağlama; ayrı kimlikle kopyasını oluştur.

### Konuşma aşamaları

| Alan | Ne zaman gösterilir? |
|---|---|
| Offer | Görev henüz alınmamışken |
| Accepted | Kabul et düğmesinden sonra |
| In Progress | Aktif görevde ürünler eksikken |
| Ready To Deliver | Aktif görevde çantada yeterli ürün varken |
| Completed | Teslim ve ödül işlemi başarıyla tamamlanınca |

Her satırda `Player` kapalıysa NPC'nin adı/portresi, açıksa oyuncu portresi ve **Sen** gösterilir. Replikleri istediğin sırada karıştırabilirsin; zorunlu NPC/oyuncu dönüşümü yoktur. `Text` alanını doldur. Uzun replikler otomatik sayfalara bölünür. 1–3 kısa cümlelik satırlar daha okunaklıdır.

Örnek Offer:

- Player kapalı: “Bana üç havuç getirir misin? Karşılığında 50 para vereceğim.”
- Player açık: “Elimdeki havuçları kullanabilirim.”
- Player kapalı: “Elbette, hazır olduğunda bana getir.”

Konuşma içinde yazılan miktarları/ödülleri elle güncelle; işlemin gerçek miktarı `Required Items`, `Reward Money` ve `Reward Items` alanlarından alınır. Boş konuşma aşamalarında kısa yedek metin kullanılır.

## NPC dosyası ve sahnedeki karakter

1. `Assets/_Farmer/Resources/Npcs` altında Create → Farmer → NPC.
2. `Id`, `Display Name`, `Portrait Id` alanlarını doldur. Portre, `Assets/_Farmer/Resources/VillageArt` altındaki görselin uzantısız adıdır.
3. `Greeting`: karakterle ilk konuşulduğunda oynayan ortak giriş replikleri. Göreve özel metinler Quest assetindedir.
4. `Quests`: bu NPC'nin sıralı görev listesi. Başka NPC'nin ilerlemesi bu listeyi etkilemez.
5. Sahnedeki karakterin köküne **NpcActor** componenti ekle ve `Definition` alanına NPC assetini sürükle. Karakterde bir collider bulunmalı. Yakında F veya karaktere tık ile konuşulur.
6. `Opens Market` yalnızca mevcut pazarın esnafı Hüseyin'de açık olmalı; yeni bağımsız karakterlerde kapalı bırak. Çoklu pazar sistemi bu işin kapsamında değil.

Hüseyin ve Defne mevcut runtime modellerine otomatik bağlanır. Yeni karakterleri sahneye normal şekilde yerleştirebilirsin; yeni C# sınıfı gerekmiyor.

## Kullanılabilir eşya kimlikleri

| Tür | Örnekler |
|---|---|
| Hasat | `crop:turnip`, `crop:carrot`, `crop:tomato`, `crop:lettuce`, `crop:wheat`, `crop:pumpkin` |
| Tohum | Aynı ürünler için `seed:turnip`, `seed:carrot` vb. |
| Üretim | `crafted:vegetable_crate`, `crafted:harvest_basket` |
| Kaynak | `wood`, `stone` |
| Av | `hunt:raw_meat`, `hunt:cooked_meat`, `hunt:hide` |
| Mobilya | `furniture:home_stove`, `furniture:home_chest`, `furniture:home_table` vb. |

Kalıcı aletler teslim/ödül listesinde desteklenmez. Sayılar 1–999, para ödülü 0 veya pozitif olmalı. Aynı eşya kimliğini bir hedef/ödül listesinde iki kez yazma; miktarını tek satırda artır.

## Kontrol ve kayıt davranışı

- Üst menü: **Farmer → Validate Quest Content**. Hatalı kimlik, eksik eşya/portre, mükerrer görev gibi sorunları Console'da kontrol et.
- Q: görevler; Q/Esc: kapat. İnşa parçası değiştirme eski Q yerine E oldu.
- Hüseyin/Defne ile konuş → **Bir iş var mı?** → teklifin sonuna gel → **Kabul et**. Alışveriş ayrı seçenek olarak kalır.
- Çantada önceden bulunan eşyalar sayılır. Sandıktaki eşyalar sayılmaz. Satılan veya kullanılan ürünler ilerlemeden düşer.
- Teslim sadece ilgili NPC yanında, konuşma seçeneğiyle yapılır. Eşyalar tek seferde düşer, ödül tek seferde verilir. Ödül kapasitesi doluysa işlem yapılmaz.
- Kabul/tamamlanma v14 kayıtta korunur. Eski kayıtlar görev alınmamış olarak yükseltilir; ilk yazımda `.pre-v14` yedeği oluşur.
- Sıra veya içerik değişikliği, daha önce kabul edilmiş/tamamlanmış görevi sıfırlamaz. Emekli edilen kimlikler kayıtta tutulur. Yeni sıralamayı sıfır ilerlemeyle denemek için ayrı bir test kaydı kullan; oyuncunun normal kaydını silme.
