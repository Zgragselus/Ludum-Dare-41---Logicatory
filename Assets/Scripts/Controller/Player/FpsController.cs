﻿using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public static class Util
{
    public static Vector3 WithX(this Vector3 v, float x)
    {
        return new Vector3(x, v.y, v.z);
    }

    public static Vector3 WithY(this Vector3 v, float y)
    {
        return new Vector3(v.x, y, v.z);
    }

    public static Vector3 WithZ(this Vector3 v, float z)
    {
        return new Vector3(v.x, v.y, z);
    }

    public static Vector3 ToHorizontal(this Vector3 v)
    {
        return Vector3.ProjectOnPlane(v, Vector3.up);
    }

    public static Vector3 TransformDirectionHorizontal(this Transform t, Vector3 v)
    {
        return t.TransformDirection(v).ToHorizontal().normalized;
    }

    public static Vector3 InverseTransformDirectionHorizontal(this Transform t, Vector3 v)
    {
        return t.InverseTransformDirection(v).ToHorizontal().normalized;
    }

    public static Vector3 TransformDirection(this Transform t, Vector3 v)
    {
        return t.TransformDirection(v).normalized;
    }
}

public class FpsController : MonoBehaviour
{
    // It's better for camera to be a seperate object, not under the controller
    // Since we update the position in FixedUpdate(), it would cause a jittery vision
    [SerializeField]
    private Transform _camTransform;

    // Collision resolving is done with respect to these volumes
    // There are more than one
    // Because apart from the main capsule itself,
    // another one is needed for allowing 'ghost jumps'
    [SerializeField]
    private List<Collider> _collisionElements;

    // Collision will not happend with these layers
    // One of them has to be this controller's own layer
    [SerializeField]
    private LayerMask _excludedLayers;

    // Temporary visual for grappling hook
    [SerializeField]
    private GameObject _hookVisual;

    // The controller can collide with colliders within this radius
    private const float Radius = 2f;

    // Ad-hoc approach to make the controller accelerate faster
    private const float GroundAccelerationCoeff = 25.0f;

    // How fast the controller accelerates while it's not grounded
    private const float AirAccelCoeff = 2.5f;

    // Air deceleration occurs when the player gives an input that's not aligned with the current velocity
    private const float AirDecelCoeff = 5.0f;

    // Along a dimension, we can't go faster than this
    // This dimension is relative to the controller, not global
    // Meaning that "max speend along X" means "max speed along 'right side' of the controller"
    private float MaxSpeedAlongOneDimension = 6.0f;

    // How fast the controller decelerates on the grounded
    private const float Friction = 20.0f;

    // Stop if under this speed
    private const float FrictionSpeedThreshold = 0.1f;

    // Push force given when jumping
    private const float JumpStrength = 8.0f;

    // yeah...
    private const float Gravity = 25f;

    // How precise the controller can change direction while not grounded 
    private const float AirControlPrecision = 15.0f;

    // Caching this always a good practice
    private Transform _transform;

    // A crude way to look around, nothing fancy
    private MouseLook _mouseLook;

    // The code that handles hook related things
    private GrapplingHook _hook;

    // The real velocity of this controller
    private Vector3 _velocity;

    // Raw input taken with GetAxisRaw()
    private Vector3 _moveInput;

    // Caching...
    private Vector3 _screenMidPoint;

    // Some information to persist between frames or between Update() - FixedUpdate()
    private bool _isGroundedInThisFrame;
    private bool _isGroundedInPrevFrame;
    private bool _isGonnaJump;

    private readonly Collider[] _overlappingColliders = new Collider[10]; // Hope no more is needed

    private Vector3 _wishDirDebug;

    private bool _onLadder;

    public Vector3 _originalPosition;
    public float _hp;
    public GameObject _hpText;

    private void Start()
    {
        //Application.targetFrameRate = 60; // My laptop is shitty and burn itself to death if not for this
        _transform = transform;
        _mouseLook = new MouseLook(_camTransform);
        _screenMidPoint = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
        _onLadder = false;

        _originalPosition = transform.position;

        _hook = new GrapplingHook(_hookVisual, _screenMidPoint);
    }

    public void Damage(float hp)
    {
        _hp -= hp;
        if (_hp <= 0.0f)
        {
            _hp = 100.0f;
            transform.position = _originalPosition;
        }
    }

