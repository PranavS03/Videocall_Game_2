using Photon.Pun;
using UnityEngine;
using Photon.Realtime;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-1)]

public class PhotonManager : MonoBehaviourPunCallbacks
{
    private const string GAME_SCENE = "GameScene";

    [SerializeField] private string sampleRoomName = "My Room";

    //Connect to the Photon server
    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    //When the player is connected to the server, join a lobby
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        JoinOrCreateRoom();
    }

    public void JoinOrCreateRoom()
    {
        //The player who joined first, creates a sample room and all the players who join later will join that room

        PhotonNetwork.JoinOrCreateRoom(sampleRoomName, new RoomOptions { MaxPlayers = 20, BroadcastPropsChangeToAll = true }, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel(GAME_SCENE);
    }
}
