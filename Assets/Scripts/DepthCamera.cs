using UnityEngine;

public class DepthCamera : MonoBehaviour
{
    [SerializeField]
    private Camera depthCamera;

    [SerializeField]
    private Shader depthShader;

    // Start is called before the first frame update
    void Start()
    {
        Shader.SetGlobalFloat("_MinDepthValue", GlobalConstants.MIN_DEPTH_VALUE);
        depthCamera.SetReplacementShader(depthShader, null);
    }
}
