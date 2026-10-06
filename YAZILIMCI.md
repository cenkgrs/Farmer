# Yazılımcı — proje hafızası ve devir

Son güncelleme: **6 Ekim 2026 — Europe/Istanbul**.

## Şu an nerede kaldık?

Tasarım ve depo başlangıcı aşamasındayız. Kullanıcının Unity'de daha deneyimli olduğunu belirtmesi üzerine teknik yön **Unity 6.3 LTS + C# + URP** olarak güncellendi. Oyun motoru kurulumu ve oyun kodu henüz yapılmadı. Aktif hedef **0.1 — çekirdek prototip**.

Kullanıcı tek başına geliştirecek, Linux ve Windows bilgisayarlar arasında çalışacak. Python, C# ve Java deneyimi yüksek; oyun motoru deneyimi sınırlı ancak Unity'de Godot'tan daha deneyimli. Modelleri harici AI araçlarıyla üretebilir; biz prompt ve entegrasyon gereksinimlerini hazırlayacağız.

## Kesinleşen kararlar

- Sabit açılı izometrik kamera ve stilize 3D görsel dil.
- `docs/references/` altındaki iki konseptin sıcak, yumuşak, boyanmış hissi veren tasarım dili kullanıcı tarafından beğenildi. Kamera referansı izometrik görseldir.
- Çiftlik huzurlu; dışarıda basit moblar ve hafif risk bulunur.
- Pazardan çeşitli tohumlar alma, görünür büyüme, bakım, hasat, satış ve üretimde kullanım ana sistemdir.
- Ahşap blokları üst üste/yan yana yerleştirme; çit, kapı, çatı ve iç mekân eşyalarıyla oyuncunun kendi evini yapabilmesi gerekir.
- Sürüm sürüm geliştirme; kapsam `docs/YOL_HARITASI.md` içinde.
- Kaynak kodu, kararlar, asset rehberi ve devir bilgisi Git'te tutulacak.

## Öneriler ve açık kararlar

| Konu | Durum |
|---|---|
| Motor | Kullanıcının deneyimine göre Unity 6.3 LTS + C# + URP yönüne geçildi. |
| Tam editör yaması / paketler | Sabitlenmedi; proje oluşturulmadı. İki bilgisayarda aynı sürüm kullanılacak. |
| Geliştirme sistemleri | Linux ve Windows — kullanıcı tarafından belirtildi. |
| Yayın hedefi | İlk prototip masaüstü PC varsayımı; mağaza/diğer platformlar seçilmedi. |
| Uzak Git deposu | `origin`: `https://github.com/cenkgrs/Farmer.git`. Özel `cenkgrs/Farmer` deposu doğrulandı; ilk push başarılı, `main` dalı `origin/main` izliyor. |
| Eşzamanlı çalışma | Sırayla devir varsayımı; aynı anda çalışılırsa ayrı görev dalları kullanılacak. |
| Donanım | İki bilgisayarın GPU/özellikleri bilinmiyor; render ve performans ayarları ölçümle seçilecek. |
| Asset üretim aracı | Kullanıcı harici AI aracı kullanacak; marka/araç henüz belirtilmedi. |

## Hazırlananlar

- `AGENTS.md`: yeni oturumların başlangıç ve devir kuralları.
- Bu dosya: proje durumu ve sıradaki adımlar.
- Oyun tasarımı, sürüm kabul ölçütleri, motor karşılaştırması, model teslim kuralları ve ilk promptlar.
- İki konsept görselinin depo içindeki kopyaları.
- Git ignore ve platformlar arası satır sonu kuralları.

## Teknik durum ve kontroller

- Başlangıçta kullanılabilir Git deposu yoktu; yerel `main` oluşturuldu.
- Yalnızca repo kimliği `Cenk Gürses <cenkgrs@gmail.com>` olarak ayarlandı. Global Git config değiştirilmedi. Kesin commit ve çalışma ağacı durumu için `git log -3 --oneline` ve `git status --short --branch` kullan.
- Origin doğrulandı; `d3ce1b4` ilk commit'i GitHub main dalına gönderildi. Diğer bilgisayardan klonlanabilir; o bilgisayarda klon/kurulum henüz doğrulanmadı.
- Unity/Unity Hub kurulumu henüz incelenmedi. İlk araştırmada `godot` ve `dotnet` PATH üzerinde bulunmadı; bu Unity kurulum durumu hakkında sonuç vermez.
- Git LFS kurulu değil. Henüz model yok; LFS filtreleri etkinleştirilmedi. İki küçük konsept PNG normal Git'te tutulacak.
- Henüz çalıştırılabilir uygulama olmadığından oyun testi/build sonucu yok.
- Başlangıç belgelerinin yerel bağlantıları, iki PNG dosyasının başlıkları ve depoya özel Git ayarları kontrol edildi; başarılı. `git diff --cached --check` hata vermedi.

## Sıradaki somut işler

1. Unity/Hub kurulumlarını kontrol et; 6.3 LTS ailesindeki tam editör yamasını sabitle, Universal 3D/URP projesi oluştur ve iki sistem için kurulum/çalıştırma adımlarını belgele. Linux dağıtımı/donanımı henüz bilinmiyor.
2. Kullanıcı depoyu gözden geçirdikten sonra kurulum adımına devam et. Diğer bilgisayarda repo klonunu ve aynı Unity sürümünü doğrula. İlk push tamamlandı; sonraki devirlerde upstream durumunu kontrol et.
3. 0.1 için küçük 3D sahne, sabit ortografik kamera, karakter hareketi ve kare seçimi oluştur.
4. Tek ürünün ekim → sulama → görünür büyüme → hasat → satış → yeniden tohum alma döngüsünü ekle.
5. Yer tutucu modellerle davranışı doğrularken görsel örnek setini hazırla. Büyük asset listesi istemeden önce tek modelin stil/ölçek uyumunu sınayarak ilerle.

## Son oturum kaydı

**2026-10-06:** Önceki tasarım konuşması depoya aktarıldı. Kullanıcının Unity deneyimi nedeniyle teknik yön Unity 6.3 LTS/C#/URP oldu. Origin ve yalnızca bu depoya özel Git kimliği ayarlandı. İki konsept ve ilk model promptları kaydedildi. Oyun geliştirmesi henüz başlamadı. Kullanıcının sonraki push isteğiyle özel `cenkgrs/Farmer` deposunun boş olduğu doğrulandı ve `d3ce1b4` main dalına gönderildi. Kullanıcı önce depoyu görmek istediğinden Unity kurulumuna geçilmedi.
