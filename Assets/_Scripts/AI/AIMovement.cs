using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIMovement : MonoBehaviour
{
    public static AIMovement instance;

    [SerializeField] public AIBlob aiBlob;
    
    public State state;
    
    public float movementSpeed = PlayerMovement.MOVEMENT_SPEED_MAX;

    public Vector2 direction = Vector2.zero;
    public Vector2 lastDirection = Vector2.zero;
    public Vector2 roamingPosition = Vector2.zero;

    public enum State
    {
        Roaming,
        Chasing,
        Running,
    }

    private void Awake()
    {
        instance = this;

        state = State.Roaming;
    }

    private void Start() 
    {
        spawn();
    }

    private void FixedUpdate() 
    {
        if(!aiBlob.spawned)
        {
            return;
        }

        Vector3 pos = Vector3.zero;
        switch(state)
        {
            case State.Chasing:
                Collider2D target = aiBlob.aiRange.target;
                if(target != null)
                {
                    pos = -(aiBlob.transform.position - target.transform.position).normalized;
                }
                break;
            case State.Running:
                Collider2D threat = aiBlob.aiRange.threat;
                if(threat != null)
                {
                    pos = (aiBlob.transform.position - threat.transform.position).normalized;
                }
                break;
            default:
                if(roamingPosition == Vector2.zero || Vector3.Distance(transform.position, roamingPosition) < 30 || Random.Range(1f, 1000f) > 995f)//999.5f
                {
                    GenerateRoamingPosition();
                }
                break;
        }
        
        if(pos != Vector3.zero)
        {
            direction = (pos - aiBlob.transform.position).normalized;
            lastDirection = direction;
            aiBlob.rigidBody.AddRelativeForce(pos * (movementSpeed * aiBlob.rigidBody.mass * Time.fixedDeltaTime), ForceMode2D.Impulse);
            //transform.position += pos;
        }
        else
        {
            aiBlob.rigidBody.AddRelativeForce(direction * (movementSpeed * aiBlob.rigidBody.mass * Time.fixedDeltaTime), ForceMode2D.Impulse);
            //transform.position = Vector3.MoveTowards(transform.position, roamingPosition, movementSpeed);
        }

        Map.BorderCheck(aiBlob.rigidBody);
    }

    public void spawn()
    {
        if(aiBlob.spawned)
        {
            return;
        }

        GenerateRoamingPosition();
    }

    public void despawn(bool bypass = false)
    {
        if(!aiBlob.spawned && !bypass)
        {
            return;
        }

        SetMovementSpeed(PlayerMovement.MOVEMENT_SPEED_MAX);

        direction = Vector2.zero;
        lastDirection = Vector2.zero;
        roamingPosition = Vector2.zero;
    }

    private void GenerateRoamingPosition()
    {
        Vector2 pos = Map.GenerateRandomPosition();
        direction = (pos - (Vector2)aiBlob.transform.position).normalized;
        lastDirection = direction;
        roamingPosition = pos;

        if(state != State.Roaming && aiBlob.aiRange.target == null && aiBlob.aiRange.threat == null)
        {
            state = State.Roaming;
        }
    }

    public void SetMovementSpeed(float speed)
    {
        if(speed < PlayerMovement.MOVEMENT_SPEED_MIN)
        {
            speed = PlayerMovement.MOVEMENT_SPEED_MIN;
        }
        if(speed > PlayerMovement.MOVEMENT_SPEED_MAX)
        {
            speed = PlayerMovement.MOVEMENT_SPEED_MAX;
        }

        movementSpeed = speed;// / 65
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(roamingPosition, 10);
    }
}
