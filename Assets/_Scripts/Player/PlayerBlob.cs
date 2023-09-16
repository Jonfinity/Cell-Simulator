using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Cinemachine;
using UnityEngine.Rendering;

public class PlayerBlob : MonoBehaviour
{
    public const int CLONES_MAX = 7;

    public const int BLOB_SCALE_MIN = 2;
    public const int BLOB_SCALE_MAX = 2000;

    public const int MASS_MIN = 5;
    public const int MASS_MAX = 1000000;

    private const int CAMERA_SIZE_MIN = 20;
    
    public static PlayerBlob instance;
    
    public bool isGuest = true;
    public string username;
    
    public bool spawned = false;

    public float totalMass = MASS_MIN;

    public Vector2 nextScale = new Vector2(BLOB_SCALE_MIN, BLOB_SCALE_MIN);

    [SerializeField] public UniversalPlayer universalPlayer;
    [SerializeField] public BlobCircle blobCircle;
    [SerializeField] public PlayerStats playerStats;
    [SerializeField] public PlayerSettings playerSettings;
    [SerializeField] public PlayerHUD playerHud;
    [SerializeField] public PlayerMovement playerMovement;
    
    [SerializeField] public GameObject playerMassPrefab;
    [SerializeField] public GameObject playerClonePrefab;
    
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private Transform cameraLock;
    private float nextOrthographicSize = CAMERA_SIZE_MIN;
    
    [SerializeField] private SortingGroup sortingGroup;
    [SerializeField] public Rigidbody2D rigidBody;
    [SerializeField] public CircleCollider2D circleCollider;

    [SerializeField] public Canvas blobDetailCanvas;

    private long shootMassTimestamp;
    private long splitTimestamp;

    public Color currentColor;
    public Quaternion facing;
    
    public List<PlayerClone> clones;

    public int recombineTime = -1;
    public int recombineDuration;

    private void Awake()
    {
        instance = this;

        blobDetailCanvas.worldCamera = Camera.main;
    }

    private void Start() 
    {
        despawn(true);
    }

