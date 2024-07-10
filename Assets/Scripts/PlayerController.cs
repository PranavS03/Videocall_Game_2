using Photon.Pun;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float movementSpeed = 10f;

    private Rigidbody2D rigidbody;
    private PhotonView photonView;
    private bool hasMoved = false;

    private Vector2 previousPosition = Vector2.zero;
    private Vector2 currentPosition = Vector2.zero;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        photonView = GetComponent<PhotonView>();
    }

    private void FixedUpdate()
    {
        previousPosition = currentPosition;
        currentPosition = transform.position;
        //To avoid moving all the players by a single player
        if (photonView.IsMine)
        {

            Vector3 movementDirection = InputManager.Instance.GetMovementDirectionNormalized();

            rigidbody.MovePosition(transform.position + (movementDirection * movementSpeed * Time.fixedDeltaTime));

            if (currentPosition - previousPosition != Vector2.zero)
            {
                hasMoved = true;
            }
            else
            {
                hasMoved = false;
            }
        }
    }

    public bool HasPlayerMoved()
    {
        return hasMoved;
    }
}
