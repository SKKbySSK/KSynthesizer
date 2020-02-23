# KSynthesizer
4Eのデザイン実験で用いるシンセサイザーライブラリ

## 構成
- KSynthesizer
    - シンセサイザーのコアライブラリ
    - クロスプラットフォーム対応
    - .Net Standard 2.1, .Net Core 3.0
- TestTool.Windows
    - Windows向けのデバッグツール
    - 波形表示やオシレータの調整が可能
    
## 実装
- [x] Sin波
- [x] 矩形波
- [x] ノコギリ波
- [x] 三角波
- [x] ミキサー
- [ ] LPF
- [ ] HPF
- [ ] リバーブ

### 準備ができ次第実装
- [ ] キーボードからの入力
- [ ] PWM出力

### 追々実装
- [ ] BPF
- [ ] コーラス
- [ ] ディレイ

## 参考
[音階と周波数](https://tomari.org/main/java/oto.html)

[LPF](https://org-technology.com/posts/low-pass-filter.html)