    private void FixedUpdate() 
    {
        if(!spawned)
        {
            return;
        }

        DevControls();

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

        if(playerMovement.joystick.Direction == Vector2.zero)
        {
            float calc = ((transform.localScale.x * 60f) - 200);
            if(calc < 1)
            {
                calc = 1;
            }

            foreach (PlayerClone clone in clones)
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

    private void LateUpdate()
    {
        if(!spawned)
        {
            return;
        }

        int count = clones.Count;
        if(count > 0)
        {
            Vector3 offset = transform.position;
            foreach(var c in clones)
            {
                if(c != null)
                {
                    offset += c.transform.position;
                }
            }

            count += 1;

            Vector3 pos = offset / count;
            if(cameraLock.transform.position != pos)
            {
                cameraLock.transform.position = pos;
                
            }
            
            if(virtualCamera.m_Follow != cameraLock.transform)
            {
                virtualCamera.m_Follow = cameraLock.transform;
            }
        }
        else
        {
            if(virtualCamera.m_Follow != transform)
            {
                virtualCamera.m_Follow = transform;
            }

            if(cameraLock.transform.position != Vector3.zero)
            {
                cameraLock.transform.position = Vector3.zero;
            }
        }

        if(spawned && virtualCamera.m_Lens.OrthographicSize != nextOrthographicSize)
        {
            virtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(virtualCamera.m_Lens.OrthographicSize, nextOrthographicSize, 3f * Time.deltaTime);
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
            case "Player":
                if(other.gameObject.GetInstanceID() != gameObject.GetInstanceID())
                {
                    Physics2D.IgnoreCollision(collider, circleCollider, true);
                }
                break;
            case "PlayerClone":
                if(other.gameObject.GetComponent<PlayerClone>().fakeParent.gameObject.GetInstanceID() != gameObject.GetInstanceID())
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
        int exp = 0;
        switch(other.gameObject.tag)
        {
            case "Food":
                Game.instance.eatFood(other.gameObject);
                Utils.PlaySound(playerSettings, Game.instance.foodConsumeSound, transform.position, 1.25f);
                playerStats.foodEaten++;
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
                bool boolean = Utils.CanEat(other.transform, transform);
                if(other.gameObject.tag != "PlayerClone" && !boolean)
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
                            PlayerClone component = other.GetComponent<PlayerClone>();
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
                        case "AI":
                            other.GetComponent<AIBlob>().grow(rigidBody.mass, false);
                            break;
                        case "AIClone":
                            other.GetComponent<AIClone>().grow(rigidBody.mass, false);
                            break;
                    }

                    if(clones.Count > 0)
                    {
                        PlayerClone biggestClone = null;
                        float minimumSize = 0;
                        foreach (PlayerClone cloneObject in clones)
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

        if(exp > 0) playerStats.AddExperience(exp);
        if(mass > 0) grow(mass, checkSize);
    }

    private void DevControls()
    {
        float c = transform.localScale.x * 2;

        if (Input.GetKey(KeyCode.Alpha1))
        {
            grow(c, false);
        }
        if (Input.GetKey(KeyCode.Alpha2))
        {
            shrink(c, false);
        }

        if (Input.GetKey(KeyCode.W))
        {
            shootMass();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            split();
        }

        if (Input.GetKey(KeyCode.RightControl))
        {
            kill();
        }
    }

    public void spawn()
    {
        if(spawned)
        {
            playerHud.setIdleCanvasActivity(false);
            playerHud.setPlayingCanvasActivity(true);
            return;
        }
        
        universalPlayer.AddToGame();

        currentColor = Utils.generateBlobColor();
        blobCircle.DrawLine(100, 0.5f, currentColor);
        blobCircle.DrawFilledMesh(100, 0.5f, currentColor);
        
        blobCircle.Spawn();
        playerStats.Spawn();
        playerHud.Spawn();
        playerMovement.Spawn();
        
        rigidBody.WakeUp();

        circleCollider.enabled = true;

        transform.position = Map.GenerateRandomPosition();
        UpdateOrderLayer((int)transform.localScale.x);
        UpdateOrthographicSize();

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
        playerStats.Despawn(bypass);
        playerHud.Despawn(bypass);
        playerMovement.Despawn(bypass);

        shootMassTimestamp = 0;
        splitTimestamp = 0;

        spawned = false;

        rigidBody.velocity = Vector2.zero;
        rigidBody.mass = MASS_MIN;
        totalMass = MASS_MIN;

        Vector2 scaleMin = new Vector2(BLOB_SCALE_MIN, BLOB_SCALE_MIN);
        nextScale = scaleMin;
        transform.localScale = scaleMin;

        rigidBody.Sleep();

        circleCollider.enabled = false;
        transform.position = Vector2.zero;

        nextOrthographicSize = CAMERA_SIZE_MIN;
        virtualCamera.m_Lens.OrthographicSize = Map.orthographicSpectatingSize;
        
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

        PlayerHandleData.Save(this);
    }

    public void respawn()
    {
        despawn();
        spawn();
    }

    public void spectate()
    {
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

        float mass = rigidBody.mass;
        float c = Mathf.Sqrt((mass * Utils.SCALE_MULTIPLIER));
        
        if(c < BLOB_SCALE_MIN)
        {
            c = BLOB_SCALE_MIN;
        }
        if(c > BLOB_SCALE_MAX)
        {
            c = BLOB_SCALE_MAX;
        }

        nextScale = new Vector2(c, c);
    }

    public void shrink(float amount, bool checkSize = true, bool local = false, bool instant = false)
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
        
        float mass = rigidBody.mass;
        float c = Mathf.Sqrt((mass * Utils.SCALE_MULTIPLIER));
        
        if(c < BLOB_SCALE_MIN)
        {
            c = BLOB_SCALE_MIN;
        }
        if(c > BLOB_SCALE_MAX)
        {
            c = BLOB_SCALE_MAX;
        }

        nextScale = new Vector2(c, c);
        if(instant)
        {
            transform.localScale = nextScale;
        }
    }

    public void setScale(Vector3 vector, bool includeNext = false)
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
        UpdateOrthographicSize();
        
        playerMovement.SetMovementSpeed(Mathf.Sqrt(5000 / ((scale + 6) * 0.01f)));
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

        if(rigidBody.mass - mass >= MASS_MIN)
        {
            Vector3 offset = transform.position + playerMovement.lastJoystickDirection * ((transform.localScale.x / 2) + (mass / 10));
            GameObject obj = Instantiate(playerMassPrefab, offset, Quaternion.identity);

            PlayerMass script = obj.GetComponent<PlayerMass>();
            script.fakeParent = this;
            script.rigidBody.mass = mass;

            shrink(mass * 2);

            shootMassTimestamp = Utils.millisecondsSinceEpoch();

            for (var i = 0; i < clones.Count; i++)
            {
                if(clones[i] != null)
                {
                    PlayerClone s = clones[i].GetComponent<PlayerClone>();
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
        if(count >= CLONES_MAX)
        {
            return;
        }

        for (var i = 0; i < count; i++)
        {
            if(clones[i] != null)
            {
                PlayerClone s = clones[i].GetComponent<PlayerClone>();
                s.split();
            }
        }

        float halfMass = rigidBody.mass / 2;
        if(halfMass >= MASS_MIN)
        {
            Vector3 offset = transform.position + playerMovement.lastJoystickDirection;
            GameObject obj = Instantiate(playerClonePrefab, offset, Quaternion.identity);

            PlayerClone component = obj.GetComponent<PlayerClone>();
            clones.Add(component);

            component.fakeParent = this;
            component.currentColor = currentColor;
            component.rigidBody.mass = halfMass;
            component.direction = playerMovement.lastJoystickDirection;
            
            float c = Mathf.Sqrt(component.rigidBody.mass * Utils.SCALE_MULTIPLIER);
            component.setScale(new Vector2(c, c), true);

            shrink(halfMass, false, true, true);

            AddRecombineDuration();

            splitTimestamp = Utils.millisecondsSinceEpoch();
            recombineTime = Utils.secondsSinceEpoch();
        }

        UpdateOrthographicSize();
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
        if(pieces <= CLONES_MAX && pieces > 0)
        {
            float newMass = rigidBody.mass / (pieces + 1);//CLONES_MAX starts at 0..
            for (var i = 0; i < pieces; i++)
            {
                if(newMass >= MASS_MIN)
                {
                    GameObject obj = Instantiate(playerClonePrefab, transform.position, Quaternion.identity);

                    PlayerClone component = obj.GetComponent<PlayerClone>();
                    clones.Add(component);

                    component.fakeParent = this;
                    component.currentColor = currentColor;
                    component.rigidBody.mass = newMass;
                    component.direction = playerMovement.lastJoystickDirection;

                    Vector3 dir = (component.transform.position - transform.position).normalized;
                    component.rigidBody.AddForce(dir * 100, ForceMode2D.Impulse);
                    
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

        UpdateOrthographicSize();
    }

    public void Recombine()
    {
        if(!circleCollider.isTrigger)
        {
            circleCollider.isTrigger = true;
        }

        if(playerHud.recombine.gameObject.activeSelf)
        {
            playerHud.recombine.gameObject.SetActive(false);
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
                PlayerClone s = clones[i].GetComponent<PlayerClone>();
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

        UpdateOrthographicSize();
    }

    private void checkMass()
    {//TODO: fix single blobs and single clone blobs from growing when the total 
    //mass of every blob combined is at or over the max
        if(!spawned)
        {
            return;
        }

        if(rigidBody.mass > MASS_MAX)
        {
            rigidBody.mass = MASS_MAX;
        }
        if(rigidBody.mass < MASS_MIN)
        {
            rigidBody.mass = MASS_MIN;
        }

        if(totalMass > MASS_MAX)
        {
            totalMass = MASS_MAX;
            playerHud.setScoreCounterText(totalMass);
        }
        if(totalMass < MASS_MIN)
        {
            totalMass = MASS_MIN;
            playerHud.setScoreCounterText(totalMass);
        }
    }

    private void AddMass(float amount)
    {
        rigidBody.mass += amount;
        playerHud.setBlobScoreText(rigidBody.mass);
    }

    private void SubtractMass(float amount)
    {
        rigidBody.mass -= amount;
        playerHud.setBlobScoreText(rigidBody.mass);
    }

    public void AddTotalMass(float amount)
    {
        totalMass += amount;
        playerHud.setScoreCounterText(totalMass);

        universalPlayer.totalMass = totalMass;
        playerStats.massGained += (int)amount;
    }

    public void SubtractTotalMass(float amount)
    {
        totalMass -= amount;
        playerHud.setScoreCounterText(totalMass);

        universalPlayer.totalMass = totalMass;
        playerStats.massLost += (int)amount;
    }

    private void UpdateOrderLayer(int order)
    {
        order += 100;
        sortingGroup.sortingOrder = order;
        blobDetailCanvas.sortingOrder = order;
    }

    public void UpdateOrthographicSize()
    {
        float scale = transform.localScale.x;
        float m = Mathf.Sqrt(scale + 26);
        float calc = (m * m) / 1.4f;

        int count = clones.Count;
        if(count > 0)
        {
            calc += transform.localScale.x / 5;
            for (var i = 0; i < count; i++)
            {
                if(clones[i] != null)
                {
                    calc += clones[i].transform.localScale.x / 5;
                }
            }
        }

        if(calc < CAMERA_SIZE_MIN)
        {
            calc = CAMERA_SIZE_MIN;
        }
        
        if(calc > Map.orthographicSpectatingSize)
        {
            calc = Map.orthographicSpectatingSize;
        }

        nextOrthographicSize = calc;
    }

    public void SwapWithClone(PlayerClone clone)
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
        recombineDuration += 2;//(int)(scale / 100)
        if(recombineDuration > 120)
        {
            recombineDuration = 120;
        }

        if(!playerHud.recombine.gameObject.activeSelf)
        {
            playerHud.recombine.gameObject.SetActive(true);
            playerHud.setRecombineText(recombineDuration);
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
            playerHud.setRecombineText(recombineDuration);
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
