using UnityEngine;

public class DepthCamera : MonoBehaviour
{
    [SerializeField]
    private Camera depthCamera;

    [SerializeField]
    private Shader depthShader;

    // Start is called before the first frame update
    void Awake()
    {
        depthCamera.SetReplacementShader(depthShader, null);
        depthCamera.targetTexture = new RenderTexture(Screen.width, Screen.height, 0);
    }
}
