using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scoring : MonoBehaviour
{
    private bool scored = false; // Ensures only one score is counted

    private void OnCollisionEnter(Collision collision)
    {
        if (scored) return; // Ignore multiple collisions

        // Get the first contact point
        ContactPoint contact = collision.contacts[0];
        Collider hitCollider = contact.otherCollider; // Get the collider that was hit

        int score = 0;

        if (hitCollider.CompareTag("Center"))
            score = 50;
        else if (hitCollider.CompareTag("Red"))
            score = 30;
        else if (hitCollider.CompareTag("Green"))
            score = 20;

        if (score > 0)
        {
            scored = true; // Prevents duplicate scoring
            Debug.Log("First Contact: " + hitCollider.tag + " | Score: " + score);
            GameManager.Instance.UpdateScore(score);
        }

        
    }
}
