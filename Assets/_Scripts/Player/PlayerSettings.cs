using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSettings : MonoBehaviour
{
    [SerializeField] public PlayerBlob playerBlob;

    [SerializeField] public Slider masterVolumeSlider;
    [SerializeField] public Toggle nightModeToggle;
    [SerializeField] public Dropdown playingButtonsPositionDropdown;

    private void Awake()
    {
        UpdateColors();
    }
    
    public void handleMasterVolumeSlider()
    {
        AudioListener.volume = masterVolumeSlider.value;

        PlayerHandleData.Save(playerBlob);
    }

    public void handleNightModeToggle()
    {
        if(nightModeToggle.isOn)
        {
            Camera.main.backgroundColor = Color.black;
            Map.instance.map.color = Utils.mapDarkColor;
        } else
        {
            Camera.main.backgroundColor = Color.white;
            Map.instance.map.color = Utils.mapBrightColor;
        }

        UpdateColors();

        PlayerHandleData.Save(playerBlob);
    }

    public void handlePlayingButtonsPositionDropdown()
    {
        PlayerHandleData.Save(playerBlob);
    }

    public void UpdateColors()
    {
        playerBlob.playerHud.updateScoreCounterColor();
        playerBlob.playerHud.updateRecombineColor();
        playerBlob.playerHud.updateJoystickColor();
    }
}
