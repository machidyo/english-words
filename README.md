# English Words

英単語をタイピングして覚えるゲーム。仕様は[Docs/src/game-design/](Docs/src/game-design/index.md)にある。

## セットアップ

ライセンス上リポジトリに含められないアセットがあるため、クローン後に自分で用意する。

| 用意するもの | 置き場所 | 補足 |
| :--- | :--- | :--- |
| 游ゴシック Medium（`YuGothic-Medium.otf`） | `Assets/EnglishWords/Fonts/` | macOS / Windows に同梱されている商用フォント。OSから複製して置く |
| 上記から生成したTMPフォントアセット | `Assets/EnglishWords/Fonts/YuGothic-Medium SDF.asset` | Window > TextMeshPro > Font Asset Creator で生成する |
| Synty のポリゴンアセット | `Assets/Synty/` | 購入済みならストアから再ダウンロードする |

シーンとプレハブは游ゴシックのフォントアセットをGUID `50d6e160480554036b89cfb4e9e01c19` で参照している。生成した`.asset`の`.meta`の`guid`をこの値に書き換えると、参照切れなしでそのまま動く。書き換えない場合はテキストが既定フォントにフォールバックする。

## 入口となるシーン

**`Assets/EnglishWords/Scenes/Start.unity`** から再生する。

`Start`はタイトルを表示し、Enterで`Main`を加算ロードする。プレイが終わると`Main`を破棄してタイトルへ戻る（[実装構成](Docs/src/game-design/architecture.md)）。`Main`単体で再生してもインゲームは動くが、リトライとタイトルへ戻る操作は`Start`がないと効かない。

## 操作

| キー | 操作 |
| :--- | :--- |
| `a`〜`z` | 英単語の入力 |
| Backspace | 1文字消す |
| Enter | タイトルで開始 / インゲームで確定 / リザルトでもう一度 |
| Esc | リザルトからタイトルへ |

## フォルダー

```text
Assets/EnglishWords/
├── Runtime/            スクリプト（asmdef: EnglishWords.Runtime）
├── Prefabs/            Monster, MagicBolt
├── Scenes/             Start, Main
├── ScriptableObjects/  GameFlow（Start と Main をつなぐイベント）
├── Materials/          Ground
├── Fonts/              日本語TMPフォント（リポジトリには含まない。セットアップ参照）
└── Resources/          Words.csv（出題データ）
```

## 出題データ

`Assets/EnglishWords/Resources/Words.csv`。1行目は見出し。

```csv
english,japanese,difficulty,category
cat,"ねこ,ネコ",1,animal
```

日本語は複数の訳を`"`で囲んで`,`区切りで持つ。モンスターの頭上に出るのは先頭のひとつ。
