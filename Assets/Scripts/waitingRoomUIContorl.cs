using UnityEngine;
using UnityEngine.UI;

public class waitingRoomUIContorl : MonoBehaviour
{
    [SerializeField] private Image statusP1;
    [SerializeField] private Image statusP2;
    [SerializeField] private Image statusP3;
    [SerializeField] private Image statusP4;

    [SerializeField] private Color readyColor;
    [SerializeField] private Color notReadyColor;

    public void UpdateStatus(int readyPlayerCount)
    {
        statusP1.color = notReadyColor;
        statusP2.color = notReadyColor;
        statusP3.color = notReadyColor;
        statusP4.color = notReadyColor;
        switch (readyPlayerCount)
        {
            case 0:
                break;
            case 1:
                statusP1.color = readyColor;
                break;
            case 2:
                statusP1.color = readyColor;
                statusP2.color = readyColor;
                break;
            case 3:
                statusP1.color = readyColor;
                statusP2.color = readyColor;
                statusP3.color = readyColor;
                break;
            case 4:
                statusP1.color = readyColor;
                statusP2.color = readyColor;
                statusP3.color = readyColor;
                statusP4.color = readyColor;
                break;
        }
    }
}