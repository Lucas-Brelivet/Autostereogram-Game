using UnityEditor;
using UnityEngine;

public class GlobalShaderVariablesControlPanel : EditorWindow
{
    float offsetCoef;

    [MenuItem("Tools/Global Shader Variables Control Panel")]
    public static void ShowWindow()
    {
        GetWindow(typeof(GlobalShaderVariablesControlPanel));
    }

    private void Awake()
    {
        offsetCoef = Shader.GetGlobalFloat("_OffsetCoef");
    }

    private void OnGUI()
    {
        offsetCoef = EditorGUILayout.Slider("Offset Coef", offsetCoef, 0, 1);
        Shader.SetGlobalFloat("_OffsetCoef", offsetCoef);
    }
}
