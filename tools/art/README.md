# Yerel art araçları

Kaynaklar ve tekrar üretim: `../../ArtSource/tripo_v01/README.md`.

`prepare_models.py` Python + Pillow ister; `convert_props.py` Blender 4.5 LTS içinde çalışır. Bunlar model üretimi yapmaz, teslim edilen dosyaları Unity için dönüştürür.

`run_player_checks.py` Linux'ta ayrı bir Xvfb ekranında gerçek player'ı çalıştırır. `xwininfo` ve Xvfb gerekir. Masaüstü penceresini/odağını değiştirmez. Test yalnızca izole FarmerQA kaydını kullanır. Geçici X sunucusunu sonunda kapatır; 150 saniyelik üst sınır vardır.

```bash
python3 tools/art/run_player_checks.py --name art-check-unique
# Xvfb PATH içinde değilse: --xvfb /yerel/arac/yolu/Xvfb
```

Her denemede yeni bir `--name` ver. Kontrol günlüğü `Logs/<name>.log`, genel ve yakın plan görüntüleri `builds/QA/<name>*.png` olur. Yazılım OpenGL ile görsel/giriş kontrolüdür; normal masaüstündeki GPU performans ölçümü değildir.
