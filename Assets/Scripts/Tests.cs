using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tests : MonoBehaviour
{
    public Texture2D sourceTexture;
    public Rect sourceRect;
    public Rect destRect;

    public RenderTexture compositeImage;

    // Start is called before the first frame update
    void Start()
    {
        //compositeImage = new RenderTexture(Screen.width, Screen.height, 32);
    }

    // Update is called once per frame
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        GraphicTools.Blit(sourceTexture, null, null, sourceRect, destRect);
        return;
        GraphicTools.Blit(sourceTexture, compositeImage, null, sourceRect, destRect);
        GraphicTools.Blit(sourceTexture, compositeImage, null, sourceRect, new Rect(destRect.position.x+200, destRect.position.y-100, destRect.width, destRect.height));
        GraphicTools.Blit(compositeImage);
    }
}
