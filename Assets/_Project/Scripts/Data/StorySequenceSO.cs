using System.Collections.Generic;
using UnityEngine;

namespace PhoenixReborn.Data
{
    /// <summary>
    /// ScriptableObject grouping an ordered sequence of dialogues for a story chapter.
    /// </summary>
    [CreateAssetMenu(fileName = "StorySequence_Intro", menuName = "PhoenixReborn/Story Sequence")]
    public class StorySequenceSO : ScriptableObject
    {
        [SerializeField] private string sequenceTitle = "Capítulo 1";
        [SerializeField] private List<StoryDialogueSO> dialogues = new List<StoryDialogueSO>();

        public string SequenceTitle => sequenceTitle;
        public IReadOnlyList<StoryDialogueSO> Dialogues => dialogues;
    }
}
