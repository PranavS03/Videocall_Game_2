using Photon.Pun;
using UnityEngine;
using Photon.Realtime;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    private string sampleRoomName = "MyRoom";

    // Connect to the Photon server
    private void Awake()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    // When the player is connected to the server, join a lobby
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
        // The player who joined first creates a sample room, and all players who join later will join that room
        PhotonNetwork.JoinOrCreateRoom(sampleRoomName, new RoomOptions { MaxPlayers = 20 }, TypedLobby.Default);
    }

    // Optionally, handle additional logic when a new player enters the room
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("New player entered the room: " + newPlayer.NickName);
    }

    // Optionally, handle cleanup or other logic when a player leaves the room
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("Player left the room: " + otherPlayer.NickName);
    }
}
