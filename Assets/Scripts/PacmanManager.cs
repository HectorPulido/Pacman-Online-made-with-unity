using UnityEngine;
using Mirror;

public class PacmanManager : NetworkBehaviour
{
    public static PacmanManager singleton;

    [Range(0, 1)]
    public float specialFoodPosibility;
    public Vector2 size;
    public float circleScale;
    public GameObject foodPrefab;
    public GameObject specialFoodPrefab;

    [HideInInspector]
    public int FoodCount;
    private int Pacman;
    private Pacman[] Players;
    private NetworkStartPosition[] SpawnPoints;

    public void Start()
    {
        if (singleton != null)
        {
            Destroy(gameObject);
            return;
        }
        singleton = this;

        SpawnPoints = GameObject.FindObjectsOfType<NetworkStartPosition>();
    }

    [ServerCallback]
    public void StartPacman()
    {
        CreateFood();
        ChoosePacman();
    }

    public void ChoosePacman()
    {
        Players = GameObject.FindObjectsOfType<Pacman>();

        Pacman = Random.Range(0, Players.Length);
        for (int i = 0; i < Players.Length; i++)
        {
            if (i == Pacman)
                Players[i].RpcSetPacman();
            else
                Players[i].RpcSetGhost();
        }
    }

    public void CreateFood()
    {
        FoodCount = 0;
        var parent = new GameObject("Foods");

        for (int i = 0; i * circleScale < size.x; i++)
        {
            for (int j = 0; j * circleScale < size.y; j++)
            {
                var special = Random.Range(0f, 1f) < specialFoodPosibility;
                var position = new Vector3(i * circleScale - size.x / 2f, j * circleScale - size.y / 2f, 0);
                if (Physics2D.CircleCast(position, circleScale, Vector2.zero).collider == null)
                {
                    var goToInstantiate = special ? specialFoodPrefab : foodPrefab;
                    var prefabToInstantiate = Instantiate(goToInstantiate,
                                position,
                                Quaternion.identity,
                                parent.transform);

                    NetworkServer.Spawn(prefabToInstantiate);
                    FoodCount++;
                }
            }
        }

    }

    public void SpecialFoodEated()
    {
        for (int i = 0; i < Players.Length; i++)
        {
            Players[i].RpcBlue();
        }
        Invoke("SetNormal", 15);
    }

    public void SetNormal()
    {
        for (int i = 0; i < Players.Length; i++)
        {
            if (i == Pacman)
                continue;
            Players[i].RpcSetGhost();
        }
        CancelInvoke();
        Invoke("SetNormal", 15);
    }

    public Vector3 GetRandomSpawnPoint()
    {
        var i = Random.Range(0, SpawnPoints.Length);

        return SpawnPoints[i].transform.position;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(size.x, size.y, 1));
    }

}
