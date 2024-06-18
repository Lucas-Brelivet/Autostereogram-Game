using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Search;
using UnityEngine;

public class AutostereogramGenerator : MonoBehaviour
{
    private readonly int randomSeedPropertyId = Shader.PropertyToID("_RandomSeed");

    [SerializeField]
    private Material autostereogramMaterial;

    [SerializeField]
    private Camera leftEyeDepthCamera;
    [SerializeField]
    private Camera rightEyeDepthCamera;

    [SerializeField]
    private RectTransform leftVisualGuide;

    [SerializeField]
    private RectTransform rightVisualGuide;

    [SerializeField]
    private float pupilDistance = 0.066f;

    [SerializeField]
    private float eyesToScreenDistance = 0.3f;

    [SerializeField]
    private float maxDepthValue = 100f;

    [SerializeField][Tooltip("size of a texel of the autostereogram in termes of actual screen pixels")]
    [Range(1, 5)]
    private int texelSize = 1;
    private RenderTexture stereoImage;

    private float texturePixelsPerMeter;
    private int maxPixelInterval;

    private int panelWidth;
    

    void Awake()
    {
        //Add a camera that doesn't render anyting so that OnRenderImage gets called
        gameObject.AddComponent<Camera>().cullingMask = 0;

        //Set various variables
        float physicalPixelsPerMeter = Screen.dpi * 39.37f;
        texturePixelsPerMeter = physicalPixelsPerMeter/texelSize;
        stereoImage = new RenderTexture((int)(Screen.width / physicalPixelsPerMeter * texturePixelsPerMeter), (int)(Screen.height / physicalPixelsPerMeter * texturePixelsPerMeter), 0)
        {
            filterMode = FilterMode.Point
        };
        //texturePixelsPerMeter = physicalPixelsPerMeter / Screen.width * stereoImage.width ;
        maxPixelInterval = (int)(pupilDistance * texturePixelsPerMeter);
        panelWidth = maxPixelInterval / 2;
        float horizontalFOV = 2 * Mathf.Atan(Screen.width / physicalPixelsPerMeter / 2 / eyesToScreenDistance) * 180 / Mathf.PI;
        float verticalFOV = Camera.HorizontalToVerticalFieldOfView(horizontalFOV, Screen.width/Screen.height);
        Shader.SetGlobalFloat("_MaxDepthValue", maxDepthValue);
        
        //Configure both depth cameras
        ConfigureDepthCamera(leftEyeDepthCamera, verticalFOV, new Vector3(-pupilDistance/2, 0, 0));
        ConfigureDepthCamera(rightEyeDepthCamera, verticalFOV, new Vector3(pupilDistance/2, 0, 0));

        //Set material properties
        autostereogramMaterial.SetFloat("_PupilDistance", pupilDistance);
        autostereogramMaterial.SetFloat("_PixelsPerMeter", texturePixelsPerMeter);
        autostereogramMaterial.SetFloat("_EyesToScreenDistance", eyesToScreenDistance);
        autostereogramMaterial.SetTexture("_LeftDepthTex", leftEyeDepthCamera.targetTexture);
        autostereogramMaterial.SetTexture("_RightDepthTex", rightEyeDepthCamera.targetTexture);


        //Position visual guides
        leftVisualGuide.anchoredPosition = new Vector2(-pupilDistance/2f * 1000, leftVisualGuide.anchoredPosition.y);
        rightVisualGuide.anchoredPosition = new Vector2(pupilDistance/2f * 1000, rightVisualGuide.anchoredPosition.y);

        SetActive(false);
    }

    private void ConfigureDepthCamera(Camera depthCamera, float verticalFOV, Vector3 localPosition)
    {
        depthCamera.targetTexture = new RenderTexture(Screen.width, Screen.height, depth: 32, RenderTextureFormat.Default, RenderTextureReadWrite.Linear)
        {
            filterMode = FilterMode.Point
        };
        depthCamera.backgroundColor = Color.white;
        depthCamera.farClipPlane = maxDepthValue;
        depthCamera.fieldOfView = verticalFOV;
        depthCamera.transform.localPosition = localPosition;
    }

    // Called when the camera component on this object finishes rendering
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        //Update the random pattern of the shader
        autostereogramMaterial.SetInt(randomSeedPropertyId, Random.Range(0, 1000));

        //Divide the image into panels and render each panel for the right eye based on the already
        //rendered left side of the image and the depth information
        Rect panelRect = new Rect(0, 0, panelWidth, stereoImage.height);
        for(int i = 0; i <= DivideRoundingUp(stereoImage.width, panelWidth); i++)
        {
            panelRect.x = i*panelWidth;
            GraphicTools.Blit(stereoImage, stereoImage, autostereogramMaterial, panelRect, panelRect);
        }

        GraphicTools.Blit(stereoImage, destination);
    }

    private int DivideRoundingUp(int x, int y)
    {
        int remainder;
        int quotient = System.Math.DivRem(x, y, out remainder);
        return remainder == 0 ? quotient : quotient + 1;
    }

    public void SetActive(bool setActive)
    {
        gameObject.SetActive(setActive);
        rightEyeDepthCamera.gameObject.SetActive(setActive);
        leftEyeDepthCamera.gameObject.SetActive(setActive);
    }
}
