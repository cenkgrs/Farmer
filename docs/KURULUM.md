# Unity kurulumu ve iki bilgisayarda çalışma

## Durum

**Editör kuruldu; proje ilk import/çalıştırma doğrulaması aktif Unity lisansını bekliyor.**

- Unity Editor: **6000.3.25f1 LTS** — revision `e1dba0a9aba4`.
- Dil: C#; render yolu: URP.
- Resmi editörle gelen URP şablon kaynakları `Assets`, `Packages` ve `ProjectSettings` altında.
- Manifest editörle uyumlu paketlere sabitlendi. Şablon kilidi manifestle tutarsız olduğu için kaldırıldı; `packages-lock.json` ilk başarılı importtan sonra üretilip commit edilecek.
- Başlangıç sahnesini hazırlayan `Assets/_Farmer/Editor/ProjectSetup.cs` yazıldı; henüz derlenmedi/çalıştırılmadı.

[Resmi editör sürüm sayfası](https://unity.com/releases/editor/whats-new/6000.3.25f1)

Linux Hub girişinde **Signing in** ekranı takılı kaldı; neden ve çözüm doğrulanmadı. Kullanıcı diğer bilgisayarda devam etmeyi seçti. Sonraki oturumda aşağıdaki Windows adımları ve `YAZILIMCI.md` devir notu esas alınmalı.

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
7. `Assets/_Farmer/Scenes/Farm.unity` sahnesini aç ve Play'e bas. Bu aşama yalnızca zemin/kamera kurulum sahnesidir; oynanabilir tarım henüz yoktur.

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

Lisans etkinleştirildikten sonra editörü kapatıp depo kökünde:

```bash
UNITY_EDITOR="$HOME/Unity/Hub/Editor/6000.3.25f1/Editor/Unity"
"$UNITY_EDITOR" -batchmode -nographics -quit -projectPath "$PWD" \
  -executeMethod Farmer.Editor.ProjectSetup.CreateInitialScene \
  -logFile /tmp/farmer-setup.log
```

İlk sahne zaten varsa `CreateInitialScene` yerine `ValidateProject` kullan. Başlangıç sahnesini tekrar üretme. Aracın başarı işaretleri `FARMER_SETUP_OK` ve `FARMER_VALIDATION_OK`.

Linux geliştirme build'i:

```bash
"$UNITY_EDITOR" -batchmode -quit -projectPath "$PWD" \
  -executeMethod Farmer.Editor.ProjectSetup.BuildLinux \
  -logFile /tmp/farmer-build.log
```

Çıktı: `builds/Linux/Farmer.x86_64`. Build komutu henüz doğrulanmadı. Güncel deneme sonuçları `YAZILIMCI.md` içinde tutulur.

## Git düzeni

İki bilgisayarda aynı tam editör sürümünü kullan. `Assets`, `Packages`, `ProjectSettings` ve `.meta` dosyalarını sürümle; `Library`, `Temp`, `Logs`, `UserSettings` ve `builds` yerel kalır. İlk importun her makinede tekrarlanması normaldir.

Devirde editörü kapat, çalışma ağacını kontrol et, notlarla birlikte commit/push yap. Diğer bilgisayarda temiz çalışma ağacıyla `git pull --ff-only` çalıştır.
