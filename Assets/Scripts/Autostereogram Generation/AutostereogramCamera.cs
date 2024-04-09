using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class AutostereogramCamera : MonoBehaviour
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

    [SerializeField]
    private RenderTexture stereoImage;

    private float texturePixelsPerMeter;
    private int maxPixelInterval;

    private int panelWidth;
    

    // Start is called before the first frame update
    void Start()
    {
        //Set various variables
        float physicalPixelsPerMeter = Screen.dpi * 39.37f;
        texturePixelsPerMeter = stereoImage.width * physicalPixelsPerMeter / Screen.width;
        maxPixelInterval = (int)(pupilDistance * texturePixelsPerMeter);
        panelWidth = maxPixelInterval / 2;
        float minDepthValue = eyesToScreenDistance * 2;
        float horizontalFOV = 2 * Mathf.Atan(Screen.width / physicalPixelsPerMeter / 2 / eyesToScreenDistance) * 180 / Mathf.PI;
        float verticalFOV = Camera.HorizontalToVerticalFieldOfView(horizontalFOV, leftEyeDepthCamera.aspect);
        
        //Set global shader properties
        Shader.SetGlobalFloat("_MinDepthValue", minDepthValue);
        Shader.SetGlobalFloat("_MaxDepthValue", maxDepthValue);

        //Set material properties
        autostereogramMaterial.SetFloat("_PupilDistance", pupilDistance);
        autostereogramMaterial.SetFloat("_PixelsPerMeter", texturePixelsPerMeter);
        autostereogramMaterial.SetFloat("_EyesToScreenDistance", eyesToScreenDistance);
        autostereogramMaterial.SetTexture("_LeftDepthTex", leftEyeDepthCamera.targetTexture);
        autostereogramMaterial.SetTexture("_RightDepthTex", rightEyeDepthCamera.targetTexture);
        

        //Position depth cameras properly
        leftEyeDepthCamera.transform.localPosition = new Vector3(-pupilDistance/2, 0, 0);
        rightEyeDepthCamera.transform.localPosition = new Vector3(pupilDistance/2, 0, 0);
        leftEyeDepthCamera.fieldOfView = verticalFOV;
        rightEyeDepthCamera.fieldOfView = verticalFOV;


        //Position visual guides
        leftVisualGuide.anchoredPosition = new Vector2(-pupilDistance/2f * 1000, leftVisualGuide.anchoredPosition.y);
        rightVisualGuide.anchoredPosition = new Vector2(pupilDistance/2f * 1000, rightVisualGuide.anchoredPosition.y);

    }

    // Called when the camera component on this object finishes rendering
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        
        //Update the random pattern of the shader
        autostereogramMaterial.SetInt(randomSeedPropertyId, Random.Range(0, 1000));

        //Divide the image into panels and render each panel for the right eye based on the already
        //rendered left side of the image and the depth information
        Rect panelRect = new Rect(0, 0, panelWidth, stereoImage.height);
        for(int i = 0; i <= DivideRoundingUp(Screen.width, panelWidth); i++)
        {
            panelRect.x = i*panelWidth;
            GraphicTools.Blit(stereoImage, stereoImage, autostereogramMaterial, panelRect, panelRect);
        }

        GraphicTools.Blit(stereoImage, destination);
    }

    public static int DivideRoundingUp(int x, int y)
    {
        int remainder;
        int quotient = System.Math.DivRem(x, y, out remainder);
        return remainder == 0 ? quotient : quotient + 1;
    }


}
