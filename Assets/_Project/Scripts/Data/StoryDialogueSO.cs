using UnityEngine;

namespace PhoenixReborn.Data
{
    /// <summary>
    /// ScriptableObject defining a single line/node of dialogue.
    /// Story text can be changed by swapping the asset references.
    /// </summary>
    [CreateAssetMenu(fileName = "Dialogue_01", menuName = "PhoenixReborn/Story Dialogue")]
    public class StoryDialogueSO : ScriptableObject
    {
        [Header("Speaker Info")]
        [SerializeField] private string speakerName = "Comandante";
        [SerializeField] private Sprite speakerAvatar;

        [Header("Dialogue Content")]
        [TextArea(3, 6)]
        [SerializeField] private string dialogueText = "¡Bienvenido a la nave, piloto!";
        [SerializeField] private AudioClip typingSoundEffect;

        public string SpeakerName => speakerName;
        public Sprite SpeakerAvatar => speakerAvatar;
        public string DialogueText => dialogueText;
        public AudioClip TypingSoundEffect => typingSoundEffect;
    }
}
