using Photon.Pun;
using UnityEngine;

public class PlayerCollisionsHandler : MonoBehaviour
{
    private const string PLAYER = "Player";

    [SerializeField] private float waitTimeBeforeJoiningChannel = 1f;

    private PlayerInfo player;
    private PlayerController playerController;
    private PhotonView photonView;

    private void Awake()
    {
        player = GetComponent<PlayerInfo>();
        playerController = GetComponent<PlayerController>();
        photonView = GetComponent<PhotonView>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!photonView.IsMine) return;

        if(collision.gameObject.CompareTag(PLAYER))
        {
            AgoraManager.Instance.JoinChannel(player, collision.GetComponent<PlayerInfo>());
        }
    }
     
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!photonView.IsMine) return;
        if (collision.gameObject.CompareTag(PLAYER) && playerController.HasPlayerMoved())
        {
            AgoraManager.Instance.LeaveChannel();
        }
        else
        {
            AgoraManager.Instance.LeaveChannelIfNoOtherUsersPresent();
        }
    }
}
