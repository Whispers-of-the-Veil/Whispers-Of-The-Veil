// Owen Ingram

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.TextCore.Text;

namespace Dialogue
{
    public class TypeWriteEffect : MonoBehaviour
    {
        [SerializeField] private float typewriterSpeed = 50f;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip shortPitchClip;
        [SerializeField] private AudioClip longPitchClip;
        [SerializeField] private Vector2 pitchRange = new Vector2(0.9f, 1.1f);
        [SerializeField] private int characterSoundFrequency = 2;

        public bool IsRunning { get; private set; }

        private readonly List<Punctuation> punctuations = new List<Punctuation>()
        {
            new Punctuation(new HashSet<char>() { '.', '!', '?' }, 0.2f),
            new Punctuation(new HashSet<char>() { ',', ';', ':' }, 0.15f)
        };

        private Coroutine typingCoroutine;

        public void Run(string textToType, TMP_Text textLabel)
        {
            typingCoroutine = StartCoroutine(TypeText(textToType, textLabel));
        }

        public void Stop()
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }
            IsRunning = false;
        }

        private IEnumerator TypeText(string textToType, TMP_Text textLabel)
        {
            IsRunning = true;
            textLabel.text = string.Empty;

            float t = 0;
            int charIndex = 0;

            while (charIndex < textToType.Length)
            {
                int lastCharIndex = charIndex;

                t += Time.deltaTime * typewriterSpeed;
                charIndex = Mathf.FloorToInt(t);
                charIndex = Mathf.Clamp(charIndex, 0, textToType.Length);

                for (int i = lastCharIndex; i < charIndex; i++)
                {
                    bool isLast = i >= textToType.Length - 1;

                    textLabel.text = textToType.Substring(0, i + 1);

                    // Play gibberish only for letters/numbers at defined intervals
                    if (char.IsLetterOrDigit(textToType[i]) && i % characterSoundFrequency == 0 && !audioSource.isPlaying)
                    {
                        audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
                        AudioClip clipToPlay = Random.value < 0.5f ? shortPitchClip : longPitchClip;
                        audioSource.PlayOneShot(clipToPlay);
                    }

                    if (IsPunctuation(textToType[i], out float waitTime) && !isLast &&
                        !IsPunctuation(textToType[i + 1], out _))
                    {
                        yield return new WaitForSeconds(waitTime);
                    }
                }

                yield return null;
            }

            IsRunning = false;
        }

        private bool IsPunctuation(char character, out float waitTime)
        {
            foreach (Punctuation punctuationCategory in punctuations)
            {
                if (punctuationCategory.Punctuations.Contains(character))
                {
                    waitTime = punctuationCategory.WaitTime;
                    return true;
                }
            }
            waitTime = default;
            return false;
        }

        private readonly struct Punctuation
        {
            public readonly HashSet<char> Punctuations;
            public readonly float WaitTime;

            public Punctuation(HashSet<char> punctuations, float waitTime)
            {
                Punctuations = punctuations;
                WaitTime = waitTime;
            }
        }
    }
}
