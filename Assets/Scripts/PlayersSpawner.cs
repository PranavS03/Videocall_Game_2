using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PlayersSpawner : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject playerPrefab;
    [Header("Boundaries for Spawning the Player")]
    [SerializeField] private float minXBoundary;
    [SerializeField] private float maxXBoundary;
    [SerializeField] private float minYBoundary;
    [SerializeField] private float maxYBoundary;

    private void Start()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            Debug.Log("Photon is connected and in a room. Spawning player...");
            SpawnPlayer();
        }
        else
        {
            Debug.LogError("Photon is not connected or not in a room.");
        }
    }

    public void SpawnPlayer()
    {
        Vector2 spawnPosition = new Vector2(Random.Range(minXBoundary, maxXBoundary), Random.Range(minYBoundary, maxYBoundary));
        GameObject playerGameObject = PhotonNetwork.Instantiate(playerPrefab.name, spawnPosition, Quaternion.identity);

        Debug.Log("Player instantiated at position: " + spawnPosition);

        // Set color and name for the local player only
        if (playerGameObject.GetComponent<PhotonView>().IsMine)
        {
            PlayerInfo playerInfo = playerGameObject.GetComponent<PlayerInfo>();
            if (playerInfo != null)
            {
                playerInfo.SetColor(PlayerData.chosenColor);
                playerInfo.SetName(PlayerData.playerName);

                Debug.Log("Player color set to: " + PlayerData.chosenColor);
                Debug.Log("Player name set to: " + PlayerData.playerName);
            }
            else
            {
                Debug.LogError("PlayerInfo component not found on the instantiated player prefab.");
            }
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("New player entered the room: " + newPlayer.NickName);
        // Optionally, you can handle additional logic when a new player joins the room.
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("Player left the room: " + otherPlayer.NickName);
        // Optionally, handle cleanup or other logic when a player leaves the room.
    }
}
