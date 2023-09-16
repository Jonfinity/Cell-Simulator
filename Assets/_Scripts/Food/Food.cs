using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour
{
    public const float SCALE = 2.5f;

    [SerializeField] private SpriteRenderer spriteRenderer;

    private void Awake() 
    {
        ResetObject();
    }

    private void LateUpdate() 
    {
        if(transform.localScale == Vector3.zero)
        {
            ResetObject();
        }
    }

    public void ResetObject()
    {
        spriteRenderer.color = Utils.generateFoodColor();
        transform.localScale = Utils.foodScale;
    }
}