/**
 * PlayerInput.cs
 * Description: This script handles the input from a shooter.
 * Programmer: Khoi Ho
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

[RequireComponent(typeof(CharacterMovement))]
[RequireComponent(typeof(ShooterCameraMovement))]
[RequireComponent(typeof(CharacterCombat))]
[RequireComponent(typeof(InputLock))]
public class ShooterInput : NetworkBehaviour
{
    // References to components.
    public CharacterMovement characterMovement;
    public ShooterCameraMovement characterCameraMovement;
    public CharacterCombat characterCombat;

    // Used for locking input.
    public InputLock inputLock;

    public class InputData : INetworkSerializable
    {
        // Move direction
        public float horizontalAxis;
        public float verticalAxis;

        // Camera rotation
        public float mouseXAxis;
        public float mouseYAxis;

        // Jump
        public bool jump;

        // Fire a projectile
        public bool fireProjectile;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref horizontalAxis);
            serializer.SerializeValue(ref verticalAxis);

            serializer.SerializeValue(ref mouseXAxis);
            serializer.SerializeValue(ref mouseYAxis);

            serializer.SerializeValue(ref jump);

            serializer.SerializeValue(ref fireProjectile);
        }
    }
    InputData inputData = new InputData();

    private void Awake()
    {
        if(!characterMovement)
        {
            characterMovement = GetComponent<CharacterMovement>();
        }

        if(!characterCameraMovement)
        {
            characterCameraMovement = GetComponent<ShooterCameraMovement>();
        }

        if(!characterCombat)
        {
            characterCombat = GetComponent<CharacterCombat>();
        }

        if(!inputLock)
        {
            inputLock = GetComponent<InputLock>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner)
            return;

        if (inputLock.isLocked)
            return;

        GetInput(inputData);

        // Move camera.
        characterCameraMovement.RotateCamera(inputData.mouseXAxis, inputData.mouseYAxis);

        // Jump.
        if(inputData.jump)
        {
            characterMovement.Jump();
        }

        // Fire a projectile.
        if(inputData.fireProjectile)
        {
            characterCombat.FireProjectile();
        }
    }

    private void FixedUpdate()
    {
        if (!IsOwner)
            return;

        if (inputLock.isLocked)
            return;

        // Move character.
        Vector3 worldMovementDirection = new Vector3(inputData.horizontalAxis, 0.0f, inputData.verticalAxis);
        characterMovement.Move(worldMovementDirection);
    }

    // Get input from the player.
    void GetInput(InputData inputData)
    {
        if (IsServer && !IsHost)
            return;

        inputData.horizontalAxis = Input.GetAxisRaw("Horizontal");
        inputData.verticalAxis = Input.GetAxisRaw("Vertical");

        inputData.mouseXAxis = Input.GetAxis("Mouse X");
        inputData.mouseYAxis = Input.GetAxis("Mouse Y");

        inputData.jump = Input.GetButtonDown("Jump");

        inputData.fireProjectile = Input.GetButtonDown("Fire1");
    }

    // Predict the movement of the character on the client side.
    void Predict(InputData inputData)
    {
        if (IsServer)
            return;     
    }
}
