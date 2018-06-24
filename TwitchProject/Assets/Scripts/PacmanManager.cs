using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PacmanManager : NetworkBehaviour
{
    public static PacmanManager singleton;

    public int FoodCount;

    int Pacman;
    Pacman[] Players;
    NetworkStartPosition[] SpawnPoints;


    public IEnumerator Start()
    {
        if (singleton != null)
        {
            Destroy(gameObject);
        }
        else
        {
            singleton = this;
        }
        FoodCount = GameObject.FindGameObjectsWithTag("Food").Length;
        SpawnPoints = GameObject.FindObjectsOfType<NetworkStartPosition>();

        yield return new WaitForSeconds(1);
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

}
