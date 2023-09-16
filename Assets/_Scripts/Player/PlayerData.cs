using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    //local username
    public string localUsername;

    //settings
    public float masterVolume;
    public bool nightMode;

    //stats
    public int level;
    public int experience;
    public int foodEaten = 0;
    public int virusesEaten = 0;
    public int massGained = 0;
    public int massLost = 0;
    public int playtime = 0;

    public PlayerData(PlayerBlob player)
    {
        localUsername = player.playerHud.usernameInput.text;
        masterVolume = player.playerSettings.masterVolumeSlider.value;
        nightMode = player.playerSettings.nightModeToggle.isOn;

        level = player.playerStats.level;
        experience = player.playerStats.experience;
        foodEaten = player.playerStats.foodEaten;
        virusesEaten = player.playerStats.virusesEaten;
        massGained = player.playerStats.massGained;
        massLost = player.playerStats.massLost;
        playtime = player.playerStats.playtimeTotal + Utils.secondsSinceEpoch() - player.playerStats.playtimeSession;
    }
}
