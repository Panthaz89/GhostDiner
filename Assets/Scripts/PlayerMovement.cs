using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;

    private CharacterController controller;
    private Camera mainCamera;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        mainCamera = Camera.main;
    }

    private void Update()
    {
        // Get input (WASD / Arrow keys)
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 inputDir = new Vector3(horizontal, 0f, vertical).normalized;

        // Convert input direction relative to camera
        Vector3 moveDir = Vector3.zero;
        if (inputDir.sqrMagnitude > 0.01f)
        {
            Vector3 camForward = mainCamera.transform.forward;
            Vector3 camRight = mainCamera.transform.right;

            camForward.y = 0f;
            camRight.y = 0f;

            camForward.Normalize();
            camRight.Normalize();

            moveDir = (camForward * inputDir.z + camRight * inputDir.x).normalized;

            // Smoothly rotate towards movement direction
            transform.forward = Vector3.Slerp(transform.forward, moveDir, rotationSpeed * Time.deltaTime);
        }

        // Always move (even if moveDir = 0)
        controller.Move(moveDir * moveSpeed * Time.deltaTime);

        float floatHeight = .5f;    // how high to float
        float floatSpeed = 2f;        // how fast to bob
        float newY = 2 + Mathf.Sin(Time.time * floatSpeed) * floatHeight;

        // Apply it on top of current grounded position
        Vector3 pos = transform.position;
        pos.y = newY;
        transform.position = pos;
    }
}