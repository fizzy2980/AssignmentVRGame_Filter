using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIStayFront : MonoBehaviour
{

    public Transform player;
    public Vector3 offset = new Vector3(0, 1.5f, 2);
    public float positionSmoothSpeed = 5f;  
    public float rotationSmoothSpeed = 5f;  

    void Update()
    {
        if (player == null) return;

        
        Vector3 targetPosition = player.position + player.forward * offset.z + player.up * offset.y;


        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * positionSmoothSpeed);


        Quaternion targetRotation = Quaternion.LookRotation(transform.position - player.position);


        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSmoothSpeed);
    }
}
