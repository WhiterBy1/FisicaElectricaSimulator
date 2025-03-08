using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private Transform target;
    private Rigidbody playerRb;
    float horizontalInput;
    float verticalInput;

    void Start()
    {
        playerRb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Control de entrada
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");


    }

    private void FixedUpdate()
    {
        // Movimiento del jugador
        Vector3 input = new Vector3(horizontalInput, 0f, verticalInput);
        playerRb.AddForce(input.normalized * speed);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Color aleatorio al colisionar
        Color nuevoColor = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
        GetComponent<Renderer>().material.color = nuevoColor;

    }

    
}