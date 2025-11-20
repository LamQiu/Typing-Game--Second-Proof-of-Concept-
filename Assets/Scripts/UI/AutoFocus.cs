using System;
using UnityEngine;
using TMPro;

public class AutoFocus : MonoBehaviour
{
    private TMP_InputField _inputField;

    private void Awake()
    {
        _inputField = GetComponent<TMP_InputField>();
    }

    void Start()
    {
        _inputField.Select();
        _inputField.ActivateInputField();
    }

    private void Update()
    {
        
    }
}