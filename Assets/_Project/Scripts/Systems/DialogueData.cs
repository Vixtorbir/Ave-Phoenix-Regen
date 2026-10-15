using System;
using UnityEngine;

namespace PhoenixReborn.Systems
{
    [Serializable]
    public class DialogueLine
    {
        public string speakerName;
        public Color speakerColor = Color.cyan;
        public Sprite portraitSprite;
        [TextArea(3, 6)]
        public string messageText;
        public float customTypeSpeed = 0.025f;
    }
}
