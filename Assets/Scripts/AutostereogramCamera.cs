using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class AutostereogramCamera : MonoBehaviour
{
    private readonly int randomSeedPropertyId = Shader.PropertyToID("_RandomSeed");

    [SerializeField]
    private Camera autostereogramCamera;

    [SerializeField]
    private Material autostereogramMaterial;

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
    private int maxPixelInterval;

    private int panelWidth;
    private const int minPanelWidth = 150;

    private RenderTexture stereoImage;
    

    // Start is called before the first frame update
    void Start()
    {
        pixelsPerMeter = Screen.dpi * 39.37f;
        maxPixelInterval = (int)(pupilDistance * pixelsPerMeter);
        
        autostereogramMaterial.SetFloat("_PupilDistance", pupilDistance);
        autostereogramMaterial.SetFloat("_PixelsPerMeter", pixelsPerMeter);
        autostereogramMaterial.SetFloat("_EyesToScreenDistance", eyesToScreenDistance);
        
        panelWidth = Mathf.Max(minPanelWidth, (int)(pupilDistance * (autostereogramCamera.nearClipPlane - eyesToScreenDistance) / autostereogramCamera.nearClipPlane * pixelsPerMeter));
        autostereogramMaterial.SetInt("_PanelWidth", panelWidth);

        depthTexture.width = Screen.width - panelWidth;
        depthTexture.height = Screen.height;
        autostereogramMaterial.SetTexture("_DepthTex", depthTexture);

        stereoImage  = new RenderTexture(new RenderTextureDescriptor(Screen.width, Screen.height));

        leftVisualGuide.anchoredPosition = new Vector2(-maxPixelInterval/2f, leftVisualGuide.anchoredPosition.y);
        rightVisualGuide.anchoredPosition = new Vector2(maxPixelInterval/2f, rightVisualGuide.anchoredPosition.y);

    }

    // Called when the camera component on this object finishes rendering
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        
        //Update the random patern of the shader
        autostereogramMaterial.SetInt(randomSeedPropertyId, Random.Range(0, 1000));

        //Divide the image into panels and render each panel for the right eye based on the already
        //rendered left side of the image and the depth information
        GraphicTools.Blit((Texture2D)null, stereoImage, autostereogramMaterial, null, new Rect(0, 0, panelWidth, stereoImage.height));
        for(int i = 1; i <= DivideRoundingUp(Screen.width, panelWidth); i++)
        {
            GraphicTools.Blit(stereoImage, stereoImage, autostereogramMaterial, null, new Rect(0, 0, panelWidth, stereoImage.height));
        }

        GraphicTools.Blit(stereoImage, destination);
    }

    public static int DivideRoundingUp(int x, int y)
    {
        int remainder;
        int quotient = System.Math.DivRem(x, y, out remainder);
        return remainder == 0 ? quotient : quotient + 1;
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
