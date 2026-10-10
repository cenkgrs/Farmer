# Kazma ve kırılabilir kaya — kullanıcı teslimi

10 Ekim 2026: `hammer 3d model.glb` kazma, `rock 3d model.glb` kaya olarak teslim edildi. Dosya adından bağımsız olarak sahnedeki silüet kontrol edildi. Özgün GLB'ler burada değiştirilmeden `pickaxe.glb` ve `stone_outcrop.glb` olarak korunur. Üretim hesabı/lisans belgesi ayrıca teslim edilmedi; bu tur uzaktan üretim veya Tripo API çağrısı yapılmadı.

| Model | Üçgen / vertex | Unity boyutları (X/Y/Z, metre) |
|---|---|---|
| Kazma | 6029 / 4393 | 0,532 / 0,800 / 0,125 |
| Kaya | 5002 / 3628 | 1,200 / 0,665 / 0,773 |

İki model de tek mesh/materyal; kaynakta üç adet 4K gömülü doku var. Çalışma dokuları 1K, URP Lit basecolor/normal/metallic-smoothness. Kaynak baseColorFactor 0,8 korunur. Kaynak hedeflerden biraz yoğun; bu teslimde sadeleştirme yapılmadı. SHA-256 ve gerçek boyutlar `source_manifest.json` içindedir.

`tools/art/prepare_mining.py` Python + numpy + Pillow ile GLB bufferlarını okur; statik, dönüşümsüz düğümleri açıkça doğrular. Sağ/sol el koordinat dönüşümü, winding ve UV dönüşümü uygulanır. Normal ve UV'ler korunur; paket/motor yükseltmesi yok. Ara `*_mesh.json` dosyalarından `Farmer.Editor.MiningArtSetup.Apply` Unity native mesh, materyal ve iki `Resources/ExplorationArt` prefabını üretir. Kaynak JSON yalnız editör dönüşümü içindir; player'a girmez.

Kazma 0,23–0,29 m sap kesitinin merkezinden avuç socket'ine oturur; başın geniş ekseni vuruş düzlemine döndürülür. Kaya taban merkezi y=0 ve en geniş yatay kenarı 1,2 m. Prefablarda collider yok; ResourceView bağımsız basit kutu collider'ı kurar. Dekor granit prefabı ve kullanıcının sahne yerleşimi değiştirilmedi.

İzole gerçek Farm sahnesinde alım/kuşanma/üç vuruş/kaybolma/kayıt kontrolü; model üçgenleri, zemine temas ve sap–socket geometrisi kontrol edilir. Görsel çıktılar `docs/screenshots/mining-delivered.png`. Odaksız batch editörde kol aksiyonu odak korumasıyla durur; bu tur vuruş animasyonunun zaman içindeki hareketini onaylamaz.
