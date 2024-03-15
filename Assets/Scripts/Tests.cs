using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tests : MonoBehaviour
{
    public Texture2D sourceTexture;
    public Rect sourceRect;
    public Rect destRect;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        
        GraphicTools.Blit(sourceTexture, destination, null, sourceRect, destRect);
    }
}
