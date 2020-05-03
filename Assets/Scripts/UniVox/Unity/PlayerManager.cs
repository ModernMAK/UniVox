using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerManager : MonoBehaviour
{
    [SerializeField] private float _maxSpeed;
    [SerializeField] private float _jumpSpeed;
    private CharacterController _characterController;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
    }


    // Update is called once per frame
    void Update()
    {
        var deltaX = Input.GetAxis("Horizontal");
        var deltaZ = Input.GetAxis("Vertical");
        var axisAlignedVelocity = new Vector3(deltaX, 0, deltaZ).normalized * _maxSpeed;
        
        var localVelocity = transform.rotation * axisAlignedVelocity;
        
        if (!_characterController.isGrounded)
        {
            localVelocity += Physics.gravity;
        }
        else
        {
            if (Input.GetKey(KeyCode.Space))
                localVelocity += Vector3.up * _jumpSpeed;
        }

        _characterController.Move(localVelocity * Time.deltaTime);
    }
}