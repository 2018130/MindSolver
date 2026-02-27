using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ColorType
{
    Red,
    Blue,
    Mixed
}

public class PipeWater : MonoBehaviour
{
    private Color redColor = new Color(1f, 77f/256, 73f/256);
    private Color mixedColor = new Color(245f / 256, 74f / 256, 1f);
    private Color blueColor = new Color(76f / 256, 178f / 256, 1f);


    [SerializeField]
    private ColorType colorType = ColorType.Red;
    public ColorType ColorType => colorType;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer.color.r > 254f / 256)
        {
            colorType = ColorType.Red;
        }
        else if(spriteRenderer.color.r > 243f / 256)
        {
            colorType = ColorType.Mixed;
        }
        else
        {
            colorType = ColorType.Blue;
        }
    }

    private void OnDisable()
    {
        WaterSpawner_Single.ReturnToPool(gameObject);
    }

    private void Update()
    {
        if(!Pipe.s_isConnected && colorType == ColorType.Mixed)
        {
            WaterSpawner.ReturnToPool(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.collider.TryGetComponent(out PipeWater pipeWater))
        {
            if(colorType != pipeWater.colorType)
            {
                SetColor(ColorType.Mixed);
                pipeWater.SetColor(ColorType.Mixed);
            }
        }
    }

    public void SetColor(ColorType colorType)
    {
        this.colorType = colorType;
        spriteRenderer.color = GetColor(colorType);
    }

    private Color GetColor(ColorType colorType)
    {
        switch (colorType)
        {
            case ColorType.Red:
                return redColor;
            case ColorType.Blue:
                return blueColor;
            case ColorType.Mixed:
                return mixedColor;
        }

        return redColor;
    }
}