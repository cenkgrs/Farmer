# Geyik yürüyüşü

13 Ekim 2026. Orijinal model değişmeden çalışma anında 9 kemikli skin oluşturulur: gövde + dört üst/alt bacak çifti. Unity SkinnedMeshRenderer ve iki eklemli ayak hedef çözümü kullanılır. Dört zamanlı yürüyüş, kaçışta çapraz bacak çiftlerine geçer. Hız CharacterController'ın gerçek yatay yer değiştirmesinden gelir; engelde adımlar durur, beklemede nötr poza yumuşak dönüş olur. Harici Tripo işlemi/ücretli üretim yapılmadı.

step-a.png ve step-b.png aynı native player turundaki iki farklı adım pozudur. Bu bir animasyon sanatçısının hazırladığı klip değil, teslim edilen tek mesh için yerel prosedürel yürüyüştür. Diz/kalça deformasyonları ve üst gövdenin sabit kalması kontrol edildi.
