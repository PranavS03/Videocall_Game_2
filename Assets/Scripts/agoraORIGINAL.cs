/*using UnityEngine;
using Agora.Rtc;
using Agora_RTC_Plugin.API_Example;
using UnityEngine.UI;
using System.Collections.Generic;

public class AgoraManager : MonoBehaviour
{
    public static AgoraManager Instance { get; private set; }

    [SerializeField] private string appID;
    [SerializeField] private GameObject canvas;
    [SerializeField] private string tokenBase = "https://agora-token-server-qrv9.onrender.com";

    private IRtcEngine RtcEngine;
    VideoSurface myView;
    VideoSurface remoteView;

    private string token = "";
    private string channelName = "Sample";
    private PlayerInfo mainPlayerInfo;

    public CONNECTION_STATE_TYPE connectionState = CONNECTION_STATE_TYPE.CONNECTION_STATE_DISCONNECTED;
    public Dictionary<string, List<uint>> usersJoinedInAChannel;

    private void Awake()
    {
        Instance = this;
        usersJoinedInAChannel = new Dictionary<string, List<uint>>();
    }

    private void Start()
    {
        InitRtcEngine();
        SetBasicConfiguration();
        GameObject go = GameObject.Find("MyView");
        myView = go.AddComponent<VideoSurface>();
    }

    #region Configuration Functions

    private void InitRtcEngine()
    {
        RtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngine();
        UserEventHandler handler = new UserEventHandler(this);
        RtcEngineContext context = new RtcEngineContext();
        context.appId = appID;
        context.channelProfile = CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING;
        context.audioScenario = AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT;
        context.areaCode = AREA_CODE.AREA_CODE_GLOB;

        RtcEngine.Initialize(context);
        RtcEngine.InitEventHandler(handler);
    }

    private void SetBasicConfiguration()
    {
        RtcEngine.EnableAudio();
        RtcEngine.EnableVideo();

        VideoEncoderConfiguration config = new VideoEncoderConfiguration();
        config.dimensions = new VideoDimensions(640, 360);
        config.frameRate = 15;
        config.bitrate = 0;
        RtcEngine.SetVideoEncoderConfiguration(config);

        RtcEngine.SetChannelProfile(CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING);
        RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
    }

    #endregion

    #region Channel Join/Leave Handler Functions



    public void JoinChannel(PlayerInfo player1Info, PlayerInfo player2Info)
    {
        string player1ChannelName = player1Info.GetChannelName();
        string player2ChannelName = player2Info.GetChannelName();

        mainPlayerInfo = player1Info;

        //If both the players have not joined a channel
        if (player1ChannelName == "" && player2ChannelName == "")
        {
            string newChannelName = GenerateChannelName();
            channelName = newChannelName;

            mainPlayerInfo.SetChannelName(channelName);
        }
        //If both the players are already in a channel
        else if (player1ChannelName != "" && player2ChannelName != "")
        {
            return;
        }
        //If the other player has joined a channel, join their channel
        else if (player2ChannelName != "")
        {
            UpdatePropertiesForPlayer(mainPlayerInfo, player2ChannelName, player2Info.GetToken());
        }

        JoinChannel();
    }

    private void JoinChannel()
    {
        //If a token is not yet generated, we first generate one and then join the channel

        if (token.Length == 0)
        {
            StartCoroutine(HelperClass.FetchToken(tokenBase, channelName, 0, UpdateToken));
            return;
        }

        RtcEngine.JoinChannel(token, channelName, "", 0);
        UpdateUsersInAChannelTable(channelName, 0);
        RtcEngine.StartPreview();
        MakeVideoView(0);
    }

    public void LeaveChannel()
    {
        UpdatePropertiesForPlayer(mainPlayerInfo, "", "");

        RtcEngine.StopPreview();
        DestroyVideoView(0);
        RtcEngine.LeaveChannel();
    }

    public void LeaveChannelIfNoOtherUsersPresent()
    {
        string channel = mainPlayerInfo.GetChannelName();
        if (usersJoinedInAChannel[channel].Count != 1) return;

        RemoveAllTheUsersFromChannel(channel);
        LeaveChannel();
    }

    #endregion

    #region Helper Functions

    private void DestroyVideoView(uint uid)
    {
        GameObject videoView = GameObject.Find(uid.ToString());
        if (videoView != null)
        {
            Destroy(videoView);
        }
    }


    private void UpdateUsersInAChannelTable(string channel, uint uid)
    {
        if (usersJoinedInAChannel.ContainsKey(channel))
        {
            usersJoinedInAChannel[channel].Add(uid);
        }
        else
        {
            usersJoinedInAChannel.Add(channel, new List<uint> { uid });
        }
    }

    /// <summary>
    /// Generate a channel name at the runtime
    /// </summary>
    /// <returns></returns>
    private string GenerateChannelName()
    {
        return GetRandomChannelName(10);
    }

    /// <summary>
    /// Generate a random channel name of a specified length
    /// </summary>
    /// <param name="length">
    /// Required length for the channel name
    /// </param>
    /// <returns>
    /// Returns a randomly generated channel name
    /// </returns>
    private string GetRandomChannelName(int length)
    {
        string characters = "abcdefghijklmnopqrstuvwxyzABCDDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        string randomChannelName = "";

        for (int i = 0; i < length; i++)
        {
            randomChannelName += characters[Random.Range(0, characters.Length)];
        }

        return randomChannelName;
    }

    /// <summary>
    /// Callback function for updating the token, whenever it is generated from the server
    /// </summary>
    /// <param name="newToken"></param>
    private void UpdateToken(string newToken)
    {
        token = newToken;

        mainPlayerInfo.SetToken(token);

        if(connectionState == CONNECTION_STATE_TYPE.CONNECTION_STATE_DISCONNECTED || connectionState == CONNECTION_STATE_TYPE.CONNECTION_STATE_FAILED)
        {
            JoinChannel();
        }
    }


    private void RemoveAllTheUsersFromChannel(string userChannel)
    {
        uint uid = usersJoinedInAChannel[userChannel][0];
        usersJoinedInAChannel.Remove(userChannel);
        DestroyVideoView(uid);
    }


    private void UpdatePropertiesForPlayer(PlayerInfo player, string channelName, string token)
    {
        player.SetChannelName(channelName);
        player.SetToken(token);

        if(player == mainPlayerInfo)
        {
            this.channelName = channelName;
            this.token = token;
        }
    }

    #endregion

    #region Video View Rendering Logic
    private void MakeVideoView(uint uid, string channelId = "")
    {
        GameObject videoView = GameObject.Find(uid.ToString());
        if (videoView != null)
        {
            //Video view for this user id already exists
            return;
        }

        // create a video surface game object and assign it to the user
        VideoSurface videoSurface = MakeImageSurface(uid.ToString());
        if (videoSurface == null) return;

        // configure videoSurface
        if (uid == 0)
        {
            videoSurface.SetForUser(uid, channelId);
            videoSurface.SetEnable(true);
        }
        else
        {
            videoSurface.SetForUser(uid, channelId, VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE);
            videoSurface.SetEnable(true);
        }
        

        videoSurface.OnTextureSizeModify += (int width, int height) =>
        {
            RectTransform transform = videoSurface.GetComponent<RectTransform>();
            if (transform)
            {
                //If render in RawImage. just set rawImage size.
                transform.sizeDelta = new Vector2(width / 2, height / 2);
                transform.localScale = Vector3.one;
            }
            else
            {
                //If render in MeshRenderer, just set localSize with MeshRenderer
                float scale = (float)height / (float)width;
                videoSurface.transform.localScale = new Vector3(1, 1, scale);
            }
            Debug.LogError("OnTextureSizeModify: " + width + "  " + height);
        };

        videoSurface.SetEnable(true);
    }

    private VideoSurface MakeImageSurface(string goName)
    {
        GameObject gameObject = new GameObject();

        if (gameObject == null)
        {
            return null;
        }

        gameObject.name = goName;
        // to be renderered onto
        gameObject.AddComponent<RawImage>();
        // make the object draggable
        gameObject.AddComponent<UIElementDrag>();
        if (canvas != null)
        {
            //Add the video view as a child of the canvas
            gameObject.transform.parent = canvas.transform;
        }
        else
        {
            Debug.LogError("Canvas is null video view");
        }

        // set up transform
        gameObject.transform.Rotate(0f, 0.0f, 180.0f);
        gameObject.transform.localPosition = Vector3.zero;
        gameObject.transform.localScale = new Vector3(2f, 3f, 1f);

        // configure videoSurface
        VideoSurface videoSurface = gameObject.AddComponent<VideoSurface>();
        return videoSurface;
    }

    #endregion

    #region User Events
    internal class UserEventHandler : IRtcEngineEventHandler
    {
        private AgoraManager agoraManager;
        internal UserEventHandler(AgoraManager agoraManager)
        {
            this.agoraManager = agoraManager;
        }

        /// <summary>
        /// Responsible for deleting all the views that are present on a user's screen, when the user leaves a channel
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="stats"></param>
        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {

            if (!agoraManager.usersJoinedInAChannel.ContainsKey(connection.channelId)) return;
            
            foreach (uint uid in agoraManager.usersJoinedInAChannel[connection.channelId])
            {
                agoraManager.DestroyVideoView(uid);
            }
        }

        /// <summary>
        /// Responsible for adding the newly joined user to the channel's uid pool
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="uid"></param>
        /// <param name="elapsed"></param>
        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            agoraManager.MakeVideoView(uid, connection.channelId);

            agoraManager.UpdateUsersInAChannelTable(connection.channelId, uid);
        }

        /// <summary>
        /// Responsible for removing a remote user's video view, if the user leaves the channel
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="uid"></param>
        /// <param name="reason"></param>
        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            agoraManager.DestroyVideoView(uid);


            string userChannel = connection.channelId;

            if (agoraManager.usersJoinedInAChannel.ContainsKey(userChannel))
            {
                agoraManager.usersJoinedInAChannel[userChannel].Remove(uid);
            }

        }

        public override void OnConnectionStateChanged(RtcConnection connection, CONNECTION_STATE_TYPE state, CONNECTION_CHANGED_REASON_TYPE reason)
        {
            agoraManager.connectionState = state;
        }
    }
    #endregion
}
*/



