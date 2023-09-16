using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public const float MOVEMENT_SPEED_MIN = 14.0f;
    public const float MOVEMENT_SPEED_MAX = 244.0f;
    //public const float MOVEMENT_SPEED_MIN = 6.0f;
    //public const float MOVEMENT_SPEED_MAX = 33.0f;

    public static PlayerMovement instance;

    [SerializeField] public PlayerBlob playerBlob;
    
    [SerializeField] public Joystick joystick;

    public float movementSpeed = MOVEMENT_SPEED_MAX;

    public Vector3 lastJoystickDirection = Vector3.zero;
    public Vector3 oldPosition = Vector3.zero;

    private void Awake()
    {
        instance = this;
    }

    private void Start() 
    {
        Despawn(true);
    }

    private void FixedUpdate() 
    {
        if(!playerBlob.spawned)
        {
            return;
        }

        playerBlob.rigidBody.AddRelativeForce((Vector3)joystick.Direction * (movementSpeed * playerBlob.rigidBody.mass * Time.fixedDeltaTime), ForceMode2D.Impulse);
        //transform.position += (Vector3)joystick.Direction * movementSpeed;

        if(joystick.Direction != Vector2.zero)
        {
            lastJoystickDirection = joystick.Direction;

            /*if(!playerBlob.playerHud.blobPointer.gameObject.activeSelf)
            {
                playerBlob.playerHud.blobPointer.gameObject.SetActive(true);
            }

            Vector3 dir = Camera.main.ViewportToScreenPoint(lastJoystickDirection) - joystick.transform.position.normalized;
            Quaternion q = Quaternion.AngleAxis(Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f, Vector3.forward);
            playerBlob.playerHud.blobPointer.rotation = q;*/
        } 
        else 
        {
            /*if(playerBlob.playerHud.blobPointer.gameObject.activeSelf)
            {
                playerBlob.playerHud.blobPointer.gameObject.SetActive(false);
            }*/
        }

        Map.BorderCheck(playerBlob.rigidBody);
    }

    public void Spawn()
    {
        if(playerBlob.spawned)
        {
            return;
        }

        joystick.gameObject.SetActive(true);
    }

    public void Despawn(bool bypass = false)
    {
        if(!playerBlob.spawned && !bypass)
        {
            return;
        }

        SetMovementSpeed(MOVEMENT_SPEED_MAX);

        joystick.ResetHandlePosition();
        joystick.ResetInput();

        if(lastJoystickDirection != Vector3.zero)
        {
            lastJoystickDirection = Vector3.zero;
        }
        
        joystick.gameObject.SetActive(false);
    }

    public void SetMovementSpeed(float speed)
    {
        if(speed < MOVEMENT_SPEED_MIN)
        {
            speed = MOVEMENT_SPEED_MIN;
        }
        if(speed > MOVEMENT_SPEED_MAX)
        {
            speed = MOVEMENT_SPEED_MAX;
        }

        movementSpeed = speed;// / 65
    }
}
