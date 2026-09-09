using System;
using System.Collections.Generic;
using EnglishWords.Runtime.Monster;

namespace EnglishWords.Runtime.Typing
{
    public sealed class TypingJudgement
    {
        private static readonly IReadOnlyList<int> NO_INDICES = Array.Empty<int>();

        public TypingOutcome Outcome { get; }
        public MonsterUnit Target { get; }
        public IReadOnlyList<int> MisspelledIndices { get; }

        private TypingJudgement(TypingOutcome outcome, MonsterUnit target, IReadOnlyList<int> misspelledIndices)
        {
            Outcome = outcome;
            Target = target;
            MisspelledIndices = misspelledIndices;
        }

        public static TypingJudgement Defeated(MonsterUnit target) => new(TypingOutcome.Defeated, target, NO_INDICES);

        public static TypingJudgement Continuing() => new(TypingOutcome.Continuing, null, NO_INDICES);

        public static TypingJudgement Misspelled(IReadOnlyList<int> misspelledIndices) => new(TypingOutcome.Misspelled, null, misspelledIndices);

        public static TypingJudgement Missed() => new(TypingOutcome.Missed, null, NO_INDICES);
    }
}
