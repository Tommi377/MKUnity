using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Strategy Game Camera with Unity's New Input System
// By One Wheel Studio
// https://www.youtube.com/watch?v=3Y7TFN_DsoI
public class CameraControl : MonoBehaviour {
    [SerializeField] private Player _player;

    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float damping = 15f;
    private float speed;

    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float zoomDamping = 7.5f;
    [SerializeField] private float stepSize = 2f;
    [SerializeField] private float maxHeight = 20f;
    [SerializeField] private float minHeight = 3f;
    private float zoomHeight;

    [SerializeField] private float maxRotateSpeed = 2f;
    [SerializeField][Range(0f, 0.1f)] private float edgeTolerance = 0.05f;
    [SerializeField] private bool useScreenEdge = true;

    private CameraControlActions cameraActions;
    private InputAction movement;
    private Transform cameraTransform;

    private Vector3 targetPosition;
    private Vector3 horizontalVelocity;
    private Vector3 lastPosition;
    private Vector3 startDrag;

    private void Awake() {
        cameraActions = new CameraControlActions();
        cameraTransform = GetComponentInChildren<Camera>().transform;
    }

    private void OnEnable() {
        zoomHeight = cameraTransform.localPosition.y;
        cameraTransform.LookAt(transform);

        lastPosition = transform.position;
        movement = cameraActions.Camera.Movement;
        cameraActions.Camera.Rotate.performed += RotateCamera;
        cameraActions.Camera.Zoom.performed += ZoomCamera;
        cameraActions.Camera.Enable();
    }

    private void OnDisable() {
        cameraActions.Disable();
        cameraActions.Camera.Rotate.performed -= RotateCamera;
        cameraActions.Camera.Zoom.performed -= ZoomCamera;
    }

    private void Update() {
        GetKeyboardMovement();
        CheckMousetAtScreenEdge();

        UpdateVelocity();
        UpdatecameraPosition();
        UpdateBasePosition();
    }

    private Vector3 GetCameraRight() => new Vector3(cameraTransform.right.x, 0, cameraTransform.right.z).normalized;
    private Vector3 GetCameraForward() => new Vector3(cameraTransform.forward.x, 0, cameraTransform.forward.z).normalized;

    private void GetKeyboardMovement() {
        Vector3 inputValue = movement.ReadValue<Vector2>().x * GetCameraRight() + movement.ReadValue<Vector2>().y * GetCameraForward();

        inputValue = inputValue.normalized;

        if (inputValue.sqrMagnitude > 0.1f) {
            targetPosition += inputValue;
        }
    }

    private void UpdateVelocity() {
        horizontalVelocity = (transform.position - lastPosition) / Time.deltaTime;
        horizontalVelocity.y = 0;
        lastPosition = transform.position;
    }

    private void UpdateBasePosition() {
        if (targetPosition.sqrMagnitude > 0.1f) {
            speed = Mathf.Lerp(speed, maxSpeed, Time.deltaTime * acceleration);
            transform.position += targetPosition * speed * Time.deltaTime;
        } else {
            horizontalVelocity = Vector3.Lerp(horizontalVelocity, Vector3.zero, Time.deltaTime * damping);
            transform.position += horizontalVelocity * Time.deltaTime;
        }

        targetPosition = Vector3.zero;
    }

    private void RotateCamera(InputAction.CallbackContext inputValue) {
        if(!Mouse.current.rightButton.isPressed) return;

        float value = inputValue.ReadValue<Vector2>().x;
        transform.rotation = Quaternion.Euler(0f, value * maxRotateSpeed + transform.rotation.eulerAngles.y, 0f);
    }

    private void ZoomCamera(InputAction.CallbackContext inputValue) {
        float value = -inputValue.ReadValue<Vector2>().y / 100f;

        if (Mathf.Abs(value) > 0.1f) {
            zoomHeight = cameraTransform.localPosition.y + value * stepSize;
            zoomHeight = Mathf.Clamp(zoomHeight, minHeight, maxHeight);
        }
    }

    private void UpdatecameraPosition() {
        Vector3 zoomTarget = new Vector3(cameraTransform.localPosition.x, zoomHeight, cameraTransform.localPosition.z);
        zoomTarget -= zoomSpeed * (zoomHeight - cameraTransform.localPosition.y) * Vector3.forward;

        cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, zoomTarget, Time.deltaTime * zoomDamping);
        cameraTransform.LookAt(transform);
    }

    private void CheckMousetAtScreenEdge() {
        if (!useScreenEdge) return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector3 moveDirection = Vector3.zero;

        if (mousePosition.x < edgeTolerance * Screen.width)
            moveDirection -= GetCameraRight();
        else if (mousePosition.x > (1f - edgeTolerance) * Screen.width)
            moveDirection += GetCameraRight();


        if (mousePosition.y < edgeTolerance * Screen.height)
            moveDirection -= GetCameraForward();
        else if (mousePosition.y > (1f - edgeTolerance) * Screen.height)
            moveDirection += GetCameraForward();

        targetPosition += moveDirection;
    }
}
