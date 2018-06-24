using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

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
    public RuntimeAnimatorController ghostController;
    public RuntimeAnimatorController blueGhostController;
    public RuntimeAnimatorController pacmanController;
    public float PacmanVelocity;
    public float GhostVelocity;

    [SyncVar(hook = "ChangeName")]
    public string pacmanName;

    [SyncVar(hook = "ChangeColor")]
    public Color color = Color.white;

    Direction direction = Direction.left;
    PlayerType playerType = PlayerType.pacman;

    Rigidbody2D rb;
    Animator anim;
    SpriteRenderer sr;

    bool canMove = false;

    void Start()
    {
        canMove = false;

        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        rb.gravityScale = 0;
        rb.freezeRotation = true;
    }

    void Update()
    {
        if (rb.velocity.sqrMagnitude == 0)
            anim.speed = 0;
        else
            anim.speed = 1;

        if (!isLocalPlayer)
            return;
        if (!canMove)
            return;

        if (playerType == PlayerType.pacman)
        {
            SetDirectionPacman();
        }
        else if (playerType == PlayerType.ghost)
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

        if (playerType == PlayerType.pacman)
        {
            FixedUpdatePacman();
        }
        else if (playerType == PlayerType.ghost)
        {
            FixedUpdateGhost();
        }

    }
    void FixedUpdateGhost()
    {
        Vector2 vel = transform.right;

        if (direction == Direction.left)
        {
            vel = -transform.right;
        }
        else if (direction == Direction.right)
        {
            vel = transform.right;
        }
        else if (direction == Direction.up)
        {
            vel = transform.up;
        }
        else if (direction == Direction.down)
        {
            vel = -transform.up;
        }

        vel.Normalize();
        vel *= GhostVelocity;

        rb.velocity = vel;
    }
    void SetDirectionGhost()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            direction = Direction.left;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            direction = Direction.right;
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            direction = Direction.up;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            direction = Direction.down;
        }
    }

    void FixedUpdatePacman()
    {
        Vector2 vel = transform.right;
        vel.Normalize();
        vel *= PacmanVelocity;

        rb.velocity = vel;
    }
    void SetDirectionPacman()
    {

        if (direction != Direction.left)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                direction = Direction.left;
                transform.eulerAngles = new Vector3(0, 0, 180);
            }
        }
        if (direction != Direction.right)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                direction = Direction.right;
                transform.eulerAngles = new Vector3(0, 0, 0);
            }
        }
        if (direction != Direction.up)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                direction = Direction.up;
                transform.eulerAngles = new Vector3(0, 0, 90);
            }
        }
        if (direction != Direction.down)
        {
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                direction = Direction.down;
                transform.eulerAngles = new Vector3(0, 0, 270);
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isServer)
            return;
        if (playerType == PlayerType.pacman)
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
                {                    NetworkLobbyManager.singleton.StopServer();

                }
            }
            else if (collision.CompareTag("Enemy"))
            {
                NetworkLobbyManager.singleton.StopHost();
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
        anim.runtimeAnimatorController = ghostController;

        sr.color = color; // Color elegido por usuario
        tag = "Enemy";
        playerType = PlayerType.ghost;
        canMove = true;
    }
    [ClientRpc]
    public void RpcSetPacman()
    {
        anim.runtimeAnimatorController = pacmanController;

        sr.color = color; // Color elegido por usuario
        tag = "Player";
        playerType = PlayerType.pacman;
        canMove = true;
    }
    [ClientRpc]
    public void RpcBlue()
    {
        if (playerType != PlayerType.ghost)
            return;

        sr.color = Color.white;
        anim.runtimeAnimatorController = blueGhostController;
        tag = "EnemyBlue";
    }
    [ClientRpc]
    public void RpcTeleport(Vector3 Point)
    {
        if (!isLocalPlayer)
            return;
        rb.position = Point;
    }


    void ChangeName(string Name)
    {
        pacmanName = Name;
        name = Name;
    }

    void ChangeColor(Color Color)
    {
        color = Color;
        if (color == Color.clear)
            color = Color.white;
    }
}
