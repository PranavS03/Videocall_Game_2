using Photon.Pun;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class PlayersSpawner : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;

    [Header("Boundaries for Spawning the Player")]

    [SerializeField] private float minXBoundary;
    [SerializeField] private float maxXBoundary;
    [SerializeField] private float minYBoundary;
    [SerializeField] private float maxYBoundary;

    private void Start()
    {
        //Spawn every player at a randomly generated position
        //Vector2 spawnPosition = new Vector2(Random.Range(minXBoundary, maxXBoundary), Random.Range(minYBoundary, maxYBoundary));

        //GameObject playerGameObject = PhotonNetwork.Instantiate(playerPrefab.name, spawnPosition, Quaternion.identity);

        PlayerInformationUI.OnPlayerInfoDone += PlayerInformationUI_OnPlayerInfoDone;
    }

    private void PlayerInformationUI_OnPlayerInfoDone(object sender, PlayerInformationUI.OnPlayerInfoDoneEventArgs e)
    {
        GameObject playerGameObject = SpawnPlayer();

        SpriteRenderer playerSpriteRenderer = playerGameObject.GetComponent<SpriteRenderer>();
        PlayerInfo playerInfo = playerGameObject.GetComponent<PlayerInfo>();

        playerSpriteRenderer.material.color = e.color;
        playerInfo.SetColor(e.color);
        playerInfo.SetName(e.playerName);
    }

    private GameObject SpawnPlayer()
    {
        Vector2 spawnPosition = new Vector2(Random.Range(minXBoundary, maxXBoundary), Random.Range(minYBoundary, maxYBoundary));

        return PhotonNetwork.Instantiate(playerPrefab.name, spawnPosition, Quaternion.identity);
    }
}
