using Photon.Pun;
using UnityEngine;

[DefaultExecutionOrder(-1)]
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

    private void SpawnPlayer()
    {
        Vector2 spawnPosition = new Vector2(Random.Range(minXBoundary, maxXBoundary), Random.Range(minYBoundary, maxYBoundary));
        GameObject playerGameObject = PhotonNetwork.Instantiate(playerPrefab.name, spawnPosition, Quaternion.identity);
        
        Debug.Log("Player instantiated at position: " + spawnPosition);

        PlayerInfo playerInfo = playerGameObject.GetComponent<PlayerInfo>();

        playerInfo.SetColor(PlayerData.chosenColor);
        playerInfo.SetName(PlayerData.playerName);

        Debug.Log("Player color set to: " + PlayerData.chosenColor);
        Debug.Log("Player name set to: " + PlayerData.playerName);
    }
}
