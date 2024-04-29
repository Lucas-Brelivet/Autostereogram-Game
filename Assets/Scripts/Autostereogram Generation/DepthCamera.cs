using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class DepthCamera : MonoBehaviour
{
    [SerializeField]
    private Camera depthCamera;

    [SerializeField]
    private Shader depthShader;


    // Start is called before the first frame update
    void Start()
    {
        depthCamera.SetReplacementShader(depthShader, null);
        depthCamera.targetTexture = new RenderTexture(Screen.width, Screen.height, depth: 32, format: GraphicsFormat.R32_SFloat);
        depthCamera.backgroundColor = Color.red;
        depthCamera.farClipPlane = AutostereogramGenerator.MaxDepthValue;
    }
}
