using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeTrigger : MonoBehaviour
{
    public GameObject player;

    private void Awake()
    {
        player = GameObject.Find("Player");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player)
        {
            PlayerController playerScript = player.GetComponent<PlayerController>();
            playerScript.isTouchingWallrunnable = true;
            
        }
    }

    // Maybe dont need this cuz changed collider to 
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject == player)
        {
            PlayerController playerScript = player.GetComponent<PlayerController>();
            playerScript.isTouchingWallrunnable = true;
            
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == player)
        {
            PlayerController playerScript = player.GetComponent<PlayerController>();
            playerScript.isTouchingWallrunnable = false;
        }
    }
}
