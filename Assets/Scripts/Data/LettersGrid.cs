using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class LettersGrid : MonoBehaviour
{
    [Header("Test")]
    public string word = "HELLO";
    public int x = 0;
    public int y = 0;
    
    public int width;
    public int height;
    public Letter[,] letters;

    public float letterSpacing = 1f;
    public GameObject LetterPrefab;

    private void Start()
    {
        Initialize();
    }

    private void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            InstantiateWord(word, x, y);
        }
    }
    public void Initialize()
    {
        letters = new Letter[width, height];
        for (int i = 0; i < letters.GetLength(0); i++)
        {
            for (int j = 0; j < letters.GetLength(1); j++)
            {
                letters[i, j] = null;
            }
        }
    }

    public void InstantiateWord(string word, int x, int y)
    {
        var len = word.Length;
        if (CheckBounds(x + len, y))
        {
            for (int i = 0; i < word.Length; i++)
            {
                Debug.Log(word[i]);
                char letter = word[i];
                Letter newLetter = Instantiate(LetterPrefab,
                        new Vector3((x + i) * letterSpacing, y * letterSpacing, 0f), Quaternion.identity)
                    .GetComponent<Letter>();
                newLetter.Initialize(letter);
                letters[x + i, y] = newLetter;
            }
        }
    }

    public bool CheckBounds(int x, int y)
    {
        if (x < 0 || y < 0 || x >= letters.GetLength(0) || y >= letters.GetLength(1))
        {
            return false;
        }
        return true;
    }
    public Vector3 GetWorldPos(int x, int y)
    {
        return new Vector3(x * letterSpacing, y * letterSpacing, 0f);
    }
}