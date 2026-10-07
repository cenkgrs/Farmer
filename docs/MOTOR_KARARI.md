# Motor kararı

Tarih: 6 Ekim 2026. **Teknik yön: Unity 6.3 LTS + C# + Universal Render Pipeline (URP).** Tam sürüm **6000.3.25f1 (e1dba0a9aba4)** olarak sabitlendi. Linux editörü kuruldu; URP şablon kaynakları depoya alındı. 7 Ekimde hesap/lisansın aktif olduğu ve ilk editör importunun başarılı tamamlandığı doğrulandı; güncel build sonucu `YAZILIMCI.md` içinde.

## Gerekçe

Kullanıcının C# deneyimi güçlü; Unity'de Godot'tan daha fazla deneyimi var. Tek geliştiricinin bildiği araçla başlaması bu projede önemli bir avantaj. İlk Godot önerisi bu bilgi gelmeden yapılmıştı; çalışma planı Unity olarak güncellendi.

Sabit ortografik kamera, 3D modüler inşaat, tarım, envanter ve basit savaş Unity'de uygulanabilir. URP, bu stilize 3D kapsam için önerilen render yolu. Konseptteki görünüm modeller, malzemeler, ışık ve kompozisyonla üretilecek; motor tek başına görsel kalite garantisi değildir. [Kamera](https://docs.unity3d.com/Manual/class-Camera.html) · [URP](https://docs.unity3d.com/Manual/urp/urp-introduction.html)

Resmi destek sayfası Unity 6.3 LTS için Aralık 2027'ye kadar destek belirtiyor. Bu yüzden sürüm ailesi olarak 6.3 LTS öneriliyor. [Sürüm desteği](https://unity.com/releases/unity-6/support)

## Linux ve Windows düzeni

- İki makinede Unity Hub üzerinden aynı tam editör yaması ve gerekli build modülleri kullanılacak.
- `ProjectSettings/ProjectVersion.txt`, `Packages/manifest.json` ve `Packages/packages-lock.json` Git'te tutulur. Kilit dosyası gerçek editör importunda üretildi; artık manifestle birlikte sürümleniyor.
- `Assets/`, `Packages/`, `ProjectSettings/` ve `.meta` dosyaları paylaşılacak. `Library/`, `Temp/`, `Logs/` ve `UserSettings/` yerel kalacak.
- Visible Meta Files ve Force Text ayarları kontrol edilecek. Assetler `.meta` dosyalarıyla birlikte taşınacak; GUID değerleri korunacak.
- Unity'nin ürettiği IDE `.csproj`/`.sln` dosyaları yeniden üretilebilir. Haricen elle oluşturulmuş bağımsız .NET test projesi olursa ayrıca sürümlenecek.
- Linux dağıtımı ve GPU desteği kurulumdan önce kontrol edilecek. [Unity 6.3 sistem gereksinimleri](https://docs.unity3d.com/6000.3/Documentation/Manual/system-requirements.html)

## Model aktarımı

AI aracından öncelik FBX + doku dosyaları. Unity belgeleri üretim importu için FBX'i öneriyor. Yalnızca GLB verilebiliyorsa kaynak korunur; kontrollü dönüşüm veya uygun glTF import paketi proje kurulurken değerlendirilir. Varsayılan Unity projesinin GLB'yi doğrudan okuyabildiği kabul edilmez. [Model biçimleri](https://docs.unity3d.com/6000.3/Documentation/Manual/3D-formats.html)

## Kurulum sırasında tamamlanacaklar

1. Mevcut Unity/Hub kurulumunu ve platform gereksinimlerini kontrol et.
2. Kararlı 6.3 LTS yamasını seç ve Universal 3D/URP projesi oluştur.
3. Editör sürümünü, paketleri ve serileştirme ayarlarını doğrula.
4. Boş sahneyi aç/çalıştır ve kullanılacak build hedefini dene.
5. README'ye gerçek, denenmiş komut ve adımları ekle; ardından 0.1'e başla.

Unity Personal uygunluğu gelir/fonlama ve kullanım durumuna bağlıdır; güncel şartlar kurulumda incelenir. [Resmi planlar](https://unity.com/products)
