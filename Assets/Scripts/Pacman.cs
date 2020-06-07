using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public enum Direction
{
    up, down, right, left
}
public enum PlayerType
{
    ghost, pacman
}

public class Pacman : NetworkBehaviour
{

    [SyncVar(hook = "ChangeName")]
    public string pacmanName;

    [SyncVar(hook = "ChangeColor")]
    public Color color = Color.white;

    public RuntimeAnimatorController ghostController;
    public RuntimeAnimatorController blueGhostController;
    public RuntimeAnimatorController pacmanController;
    public float PacmanVelocity;
    public float GhostVelocity;

    Direction Direction = Direction.left;
    PlayerType PlayerType = PlayerType.pacman;

    Rigidbody2D SelfRigidBody;
    Animator SelfAnimator;
    SpriteRenderer SelfSpriteRenderer;

    bool canMove = false;

    void Awake()
    {
        canMove = false;

        SelfRigidBody = GetComponent<Rigidbody2D>();
        SelfAnimator = GetComponent<Animator>();
        SelfSpriteRenderer = GetComponent<SpriteRenderer>();

        SelfRigidBody.gravityScale = 0;
        SelfRigidBody.freezeRotation = true;
    }

    void Update()
    {
        if (SelfRigidBody.velocity.sqrMagnitude == 0)
            SelfAnimator.speed = 0;
        else
            SelfAnimator.speed = 1;

        if (!isLocalPlayer)
            return;
        if (!canMove)
            return;

        if (PlayerType == PlayerType.pacman)
        {
            SetDirectionPacman();
        }
        else if (PlayerType == PlayerType.ghost)
        {
            SetDirectionGhost();
        }
    }


    void FixedUpdate()
    {
        if (!isLocalPlayer)
            return;
        if (!canMove)
            return;

        if (PlayerType == PlayerType.pacman)
        {
            FixedUpdatePacman();
        }
        else if (PlayerType == PlayerType.ghost)
        {
            FixedUpdateGhost();
        }

    }
    void FixedUpdateGhost()
    {
        Vector2 vel = transform.right;

        if (Direction == Direction.left)
        {
            vel = -transform.right;
        }
        else if (Direction == Direction.right)
        {
            vel = transform.right;
        }
        else if (Direction == Direction.up)
        {
            vel = transform.up;
        }
        else if (Direction == Direction.down)
        {
            vel = -transform.up;
        }

        vel.Normalize();
        vel *= GhostVelocity;

        SelfRigidBody.velocity = vel;
    }
    void SetDirectionGhost()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Direction = Direction.left;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Direction = Direction.right;
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            Direction = Direction.up;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            Direction = Direction.down;
        }
    }

    void FixedUpdatePacman()
    {
        Vector2 vel = transform.right;
        vel.Normalize();
        vel *= PacmanVelocity;

        SelfRigidBody.velocity = vel;
    }
    void SetDirectionPacman()
    {

        if (Direction != Direction.left)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                Direction = Direction.left;
                transform.eulerAngles = new Vector3(0, 0, 180);
            }
        }
        if (Direction != Direction.right)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                Direction = Direction.right;
                transform.eulerAngles = new Vector3(0, 0, 0);
            }
        }
        if (Direction != Direction.up)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                Direction = Direction.up;
                transform.eulerAngles = new Vector3(0, 0, 90);
            }
        }
        if (Direction != Direction.down)
        {
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                Direction = Direction.down;
                transform.eulerAngles = new Vector3(0, 0, 270);
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isServer)
            return;
        if (PlayerType == PlayerType.pacman)
        {
            if (collision.CompareTag("Food"))
            {
                if (collision.GetComponent<SpecialFood>() != null)
                {
                    PacmanManager.singleton.SpecialFoodEated();
                }
                NetworkServer.Destroy(collision.gameObject);

                PacmanManager.singleton.FoodCount--;
                if (PacmanManager.singleton.FoodCount <= 0)
                {
                    NetworkManager.singleton.StopServer();
                }
            }
            else if (collision.CompareTag("Enemy"))
            {
                NetworkManager.singleton.StopServer();
            }
            else if (collision.CompareTag("EnemyBlue"))
            {
                var blue = collision.GetComponent<Pacman>();

                blue.RpcTeleport(PacmanManager.singleton.GetRandomSpawnPoint());
                blue.Invoke("RpcSetGhost", 0.1f);
            }
        }
    }

    [ClientRpc]
    public void RpcSetGhost()
    {
        SelfAnimator.runtimeAnimatorController = ghostController;

        SelfSpriteRenderer.color = color; // Color elegido por usuario
        tag = "Enemy";
        PlayerType = PlayerType.ghost;
        canMove = true;
    }
    [ClientRpc]
    public void RpcSetPacman()
    {
        SelfAnimator.runtimeAnimatorController = pacmanController;

        SelfSpriteRenderer.color = color; // Color elegido por usuario
        tag = "Player";
        PlayerType = PlayerType.pacman;
        canMove = true;
    }
    [ClientRpc]
    public void RpcBlue()
    {
        if (PlayerType != PlayerType.ghost)
            return;

        SelfSpriteRenderer.color = Color.white;
        SelfAnimator.runtimeAnimatorController = blueGhostController;
        tag = "EnemyBlue";
    }
    [ClientRpc]
    public void RpcTeleport(Vector3 Point)
    {
        if (!isLocalPlayer)
            return;
        SelfRigidBody.position = Point;
    }

    void ChangeName(string OldName, string Name)
    {
        pacmanName = Name;
        name = Name;
    }

    void ChangeColor(Color OldColor, Color Color)
    {
        color = Color;
        if (color == Color.clear)
            color = Color.white;
    }
}
