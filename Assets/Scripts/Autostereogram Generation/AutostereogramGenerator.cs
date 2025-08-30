using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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
    private Camera normalCamera;

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

        //Set material properties
        Shader.SetGlobalFloat("_MaxDepthValue", maxDepthValue);
        autostereogramMaterial.SetFloat("_PupilDistance", pupilDistance);
        autostereogramMaterial.SetFloat("_EyesToScreenDistance", eyesToScreenDistance);

        ConfigureStereoImage(); //Mostly redundant, but needed for the normal camera configuration

        SetActive(false);
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
        if(!setActive)
        {
            QualitySettings.vSyncCount = 1;
            Application.targetFrameRate = 60;
            return;
        }
        ConfigureStereoImage();
        rightEyeDepthCamera.gameObject.SetActive(setActive);
        leftEyeDepthCamera.gameObject.SetActive(setActive);

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 24;
    }

    private void ConfigureStereoImage()
    {
        //Set various variables
        float physicalPixelsPerMeter = Screen.dpi * 39.37f;
        texturePixelsPerMeter = physicalPixelsPerMeter / texelSize;
        stereoImage = new RenderTexture((int)(Screen.width / physicalPixelsPerMeter * texturePixelsPerMeter), (int)(Screen.height / physicalPixelsPerMeter * texturePixelsPerMeter), 0)
        {
            filterMode = FilterMode.Point
        };
        maxPixelInterval = (int)(pupilDistance * texturePixelsPerMeter);
        panelWidth = maxPixelInterval / 2;
        float horizontalFOV = 2 * Mathf.Atan(Screen.width / 2 / physicalPixelsPerMeter / eyesToScreenDistance) * 180 / Mathf.PI;
        float verticalFOV = Camera.HorizontalToVerticalFieldOfView(horizontalFOV, Screen.width / Screen.height);

        //Configure both depth cameras
        ConfigureDepthCamera(leftEyeDepthCamera, verticalFOV, new Vector3(-pupilDistance / 2, 0, 0));
        ConfigureDepthCamera(rightEyeDepthCamera, verticalFOV, new Vector3(pupilDistance / 2, 0, 0));

        //also configure the normal camera FOV for consistency between views.
        normalCamera.fieldOfView = verticalFOV;

        //Position visual guides
        leftVisualGuide.anchoredPosition = new Vector2(-pupilDistance / 2f * 1000, leftVisualGuide.anchoredPosition.y);
        rightVisualGuide.anchoredPosition = new Vector2(pupilDistance / 2f * 1000, rightVisualGuide.anchoredPosition.y);


        autostereogramMaterial.SetFloat("_PixelsPerMeter", texturePixelsPerMeter);
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

        //Communicate the new textures to the stereo shader
        autostereogramMaterial.SetTexture("_LeftDepthTex", leftEyeDepthCamera.targetTexture);
        autostereogramMaterial.SetTexture("_RightDepthTex", rightEyeDepthCamera.targetTexture);

    }
}
