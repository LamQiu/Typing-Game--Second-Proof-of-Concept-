using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Obsolete
{
    public class MoveBehaviour : MonoBehaviour
    {
        public float xOffset = 1f;
        public float yOffset = 1f;

        public int startGridX;
        public int startGridY;

        public int currentGridX;
        public int currentGridY;

        public GameObject outlineIndicator;

        public LettersGrid lettersGrid;

        private void Start()
        {
            currentGridX = startGridX;
            currentGridY = startGridY;
        }

        public void Reset()
        {
            currentGridX = startGridX;
            currentGridY = startGridY;
        }

        private void Update()
        {
            var currentLetter = lettersGrid.letters[currentGridX, currentGridY];
            var offset = new Vector3(xOffset, yOffset, 0);
            var worldPos = lettersGrid.GetWorldPos(currentGridX, currentGridY);
            transform.SetPositionAndRotation(worldPos + offset,
                Quaternion.identity);
            outlineIndicator.transform.SetLocalY(-yOffset);
            if (currentLetter != null)
            {
                var targetPos = currentGridX;
                if (currentLetter.CheckKey())
                {
                    targetPos = currentGridX + 1;
                    if (lettersGrid.CheckBounds(targetPos, currentGridY))
                    {
                        currentGridX = targetPos;
                    }
                }
            }

            if (Keyboard.current[Key.Backspace].wasPressedThisFrame)
            {
                if (lettersGrid.CheckBounds(currentGridX - 1, currentGridY))
                {
                    currentGridX--;
                }
            }
        }
    }
}