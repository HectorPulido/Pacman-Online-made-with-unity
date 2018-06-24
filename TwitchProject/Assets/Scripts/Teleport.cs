using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleport : MonoBehaviour {

    public Transform otherPortal;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var pacman = collision.GetComponent<Pacman>();
        pacman.RpcTeleport(otherPortal.position);
    }
}
