using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerClone : MonoBehaviour
{
    public PlayerBlob fakeParent;

    [SerializeField] private SortingGroup sortingGroup;
    [SerializeField] public BlobCircle blobCircle;
    [SerializeField] public CircleCollider2D circleCollider;
    [SerializeField] public Rigidbody2D rigidBody;

    public float movementSpeed = PlayerMovement.MOVEMENT_SPEED_MAX;

    private long shootMassTimestamp;
    private long splitTimestamp;
    private int shrinkTimestamp;

    public Color currentColor;

    public bool recombine = false;
    private bool killed = false;

    public Vector2 nextScale = new Vector2(PlayerBlob.BLOB_SCALE_MIN, PlayerBlob.BLOB_SCALE_MIN);
    public Vector2 direction;

    private void Start()
    {
        float mass = rigidBody.mass;
        float scale = transform.localScale.x;
        rigidBody.AddRelativeForce(direction * ((scale + 30) * (rigidBody.mass / 2)) * 7, ForceMode2D.Impulse);

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

        if(shrinkTimestamp + 1 < Utils.secondsSinceEpoch())
        {
            shrink(rigidBody.mass * 0.005f);

            shrinkTimestamp = Utils.secondsSinceEpoch();
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
        
        bool isMoving = fakeParent.playerMovement.joystick.Direction != Vector2.zero;
        if(isMoving)
        {
            rigidBody.AddRelativeForce((Vector3)fakeParent.playerMovement.joystick.Direction * (movementSpeed * rigidBody.mass * Time.fixedDeltaTime), ForceMode2D.Impulse);
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

        foreach (PlayerClone clone in fakeParent.clones)
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
            case "Player":
                if(other.gameObject.GetInstanceID() != fakeParentId)
                {
                    Physics2D.IgnoreCollision(collider, circleCollider, true);
                }
                break;
            case "PlayerClone":
                if(collider.gameObject.GetComponent<PlayerClone>().fakeParent.gameObject.GetInstanceID() != fakeParentId)
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
        int exp = 0;
        switch(other.gameObject.tag)
        {
            case "Food":
                Game.instance.eatFood(other.gameObject);
                Utils.PlaySound(fakeParent.playerSettings, Game.instance.foodConsumeSound, transform.position, 1.25f);
                fakeParent.playerStats.foodEaten++;
                mass = 1;
                exp = 1;
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

        if(exp > 0) fakeParent.playerStats.AddExperience(exp);
        if(mass > 0) grow(mass, checkSize);
    }

    public void grow(float amount, bool checkSize = true)
    {
        if(checkSize && amount >= rigidBody.mass)
        {
            return;
        }

        AddMass(amount);
        fakeParent.AddTotalMass(amount);

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

    public void shrink(float amount, bool checkSize = true, bool local = false, bool instant = false)
    {
        if(checkSize && amount >= rigidBody.mass)
        {
            return;
        }
        
        SubtractMass(amount);
        if(!local)
        {
            fakeParent.SubtractTotalMass(amount);
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
        if(instant)
        {
            transform.localScale = nextScale;
        }
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
            Vector3 offset = transform.position + fakeParent.playerMovement.lastJoystickDirection * ((transform.localScale.x / 2) + (mass / 10));
            GameObject obj = Instantiate(fakeParent.playerMassPrefab, offset, Quaternion.identity);

            PlayerMass script = obj.GetComponent<PlayerMass>();
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

            Vector3 offset = transform.position + fakeParent.playerMovement.lastJoystickDirection;
            GameObject obj = Instantiate(fakeParent.playerClonePrefab, offset, Quaternion.identity);

            PlayerClone component = obj.GetComponent<PlayerClone>();
            fakeParent.clones.Add(component);

            component.fakeParent = fakeParent;
            component.currentColor = fakeParent.currentColor;
            component.rigidBody.mass = halfMass;
            component.direction = fakeParent.playerMovement.lastJoystickDirection;

            float c = Mathf.Sqrt(component.rigidBody.mass * Utils.SCALE_MULTIPLIER);
            component.setScale(new Vector2(c, c), true);

            shrink(halfMass, false, true, true);

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
                    GameObject obj = Instantiate(fakeParent.playerClonePrefab, transform.position, Quaternion.identity);

                    PlayerClone component = obj.GetComponent<PlayerClone>();
                    fakeParent.clones.Add(component);

                    component.fakeParent = fakeParent;
                    component.currentColor = fakeParent.currentColor;
                    component.rigidBody.mass = newMass;
                    component.direction = fakeParent.playerMovement.lastJoystickDirection;

                    Vector3 dir = (component.transform.position - transform.position).normalized;
                    component.rigidBody.AddForce(dir * 100, ForceMode2D.Impulse);

                    float c = Mathf.Sqrt(component.rigidBody.mass * Utils.SCALE_MULTIPLIER);
                    component.setScale(new Vector2(c, c), true);

                    fakeParent.AddRecombineDuration(transform.localScale.x);

                    splitTimestamp = Utils.millisecondsSinceEpoch();
                }
            }

            shrink(rigidBody.mass - newMass, false, true);
            fakeParent.recombineTime = Utils.secondsSinceEpoch();
            fakeParent.playerHud.recombine.gameObject.SetActive(true);
        }

        if(mass > 0)
        {
            grow(mass, false);
        }
    }

    private void AddMass(float amount)
    {
        rigidBody.mass += amount;
    }

    private void SubtractMass(float amount)
    {
        rigidBody.mass -= amount;
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

        movementSpeed = speed;
    }

    IEnumerator ShrinkCoroutine()
    {
        yield return new WaitForSeconds(1f);

        shrink(rigidBody.mass * 0.0015f);

        StartCoroutine(ShrinkCoroutine());
    }
}
