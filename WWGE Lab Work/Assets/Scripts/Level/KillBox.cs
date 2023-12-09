using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> A script that respawns the player at its position when they enter an attached trigger.</summary>
public class KillBox : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CharacterController controller = other.GetComponent<CharacterController>();

            controller.enabled = false;
            other.transform.position = transform.position;
            controller.enabled = true;
        }
    }
}
