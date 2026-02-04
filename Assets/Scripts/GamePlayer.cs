using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class GamePlayer : NetworkBehaviour
{
    [SerializeField]
    public readonly float MoveSpeed = 8f, RotationSmoothTime = 0f, SpeedChangeRate = 0.3f;
    private float rotationVelocity = 0f;

    [SerializeField]
    private new Rigidbody rigidbody = null;

    [SerializeField]
    private PlayerInput playerInput;
    public Vector2 inputMove { get => playerInput.actions["Move"].ReadValue<Vector2>(); }
    public Vector2 inputAim { get => playerInput.actions["Look"].ReadValue<Vector2>(); }

    [SerializeField]
    private GameObject laserPrefab;
    [SerializeField]
    private Transform laserSpawn;

    [SyncVar]
    private int ammo = 3;
    private int hitLayer;
    [SyncVar]
    public string PlayerName = "Player";
    [SyncVar]
    public int PlayerIndex = 0;
    [SyncVar]
    public int TeamID = 0;
    [SyncVar]
    public int PlayerColourIndex = 0;

    [SerializeField]
    public NetworkGameManager gameManager;

    public void AmmoChanged(int oldAmmoCount, int newAmmoCount) { }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        name = "Local Player";
        playerInput.actions["Fire"].performed += OnFire;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        hitLayer = LayerMask.NameToLayer("");
        //Debug.Log("Start Client");
        if (!isLocalPlayer)
        {
            Destroy(playerInput);
            playerInput = null;
        }
    }

    public void Setup(NetworkGameManager gameManager, int playerID)
    {
        this.gameManager = gameManager;
        PlayerIndex = playerID;
        hitLayer = LayerMask.NameToLayer("Player" + playerID);
        gameObject.layer = hitLayer;
    }

    private void FixedUpdate()
    {
        if (isLocalPlayer)
        {
            Move();
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                FireBolt();
            }
        }
    }

    [Command]
    public void FireBolt()
    {
        if (ammo > 0)
        {
            gameManager.SpawnProjectile(PlayerIndex, transform.position, transform.rotation);
            ammo--;
        }
    }

    public void Move()
    {
        float deltaTime = Time.deltaTime;
        float speed = 0f;
        float targetSpeed = inputMove == Vector2.zero ? 0f : MoveSpeed * inputMove.magnitude;

        float currentSpeed = new Vector2(rigidbody.linearVelocity.x, rigidbody.linearVelocity.z).magnitude;

        if (Mathf.Abs(targetSpeed - currentSpeed) > 0.1f)
        {
            speed = Mathf.Lerp(currentSpeed, targetSpeed, deltaTime * SpeedChangeRate);
        }

        Vector3 moveDirection = new Vector3(inputMove.x, 0f, inputMove.y).normalized;

        if (inputMove != Vector2.zero || inputAim != Vector2.zero)
        {
            Vector3 aimDirection = inputAim != Vector2.zero ?
                new Vector3(inputAim.x, 0f, inputAim.y).normalized : moveDirection;

            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y,
                Mathf.Atan2(aimDirection.x, aimDirection.z) * Mathf.Rad2Deg,
                ref rotationVelocity,
                RotationSmoothTime);

            transform.rotation = Quaternion.Euler(0f, rotation, 0f);
        }

        moveDirection *= speed;
        Vector3 relativeMoveDir = transform.worldToLocalMatrix * moveDirection;

        //animator.SetFloat("X", relativeMoveDir.x / MoveSpeed);
        //animator.SetFloat("Y", relativeMoveDir.z / MoveSpeed);
        Vector3 gravVel = rigidbody.linearVelocity.y * Vector3.up;
        rigidbody.linearVelocity = moveDirection + gravVel;
        rigidbody.angularVelocity = Vector3.zero;
        //animator.SetFloat("MoveSpeed", speed);
        //if (gravVel.y < -1)
        //    animator.SetBool("Fall", true);
    }

    public void OnFire(InputAction.CallbackContext ctx)
    {
        FireBolt();
    }

    [Command]
    public void CMDFire()
    {
        if (ammo > 0)// && !Physics.CheckSphere(laserSpawn.position, 0.05f, LayerMask.GetMask("Default")))
        {
            GameObject laser = Instantiate(laserPrefab, laserSpawn.position, laserSpawn.rotation);
            NetworkServer.Spawn(laser, connectionToClient);
            laser.name = "TestLaser";
            //laser.SetActive(false);
            //laser.GetComponent<Laser>().Setup(PlayerIndex, hitLayer, Color.green, () => CMDUpdateAmmo(1));
            //CMDUpdateAmmo(-1);
        }
    }

    [Command]
    private void CMDUpdateAmmo(int change)
    {
        ammo += change;
    }
}
