using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Android;

public class MainMenuHandler : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button infoButton;
    [SerializeField] private Button discordButton;

    [SerializeField] private TMP_Dropdown mapSizeDropdown;

    public static int mapSize;

    private void Awake() {
        if(!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageRead);
        }
        if(!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageWrite);
        }
        
        playButton.onClick.AddListener(() => {
            SceneManager.LoadScene("Game", LoadSceneMode.Single);
        });

        infoButton.onClick.AddListener(() => {
            Application.OpenURL("https://jonfinity.github.io");
        });

        discordButton.onClick.AddListener(() => {
            Application.OpenURL("https://discord.com/invite/A6hHVCAKcW");
        });
    }

    public void UpdateMapSize()
    {
        mapSize = mapSizeDropdown.value;
    }
}
