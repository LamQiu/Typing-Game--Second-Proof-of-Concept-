using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class waitingRoomUIContorl : MonoBehaviour
{
    [SerializeField] private Image statusP1;
    [SerializeField] private Image P1JoinedImage;
    [SerializeField] private TMP_Text P1NameText;
    [SerializeField] private Image statusP2;
    [SerializeField] private Image P2JoinedImage;
    [SerializeField] private TMP_Text P2NameText;
    [SerializeField] private Image statusP3;
    [SerializeField] private Image statusP4;

    [SerializeField] private Color blackColor;
    [SerializeField] private Color whiteColor;
    [SerializeField] private Color grayColor;

    [SerializeField] private PlayerStatus p1Status;
    [SerializeField] private PlayerStatus p2Status;
    [SerializeField] private PlayerStatus p3Status;
    [SerializeField] private PlayerStatus p4Status;

    public GameObject iconP1;
    public GameObject iconP2;
    public GameObject iconP3;
    public GameObject iconP4;

    [SerializeField] private TextMeshProUGUI readyPlayerCountText;

    public enum PlayerStatus
    {
        NotExist,
        NotJoined,
        Joined,
        Self,
    }

    private void Awake()
    {
        p3Status = PlayerStatus.NotExist;
        p4Status = PlayerStatus.NotExist;
    }

    // update is called once per frame
    void Update(){
        UpdateStatus();
    }

    public void UpdateStatus()
    {
        statusP1.color = grayColor;
        statusP2.color = grayColor;
        statusP3.color = grayColor;
        statusP4.color = grayColor;

        int readyPlayerCount = p1Status == PlayerStatus.Joined || p1Status == PlayerStatus.Self ? 1 : 0;
        readyPlayerCount += p2Status == PlayerStatus.Joined || p2Status == PlayerStatus.Self ? 1 : 0;
        readyPlayerCount += p3Status == PlayerStatus.Joined || p3Status == PlayerStatus.Self ? 1 : 0;
        readyPlayerCount += p4Status == PlayerStatus.Joined || p4Status == PlayerStatus.Self ? 1 : 0;

        statusP1.gameObject.SetActive(true);
        statusP2.gameObject.SetActive(true);
        statusP3.gameObject.SetActive(true);
        statusP4.gameObject.SetActive(true);
        switch (readyPlayerCount)
        {
            case 0:
                break;
            case 1:
                statusP1.color = whiteColor;
                break;
            case 2:
                statusP1.color = whiteColor;
                statusP2.color = whiteColor;
                break;
            case 3:
                statusP1.color = whiteColor;
                statusP2.color = whiteColor;
                statusP3.color = whiteColor;
                break;
            case 4:
                statusP1.color = whiteColor;
                statusP2.color = whiteColor;
                statusP3.color = whiteColor;
                statusP4.color = whiteColor;
                break;
        }

        // void setIconStatus(GameObject icon, PlayerStatus playerStatus)
        // {
        //     switch (playerStatus)
        //     {
        //         case PlayerStatus.NotExist:
        //             icon.SetActive(false);
        //             break;
        //         case PlayerStatus.NotJoined:
        //             icon.SetActive(true);
        //             icon.transform.Find("Image").gameObject.SetActive(true);
        //             icon.transform.Find("Text").GetComponent<TextMeshProUGUI>().color = whiteColor;
        //             break;
        //         case PlayerStatus.Joined:
        //             icon.SetActive(true);
        //             icon.transform.Find("Image").gameObject.SetActive(false);
        //             icon.transform.Find("Text").GetComponent<TextMeshProUGUI>().color = blackColor;
        //             icon.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        //             break;
        //         case PlayerStatus.Self:
        //             icon.SetActive(true);
        //             icon.transform.Find("Image").gameObject.SetActive(true);
        //             icon.transform.Find("Text").GetComponent<TextMeshProUGUI>().color = whiteColor;
        //             icon.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
        //             break;
        //     }
        // }

        // setIconStatus(iconP1, p1Status);
        // setIconStatus(iconP2, p2Status);
        // setIconStatus(iconP3, p3Status);
        // setIconStatus(iconP4, p4Status);
        
        if(p1Status == PlayerStatus.Joined)
        {
            P1JoinedImage.gameObject.SetActive(true);
            P1JoinedImage.color = whiteColor;
            P1NameText.color = blackColor;
        }

        if (p2Status == PlayerStatus.Joined)
        {
            P2JoinedImage.gameObject.SetActive(true);
            P2JoinedImage.color = whiteColor;
            P2NameText.color = blackColor;
        }

        int joinedPlayerCount = p1Status == PlayerStatus.NotJoined ? 1 : 0;
        joinedPlayerCount += p2Status == PlayerStatus.NotJoined ? 1 : 0;
        joinedPlayerCount += p3Status == PlayerStatus.NotJoined ? 1 : 0;
        joinedPlayerCount += p4Status == PlayerStatus.NotJoined ? 1 : 0;
        joinedPlayerCount += readyPlayerCount;
        readyPlayerCountText.text = readyPlayerCount + "/" + joinedPlayerCount;

        switch (joinedPlayerCount)
        {
            case 0:
                statusP1.gameObject.SetActive(false);
                statusP2.gameObject.SetActive(false);
                statusP3.gameObject.SetActive(false);
                statusP4.gameObject.SetActive(false);
                break;
            case 1:
                statusP2.gameObject.SetActive(false);
                statusP3.gameObject.SetActive(false);
                statusP4.gameObject.SetActive(false);
                break;
            case 2:
                statusP3.gameObject.SetActive(false);
                statusP4.gameObject.SetActive(false);
                break;
            case 3:
                statusP4.gameObject.SetActive(false);
                break;
            case 4:
                break;
        }
    }
}