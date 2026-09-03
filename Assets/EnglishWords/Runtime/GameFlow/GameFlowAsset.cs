using System;
using UnityEngine;

namespace EnglishWords.Runtime.GameFlow
{
    // タイトル（Startシーン）とインゲーム（Mainシーン）は互いを直接参照できないため、
    // 双方が参照するこのアセットを経由して要求を渡す
    [CreateAssetMenu(fileName = "GameFlow", menuName = "EnglishWords/Game Flow")]
    public sealed class GameFlowAsset : ScriptableObject
    {
        public event Action OnRetryRequested;
        public event Action OnReturnToTitleRequested;

        public void RequestRetry() => OnRetryRequested?.Invoke();

        public void RequestReturnToTitle() => OnReturnToTitleRequested?.Invoke();
    }
}
