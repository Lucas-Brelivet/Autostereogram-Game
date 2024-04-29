using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public static class GraphicTools
{
    private const int EFFECTS_RENDERING_LAYER = 7;
    private static readonly Material DEFAULT_BLIT_MATERIAL = new Material(Shader.Find("Unlit/Texture"));
    private static readonly int MAIN_TEX_ID = Shader.PropertyToID("_MainTex");

    private static bool graphicToolsInitialized = false;

    //A transform to put objects created here in, to tidy up the scene
    private static Transform rootTransform = new GameObject("[GRAPHIC TOOLS]").transform;

    //Camera used to render effects
    private static Camera camera;

    //Components used for Blit
    private static MeshRenderer quadMeshRenderer;
    private static Mesh quadMesh = new Mesh();


    private static void InitializeGraphicTools()
    {
        InitializeCamera();

        InitializeQuad();

        graphicToolsInitialized = true;
    }

    private static void InitializeCamera()
    {
        camera = new GameObject("BlitCamera").AddComponent<Camera>();
        camera.transform.parent = rootTransform;
        camera.enabled = false;
        camera.clearFlags = CameraClearFlags.Nothing;
        camera.cullingMask = 1 << EFFECTS_RENDERING_LAYER;
        camera.orthographic = true;
        camera.orthographicSize = 1;
        camera.nearClipPlane = 0;
    }

    private static void InitializeQuad()
    {
        quadMesh.vertices = new Vector3[4];
        quadMesh.uv = new Vector2[4];
        quadMesh.triangles = new int[]{0,1,2,0,2,3};

        quadMeshRenderer = new GameObject("Quad").AddComponent<MeshRenderer>();
        quadMeshRenderer.transform.SetParent(rootTransform);
        quadMeshRenderer.gameObject.layer = EFFECTS_RENDERING_LAYER;

        quadMeshRenderer.AddComponent<MeshFilter>().mesh = quadMesh;
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
    public static void Blit(Texture source = null, RenderTexture dest = null, Material material = null, Rect? sourceRect = null, Rect? destRect = null)
    {
        if (!graphicToolsInitialized) InitializeGraphicTools();

        camera.targetTexture = dest;

        Rect _sourceRect = sourceRect != null ? (Rect) sourceRect : GetFullScreenRect(source);
        Rect _destRect = destRect != null ? (Rect) destRect : GetFullScreenRect(dest);

        quadMeshRenderer.transform.position = camera.ScreenToWorldPoint((Vector3)_destRect.center);

        Vector3[] vertices = new Vector3[4];
        vertices[0] = ScreenToQuadMeshPoint(_destRect.min);
        vertices[1] = ScreenToQuadMeshPoint(new Vector2(_destRect.xMin, _destRect.yMax));
        vertices[2] = ScreenToQuadMeshPoint(_destRect.max);
        vertices[3] = ScreenToQuadMeshPoint(new Vector2(_destRect.xMax, _destRect.yMin));

        quadMesh.vertices = vertices;

        Vector2[] uv = new Vector2[4];
        if(source != null)
        {
            
            uv[0] = new Vector2(_sourceRect.xMin/source.width, _sourceRect.yMin/source.height);
            uv[1] = new Vector2(_sourceRect.xMin/source.width, _sourceRect.yMax/source.height);
            uv[2] = new Vector2(_sourceRect.xMax/source.width, _sourceRect.yMax/source.height);
            uv[3] = new Vector2(_sourceRect.xMax/source.width, _sourceRect.yMin/source.height);
        }
        else
        {
            uv[0] = Vector2.zero;
            uv[1] = Vector2.up;
            uv[2] = Vector2.one;
            uv[3] = Vector2.right;
        }
        quadMesh.uv = uv;

        quadMeshRenderer.material = material != null ? material : DEFAULT_BLIT_MATERIAL;
        quadMeshRenderer.material.SetTexture(MAIN_TEX_ID, source);

        quadMeshRenderer.gameObject.SetActive(true);
        camera.Render();
        quadMeshRenderer.gameObject.SetActive(false);
    }

    private static Vector3 ScreenToQuadMeshPoint(Vector3 point)
    {
        return quadMeshRenderer.transform.InverseTransformPoint(camera.ScreenToWorldPoint(point));
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

}
