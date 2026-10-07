# Unity kurulumu ve iki bilgisayarda çalışma

## Durum

**Unity lisansı aktif; ilk import, C# derlemesi, başlangıç sahnesi, Linux build ve gerçek player görüntüsü 7 Ekim 2026 tarihinde doğrulandı.**

- Unity Editor: **6000.3.25f1 LTS** — revision `e1dba0a9aba4`.
- Dil: C#; render yolu: URP.
- Resmi editörle gelen URP şablon kaynakları `Assets`, `Packages` ve `ProjectSettings` altında.
- Manifest editörle uyumlu paketlere sabitlendi. Şablon kilidi manifestle tutarsız olduğu için kaldırılmıştı; `packages-lock.json` artık gerçek editör importuyla üretildi ve sürümleniyor.
- Başlangıç sahnesini hazırlayan `Assets/_Farmer/Editor/ProjectSetup.cs` derlendi ve çalıştırıldı. `Assets/_Farmer/Scenes/Farm.unity` depoda mevcut; tekrar üretme.

[Resmi editör sürüm sayfası](https://unity.com/releases/editor/whats-new/6000.3.25f1)

6 Ekim tarihindeki Linux Hub **Signing in** beklemesi sonrasında hesap/lisans etkinleştiği 7 Ekimde doğrulandı. Kullanıcı diğer bilgisayarda işlem yapmadı; geliştirme bu Linux bilgisayarda devam ediyor. Giriş sorununun kök nedeni belirlenmiş değildir.

## Windows

1. [Unity Hub](https://unity.com/download) kur, hesabınla giriş yap ve hesabına uygun lisansı etkinleştir.
2. **6000.3.25f1** editörünü yükle. Başlangıç için masaüstü hedefi yeterli.
3. Depoyu klonla ve bu depoya özel Git kimliğini ayarla:

```powershell
git clone https://github.com/cenkgrs/Farmer.git
cd Farmer
git config --local user.name "Cenk Gürses"
git config --local user.email "cenkgrs@gmail.com"
```

4. Unity Hub → Projects → Add/Open from disk ile deponun kökünü seç. Mevcut deponun üzerine yeni şablon oluşturma.
5. İlk import ve paket çözümlemesini bekle. Lisans/derleme hatası varsa önce onu çöz.
6. `Farm.unity` yoksa Farmer → Create Initial Scene menüsünü bir kez çalıştır. Araç mevcut Farm sahnesinin üzerine yazmaz.
7. `Assets/_Farmer/Scenes/Farm.unity` sahnesini aç ve Play'e bas. Sahnede WASD/oklarla hareket, fareyle tıklamadan kare hedefleme vardır. 1/2/3 ile tohum/sulama kabı/orak kuşan, tohum/orak için hedeflenen yakındaki kareye sol tıkla; sulama kabında sol tuşu basılı tutup fareyi gezdir. Pazar yakınında B/V ile alış/satış, kampta N ile yeni gün kullanılabilir.

Windows adımları diğer makinede henüz denenmedi.

## Linux

Unity Hub **3.22.2** açılışı doğrulandı. Bu makinede AppImage için `--appimage-extract-and-run` gerekiyor; uygulama menüsüne bu yöntemle çalışan Unity Hub kısayolu eklendi. Standart bağlama yöntemi pencere açmadan takıldı.

Doğrulanan sistem: Ubuntu 24.04 x86_64, X11, RTX 4090, NVIDIA 595.91.07 ve yaklaşık 94 GiB RAM. [Resmi gereksinimler](https://docs.unity3d.com/6000.3/Documentation/Manual/system-requirements.html)

Unity CLI **1.0.0-beta.12** resmi kurucuyla kuruldu. Bu deneysel yönetim aracı proje bağımlılığı değildir; Windows'ta Hub kullanmak yeterlidir. [CLI belgesi](https://docs.unity.com/en-us/unity-cli/use-unity-cli)

Bu makinede editör yolu:

```text
~/Unity/Hub/Editor/6000.3.25f1/Editor/Unity
```

CLI kuruluysa hesabı ve projeyi yönetmek için:

```bash
unity auth login
unity license status
unity open .
```

Lisansı hesap koşullarına göre etkinleştir. Parola/aktivasyon dosyası/token Git'e girmez. Hesap girişi insan tarafından tarayıcıda tamamlanır.

## Doğrulama ve ilk sahne

Depodaki sahneyi doğrulamak için editörü kapatıp depo kökünde:

```bash
UNITY_EDITOR="$HOME/Unity/Hub/Editor/6000.3.25f1/Editor/Unity"
"$UNITY_EDITOR" -batchmode -nographics -quit -projectPath "$PWD" \
  -executeMethod Farmer.Editor.ProjectSetup.ValidateProject \
  -logFile /tmp/farmer-setup.log
```

Başarı işareti `FARMER_VALIDATION_OK`. `CreateInitialScene` yalnızca Farm sahnesinin hiç bulunmadığı ilk kurulum içindir; mevcut sahneyi yeniden üretme.

Linux geliştirme build'i:

```bash
"$UNITY_EDITOR" -batchmode -quit -projectPath "$PWD" \
  -executeMethod Farmer.Editor.ProjectSetup.BuildLinux \
  -logFile /tmp/farmer-build.log
```

Çıktı: `builds/Linux/Farmer.x86_64`. Build komutu Linux üzerinde başarıyla doğrulandı. Güncel deneme sonuçları `YAZILIMCI.md` içinde tutulur.

## Geliştirme build'inde görsel kontrol

Normal pencere açılışı için `./builds/Linux/Farmer.x86_64` çalıştır. Otomatik ekran görüntüsü ve kısa çalıştırma kontrolü için daha önce kullanılmamış bir çıktı yolu seç:

```bash
./builds/Linux/Farmer.x86_64 -screen-fullscreen 0 -screen-width 1280 -screen-height 720 \
  --farmer-smoke-capture builds/QA/farm-setup.png -logFile /tmp/farmer-player.log
```

Bu kontrol grafik oturumu gerektirir; `-nographics` veya `-batchmode` kullanma. Geliştirme build'i birkaç saniye render aldıktan sonra PNG üretir ve kapanır. Başarıda çıkış kodu 0 ve günlükte `FARMER_PLAYER_SMOKE_OK` bulunur; görüntüyü ayrıca görsel olarak incele. Mevcut PNG üzerine yazmaz. `builds/` Git'e girmez. Normal çalıştırmada kontrol aracı etkinleşmez; yayın build'inde kontrol kodu derlenmez.

## Git düzeni

İki bilgisayarda aynı tam editör sürümünü kullan. `Assets`, `Packages`, `ProjectSettings` ve `.meta` dosyalarını sürümle; `Library`, `Temp`, `Logs`, `UserSettings` ve `builds` yerel kalır. İlk importun her makinede tekrarlanması normaldir.

Devirde editörü kapat, çalışma ağacını kontrol et, notlarla birlikte commit/push yap. Diğer bilgisayarda temiz çalışma ağacıyla `git pull --ff-only` çalıştır.

## Arkadaşa gönderilecek Windows paketi

Unity 6000.3.25f1 için **Windows Build Support (Mono)** modülü gerekir.
Linux'ta mevcut Unity CLI ile `unity install-modules -e 6000.3.25f1 -a x86_64 -m windows-mono` kurulabilir.
Editör kapalıyken depo kökünde:

```bash
"$UNITY_EDITOR" -batchmode -nographics -quit -job-worker-count 4 -buildTarget Win64 -projectPath "$PWD" \
  -executeMethod Farmer.Editor.ProjectSetup.BuildWindows \
  -logFile "$PWD/Logs/windows-playtest-build.log"
python3 tools/build/package_windows.py
```

Unity çıkış kodu 0 ve `FARMER_WINDOWS_BUILD_OK` görülmeden paketleme yapma.
`BuildWindows`, mevcut Mono ayarını doğrular ve x64 release build üretir;
oyun içi geliştirme testleri dahil edilmez. StrictMode ve hata sayısı kontrolü açıktır. Çıktı `builds/Windows/Farmer.exe`.
Aynı araç editörde **Farmer → Build Windows Playtest** menüsündedir.
Linux build'e dönmek için batch komutuna `-buildTarget Linux64` ekle.

Paketleyici gerekli runtime dosyalarını ve EXE'nin Windows x64 başlığını kontrol eder,
oyun dosyalarını `BENI_OKU.txt` ile ZIP'e koyar, arşiv CRC kontrolü ve SHA-256 üretir.
Çıktı `builds/Releases/Farmer-0.1.0-Windows-x64.zip`; dosya varsa üzerine yazmaz,
yeni paket için `--name Farmer-0.1.0-Windows-x64-r2` gibi bir ad ver.
`builds/` Git dışında kalır. Paketleme tek başına oyunun Windows'ta çalıştığını doğrulamaz.

Arkadaşına ZIP'in tamamını gönder. ZIP tamamen çıkarıldıktan sonra `Farmer.exe`
açılır; Unity kurulumu gerekmez. EXE, `Farmer_Data`, `MonoBleedingEdge` ve DLL'ler
birlikte kalmalıdır. Kontroller ve sorun bildirimi bilgileri paketteki rehberdedir.

## İlk inşa denemesi

Güncel Linux build'inde `4` ile inşa moduna gir; tarlanın sağındaki çizgili alana yaklaş. Sol tık yerleştirir, `R` döndürür, tekerlek yüksekliği seçer, sağ tık bloğu söker. `Esc` veya `1/2/3` ile tarıma dön. Başlangıç 24 odun, blok 2 odun; pazarın yeni düğmesinden 20 paraya 10 odun alınır. Üst blok için alt destek gerekir. Mevcut Windows ZIP bu değişikliği içermez; güncel kaynakla yeniden build/paket gerekir.
