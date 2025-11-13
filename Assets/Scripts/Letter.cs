using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Letter : MonoBehaviour
{
    public char letter;
    public TMP_Text text;
    public Key Key => key;
    private Key key;
    
    [Range(0, 1)] public float attachDummyChance = 0.5f;
    public DummyBehaviour attachedDummy;
    public GameObject dummyPrefab;

    public void Initialize(char l)
    {
        letter = l;
        var uppercaseL = letter.ToString().ToUpper();
        key = (Key)System.Enum.Parse(typeof(Key), uppercaseL);
        text.text = letter.ToString();

        float r = Random.Range(0f, 1f);
        Debug.Log($"Random number: {r}");
        if (r < attachDummyChance)
        {
            attachedDummy = Instantiate(dummyPrefab, transform).GetComponent<DummyBehaviour>();
            attachedDummy.Initialize(transform.position);
        }
    }
    public bool CheckKey()
    {
        if (Keyboard.current[Key].wasPressedThisFrame)
        {
            if (attachedDummy != null)
            {
                if (attachedDummy.TryKill())
                {
                    return true;
                }
            }
            else
            {
                return true;
            }
        }
        return false;
    }
}
