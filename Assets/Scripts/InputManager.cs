using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private PlayerControls playerControls;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        playerControls = new PlayerControls();

        playerControls.Enable();
    }

    public Vector2 GetMovementDirectionNormalized()
    {
        Vector2 movementDirection = playerControls.Player.Movement.ReadValue<Vector2>();

        return movementDirection.normalized;
    }
}
