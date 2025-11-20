using System.Collections.Generic;
using UnityEngine;

public class WordChecker
{
    private TextAsset _wordFile;
     private HashSet<string> _dictionary;

    public WordChecker()
    {
        _wordFile = Resources.Load<TextAsset>("Words");
        _dictionary = new HashSet<string>();

        foreach (var line in _wordFile.text.Split('\n'))
        {
            string word = line.Trim().ToLower();
            if (!string.IsNullOrEmpty(word))
                _dictionary.Add(word);
        }
    }


    public bool CheckWord(string input)
    {
        return _dictionary.Contains(input.ToLower());
    }
}