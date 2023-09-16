using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIRange : MonoBehaviour
{
    public static AIRange instance;

    public const int SIZE = 60;

    [SerializeField] public AIBlob aiBlob;
    [SerializeField] private LayerMask layerMask;

    public Collider2D target;
    public Collider2D threat;

    private int targetTimestamp;
    private int threatTimestamp;

    public float size;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        StartCoroutine(RangeCoroutine());
    }

    private void FixedUpdate()
    {
        if(target != null && (target.attachedRigidbody.IsSleeping() || target.gameObject == null || Vector3.Distance(target.transform.position, transform.position) > size || !Utils.CanEat(transform, target.transform)))
        {
            ClearTarget();
        }

        if(threat != null && (threat.attachedRigidbody.IsSleeping() || threat.gameObject == null || Vector3.Distance(threat.transform.position, transform.position) > size || !Utils.CanEat(threat.transform, transform)))
        {
            ClearThreat();
        }
    }

    IEnumerator RangeCoroutine()
    {
        yield return new WaitForSeconds(0.0012f);

        Cast();

        StartCoroutine(RangeCoroutine());
    }

    public void Cast()
    {
        if(!aiBlob.spawned)
        {
            return;
        }

        Collider2D collider = Physics2D.OverlapCircle(aiBlob.transform.position, size, layerMask);
        if(collider == null)
        {
            return;
        }

        string tag = collider.tag;
        int colliderId = collider.gameObject.GetInstanceID();
        if(tag != "Player" && tag != "PlayerClone" && tag != "AI" && tag != "AIClone" || colliderId == gameObject.GetInstanceID())
        {
            return;
        }

        if(Utils.CanEat(aiBlob.transform, collider.transform))
        {
            if(target == null || Utils.CanEat(collider.transform, target.transform))
            {
                SetTarget(collider);
            }
        }
        else if(Utils.CanEat(collider.transform, aiBlob.transform))
        {
            if(threat == null || Utils.CanEat(collider.transform, threat.transform))
            {
                SetThreat(collider);
            }
        }
    }

    private void SetTarget(Collider2D c)
    {
        if(threat != null || target != null && target.gameObject.GetInstanceID() == c.gameObject.GetInstanceID())
        {
            return;
        }

        target = c;
        targetTimestamp = Utils.secondsSinceEpoch();

        aiBlob.aiMovement.state = AIMovement.State.Chasing;
        aiBlob.aiMovement.roamingPosition = Vector2.zero;
    }

    private void ClearTarget()
    {
        target = null;
        targetTimestamp = 0;
        
        var state = threat != null ? AIMovement.State.Running : AIMovement.State.Roaming;
        aiBlob.aiMovement.state = state;
    }

    private void SetThreat(Collider2D c)
    {
        if(threat != null && threat.gameObject.GetInstanceID() == c.gameObject.GetInstanceID())
        {
            return;
        }

        threat = c;
        threatTimestamp = Utils.secondsSinceEpoch();

        aiBlob.aiMovement.state = AIMovement.State.Running;
        aiBlob.aiMovement.roamingPosition = Vector2.zero;

        ClearTarget();
    }

    private void ClearThreat()
    {
        threat = null;
        threatTimestamp = 0;
        
        var state = target != null ? AIMovement.State.Chasing : AIMovement.State.Roaming;
        aiBlob.aiMovement.state = state;
    }

    private void OnDrawGizmos()
    {
        Vector3 scale = transform.localScale;

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(aiBlob.transform.position, size);
    }
}
