using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInformationUI : MonoBehaviour
{
    [SerializeField] private List<Button> colorButtonsList;
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private Button doneButton;

    public static event EventHandler OnJoinRoom;
    public static event EventHandler<OnPlayerInfoDoneEventArgs> OnPlayerInfoDone;

    public class OnPlayerInfoDoneEventArgs : EventArgs
    {
        public Color color;
        public string playerName;
    }

    private Color chosenColor;
    private string playerName;

    private void Start()
    {
        nameInputField.onEndEdit.AddListener((string name) =>
        {
            playerName = name;
        });

        doneButton.onClick.AddListener(() =>
        {
            OnJoinRoom?.Invoke(this, EventArgs.Empty);

            OnPlayerInfoDone?.Invoke(this, new OnPlayerInfoDoneEventArgs
            {
                color = chosenColor,
                playerName = playerName
            });

            Hide();
        });

        foreach(Button colorButton in colorButtonsList)
        {
            colorButton.onClick.AddListener(() =>
            {
                chosenColor = colorButton.transform.GetChild(0).GetComponent<Image>().color;
            });
        }

        Show();
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
