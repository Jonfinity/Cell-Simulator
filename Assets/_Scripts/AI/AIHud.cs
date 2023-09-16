using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AIHud : MonoBehaviour
{
    public static AIHud instance;

    [SerializeField] private AIBlob aiBlob;

    [SerializeField] public Canvas blobDetailCanvas;

    [SerializeField] private TextMeshProUGUI blobName;
    [SerializeField] public TextMeshProUGUI blobScore;

    private void Awake()
    {
        instance = this;

        blobDetailCanvas.worldCamera = Camera.main;
    }

    private void Start()
    {
        Spawn();
    }

    private void LateUpdate()
    {
        if(blobDetailCanvas.gameObject.activeSelf)
        {
            blobDetailCanvas.gameObject.SetActive(false);
        }
    }

    public void Spawn()
    {
        if(aiBlob.spawned)
        {
            return;
        }

        blobScore.rectTransform.localPosition = Utils.scoreWithNamePosition;

        string text = Utils.GenerateRandomAlphanumericString();
        aiBlob.username = text;
        aiBlob.universalPlayer.username = text;

        blobName.text = aiBlob.username;
        blobName.gameObject.SetActive(true);

        blobScore.text = aiBlob.totalMass.ToString("0");
        blobScore.gameObject.SetActive(true);
    }

    public void Despawn(bool bypass = false)
    {
        if(!aiBlob.spawned && !bypass)
        {
            return;
        }
        
        blobName.gameObject.SetActive(false);
        blobName.text = "";

        blobScore.gameObject.SetActive(false);
        blobScore.text = "";
    }

    public void setBlobScoreText(float score)
    {
        if(!aiBlob.spawned)
        {
            return;
        }

        blobScore.SetText(score.ToString("0"));
    }
}
