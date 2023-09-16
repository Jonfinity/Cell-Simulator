using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class AIClone : MonoBehaviour
{
    public AIBlob fakeParent;

    [SerializeField] private SortingGroup sortingGroup;
    [SerializeField] public BlobCircle blobCircle;
    [SerializeField] public CircleCollider2D circleCollider;
    [SerializeField] public Rigidbody2D rigidBody;

    public float movementSpeed = PlayerMovement.MOVEMENT_SPEED_MAX;

    private long shootMassTimestamp;
    private long splitTimestamp;

    public Color currentColor;

    public bool recombine = false;
    private bool killed = false;

    public Vector2 nextScale = new Vector2(PlayerBlob.BLOB_SCALE_MIN, PlayerBlob.BLOB_SCALE_MIN);

    private void Start() 
    {
        float mass = rigidBody.mass;
        float scale = transform.localScale.x;
        rigidBody.AddRelativeForce(fakeParent.aiMovement.lastDirection * ((scale + 20) * (rigidBody.mass / 2)) * 7, ForceMode2D.Impulse);

        Color color = fakeParent.currentColor;
        blobCircle.DrawLine(100, 0.5f, color);
        blobCircle.DrawFilledMesh(100, 0.5f, color);
        blobCircle.Spawn();

        UpdateOrderLayer((int)scale);

        StartCoroutine(ShrinkCoroutine());
    }

    private void FixedUpdate() 
    {
        if(rigidBody.drag < 10)
        {
            rigidBody.drag += 0.03f;
        }
        else if(rigidBody.drag > 10)
        {
            rigidBody.drag = 10;
        }

        if(rigidBody.mass > fakeParent.rigidBody.mass)
        {
            fakeParent.SwapWithClone(this);
        }

        Vector2 scale = transform.localScale;
        if(scale.x != nextScale.x)
        {
            float sub = (scale.x * 0.007f);
            if(sub > 2.5f)
            {
                sub = 2.5f;
            }
            setScale(Vector2.Lerp(scale, nextScale, (5f - sub) * Time.fixedDeltaTime));
        }
        
        bool isMoving = fakeParent.aiMovement.direction != Vector2.zero;
        if(isMoving)
        {
            rigidBody.AddRelativeForce((Vector3)fakeParent.aiMovement.direction * (movementSpeed * rigidBody.mass * Time.fixedDeltaTime), ForceMode2D.Impulse);
        }

        float multiplier = 60f;
        if(recombine)
        {
            if(!circleCollider.isTrigger)
            {
                circleCollider.isTrigger = true;
            }

            multiplier = 100f;
        }
        else
        {
            if(circleCollider.isTrigger)
            {
                circleCollider.isTrigger = false;
            }
        }

        if(isMoving)
        {
            multiplier /= 7;
        }

        float calc = (scale.x * multiplier) - 200;
        if(calc < 1)
        {
            calc = 1;
        }

        foreach (AIClone clone in fakeParent.clones)
        {
            if(clone != null)
            {
                if(recombine || !rigidBody.IsTouching(clone.circleCollider))
                {
                    rigidBody.AddRelativeForce((clone.transform.position - transform.position).normalized * calc,  ForceMode2D.Impulse);
                }
            }
        }

        if(recombine || !rigidBody.IsTouching(fakeParent.circleCollider))
        {
            rigidBody.AddRelativeForce((fakeParent.transform.position - transform.position).normalized * calc,  ForceMode2D.Impulse);
        }

        Map.BorderCheck(rigidBody);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        Collider2D collider = other.collider;
        if(collider == null)
        {
            return;
        }

        int fakeParentId = fakeParent.gameObject.GetInstanceID();
        switch(other.gameObject.tag)
        {
            case "AI":
                if(other.gameObject.GetInstanceID() != fakeParentId)
                {
                    Physics2D.IgnoreCollision(collider, circleCollider, true);
                }
                break;
            case "AIClone":
                if(collider.gameObject.GetComponent<AIClone>().fakeParent.gameObject.GetInstanceID() != fakeParentId)
                {
                    Physics2D.IgnoreCollision(collider, circleCollider, true);
                }
                break;
            default:
                Physics2D.IgnoreCollision(collider, circleCollider, true);
                return;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        float mass = 0;
        bool checkSize = true;
        switch(other.gameObject.tag)
        {
            case "Food":
                Game.instance.eatFood(other.gameObject);
                mass = 1;
                break;
            case "PlayerMass":
            case "AIMass":
                Destroy(other.gameObject);
                mass = other.attachedRigidbody.mass;
                checkSize = false;
                break;
            case "Player":
            case "PlayerClone":
            case "AI":
            case "AIClone":
                if(!Utils.CanEat(other.transform, transform))
                {
                    return;
                }

                Vector3 myPosition = transform.position;
                Vector3 otherPosition = other.transform.position;
                if(other.OverlapPoint(myPosition - (myPosition - otherPosition).normalized * (-transform.localScale.x / 6)))
                {
                    switch(other.gameObject.tag)
                    {
                        case "Player":
                            other.GetComponent<PlayerBlob>().grow(rigidBody.mass, false);
                            break;
                        case "PlayerClone":
                            other.GetComponent<PlayerClone>().grow(rigidBody.mass, false);
                            break;
                        case "AI":
                            other.GetComponent<AIBlob>().grow(rigidBody.mass, false);
                            break;
                        case "AIClone":
                            other.GetComponent<AIClone>().grow(rigidBody.mass, false);
                            break;
                    }

                    kill();
                    return;
                }
                break;
            default:
                return;
        }

        if(mass > 0) grow(mass, checkSize);
    }

    public void grow(float amount, bool checkSize = true)
    {
        if(checkSize && amount >= rigidBody.mass)
        {
            return;
        }

        rigidBody.mass += amount;
        fakeParent.totalMass += amount;
        
        float c = Mathf.Sqrt(rigidBody.mass * Utils.SCALE_MULTIPLIER);

        if(c < PlayerBlob.BLOB_SCALE_MIN)
        {
            c = PlayerBlob.BLOB_SCALE_MIN;
        }
        if(c > PlayerBlob.BLOB_SCALE_MAX)
        {
            c = PlayerBlob.BLOB_SCALE_MAX;
        }

        nextScale = new Vector2(c, c);
    }

    public void shrink(float amount, bool checkSize = true, bool local = false)
    {
        if(checkSize && amount >= rigidBody.mass)
        {
            return;
        }
        
        rigidBody.mass -= amount;
        if(!local)
        {
            fakeParent.totalMass -= amount;
        }

        float c = Mathf.Sqrt(rigidBody.mass * Utils.SCALE_MULTIPLIER);
        
        if(c < PlayerBlob.BLOB_SCALE_MIN)
        {
            c = PlayerBlob.BLOB_SCALE_MIN;
        }
        if(c > PlayerBlob.BLOB_SCALE_MAX)
        {
            c = PlayerBlob.BLOB_SCALE_MAX;
        }
        
        nextScale = new Vector2(c, c);
    }

    public void setScale(Vector3 vector, bool includeNext = false)
    {
        if(includeNext)
        {
            nextScale = vector;
        }
        
        transform.localScale = vector;
        float scale = transform.localScale.x;

        UpdateOrderLayer((int)scale);

        SetMovementSpeed(Mathf.Sqrt(5000 / ((scale + 6) * 0.01f)));
    }

    public void shootMass()
    {
        if(shootMassTimestamp + 30 > Utils.millisecondsSinceEpoch())
        {
            return;
        }
        
        float mass = Utils.MIN_SHOT_MASS_SIZE + (transform.localScale.x / 10);
        if(mass > Utils.MAX_SHOT_MASS_SIZE)
        {
            mass = Utils.MAX_SHOT_MASS_SIZE;
        }
        if(mass < Utils.MIN_SHOT_MASS_SIZE)
        {
            mass = Utils.MIN_SHOT_MASS_SIZE;
        }

        if(rigidBody.mass - mass >= PlayerBlob.MASS_MIN)
        {
            Vector3 offset = transform.position + (Vector3)fakeParent.aiMovement.lastDirection * ((transform.localScale.x / 2) + (mass / 10));
            GameObject obj = Instantiate(fakeParent.aiMassPrefab, offset, Quaternion.identity);

            AIMass script = obj.GetComponent<AIMass>();
            script.fakeParent = fakeParent;
            script.fakeParentClone = this;
            script.rigidBody.mass = mass;

            shrink(mass * 2);
            
            shootMassTimestamp = Utils.millisecondsSinceEpoch();
        }
    }

    public void split()
    {
        if(splitTimestamp + 30 > Utils.millisecondsSinceEpoch())
        {
            return;
        }

        if(circleCollider.isTrigger)
        {
            circleCollider.isTrigger = false;
        }

        float halfMass = rigidBody.mass / 2;
        if(halfMass >= PlayerBlob.MASS_MIN)
        {
            if(fakeParent.clones.Count >= PlayerBlob.CLONES_MAX)
            {
                return;
            }
            
            GameObject obj = Instantiate(fakeParent.aiClonePrefab, transform.position, Quaternion.identity);

            AIClone component = obj.GetComponent<AIClone>();
            fakeParent.clones.Add(component);

            component.fakeParent = fakeParent;
            component.currentColor = fakeParent.currentColor;
            component.rigidBody.mass = halfMass;

            float c = Mathf.Sqrt(component.rigidBody.mass * Utils.SCALE_MULTIPLIER);
            component.setScale(new Vector2(c, c), true);

            shrink(halfMass, false, true);

            fakeParent.AddRecombineDuration(transform.localScale.x);

            splitTimestamp = Utils.millisecondsSinceEpoch();
        }
    }

    public void explode(float mass = 0)
    {
        if(splitTimestamp + 3 > Utils.millisecondsSinceEpoch())
        {
            return;
        }

        if(circleCollider.isTrigger)
        {
            circleCollider.isTrigger = false;
        }

        int pieces = PlayerBlob.CLONES_MAX - fakeParent.clones.Count;
        if(pieces <= PlayerBlob.CLONES_MAX && pieces > 0)
        {
            float newMass = rigidBody.mass / (pieces + 1);//CLONES_MAX starts at 0..
            for (var i = 0; i < pieces; i++)
            {
                if(newMass >= PlayerBlob.MASS_MIN)
                {
                    GameObject obj = Instantiate(fakeParent.aiClonePrefab, transform.position, Quaternion.identity);
                    
                    AIClone component = obj.GetComponent<AIClone>();
                    fakeParent.clones.Add(component);

                    component.fakeParent = fakeParent;
                    component.currentColor = fakeParent.currentColor;
                    component.rigidBody.mass = newMass;

                    float c = Mathf.Sqrt(component.rigidBody.mass * Utils.SCALE_MULTIPLIER);
                    component.setScale(new Vector2(c, c), true);

                    fakeParent.AddRecombineDuration(transform.localScale.x);

                    splitTimestamp = Utils.millisecondsSinceEpoch();
                }
            }

            shrink(rigidBody.mass - newMass, false, true);
            fakeParent.recombineTime = Utils.secondsSinceEpoch();
        }

        if(mass > 0)
        {
            grow(mass, false);
        }
    }

    public void kill(bool isRecombine = false)
    {
        if(killed)
        {
            return;
        }

        if(isRecombine)
        {
            fakeParent.grow(rigidBody.mass, false, true);
        }
        else
        {
            fakeParent.SubtractTotalMass(rigidBody.mass);//might cause problems
        }

        int count = fakeParent.clones.Count;
        int index = fakeParent.clones.IndexOf(this);

        if(index >= 0 && index <= count)
        {
            fakeParent.clones.RemoveAt(index);
        }

        if(fakeParent.clones.Count <= 0)
        {
            fakeParent.Recombine();
        }

        killed = true;

        StopAllCoroutines();

        Destroy(gameObject);
    }

    private void UpdateOrderLayer(int order)
    {
        order += 99;//should be 100, but just for a cleaner look, always have the main blob above clones
        sortingGroup.sortingOrder = order;
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

    IEnumerator ShrinkCoroutine()
    {
        yield return new WaitForSeconds(1f);

        shrink(rigidBody.mass * 0.0015f);

        StartCoroutine(ShrinkCoroutine());
    }
}
