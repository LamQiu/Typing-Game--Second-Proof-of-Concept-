using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public struct Letter
{
    public bool enabled;
    public Key key;

    public Letter(bool enabled, Key key = Key.None)
    {
        this.enabled = enabled;
        this.key = key;
    }
}
public class LettersGrid : MonoBehaviour
{
    public int width;
    public int height;
    public Letter[,] letters;

    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        letters = new Letter[width, height];
        for (int i = 0; i < letters.GetLength(0); i++)
        {
            for (int j = 0; j < letters.GetLength(1); j++)
            {
                letters[i, j] = new Letter(false);
            }
        }
    }

    public void InstantiateWord(string word, int x, int y)
    {
        var len = word.Length;
        
    }
}
