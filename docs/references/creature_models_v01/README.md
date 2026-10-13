# Karakter ve hayvan model referansları — 13 Ekim 2026

Yerleşik ImageGen ile üretildi; prompt seti `prompts.json`, ayrı yön üretimleri `separate_view_prompts.json` içinde. Bunlar 3D model değil, üretim eskizleridir. Defne mevcut diyalog portresinin kıyafet ve yüzünü temel alır. Kullanıcı isteğiyle Defne/geyik ön, arka, sağ ve sol görünüşleri ayrı PNG olarak da hazırlanmıştır.

| Model | Ön | Arka | Sağ | Sol |
|---|---|---|---|---|
| Defne | [PNG](defne_front.png) | [PNG](defne_back.png) | [PNG](defne_right.png) | [PNG](defne_left.png) |
| Geyik | [PNG](deer_front.png) | [PNG](deer_back.png) | [PNG](deer_right.png) | [PNG](deer_left.png) |

Diğer eskizler: [tavşan](rabbit.png), [orman slime](slime.png), [taş böceği](stone_beetle.png). Birleşik tablolar: [Defne](defne.png), [geyik](deer.png).

## Model üretim isteği

Tek karakter/model üret; referansın dört görünüşünü dört ayrı model olarak üretme. Yumuşak stilize formlar, boyanmış doğal renkler, temiz UV ve nötr ışık. Zemin, dekor, yazı, kaide veya ışıkla sabitlenmiş gölge ekleme. Defne yaklaşık 1,70 m; geyik omuz 1,10 m; tavşan kulak ucu 0,65 m; slime 0,80 m; taş böceği 0,65 m yüksekliğinde. Eskiz ölçü çizgileri teknik CAD ölçüsü değildir; bu metindeki ölçüler esas alınır.

Tercih FBX + dokular; GLB de kabul. Taban merkezli pivot, metre ölçeği, +Y yukarı. Hedef: karakter 10–25 bin, hayvan 5–15 bin, slime 3–10 bin üçgen; 1–2 malzeme, 1K/2K base color ve normal. Bunlar üretim hedefi; teslimde gerçek değerler kontrol edilir.

Defne için Humanoid rig, A/T pose, idle/walk/talk; geyik ve tavşan için dört ayaklı rig, idle/walk/run/hit/death klipleri istenir. Böcekte bacaklar ayrı kemikler; slime için squash/stretch destekleyen basit rig veya blend shape. Model tek parça/rigsiz gelirse görsel olarak bağlanabilir fakat eklem animasyonları hazır sayılmaz. Defne'nin örgüsü ve kesesi sağ tarafta kalmalı; yan görünüşler nedeniyle aynalanmamalı. Eskizler arasında küçük üretim farklarında ön görünüş kimlik/kıyafet için esas alınır.
