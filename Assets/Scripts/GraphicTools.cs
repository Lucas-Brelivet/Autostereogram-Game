using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GraphicTools
{
    private const int EFFECTS_RENDERING_LAYER = 7;

    private static bool graphicToolsInitialized = false;

    //A transform to put objects created here in, to tidy up the scene
    private static Transform rootTransform;

    //Camera used to render effects
    private static Camera camera;

    //Components used for Blit
    private static Canvas canvas;
    private static Image image;

    private static Mesh quad;

    private static void InitializeGraphicTools()
    {
        rootTransform = new GameObject("[GRAPHIC TOOLS]").transform;

        camera = new GameObject("BlitCamera").AddComponent<Camera>();
        camera.transform.parent = rootTransform;
        camera.enabled = false;
        camera.clearFlags = CameraClearFlags.Nothing;
        camera.cullingMask = 1 << EFFECTS_RENDERING_LAYER;

        canvas = new GameObject("Canvas").AddComponent<Canvas>();
        canvas.transform.SetParent(rootTransform);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.gameObject.layer = EFFECTS_RENDERING_LAYER;

        image = new GameObject("Image").AddComponent<Image>();
        image.transform.SetParent(canvas.transform);
        image.gameObject.layer = EFFECTS_RENDERING_LAYER;

        graphicToolsInitialized = true;
    }

    /// <summary>
    /// Renders the portion of the source texture that is in the source rect into the destination Rect in the destination texture.
    /// For example, you can make it so the rendered image will only occupy a fourth of the destination texure.
    /// The rest of the texture will not be changed.
    /// </summary>
    /// <param name="source">The texture to render</param>
    /// <param name="dest">The texture to which to render. If null, renders to the screen</param>
    /// <param name="material">The material to use to render</param>
    /// <param name="sourceRect">The rect on the source texture to render, mesured in pixels, with the lower left corner of the texture at (0,0).</param>
    /// <param name="destRect">The rect on the dest texture in which to render, mesured in pixels, with the lower left corner of the texture at (0,0).</param>
    public static void Blit(Texture2D source = null, RenderTexture dest = null, Material material = null, Rect? sourceRect = null, Rect? destRect = null)
    {
        if (!graphicToolsInitialized) InitializeGraphicTools();

        Rect _sourceRect = sourceRect != null ? (Rect) sourceRect : GetFullScreenRect(source);
        Rect _destRect = destRect != null ? (Rect) destRect : GetFullScreenRect(dest);

        image.sprite = Sprite.Create(source, _sourceRect, Vector2.zero, 100);
        image.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, _destRect.x, _destRect.width);
        image.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, _destRect.y, _destRect.height);
        image.material = material != null ? material : image.defaultMaterial;
        
        image.gameObject.SetActive(true);
        camera.targetTexture = dest;
        camera.Render();
        image.gameObject.SetActive(false);
    }

    public static void Blit(RenderTexture source, RenderTexture dest = null, Material material = null, Rect? sourceRect = null, Rect? destRect = null)
    {
        Texture2D convertedSource = new Texture2D(source.width, source.height);
        RenderTexture previousActiveRenderTexture = RenderTexture.active;
        RenderTexture.active = source;
        convertedSource.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
        RenderTexture.active = previousActiveRenderTexture;
        Blit(convertedSource, dest, material, sourceRect, destRect);
    }

    /// <summary>
    /// Get a rect that covers the full screen or texture, mesured in pixels with the lower left corner at (0,0)
    /// </summary>
    /// <param name="texture">The texture the rect must cover. If null, the screen is used instead</param>
    /// <returns>The rect that covers the screen or texture</returns>
    private static Rect GetFullScreenRect(Texture texture = null)
    {
        if(texture == null)
        {
            return new Rect(0,0, Screen.width, Screen.height);
        }
        return new Rect(0,0,texture.width, texture.height);
    }

    

    // /// <summary>
    // /// Convert a rect from Screen coordinates (mesured in pixels with the lower left corner at (0,0))
    // /// to Canvas coordinates (mesured in pixels with the center at (0,0))
    // /// </summary>
    // /// <param name="rect">The rect to convert</param>
    // /// <param name="screenWidth">The width of the screen or texture where the rect will be used</param>
    // /// <param name="screenHeight">The height of the screen or texture where the rect will be used</param>
    // /// <returns>The converted rect.</returns>
    // private static Rect ScreenToCanvasRect(Rect rect, float screenWidth, float screenHeight)
    // {
    //     return new Rect(rect.x-screenWidth/2, rect.y-screenHeight/2, rect.width, rect.height);
    // }
}
