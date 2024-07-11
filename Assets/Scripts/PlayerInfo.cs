using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class PlayerInfo : MonoBehaviourPunCallbacks
{
    private const string CHANNEL_NAME = "ChannelName";
    private const string TOKEN = "Token";
    private const string COLOR_R = "ColorR";
    private const string COLOR_G = "ColorG";
    private const string COLOR_B = "ColorB";
    private const string PLAYER_NAME = "PlayerName";

    private PhotonView photonView;
    private SpriteRenderer spriteRenderer;
    private TextMeshPro playerNameText;

    private ExitGames.Client.Photon.Hashtable playerDetails = new ExitGames.Client.Photon.Hashtable();

    private string channelName = "";
    private string token = "";
    private Color color;
    private string playerName = "";

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerNameText = transform.GetChild(0).GetComponent<TextMeshPro>();
    }

    #region GETTERS
    public string GetChannelName() { return channelName; }
    public string GetToken() { return token; }
    public Color GetColor() { return color; }
    public string GetName() { return playerName; }
    #endregion

    #region SETTERS
    public void SetChannelName(string channelName)
    {
        this.channelName = channelName;
        AddOrUpdateProperty(CHANNEL_NAME, channelName);
    }
    public void SetToken(string token)
    {
        this.token = token;
        AddOrUpdateProperty(TOKEN, token);
    }
    public void SetColor(Color color)
    {
        this.color = color;
        spriteRenderer.material.color = color;
        AddOrUpdateProperty(COLOR_R, color.r);
        AddOrUpdateProperty(COLOR_G, color.g);
        AddOrUpdateProperty(COLOR_B, color.b);
    }
    public void SetName(string name)
    {
        this.playerName = name;
        playerNameText.text = name;
        AddOrUpdateProperty(PLAYER_NAME, name);
    }
    #endregion

    private void AddOrUpdateProperty(string key, object value)
    {
        if (playerDetails.ContainsKey(key))
        {
            playerDetails[key] = value;
        }
        else
        {
            playerDetails.Add(key, value);
        }
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerDetails);
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (targetPlayer != photonView.Owner) return;

        if (changedProps.ContainsKey(CHANNEL_NAME))
        {
            channelName = (string)changedProps[CHANNEL_NAME];
        }
        if (changedProps.ContainsKey(TOKEN))
        {
            token = (string)changedProps[TOKEN];
        }
        if (changedProps.ContainsKey(PLAYER_NAME))
        {
            playerName = (string)changedProps[PLAYER_NAME];
            playerNameText.text = playerName;
        }
        if (changedProps.ContainsKey(COLOR_R) && changedProps.ContainsKey(COLOR_G) && changedProps.ContainsKey(COLOR_B))
        {
            color = new Color((float)changedProps[COLOR_R], (float)changedProps[COLOR_G], (float)changedProps[COLOR_B], 1f);
            spriteRenderer.material.color = color;
        }
    }
}
