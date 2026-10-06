# Farmer — çalışma talimatları

## Her oturumun başlangıcı

1. Önce `YAZILIMCI.md` oku: mevcut durum, doğrulamalar ve sıradaki işler burada.
2. `docs/OYUN_TASARIMI.md` ve `docs/YOL_HARITASI.md` içindeki ilgili bölümleri oku.
3. Motor/kurulum işi için `docs/MOTOR_KARARI.md`; model/görsel işi için `docs/VARLIK_REHBERI.md` oku.
4. `git status --short --branch`, `git remote -v` ve varsa son commitleri kontrol et. Yerel dosyalar ve gerçek Git durumu, eski durum notlarından daha günceldir.
5. Kullanıcıya daha önce kaydedilmiş kararları tekrar anlattırma. Kesinleşmiş kararlarla önerileri ayır.

## Projenin sınırları

- Kullanıcı tek geliştirici; kodlama deneyimi yüksek (Python, C#, Java), motor deneyimi sınırlı ama Unity'de Godot'tan daha deneyimli. Linux ve Windows kullanıyor.
- Türkçe iletişim kur. Kod tanımlayıcılarında ve asset dosya adlarında İngilizce/ASCII kullan.
- Sabit açılı izometrik kamera, stilize 3D, huzurlu çiftlik ve dışarıda basit düşmanlar temel kararlardır.
- Çiftçilik ve oyuncunun parça parça inşa ettiği üs eşit derecede önemlidir.
- İlk hedef 0.1'dir; 0.5 ilk demo. Sonraki sürümlerin özelliklerini mevcut kapsamın içine sessizce ekleme.
- Teknik yön Unity 6.3 LTS + C# + URP. Tam editör yaması henüz sabitlenmedi; `docs/MOTOR_KARARI.md` durumunu kontrol et.
- Kullanıcı 3D modelleri harici AI aracıyla üretebilir. Gereken asset için ölçü, stil, çıktı ve kontrol listesiyle prompt hazırla. Teslim edilen modeli doğrulamadan oyun için hazır sayma.

## Uygulama ve doğrulama

- Küçük, oynanabilir adımlar üret. Yeni ürün veya tarif eklemek için mevcut kodu kopyalamak yerine veri tanımları kullan.
- Ürün, envanter ve üretim kurallarını kamera/görsel sunum kodundan ayır; başlangıçta gereksiz framework kurma.
- Makineye özel mutlak yolları, motor önbelleğini, kişisel ayarları ve sırları sürüm kontrolüne alma.
- Unity editör yamasını `ProjectSettings/ProjectVersion.txt`, paketleri `Packages/manifest.json` ve `Packages/packages-lock.json` ile sabitle; bir makinede kendiliğinden yükseltme. Bunlar proje oluşturulunca üretilecek.
- Unity `.meta` dosyalarını assetleriyle birlikte sürümle; GUID değerlerini koru. Sahne/prefab serileştirmesi Force Text, sürüm kontrol modu Visible Meta Files olmalı.
- Anlamlı mantık değişikliklerini uygun testlerle; kamera, yerleşim ve görselleri oyun içinde doğrula. Çalıştırılmamış bir kontrolü geçti diye yazma.
- Mevcut kullanıcı değişikliklerini koru. Yıkıcı Git komutları ve force push kullanma.

## İki bilgisayar arasında devir

- Remote varsa ve çalışma ağacı temizse oturum başında fetch yap; ilgili upstream'den yalnızca fast-forward ile güncelle. Ayrışma varsa değişiklikleri inceleyip koruyarak çöz.
- Tamamlanmış, incelenmiş işleri küçük commitlere kaydet. Kullanıcı 6 Ekim 2026 tarihinde ilk push'u istedi; ilk yerel hazırlık kısıtı bu adım için kalktı. Yetkili eşitleme işlerinde `origin` ve hedef dalı doğrula, push sonucunu raporla. İki bilgisayardaki devirde çalışma ağacı ve upstream durumunu kontrol et.
- Bu depoya özel Git kimliği: `Cenk Gürses <cenkgrs@gmail.com>`. Global Git ayarlarını değiştirme.
- Remote adresi olmadan yayınlanmış/eşitlenmiş olduğunu söyleme. Eşzamanlı iki oturum varsa ayrı görev dalları ve ayrı dosya sorumlulukları kullan; ortak bir dosyada paralel yazma.
- Oturum sonunda `YAZILIMCI.md` içindeki durum, kontroller, eksikler ve sonraki somut işi güncelle. Tasarım değiştiyse ilgili dokümanı da aynı committe güncelle.
- Tamamlanma notunda commit, dal ve push durumunu belirt. Ayrıntılı sohbet geçmişi yerine bu depo devir kaynağıdır.

<!-- postman-cli:installation:start -->
Postman CLI is installed on this device, and can be used for API Engineering work.
<!-- postman-cli:installation:end -->
