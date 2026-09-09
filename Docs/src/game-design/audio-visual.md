# 演出とサウンド

タイピングゲームは**打った瞬間の反応の速さと気持ちよさ**が体験の質を決める。演出は後回しにできる装飾ではなく仕様として扱う。

## 入力へのフィードバック

| 出来事 | 見た目 | 音 |
| :--- | :--- | :--- |
| 文字を打った | 入力欄に文字が増える | `KeyStroke` |
| 単語を打ち切った（撃破） | 魔法の発射 → 誘導で飛翔 → 着弾のパーティクル → モンスターの消滅 | 発射で`MagicCast`、着弾で`MonsterHit` |
| 該当する単語がなかった（ミス） | 赤い`×`を短時間表示 | `WordMissed` |
| モンスターが到達した | 画面を揺らす | `WizardDamaged` |

パーティクルはSyntyのPolygonParticleFXから選んでいる。

**反応は入力と同じフレームで返す。** アニメーションの完了を待ってから次の入力を受けるような作りにしない。

到達したときの見た目は[決まっていないこと](./open-questions.md#決まっていないこと)。

## 魔法と撃破の演出

演出は「発射 → 飛翔 → 着弾 → 消滅」に分かれる。仕様は[魔法の発射と着弾](./magic.md)を参照。

- 魔法の表現: パーティクル
- 飛翔: 誘導で1秒
- **演出中も次の入力を受け付ける。**

飛翔の軌跡を残すかは[決まっていないこと](./open-questions.md#決まっていないこと)。
魔法の種類ごとに見た目を変えることと、詠唱の表現は[初版のスコープ外](./open-questions.md#初版のスコープ外)。

## カメラ

- 視点: 固定
- 画面の揺れ: ライフが減るとき

## BGM

未選定。[決まっていないこと](./open-questions.md#決まっていないこと)。

## 素材

**外部素材を入れたら必ずここに記録する。**

| 素材 | 用途 | 出典 | ライセンス |
| :--- | :--- | :--- | :--- |
| PolygonDungeon | モンスターのモデル | Synty Studios | 購入済みアセット |
| PolygonFantasyCharacters | 魔法使いのモデルと杖 | Synty Studios | 購入済みアセット |
| PolygonParticleFX | 炎の魔法と着弾のエフェクト | Synty Studios | 購入済みアセット |
| Ultimate Animation Collection | 魔法使いとモンスターのアニメーション | PROTOFACTOR | 購入済みアセット |
| Sonniss GDC 2026 Game Audio Bundle | 効果音 | Sonniss（各サウンドライブラリのサンプル） | ロイヤリティフリー。商用可・クレジット表記不要 |
| `YuGothic-Medium.otf` | 日本語表示用のTextMeshProフォント | macOSのシステムフォント | **再配布不可。仮置きなので配布前に差し替える** |

## 効果音

`Assets/EnglishWords/Audio/`に置いてある。

元ファイル名はスペースと記号を含み[命名規約](../unity-guidelines/naming/index.md)に反するため、用途がわかる名前へ変えて取り込んだ。**元の名前をここに残すのは、どのライブラリの音か追えなくなるのを防ぐため。**

| 用途 | ファイル | 長さ | 元ファイル名 |
| :--- | :--- | :--- | :--- |
| 魔法を唱える | `MagicCast.wav` | 2.48秒 | `FIREWhsh_Whoosh Fire Deep Growl Monster Saturated Crisp 03_ESM_EMWI` |
| モンスターにヒット | `MonsterHit.wav` | 1.48秒 | `EXPLDsgn_Explosion Small Blast Enemy Death Crunchy Boom Cartoon Noisy Crash Impact Delay 03_ESM_AG` |
| モンスターに攻撃された | `WizardDamaged.wav` | 1.06秒 | `FGHTImpt_Combat Punch Impact Light Hit Delay Crunchy Vintage Quick Smack 05_ESM_AG` |
| 文字の入力 | `KeyStroke.wav` | 0.38秒 | `MECHLtch_Click Deep Mechanism Latch Button Nearfield Thunk 02_ESM_HDLM` |
| 単語がミス | `WordMissed.wav` | 0.80秒 | `Deny Muted` |

### インポート設定

5つとも同じ設定にしてある。合計で586KB。

| 項目 | 値 | 理由 |
| :--- | :--- | :--- |
| Force To Mono | ON | 3D再生に切り替えても定位が効くようにする。容量も半分になる |
| Sample Rate | Override 48000Hz | 元素材は96kHz、`KeyStroke`は192kHz。Unityの出力は48kHzなので上は再生されず容量だけ食う |
| Compression Format | PCM | どれも数秒以内で総量が小さい。展開コストゼロを取る |
| Load Type | Decompress On Load | 同上 |
| Preload Audio Data | ON | 打鍵音は初回から遅れずに鳴らす必要がある |
| Load In Background | OFF | 同上 |

### 鳴らし方

**AudioSourceは`SePlayer`1つだけ。** 効果音は鳴る場所ではなく出来事で決まり、カメラも固定なので、`PlayOneShot`で重ねて鳴らせば足りる。2D（`Spatial Blend`は0）。

着弾音だけは事情が違う。**魔法弾は撃つたびに作られて着弾で消える**ため、進行役が直接購読できない。`MagicBolt`が着弾を知らせ、それを`MagicLauncher`が中継して外へ渡している。
