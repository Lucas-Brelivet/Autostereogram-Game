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
        depthCamera.SetReplacementShader(depthShader, null);
    }
}