    // All logic, including controller displacement, happens here
    private void FixedUpdate()
    {
        var dt = Time.fixedDeltaTime;

        var wishDir = _camTransform.TransformDirectionHorizontal(_moveInput); // We want to go in this direction

        var collisionDisplacement = ResolveCollisions(ref _velocity);

        if (_isGroundedInThisFrame) // Ground move
        {
            var justLanded = !_isGroundedInPrevFrame && _isGroundedInThisFrame;

            // Don't apply friction if just landed or about to jump
            // TODO: Actually this can be extended to multiple frames, to make it easier
            // Currently you have to catch that frame to be able to bhop
            if (!justLanded && !_isGonnaJump)
            {
                ApplyFriction(ref _velocity, Friction, FrictionSpeedThreshold, dt);
            }

            Accelerate(ref _velocity, wishDir, MaxSpeedAlongOneDimension, GroundAccelerationCoeff, dt);

            _velocity.y = 0; // Ground movement always hard-resets vertical displacement
            if (_isGonnaJump)
            {
                _isGonnaJump = false;
                _velocity.y = JumpStrength;
            }

            if (_onLadder)
            {
                if (_moveInput.z > 0.01f)
                {
                    _velocity.y += _camTransform.forward.y * (GroundAccelerationCoeff * MaxSpeedAlongOneDimension * dt);
                    _velocity.y = Mathf.Clamp(_velocity.y, -MaxSpeedAlongOneDimension, MaxSpeedAlongOneDimension);
                }
                else if (_moveInput.z < -0.01f)
                {
                    _velocity.y += -_camTransform.forward.y * (GroundAccelerationCoeff * MaxSpeedAlongOneDimension * dt);
                    _velocity.y = Mathf.Clamp(_velocity.y, -MaxSpeedAlongOneDimension, MaxSpeedAlongOneDimension);
                }
                else
                {
                    _velocity.y = 0.0f;
                }
            }
        }
        else // Air move
        {
            // If the input doesn't have the same facing with the current velocity
            // then slow down instead of speeding up
            var coeff = Vector3.Dot(_velocity, wishDir) > 0 ? AirAccelCoeff : AirDecelCoeff;

            Accelerate(ref _velocity, wishDir, MaxSpeedAlongOneDimension, coeff, dt);

            if (Mathf.Abs(_moveInput.z) > 0.0001) // Pure side velocity doesn't allow air control
            {
                ApplyAirControl(ref _velocity, wishDir, dt);
            }

            if (!_onLadder)
            {
                _velocity.y -= Gravity * dt;
            }
            else
            {
                if (_moveInput.z > 0.01f)
                {
                    _velocity.y += _camTransform.forward.y * (GroundAccelerationCoeff * MaxSpeedAlongOneDimension * dt);
                    _velocity.y = Mathf.Clamp(_velocity.y, -MaxSpeedAlongOneDimension, MaxSpeedAlongOneDimension);
                }
                else if (_moveInput.z < -0.01f)
                {
                    _velocity.y += -_camTransform.forward.y * (GroundAccelerationCoeff * MaxSpeedAlongOneDimension * dt);
                    _velocity.y = Mathf.Clamp(_velocity.y, -MaxSpeedAlongOneDimension, MaxSpeedAlongOneDimension);
                }
                else
                {
                    _velocity.y = 0.0f;
                }
            }
        }

        _hook.ApplyHookAcceleration(ref _velocity, _transform.position);

        _transform.position += _velocity * dt; // Actual displacement
        _transform.position += collisionDisplacement; // Pushing out of environment
        _isGroundedInPrevFrame = _isGroundedInThisFrame;
    }

    // Input receiving happens here
    // We also bring the camera (which is a separate entity) to the controller position here
    private void Update()
    {
        _hpText.GetComponent<Text>().text = ((int)_hp).ToString();

        Cursor.lockState = CursorLockMode.Locked; // Keep doing this. We don't want cursor anywhere just yet

        var dt = Time.deltaTime;

        // We use GetAxisRaw, since we need it to feel as responsive as possible
        _moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;

        if (Input.GetKeyDown(KeyCode.Space) && !_isGonnaJump)
        {
            _isGonnaJump = true;
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            _isGonnaJump = false;
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            MaxSpeedAlongOneDimension = 10.0f;
        }
        else
        {
            MaxSpeedAlongOneDimension = 6.0f;
        }

        // Swing web with mouse button
        /*if (Input.GetMouseButtonDown(0))
        {
            _hook.CastHook();
        }
        if (Input.GetMouseButtonUp(0))
        {
            _hook.UncastHook();
        }*/

        _camTransform.position = Vector3.Lerp(_camTransform.position, _transform.position, dt * 200f);

        var mouseLookForward = _mouseLook.Update();
        _transform.rotation = Quaternion.LookRotation(mouseLookForward.WithY(0), Vector3.up); // Only rotate vertically

        // Reset player -- makes testing much easier
        /*if (Input.GetKeyDown(KeyCode.R))
        {
            _transform.position = Vector3.zero + Vector3.up * 2f;
            _velocity = Vector3.forward;
        }*/

        //_hook.Draw(_camTransform.position - Vector3.up * 0.4f);
    }

