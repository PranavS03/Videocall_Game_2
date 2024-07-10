using UnityEngine;
using Agora.Rtc;
using Agora_RTC_Plugin.API_Example;
using UnityEngine.UI;
using System.Collections.Generic;

public class AgoraManager : MonoBehaviour
{
    private bool isCameraOn = true;
    private bool isMicOn = true;
    public static AgoraManager Instance { get; private set; }

    [SerializeField] private string appID;
    [SerializeField] private GameObject canvas;
    [SerializeField] private string tokenBase = "https://agora-token-server-qrv9.onrender.com";

    private IRtcEngine RtcEngine;
    
    private VideoSurface myView;
    private VideoSurface remoteView;

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
        ToggleCamera(true);
        ToggleMicrophone(true);
        
        // Find and setup local video view
        GameObject go = GameObject.Find("MyView");
        myView = go.AddComponent<VideoSurface>();
        RtcEngine.EnableVideo();

        // Ensure canvas reference is valid
        if (canvas == null)
        {
            Debug.LogError("Canvas reference is not set in AgoraManager.");
            return;
        }
    }

    #region Configuration Functions

    private void InitRtcEngine()
    {
        RtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngine();

        // Set up event handler
        UserEventHandler handler = new UserEventHandler(this);
        RtcEngineContext context = new RtcEngineContext();
        context.appId = appID;
        context.channelProfile = CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING;
        context.audioScenario = AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT;
        context.areaCode = AREA_CODE.AREA_CODE_GLOB;

        // Initialize the engine
        RtcEngine.Initialize(context);
        RtcEngine.InitEventHandler(handler);
        RtcEngine.EnableVideo();
        RtcEngine.EnableAudio();

    }

    private void SetBasicConfiguration()
    {
        // Enable audio and video
        RtcEngine.EnableAudio();
        RtcEngine.EnableVideo();
        RtcEngine.EnableLocalVideo(true);

        // Configure video encoder
        VideoEncoderConfiguration config = new VideoEncoderConfiguration();
        config.dimensions = new VideoDimensions(640, 360); // Adjust as needed
        config.frameRate = 15; // Adjust frame rate
        config.bitrate = 0; // Use 0 for adaptive bitrate
        RtcEngine.SetVideoEncoderConfiguration(config);

        // Set channel profile and client role
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

        // Check if both players have joined a channel
        if (player1ChannelName == "" && player2ChannelName == "")
        {
            string newChannelName = GenerateChannelName();
            channelName = newChannelName;

            mainPlayerInfo.SetChannelName(channelName);
        }
        else if (player1ChannelName != "" && player2ChannelName != "")
        {
            return; // Both players already in a channel
        }
        else if (player2ChannelName != "")
        {
            UpdatePropertiesForPlayer(mainPlayerInfo, player2ChannelName, player2Info.GetToken());
        }

        JoinChannel();
    }

    private void JoinChannel()
    {
        // Generate token if not already generated
        if (token.Length == 0)
        {
            StartCoroutine(HelperClass.FetchToken(tokenBase, channelName, 0, UpdateToken));
            return;
        }

        // Join channel with token
        RtcEngine.JoinChannel(token, channelName, "", 0);
        UpdateUsersInAChannelTable(channelName, 0);
        RtcEngine.StartPreview();
        MakeVideoView(0); // Show local video view
    }

    public void LeaveChannel()
    {
        UpdatePropertiesForPlayer(mainPlayerInfo, "", "");

        RtcEngine.StopPreview();
        DestroyVideoView(0); // Destroy local video view
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

    private string GenerateChannelName()
    {
        return GetRandomChannelName(10);
    }

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

    private void UpdateToken(string newToken)
    {
        token = newToken;

        mainPlayerInfo.SetToken(token);

        if (connectionState == CONNECTION_STATE_TYPE.CONNECTION_STATE_DISCONNECTED || connectionState == CONNECTION_STATE_TYPE.CONNECTION_STATE_FAILED)
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

        if (player == mainPlayerInfo)
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
            return; // Video view already exists
        }

        // Create video surface and set it up
        VideoSurface videoSurface = MakeImageSurface(uid.ToString());
        if (videoSurface == null) return;

        // Configure videoSurface position and scale
        if (uid == 0)
        {
            videoSurface.SetForUser(uid, channelId);
            videoSurface.SetEnable(true);
            videoSurface.transform.localPosition = new Vector3(-300, 200, 0); // Adjust position
            videoSurface.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f); // Adjust scale
        }
        else
        {
            videoSurface.SetForUser(uid, channelId, VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE);
            videoSurface.SetEnable(true);
            videoSurface.transform.localPosition = new Vector3(300, 200, 0); // Adjust position
            videoSurface.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f); // Adjust scale
        }

        // Handle texture size modification
        videoSurface.OnTextureSizeModify += (int width, int height) =>
        {
            RectTransform rectTransform = videoSurface.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = new Vector2(width / 2, height / 2);
                rectTransform.localScale = Vector3.one;
            }
            else
            {
                float scale = (float)height / (float)width;
                videoSurface.transform.localScale = new Vector3(1, 1, scale);
            }

            Debug.Log("OnTextureSizeModify: " + width + " " + height);
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
        RawImage rawImage = gameObject.AddComponent<RawImage>();

        if (canvas != null)
        {
            gameObject.transform.SetParent(canvas.transform, false);
        }
        else
        {
            Debug.LogError("Canvas is null, unable to add video view");
        }

        gameObject.transform.Rotate(0f, 0.0f, 180.0f);
        gameObject.transform.localPosition = Vector3.zero;
        gameObject.transform.localScale = new Vector3(2f, 3f, 1f);

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

        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {
            if (!agoraManager.usersJoinedInAChannel.ContainsKey(connection.channelId)) return;

            foreach (uint uid in agoraManager.usersJoinedInAChannel[connection.channelId])
            {
                agoraManager.DestroyVideoView(uid);
            }
        }

        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            agoraManager.MakeVideoView(uid, connection.channelId);
            agoraManager.UpdateUsersInAChannelTable(connection.channelId, uid);
        }

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
       

    #endregion
    }
      public void ToggleCamera(bool enable)
    {
        Debug.Log("ToggleCamera: " + enable);
        isCameraOn = enable;
        if (isCameraOn)
        {
            Debug.Log("Camera is on");
            RtcEngine.EnableLocalVideo(true);// Enable local video
        }
        else
        {
            Debug.Log("Camera is off");
            RtcEngine.EnableLocalVideo(false);// Disable local video
        }
    }

    public void ToggleMicrophone(bool enable2)
    {
        Debug.Log("ToggleMicrophone: " + enable2);
        isMicOn = enable2;
        if (RtcEngine != null)
        {
            if (isMicOn)
            {
                RtcEngine.EnableAudio(); // Enable local audio
                Debug.Log("Microphone enabled.");
            }
            else
            {
                RtcEngine.DisableAudio(); // Disable local audio
                Debug.Log("Microphone disabled.");
            }
        }
        else
        {
            Debug.LogWarning("Agora engine is not initialized, failed to initialize microphone.");
        }
    }

}

