using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDetection : MonoBehaviour
{
    [SerializeField] private GameObject hit; 

    void OnCollisionEnter(Collision collision)
    {
        // Check if the other object has the layer "Target"
        if (collision.gameObject.layer == LayerMask.NameToLayer("Target"))
        {
            ContactPoint point = collision.contacts[0];
            GameObject hitEffect = Instantiate(hit, point.point, Quaternion.LookRotation(point.normal));
            Destroy(hitEffect, 1f);
        }

        // Destroy this game object after 0.2 seconds
        Destroy(gameObject, 0.2f);
    }
}