    private void Accelerate(ref Vector3 playerVelocity, Vector3 accelDir, float maxSpeedAlongOneDimension, float accelCoeff, float dt)
    {
        // How much speed we already have in the direction we want to speed up
        var projSpeed = Vector3.Dot(playerVelocity, accelDir);

        // How much speed we need to add (in that direction) to reach max speed
        var addSpeed = maxSpeedAlongOneDimension - projSpeed;
        if (addSpeed <= 0)
        {
            return;
        }

        // How much we are gonna increase our speed
        // maxSpeed * dt => the real deal. a = v * (1 / t)
        // accelCoeff => ad hoc approach to make it feel better
        var accelAmount = accelCoeff * maxSpeedAlongOneDimension * dt;

        // If we are accelerating more than in a way that we exceed maxSpeedInOneDimension, crop it to max
        if (accelAmount > addSpeed)
        {
            accelAmount = addSpeed;
        }

        playerVelocity += accelDir * accelAmount; // Magic happens here
    }

    private void ApplyFriction(ref Vector3 playerVelocity, float frictionCoeff, float frictionSpeedThreshold, float dt)
    {
        var speed = playerVelocity.magnitude;
        if (speed <= 0.00001)
        {
            return;
        }

        var downLimit = Mathf.Max(speed, frictionSpeedThreshold); // Don't drop below treshold
        var dropAmount = speed - (downLimit * frictionCoeff * dt);
        if (dropAmount < 0)
        {
            dropAmount = 0;
        }

        playerVelocity *= dropAmount / speed; // Reduce the velocity by a certain percent
    }

    private void ApplyAirControl(ref Vector3 playerVelocity, Vector3 accelDir, float dt)
    {
        // This only happens in the horizontal plane
        var playerDirHorz = playerVelocity.WithY(0).normalized;
        var playerSpeedHorz = playerVelocity.WithY(0).magnitude;

        var dot = Vector3.Dot(playerDirHorz, accelDir);
        if (dot > 0)
        {
            var k = AirControlPrecision * dot * dot * dt;

            // A little bit closer to accelDir
            playerDirHorz = playerDirHorz * playerSpeedHorz + accelDir * k;
            playerDirHorz.Normalize();

            // Assign new direction, without touching the vertical speed
            playerVelocity = (playerDirHorz * playerSpeedHorz).WithY(playerVelocity.y);
        }

    }

    // Calculates the displacement required in order not to be in a world collider
    private Vector3 ResolveCollisions(ref Vector3 playerVelocity)
    {
        _isGroundedInThisFrame = false;

        // Get nearby colliders
        Physics.OverlapSphereNonAlloc(_transform.position, Radius + 0.5f,
            _overlappingColliders, ~_excludedLayers);

        //Debug.Log("++++++++++++++");
        //foreach (var overlappingCollider in _overlappingColliders)
        //{
        //    if (overlappingCollider != null)
        //    {
        //        Debug.Log("gonna check with : " + overlappingCollider.name);
        //    }
        //}
        //Debug.Log("--------------");

        var totalDisplacement = Vector3.zero;
        var checkedColliderIndices = new HashSet<int>();
        foreach (var playerColl in _collisionElements)
        {
            // If the player is intersecting with that environment collider, separate them
            for (var i = 0; i < _overlappingColliders.Length; i++)
            {
                // Two player colliders shouldn't resolve collision with the same environment collider
                if (checkedColliderIndices.Contains(i))
                {
                    continue;
                }

                var envColl = _overlappingColliders[i];

                // Skip empty slots
                if (envColl == null)
                {
                    continue;
                }

                if (envColl.isTrigger)
                {
                    continue;
                }

                Vector3 collisionNormal;
                float collisionDistance;

                if (Physics.ComputePenetration(
                    playerColl, playerColl.transform.position, playerColl.transform.rotation, // TODO: These could be cached
                    envColl, envColl.transform.position, envColl.transform.rotation,
                    out collisionNormal, out collisionDistance))
                {
                    // If the collision pointing up to some degree
                    // then it means we're standing on something
                    if (Vector3.Dot(collisionNormal, Vector3.up) > 0.5) // TODO: This actually can be bound to a variable
                    {
                        _isGroundedInThisFrame = true;
                    }

                    // Ignore very small penetrations
                    // Required for standing still on slopes
                    // ... still far from perfect though
                    if (collisionDistance < 0.01)
                    {
                        continue;
                    }

                    checkedColliderIndices.Add(i);

                    // Get outta that collider!
                    totalDisplacement += collisionNormal * collisionDistance;

                    // Crop down the velocity component which is in the direction of penetration
                    playerVelocity -= Vector3.Project(playerVelocity, collisionNormal);
                }
                else
                {
                    //Debug.Log("doesn't collide with : " + envColl.bounds.Contains(_transform.position));
                }
            }
        }

        // It's better to be in a clean state in the next resolve call
        for (var i = 0; i < _overlappingColliders.Length; i++)
        {
            _overlappingColliders[i] = null;
        }

        return totalDisplacement;
    }

    public void ResetAt(Transform t)
    {
        _transform.position = t.position + Vector3.up * 0.5f;
        _camTransform.position = _transform.position;
        _mouseLook.LookAt(t.position + t.forward);
        _velocity = t.TransformDirection(Vector3.forward);
    }

    public void SetLadder(bool ladder)
    {
        _onLadder = ladder;
    }
}