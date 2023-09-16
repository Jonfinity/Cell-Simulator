using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class AIBlob : MonoBehaviour
{
    public static AIBlob instance;
    
    public string username;
    
    public bool spawned = false;

    public float totalMass = PlayerBlob.MASS_MIN;

    public Vector2 nextScale = new Vector2(PlayerBlob.BLOB_SCALE_MIN, PlayerBlob.BLOB_SCALE_MIN);

    [SerializeField] public UniversalPlayer universalPlayer;
    [SerializeField] public BlobCircle blobCircle;
    [SerializeField] public AIHud aiHud;
    [SerializeField] public AIMovement aiMovement;
    [SerializeField] public AIRange aiRange;
    
    [SerializeField] public SortingGroup sortingGroup;
    [SerializeField] public GameObject aiMassPrefab;
    [SerializeField] public GameObject aiClonePrefab;
    [SerializeField] public Rigidbody2D rigidBody;
    [SerializeField] public CircleCollider2D circleCollider;

    private long shootMassTimestamp;
    private long splitTimestamp;

    public Color currentColor;
    public List<AIClone> clones;

    public int recombineTime = -1;
    public int recombineDuration;

    private void Awake()
    {
        instance = this;
    }

    private void Start() 
    {
        despawn(true);
    }

    private void Update()   
    {
        if(!spawned)
        {
            spawn();
        }
    }

    private void FixedUpdate() 
    {
        if(!spawned)
        {
            return;
        }

        if(Random.Range(0, 10000) > 9999)
        {
            grow(Random.Range(1, 10));
        }

        if(rigidBody.drag < 10)
        {
            rigidBody.drag += 0.03f;
        }
        else if(rigidBody.drag > 10)
        {
            rigidBody.drag = 10;
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

        if(aiMovement.direction == Vector2.zero)
        {
            float calc = ((transform.localScale.x * 60f) - 200);
            if(calc < 1)
            {
                calc = 1;
            }

            foreach (AIClone clone in clones)
            {
                if(clone != null)
                {
                    if(clone.recombine || !rigidBody.IsTouching(clone.circleCollider))
                    {
                        rigidBody.AddRelativeForce((clone.transform.position - transform.position).normalized * calc, ForceMode2D.Impulse);
                    }
                }
            }
        }

        int count = clones.Count;
        if(count > 0 && recombineTime != -1)
        {
            if(IsRecombining())
            {
                Recombine();
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        Collider2D collider = other.collider;
        if(collider == null)
        {
            return;
        }

        switch(other.gameObject.tag)
        {
            case "AI":
                if(other.gameObject.GetInstanceID() != gameObject.GetInstanceID())
                {
                    Physics2D.IgnoreCollision(collider, circleCollider, true);
                }
                break;
            case "AIClone":
                if(other.gameObject.GetComponent<AIClone>().fakeParent.gameObject.GetInstanceID() != gameObject.GetInstanceID())
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
        if(!spawned)
        {
            return;
        }

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
                bool boolean = Utils.CanEat(other.transform, transform);
                if(other.gameObject.tag != "AIClone" && !boolean)
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
                            PlayerBlob componentA = other.GetComponent<PlayerBlob>();
                            componentA.playerStats.AddExperience((int)rigidBody.mass / 100);
                            componentA.grow(rigidBody.mass, false);
                            break;
                        case "PlayerClone":
                            PlayerClone componentB = other.GetComponent<PlayerClone>();
                            componentB.fakeParent.playerStats.AddExperience((int)rigidBody.mass / 100);
                            componentB.grow(rigidBody.mass, false);
                            break;
                        case "AI":
                            other.GetComponent<AIBlob>().grow(rigidBody.mass, false);
                            break;
                        case "AIClone":
                            AIClone component = other.GetComponent<AIClone>();
                            if(component.fakeParent.gameObject.GetInstanceID() == gameObject.GetInstanceID())
                            {
                                if(component.recombine)
                                {
                                    component.kill(true);
                                }
                                return;
                            }
                            else
                            {
                                if(!boolean)
                                {
                                    return;
                                }
                                
                                component.grow(rigidBody.mass, false);
                            }
                            break;
                    }

                    if(clones.Count > 0)
                    {
                        AIClone biggestClone = null;
                        float minimumSize = 0;
                        foreach (AIClone cloneObject in clones)
                        {
                            if(cloneObject != null)
                            {
                                float scale = transform.localScale.x;
                                if (scale > minimumSize)
                                {
                                    biggestClone = cloneObject;
                                    minimumSize = scale;
                                }
                            }
                        }
                        if(biggestClone != null)
                        {
                            SwapWithClone(biggestClone);
                            biggestClone.kill();
                        }
                        
                        return;
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

    public void spawn()
    {
        if(spawned)
        {
            return;
        }

        universalPlayer.AddToGame();

        currentColor = Utils.generateBlobColor();
        blobCircle.DrawLine(100, 0.5f, currentColor);
        blobCircle.DrawFilledMesh(100, 0.5f, currentColor);

        blobCircle.Spawn();
        aiHud.Spawn();
        
        rigidBody.WakeUp();
        
        circleCollider.enabled = true;

        transform.position = Map.GenerateRandomPosition();
        aiMovement.spawn();//this is here because I need the updated position from above

        UpdateOrderLayer((int)transform.localScale.x);

        aiRange.size = AIRange.SIZE + (transform.localScale.x / 2);

        for(int i = 0; i <= 3; i++)
        {
            Game.instance.spawnFood();
        }
        
        Game.instance.spawnWhiteHole();

        StartCoroutine(RecombineDurationCoroutine());
        StartCoroutine(ShrinkCoroutine());

        spawned = true;
    }

    public void despawn(bool bypass = false)
    {
        if(!spawned && !bypass)
        {
            return;
        }

        universalPlayer.RemoveFromGame();
        universalPlayer.totalMass = 0;

        blobCircle.Despawn(bypass);
        aiHud.Despawn(bypass);
        aiMovement.despawn(bypass);

        shootMassTimestamp = 0;
        splitTimestamp = 0;

        spawned = false;

        rigidBody.velocity = Vector2.zero;
        rigidBody.mass = PlayerBlob.MASS_MIN;
        totalMass = PlayerBlob.MASS_MIN;

        Vector2 scaleMin = new Vector2(PlayerBlob.BLOB_SCALE_MIN, PlayerBlob.BLOB_SCALE_MIN);
        nextScale = scaleMin;
        transform.localScale = scaleMin;

        rigidBody.Sleep();

        circleCollider.enabled = false;
        transform.position = Vector2.zero;

        aiRange.size = AIRange.SIZE;
        
        for (var i = 0; i < clones.Count; i++)
        {
            if(clones[i] != null)
            {
                Destroy(clones[i].gameObject);
            }
        }

        clones.Clear();
        recombineTime = -1;
        recombineDuration = 0;

        if(!circleCollider.isTrigger)
        {
            circleCollider.isTrigger = true;
        }

        StopAllCoroutines();
    }

    public void kill()
    {
        despawn();
    }

    public void grow(float amount, bool checkSize = true, bool local = false)
    {
        if(!spawned)
        {
            return;
        }

        if(checkSize && amount >= rigidBody.mass)
        {
            return;
        }

        AddMass(amount);
        if(!local)
        {
            AddTotalMass(amount);
        }
        
        checkMass();

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
        if(!spawned)
        {
            return;
        }

        if(checkSize && amount >= rigidBody.mass)
        {
            return;
        }
        
        SubtractMass(amount);
        if(!local)
        {
            SubtractTotalMass(amount);
        }
        checkMass();
        
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

    private void setScale(Vector3 vector, bool includeNext = false)
    {
        if(!spawned)
        {
            return;
        }

        if(includeNext)
        {
            nextScale = vector;
        }

        transform.localScale = vector;
        float scale = transform.localScale.x;

        UpdateOrderLayer((int)scale);

        aiRange.size = AIRange.SIZE + (scale / 2);
        aiMovement.SetMovementSpeed(Mathf.Sqrt(5000 / ((scale + 6) * 0.01f)));

        aiHud.blobScore.SetText(rigidBody.mass.ToString("0"));
    }

    public void shootMass()
    {
        if(!spawned || shootMassTimestamp + 30 > Utils.millisecondsSinceEpoch())
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
            Vector3 offset = transform.position + (Vector3)aiMovement.lastDirection * ((transform.localScale.x / 2) + (mass / 10));
            GameObject obj = Instantiate(aiMassPrefab, offset, Quaternion.identity);

            AIMass script = obj.GetComponent<AIMass>();
            script.fakeParent = this;
            script.rigidBody.mass = mass;

            shrink(mass * 2);

            shootMassTimestamp = Utils.millisecondsSinceEpoch();

            for (var i = 0; i < clones.Count; i++)
            {
                if(clones[i] != null)
                {
                    AIClone s = clones[i].GetComponent<AIClone>();
                    s.shootMass();
                }
            }
        }
    }

    public void split()
    {
        if(!spawned || splitTimestamp + 30 > Utils.millisecondsSinceEpoch())
        {
            return;
        }

        if(circleCollider.isTrigger)
        {
            circleCollider.isTrigger = false;
        }
        
        int count = clones.Count;
        if(count >= PlayerBlob.CLONES_MAX)
        {
            return;
        }

        for (var i = 0; i < count; i++)
        {
            if(clones[i] != null)
            {
                AIClone s = clones[i].GetComponent<AIClone>();
                s.split();
            }
        }

        float halfMass = rigidBody.mass / 2;
        if(halfMass >= PlayerBlob.MASS_MIN)
        {
            GameObject obj = Instantiate(aiClonePrefab, transform.position, Quaternion.identity);

            AIClone component = obj.GetComponent<AIClone>();
            clones.Add(component);

            component.fakeParent = this;
            component.currentColor = currentColor;
            component.rigidBody.mass = halfMass;
            
            float c = Mathf.Sqrt(component.rigidBody.mass * Utils.SCALE_MULTIPLIER);
            component.setScale(new Vector2(c, c), true);

            shrink(halfMass, false, true);

            AddRecombineDuration();

            splitTimestamp = Utils.millisecondsSinceEpoch();
            recombineTime = Utils.secondsSinceEpoch();
        }
    }

    public void explode(float mass = 0)
    {
        if(!spawned || splitTimestamp + 3 > Utils.millisecondsSinceEpoch())
        {
            return;
        }

        if(circleCollider.isTrigger)
        {
            circleCollider.isTrigger = false;
        }

        int pieces = PlayerBlob.CLONES_MAX - clones.Count;
        if(pieces <= PlayerBlob.CLONES_MAX && pieces > 0)
        {
            float newMass = rigidBody.mass / (pieces + 1);//CLONES_MAX starts at 0..
            for (var i = 0; i < pieces; i++)
            {
                if(newMass >= PlayerBlob.MASS_MIN)
                {
                    GameObject obj = Instantiate(aiClonePrefab, transform.position, Quaternion.identity);

                    AIClone component = obj.GetComponent<AIClone>();
                    clones.Add(component);

                    component.fakeParent = this;
                    component.currentColor = currentColor;
                    component.rigidBody.mass = newMass;

                    float c = Mathf.Sqrt(component.rigidBody.mass * Utils.SCALE_MULTIPLIER);
                    component.setScale(new Vector2(c, c), true);

                    AddRecombineDuration();
                }
            }

            shrink(rigidBody.mass - newMass, false, true);
            splitTimestamp = Utils.millisecondsSinceEpoch();
            recombineTime = Utils.secondsSinceEpoch();
        }

        if(mass > 0)
        {
            grow(mass, false);
        }
    }

    public void Recombine()
    {
        if(!circleCollider.isTrigger)
        {
            circleCollider.isTrigger = true;
        }

        if(recombineTime != -1)
        {
            recombineTime = -1;
        }
        if(recombineDuration != 0)
        {
            recombineDuration = 0;
        }

        for (var i = 0; i < clones.Count; i++)
        {
            if(clones[i] != null)
            {
                AIClone s = clones[i].GetComponent<AIClone>();
                if(!s.recombine)
                {
                    Physics2D.IgnoreCollision(s.circleCollider, circleCollider, false);

                    if(!s.circleCollider.isTrigger)
                    {
                        s.circleCollider.isTrigger = true;
                    }

                    s.recombine = true;
                }
            }
        }
    }

    private void checkMass()
    {//TODO: fix single blobs and single clone blobs from growing when the total 
    //mass of every blob combined is at or over the max
        if(!spawned)
        {
            return;
        }

        if(rigidBody.mass > PlayerBlob.MASS_MAX)
        {
            rigidBody.mass = PlayerBlob.MASS_MAX;
        }
        if(rigidBody.mass < PlayerBlob.MASS_MIN)
        {
            rigidBody.mass = PlayerBlob.MASS_MIN;
        }

        if(totalMass > PlayerBlob.MASS_MAX)
        {
            totalMass = PlayerBlob.MASS_MAX;
        }
        if(totalMass < PlayerBlob.MASS_MIN)
        {
            totalMass = PlayerBlob.MASS_MIN;
        }
    }

    private void AddMass(float amount)
    {
        rigidBody.mass += amount;
        aiHud.setBlobScoreText(rigidBody.mass);
    }

    private void SubtractMass(float amount)
    {
        rigidBody.mass -= amount;
        aiHud.setBlobScoreText(rigidBody.mass);
    }

    private void AddTotalMass(float amount)
    {
        totalMass += amount;

        universalPlayer.totalMass = totalMass;
    }

    public void SubtractTotalMass(float amount)
    {
        totalMass -= amount;

        universalPlayer.totalMass = totalMass;
    }

    private void UpdateOrderLayer(int order)
    {
        order += 100;
        sortingGroup.sortingOrder = order;
        aiHud.blobDetailCanvas.sortingOrder = order;
    }

    public void SwapWithClone(AIClone clone)
    {
        float parentDrag = rigidBody.drag;
        Vector3 parentVelocity = rigidBody.velocity;
        Vector3 parentPosition = transform.position;
        Vector3 parentScale = transform.localScale;
        Vector3 parentNextScale = nextScale;
        float parentMass = rigidBody.mass;

        float cloneDrag = clone.rigidBody.drag;
        Vector3 cloneVelocity = clone.rigidBody.velocity;
        Vector3 clonePosition = clone.transform.position;
        Vector3 cloneScale = clone.transform.localScale;
        Vector3 cloneNextScale = clone.nextScale;
        float cloneMass = clone.rigidBody.mass;

        rigidBody.mass = cloneMass;
        rigidBody.drag = cloneDrag;
        rigidBody.velocity = cloneVelocity;
        transform.position = clonePosition;
        setScale(cloneScale, true);
        shrink(0.1f);

        clone.rigidBody.mass = parentMass;
        clone.rigidBody.drag = parentDrag;
        clone.rigidBody.velocity = parentVelocity;
        clone.transform.position = parentPosition;
        clone.setScale(parentScale, true);
        clone.shrink(0.1f);
    }

    public string GetUsername()
    {
        if(username == "")
        {
            return "Unnamed";
        }

        return username;
    }

    public void AddRecombineDuration(float otherScale = 0)
    {
        float scale = otherScale == 0 ? transform.localScale.x : otherScale;
        recombineDuration += 10 + (int)(scale / 100);
        if(recombineDuration > 120)
        {
            recombineDuration = 120;
        }
    }

    public bool IsRecombining()
    {
        return recombineDuration <= 0;
    }

    IEnumerator RecombineDurationCoroutine()
    {
        yield return new WaitForSeconds(1f);

        if(recombineDuration > 0)
        {
            recombineDuration--;
        }
        else
        {
            Recombine();
        }

        StartCoroutine(RecombineDurationCoroutine());
    }

    IEnumerator ShrinkCoroutine()
    {
        yield return new WaitForSeconds(1f);

        shrink(rigidBody.mass * 0.0015f);

        StartCoroutine(ShrinkCoroutine());
    }
}
