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

    public bool CheckWordDictionaryValidity(string word)
    {
        return _dictionary.Contains(word.ToLower());
    }
    public bool CheckWordPromptValidity(string input, PromptGenerator.Prompt prompt)
    {
        var isValid = _dictionary.Contains(input.ToLower());
        if (!isValid)  return false;

        var isValidForPrompt = true;
        var content = prompt.content.ToString().ToLower();
        var lowerInput = input.ToLower();
        if (content.Length > lowerInput.Length)
            return false;
        
        switch (prompt.type)
        {
            case PromptGenerator.PromptType.None:
                return false;
            case PromptGenerator.PromptType.StartWith:
                isValidForPrompt = lowerInput.StartsWith(content);
                break;
            case PromptGenerator.PromptType.Contains:
                isValidForPrompt = lowerInput.Contains(content);
                break;
            case PromptGenerator.PromptType.EndWith:
                isValidForPrompt = lowerInput.EndsWith(content);
                break;
        }
        return isValidForPrompt;
    }
}