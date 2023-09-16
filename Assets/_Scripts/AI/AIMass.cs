using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIMass : MonoBehaviour
{
    public AIMass instance;

    public AIBlob fakeParent;
    public AIClone fakeParentClone;

    [SerializeField] public BlobCircle blobCircle;
    [SerializeField] private CircleCollider2D circleCollider;
    [SerializeField] public Rigidbody2D rigidBody;

    private int lifetimeTimestamp;

    private void Awake() 
    {
        instance = this;

        lifetimeTimestamp = Utils.secondsSinceEpoch() + 30;
    }

    private void Start() 
    {
        float mass = rigidBody.mass;
        float scale = transform.localScale.x;
        rigidBody.AddRelativeForce(fakeParent.aiMovement.lastDirection * ((scale * mass) * 12), ForceMode2D.Impulse);

        float c = Mathf.Sqrt(mass * Utils.MASS_SCALE_MULTIPLIER);
        transform.localScale = new Vector2(c, c);

        Color color = fakeParent.currentColor;
        blobCircle.DrawLine(20, 0.2f, color);
        blobCircle.DrawFilledMesh(20, 0.2f, color);
        blobCircle.Spawn();

        Vector3 velo = Vector3.zero;
        if(fakeParent != null)
        {
            velo = fakeParent.rigidBody.velocity;
        }
        else if(fakeParentClone != null)
        {
            velo = fakeParentClone.rigidBody.velocity;
        }

        if(velo != Vector3.zero)
        {
            rigidBody.AddRelativeForce(velo * 10, ForceMode2D.Impulse);
        }
    }

    private void Update()
    {
        if(lifetimeTimestamp < Utils.secondsSinceEpoch())
        {
            Destroy(gameObject);
        }
    }

    private void FixedUpdate()
    {
        Map.BorderCheck(rigidBody);
    }
}
