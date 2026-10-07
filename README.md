# Farmer

Çalışma adı: **Farmer**. Sabit açılı izometrik kamerayla oynanan, stilize 3D çiftçilik, üs kurma ve hafif keşif/savaş oyunu.

**Unity 6000.3.25f1 LTS + C# + URP** kurulumu doğrulandı. İlk import, C# derlemesi, izometrik başlangıç sahnesi ve Linux geliştirme build'i başarılı. Build çalıştırılıp gerçek ekran görüntüsü incelendi. Mevcut sahne zemin/kamera/tarla temelidir; karakter ve oynanabilir çiftçilik henüz yok.

## Başlangıç

- [Unity kurulumu ve açılış adımları](docs/KURULUM.md)

- [Yazılımcı ve oturum devir notu](YAZILIMCI.md)
- [Oturum çalışma talimatları](AGENTS.md)
- [Oyun tasarımı](docs/OYUN_TASARIMI.md)
- [Sürüm planı](docs/YOL_HARITASI.md)
- [Motor değerlendirmesi](docs/MOTOR_KARARI.md)
- [Görsel dil ve model promptları](docs/VARLIK_REHBERI.md)

## Görsel hedef

![Seçilen izometrik kamera ve onaylanan görsel dil](docs/references/farm_isometric_concept.png)

Bu görsel konsepttir; çalışan oyunun ekran görüntüsü değildir. Üçüncü şahıs konsepti de [görsel referans](docs/references/farm_third_person_concept.png) olarak saklanır; kamera kararı izometriktir.

## İki bilgisayarda çalışma

Diğer bilgisayarda [cenkgrs/Farmer](https://github.com/cenkgrs/Farmer) deposunu klonla ve proje kökünü aç. Sonraki oturumlarda temiz çalışma ağacında ilgili dalı `git pull --ff-only` ile güncelle. Codex proje talimatları `AGENTS.md` içindedir; kısa bir “Kaldığımız yerden devam et” isteğiyle proje notları okunarak devam edilebilir.

Normal devirde kodla birlikte devir notları da commit/push edilir. **İlk push 6 Ekim 2026 tarihinde kullanıcının isteğiyle tamamlandı; `main` dalı `origin/main` izliyor.** Diğer bilgisayar yalnızca uzak depoya gönderilmiş dosya ve commitleri alır; yerel sohbet, kurulu motor ve kaydedilmemiş işler Git ile aktarılmaz.

`origin`: `https://github.com/cenkgrs/Farmer.git`. Remote erişimi doğrulandı; özel `cenkgrs/Farmer` deposu ilk push öncesinde boştu. Yalnızca bu depoda Git kimliği `Cenk Gürses <cenkgrs@gmail.com>` olarak ayarlandı. Proje açılış ve lisans sonrası doğrulama adımları kurulum rehberinde.

[AGENTS.md talimatlarının keşfi — resmi belge](https://learn.chatgpt.com/docs/agent-configuration/agents-md)
