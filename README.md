# KSynthesizer
4Eのデザイン実験で用いるシンセサイザーライブラリ

## 実行方法(TestTool)

### SDKのインストール（初回のみ）
- [.Net Core 3.1 SDK](https://github.com/dotnet/core/blob/master/release-notes/3.1/3.1.2/3.1.2.md)をダウンロード
    - Windowsの場合はSDK Installer->x64で基本的にOK

### TestTool.Windowsを使う場合
- build-windows.batをダブルクリック
- Release/TestTool.Windows.exeを実行

### TestTool.Platform Raspberry Pi向けのセットアップ

#### (1) ビルド済みバイナリ
- libsoundo/Linux-Release/にlibsoundio.a, libsoundio.so, libsoundio.so.2があります

#### (2) 手動ビルド
- 任意の場所で
```
sudo apt-get install cmake curl libunwind8 gettext apt-transport-https libasound2-dev
git clone https://github.com/andrewrk/libsoundio.git
cd libsoundio/
mkdir build && cd build && cmake ..
make
```
- libsoundio.a, libsoundio.so, libsoundio.so.2ができているはず

#### TestTool.Platform
- bash等で`sh publish-linux-arm.sh`
- build/linux-arm/にlibsoundio.a, libsoundio.so, libsoundio.so.2をコピー
- `sh publish-and-sync.sh`
- ラズパイの`/home/pi/TestTool.Platform`にlinux-arm以下がすべてコピーされる
- ラズパイのターミナル上で`/home/pi/TestTool.Platform`へ移動して`TestTool.Platform`を実行する

### カスタムビルドをする場合
- Visual Studioをインストール
- Synthesizer.slnを開く

## 構成
- KSynthesizer
    - シンセサイザーのコアライブラリ
    - クロスプラットフォーム対応
    - .Net Standard 2.1, .Net Core 3.1
- TestTool.Windows
    - Windows向けのデバッグツール
    - 波形表示やオシレータの調整が可能
    
## FFTについて
画面上でFFTを確認できますが、周波数分解能が86Hzくらいなのでごく低周波では大して使えないです。
原因としては、FFTする際に最大でも512サンプルしか渡してないので、44100Hz/512=86Hzとなるからです。
必要ならWAVファイルをエクスポートして、古いですがこちらのツール等で解析するといいかと。
[WaveSpectra](http://efu.jp.net/soft/ws/ws.html)

## 実装
- [x] Sin波
- [x] 矩形波
- [x] ノコギリ波
- [x] 三角波
- [x] ミキサー
- [x] LPF
- [x] HPF
- [ ] リバーブ
- [ ] 全体用のADSR(時間変化に大して音量を変化させる e.g. 減衰していく振動など)
- [ ] フィルタ用のADSR(時間変化に対してフィルタのカットオフを変化させる)

### 準備ができ次第実装
- [ ] キーボードからの入力
- [ ] PWM出力

### 追々実装
- [x] BPF
- [ ] コーラス
- [ ] ディレイ

## 参考
[音階と周波数](https://tomari.org/main/java/oto.html)

[LPF](https://org-technology.com/posts/low-pass-filter.html)

[実波形とフーリエ変換](http://www.fbs.osaka-u.ac.jp/labs/ishijima/FFT-05.html)

[ADSR Envelope](https://www.wikiaudio.org/adsr-envelope/)

[BiQuadFilter](https://github.com/naudio/NAudio/blob/master/NAudio/Dsp/BiQuadFilter.cs)

[Cookbook formulae for audio equalizer biquad filter coefficients](https://www.w3.org/2011/audio/audio-eq-cookbook.html)
