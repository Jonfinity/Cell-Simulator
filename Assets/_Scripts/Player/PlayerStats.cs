using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats instance;

    [SerializeField] public PlayerBlob playerBlob;

    public const float EXPERIENCE_MULTIPLIER_MIN = 1f;
    public const float EXPERIENCE_MULTIPLIER_MAX = 2.0f;

    public int level { get; set; } = 1;
    
    public int experience;
    public int experienceNeeded;
    public long experienceTimestamp;

    public float experienceMultiplier { get; set; } = EXPERIENCE_MULTIPLIER_MIN;

    public int foodEaten;
    public int virusesEaten;

    public int massGained;
    public int massLost;

    public int playtimeSession;
    public int playtimeTotal;

    private void Awake() 
    {
        instance = this;

        PlayerData data = PlayerHandleData.Load(playerBlob);
        playerBlob.playerHud.usernameInput.text = data.localUsername;
        playerBlob.username = data.localUsername;

        playerBlob.playerSettings.masterVolumeSlider.value = data.masterVolume;
        playerBlob.playerSettings.nightModeToggle.isOn = data.nightMode;

        level = data.level;
        experience = data.experience;
        foodEaten = data.foodEaten;
        virusesEaten = data.virusesEaten;
        massGained = data.massGained;
        massLost = data.massLost;
        playtimeTotal = data.playtime;
        
        playtimeSession = Utils.secondsSinceEpoch();
    }

    private void Start() 
    {
        EvaluateNeededExperience();

        playerBlob.playerHud.blobLevel.text = level.ToString();
        playerBlob.playerHud.level.text = level.ToString();
        playerBlob.playerHud.experience.text = string.Format("{0}/{1} ({2}%)", experience, experienceNeeded, (((float)experience / (float)experienceNeeded) * 100).ToString("0.0"));
    }

    private void FixedUpdate() 
    {
        if(!playerBlob.spawned)
        {
            return;
        }

        if(experienceMultiplier > EXPERIENCE_MULTIPLIER_MAX)
        {
            experienceMultiplier = EXPERIENCE_MULTIPLIER_MAX;
            playerBlob.playerHud.experienceMultiplier.text = string.Format("X {0}", experienceMultiplier.ToString("0.00"));
        }
        if(experienceMultiplier < EXPERIENCE_MULTIPLIER_MIN)
        {
            experienceMultiplier = EXPERIENCE_MULTIPLIER_MIN;
            playerBlob.playerHud.experienceMultiplier.text = string.Format("X {0}", experienceMultiplier.ToString("0.00"));
        }
        
        if(experienceMultiplier != EXPERIENCE_MULTIPLIER_MIN && experienceTimestamp + 1500 < Utils.millisecondsSinceEpoch())
        {
            DecreaseExperienceMultiplier();
        }
    }

    public void Spawn()
    {
        if(playerBlob.spawned)
        {
            return;
        }
    }

    public void Despawn(bool bypass = false)
    {
        if(!playerBlob.spawned && !bypass)
        {
            return;
        }

        ResetExperienceMultiplier();
    }

    public void AddExperience(int amount)
    {
        experience += Mathf.RoundToInt(amount * experienceMultiplier);
        playerBlob.playerHud.experience.text = string.Format("{0}/{1} ({2}%)", experience, experienceNeeded, (((float)experience / (float)experienceNeeded) * 100).ToString("0.0"));
        if(experience >= experienceNeeded)
        {
            LevelUp();
        }

        IncreaseExperienceMultiplier(amount);

        experienceTimestamp = Utils.millisecondsSinceEpoch();
    }

    public void LevelUp()
    {
        level++;
        experience = 0;
        EvaluateNeededExperience();

        playerBlob.playerHud.blobLevel.text = level.ToString();
        playerBlob.playerHud.level.text = level.ToString();

        PlayerHandleData.Save(playerBlob);
    }

    private void EvaluateNeededExperience()
    {
        experienceNeeded = Utils.GetNeededExperience(level);
    }

    private void IncreaseExperienceMultiplier(int withAmount)
    {
        if(experienceMultiplier < EXPERIENCE_MULTIPLIER_MAX)
        {
            experienceMultiplier += 0.005f + (withAmount * 0.0002f);

            playerBlob.playerHud.experienceMultiplier.gameObject.SetActive(true);
            playerBlob.playerHud.experienceMultiplier.text = string.Format("X {0}", experienceMultiplier.ToString("0.00"));
        }
    }
    
    private void DecreaseExperienceMultiplier()
    {
        if(experienceMultiplier > EXPERIENCE_MULTIPLIER_MIN)
        {
            float c = experienceMultiplier - 0.001f * (experienceMultiplier * 5);
            if(c <= EXPERIENCE_MULTIPLIER_MIN)
            {
                ResetExperienceMultiplier();
                return;
            }

            experienceMultiplier = c;
            
            playerBlob.playerHud.experienceMultiplier.text = string.Format("X {0}", experienceMultiplier.ToString("0.00"));
        }
    }

    public void ResetExperienceMultiplier()
    {
        experienceMultiplier = EXPERIENCE_MULTIPLIER_MIN;

        playerBlob.playerHud.experienceMultiplier.gameObject.SetActive(false);
        playerBlob.playerHud.experienceMultiplier.text = "X 1";
    }
}
