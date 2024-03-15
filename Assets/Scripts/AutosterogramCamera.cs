using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class AutosterogramCamera : MonoBehaviour
{
    private readonly int rectPropertyId = Shader.PropertyToID("_Rect");
    private readonly int randomSeedPropertyId = Shader.PropertyToID("_RandomSeed");

    [SerializeField]
    private Material autostereogramMaterial;

    [SerializeField]
    private Material blitToRectMaterial;

    [SerializeField]
    private Texture depthTexture;

    [SerializeField]
    private RectTransform leftVisualGuide;

    [SerializeField]
    private RectTransform rightVisualGuide;

    [SerializeField]
    private float pupilDistance = 0.066f;

    [SerializeField]
    private float eyesToScreenDistance = 0.3f;

    private float pixelsPerMeter;

    private RenderTexture stereoImage;
    
    private RenderTexture column;

    // Start is called before the first frame update
    void Start()
    {
        GraphicTools.InitializeGraphicTools();
        GraphicTools.Blit(null, null);

        pixelsPerMeter = Screen.dpi * 0.394f;
        int maxPixelInterval = (int)(pupilDistance * pixelsPerMeter);
        
        autostereogramMaterial.SetFloat("_PupilDistance", pupilDistance);
        autostereogramMaterial.SetFloat("_PixelsPerMeter", pixelsPerMeter);
        autostereogramMaterial.SetFloat("_EyesToScreenDistance", eyesToScreenDistance);
        
        depthTexture.width = Screen.width;
        depthTexture.height = Screen.height;
        autostereogramMaterial.SetTexture("_DepthTex", depthTexture);

        column = new RenderTexture(new RenderTextureDescriptor(1, Screen.height));
        stereoImage  = new RenderTexture(new RenderTextureDescriptor(1, Screen.height));

        leftVisualGuide.anchoredPosition = new Vector2(-maxPixelInterval/2f, leftVisualGuide.anchoredPosition.y);
        rightVisualGuide.anchoredPosition = new Vector2(maxPixelInterval/2f, rightVisualGuide.anchoredPosition.y);

    }

    // Called when the camera component on this object finishes rendering
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        //Render each pixel column for the right eye based on the already
        //rendered left side of the image and the depth information
        autostereogramMaterial.SetInt(randomSeedPropertyId, Random.Range(0, 1000));
        stereoImage.Release();
        stereoImage  = new RenderTexture(new RenderTextureDescriptor(1, Screen.height));
        Graphics.Blit(null, stereoImage, autostereogramMaterial);

        
        // for(int x = 1; x < Screen.width; x++)
        // {
        //     Graphics.Blit(stereoImage, column, autostereogramMaterial);
        //     AddColumn(column, ref stereoImage);
        // }

        //Display the stereo image on screen
        destination = stereoImage;
    }


    /// <summary>
    /// Adds the given column texture at the end of the destination texture
    /// </summary>
    /// <param name="column">The 1 pixel wide texture to add</param>
    /// <param name="dest"> The destination texture
    private void AddColumn(Texture column, ref RenderTexture dest)
    {
        RenderTexture result = StitchTextures(dest, column);
        dest.Release();
        dest = result;
    }


    private RenderTexture StitchTextures(Texture left, Texture right)
    {
        RenderTexture output = new RenderTexture(new RenderTextureDescriptor(left.width + right.width, left.height));
        float stitchClipSpaceLocation = 2 * left.width/output.width - 1;
        blitToRectMaterial.SetVector(rectPropertyId, new Vector4(0, -1, stitchClipSpaceLocation, 1));
        Graphics.Blit(left, output, blitToRectMaterial);
        
        blitToRectMaterial.SetVector(rectPropertyId, new Vector4(stitchClipSpaceLocation, -1, 1, 1));
        Graphics.Blit(right, output, blitToRectMaterial);

        return output;
    }

    /// <summary>
    /// Calculates the minimum depth of the depth texture in the given rect
    /// </summary>
    /// <returns>The calculated depth value</returns>
    private float MinDepth(int xMin, int yMin, int width, int height)
    {
        RenderTextureDescriptor rtDescriptor = new RenderTextureDescriptor(width/2, height/2, RenderTextureFormat.Depth);
        return 0;
    }

    private void CreateScreenMesh(int width, int height, float pixelsPerUnit, out Mesh mesh)
    {
        mesh = new Mesh();
        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                mesh.vertices.Append<Vector3>(new Vector3(i*pixelsPerUnit, j*pixelsPerUnit, 0));
            }
        }

        //Add triangles
        for(int i = 0; i < width-1; i++)
        {
            for(int j = 0; j < height-1; j++)
            {
                int lowerLeftIndex = i*height + j;
                mesh.triangles.Append(lowerLeftIndex);
                mesh.triangles.Append(lowerLeftIndex + 1);
                mesh.triangles.Append(lowerLeftIndex + 1 + height);

                
                mesh.triangles.Append(lowerLeftIndex);
                mesh.triangles.Append(lowerLeftIndex + 1 + height);
                mesh.triangles.Append(lowerLeftIndex + height);
            }
        }
    }

}
