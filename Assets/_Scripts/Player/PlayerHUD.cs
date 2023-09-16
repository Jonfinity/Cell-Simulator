using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHUD : MonoBehaviour
{
    public static PlayerHUD instance;

    [SerializeField] private PlayerBlob playerBlob;

    [SerializeField] private Button closeButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button respawnButton;

    [SerializeField] public TextMeshProUGUI level;
    [SerializeField] public TextMeshProUGUI experience;
    [SerializeField] public TextMeshProUGUI experienceMultiplier;

    [SerializeField] public TextMeshProUGUI leaderboardFirst;
    [SerializeField] public TextMeshProUGUI leaderboardSecond;
    [SerializeField] public TextMeshProUGUI leaderboardThird;
    [SerializeField] public TextMeshProUGUI leaderboardFourth;
    [SerializeField] public TextMeshProUGUI leaderboardFifth;
    [SerializeField] public TextMeshProUGUI leaderboardMyPosition;
    [SerializeField] private TextMeshProUGUI scoreCounter;
    [SerializeField] public TextMeshProUGUI recombine;

    [SerializeField] private TextMeshProUGUI blobName;
    [SerializeField] public TextMeshProUGUI blobScore;
    [SerializeField] public TextMeshProUGUI blobLevel;
    [SerializeField] public Transform blobPointer;
    
    [SerializeField] private SpriteRenderer blobPointerSpriteRenderer;

    [SerializeField] private RawImage pause;
    [SerializeField] private RawImage shoot;
    [SerializeField] private RawImage split;

    [SerializeField] private Canvas playingCanvas;
    [SerializeField] private Canvas idleCanvas;

    [SerializeField] public TMP_InputField usernameInput;

    [SerializeField] public Image joystickBackground;
    [SerializeField] public Image joystickHandle;

    private void Awake()
    {
        instance = this;

        mainMenuButton.onClick.AddListener(() => {
            PlayerHandleData.Save(playerBlob);
            
            SceneManager.LoadScene("Main Menu", LoadSceneMode.Single);
        });
    }

    private void Start()
    {
        Despawn(true);
    }

    public void handleIdleHudOpen()
    {
        bool active = playerBlob.spawned ? true : false;

        if(respawnButton.gameObject.activeSelf != active)
        {
            respawnButton.gameObject.SetActive(active);
        }

        if(active && !Game.instance.paused)
        {
            Game.instance.paused = true;
            Time.timeScale = 0;
        }
    }

    public void handleIdleHudClose()
    {
        if(Game.instance.paused)
        {
            Game.instance.paused = false;
            Time.timeScale = 1;
        }
    }

    public void setIdleCanvasActivity(bool active)
    {
        if(idleCanvas.gameObject.activeSelf != active)
        {
            idleCanvas.gameObject.SetActive(active);
        }
    }

    public void setPlayingCanvasActivity(bool active)
    {
        if(playingCanvas.gameObject.activeSelf != active)
        {
            playingCanvas.gameObject.SetActive(active);
        }
    }

    public void Spawn()
    {
        if(playerBlob.spawned)
        {
            return;
        }

        setPlayingCanvasActivity(true);
        setIdleCanvasActivity(false);

        closeButton.gameObject.SetActive(false);

        string text = usernameInput.text;
        playerBlob.username = text;
        playerBlob.universalPlayer.username = text;

        bool hasUsername = playerBlob.username != "";
        blobScore.rectTransform.localPosition = hasUsername ? Utils.scoreWithNamePosition : Utils.scoreNoNamePosition;
        blobLevel.rectTransform.localPosition = hasUsername ? Utils.levelWithNamePosition : Utils.levelNoNamePosition;
        
        blobName.text = text;
        blobName.gameObject.SetActive(true);

        setBlobScoreText(playerBlob.rigidBody.mass);
        blobScore.gameObject.SetActive(true);

        blobLevel.text = playerBlob.playerStats.level.ToString();
        blobLevel.gameObject.SetActive(true);

        blobPointer.gameObject.SetActive(true);

        setScoreCounterText(PlayerBlob.MASS_MIN);
        scoreCounter.gameObject.SetActive(true);

        setRecombineText(0);
        recombine.gameObject.SetActive(false);

        pause.gameObject.SetActive(true);
        shoot.gameObject.SetActive(true);
        split.gameObject.SetActive(true);
    }

    public void Despawn(bool bypass = false)
    {
        if(!playerBlob.spawned && !bypass)
        {
            return;
        }

        setPlayingCanvasActivity(false);
        setIdleCanvasActivity(true);

        closeButton.gameObject.SetActive(true);

        blobName.gameObject.SetActive(false);
        blobName.text = "";

        blobScore.gameObject.SetActive(false);
        setBlobScoreText(0);

        blobLevel.text = "";
        blobLevel.gameObject.SetActive(false);
        
        blobPointer.gameObject.SetActive(false);

        scoreCounter.gameObject.SetActive(false);
        setScoreCounterText(0);

        recombine.gameObject.SetActive(false);
        setRecombineText(0);

        pause.gameObject.SetActive(false);
        shoot.gameObject.SetActive(false);
        split.gameObject.SetActive(false);

        respawnButton.gameObject.SetActive(false);
    }

    public void updateScoreCounterColor()
    {
        if(Utils.isColorAlmostBlack(Camera.main.backgroundColor) && scoreCounter.color != Utils.playingHudTextBrightColor)
        {
            scoreCounter.color = Utils.playingHudTextBrightColor;
        } else if(Utils.isColorAlmostWhite(Camera.main.backgroundColor) && scoreCounter.color != Utils.playingHudTextDarkColor)
        {
            scoreCounter.color = Utils.playingHudTextDarkColor;
        }
    }

    public void setScoreCounterText(float score)
    {
        if(!playerBlob.spawned)
        {
            return;
        }

        scoreCounter.SetText(string.Format("Score: {0}", score.ToString("0")));
    }

    public void setBlobScoreText(float score)
    {
        if(!playerBlob.spawned)
        {
            return;
        }

        blobScore.SetText(score.ToString("0"));
    }

    public void updateRecombineColor()
    {
        if(Utils.isColorAlmostBlack(Camera.main.backgroundColor) && recombine.color != Utils.playingHudTextBrightColor)
        {
            recombine.color = Utils.playingHudTextBrightColor;
        } else if(Utils.isColorAlmostWhite(Camera.main.backgroundColor) && recombine.color != Utils.playingHudTextDarkColor)
        {
            recombine.color = Utils.playingHudTextDarkColor;
        }
    }

    public void setRecombineText(int seconds)
    {
        if(!playerBlob.spawned)
        {
            return;
        }

        recombine.SetText(string.Format("Recombine: {0}", seconds));
    }

    public void updateJoystickColor()
    {
        if(Utils.isColorAlmostBlack(Camera.main.backgroundColor) && joystickBackground.color != Utils.brightJoystickBackgroundColor)
        {
            joystickBackground.color = Utils.brightJoystickBackgroundColor;
        } else if(Utils.isColorAlmostWhite(Camera.main.backgroundColor) && joystickBackground.color != Utils.darkJoystickBackgroundColor)
        {
            joystickBackground.color = Utils.darkJoystickBackgroundColor;
        }

        if(Utils.isColorAlmostBlack(Camera.main.backgroundColor) && joystickHandle.color != Utils.brightJoystickHandleColor)
        {
            joystickHandle.color = Utils.brightJoystickHandleColor;
        } else if(Utils.isColorAlmostWhite(Camera.main.backgroundColor) && joystickHandle.color != Utils.darkJoystickHandleColor)
        {
            joystickHandle.color = Utils.darkJoystickHandleColor;
        }
    }
}
