using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class PlayerInformationUI : MonoBehaviour
{
    [SerializeField] public List<Button> colorButtonsList;
    [SerializeField] public TMP_InputField nameInputField;
    [SerializeField] public Button doneButton;

    public static event EventHandler OnJoinRoom;
    public static event EventHandler<OnPlayerInfoDoneEventArgs> OnPlayerInfoDone;

    public class OnPlayerInfoDoneEventArgs : EventArgs
    {
        public Color color;
        public string playerName;
    }

    public Color chosenColor;
    public string playerName;

    private void Start()
    {
        nameInputField.onEndEdit.AddListener((string name) =>
        {
            playerName = name;
        });

        foreach (Button colorButton in colorButtonsList)
        {
            colorButton.onClick.AddListener(() =>
            {
                chosenColor = colorButton.transform.GetChild(0).GetComponent<Image>().color;
            });
        }

        Show();

        doneButton.onClick.AddListener(() =>
        {
            OnJoinRoom?.Invoke(this, EventArgs.Empty);

            OnPlayerInfoDone?.Invoke(this, new OnPlayerInfoDoneEventArgs
            {
                color = chosenColor,
                playerName = playerName
            });

            // Store player info in a static class to persist data across scenes
            PlayerData.chosenColor = chosenColor;
            PlayerData.playerName = playerName;

            // Load the second scene
            PhotonNetwork.LoadLevel("MainScreen");
        });
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}

public static class PlayerData
{
    public static Color chosenColor;
    public static string playerName;
}
