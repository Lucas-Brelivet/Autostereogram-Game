using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MeshCode : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> numberTemplates = new List<GameObject>();
    private List<GameObject> codeObjects = new List<GameObject>();

    [SerializeField]
    private float numberScale = 1;
    [SerializeField]
    private float numberSpacing = 0.23f;


    [SerializeField]
    private UnityEvent<string> onCodeGenerated;

    private string code;
    public string Code
    {
        get { return code; }
    }


    // Start is called before the first frame update
    void Start()
    {
        GenerateRandomCode(4);
        onCodeGenerated.Invoke(Code);
        Debug.Log(code);
    }

    public void SetCode(string newCode)
    {
        code = newCode;
        for (int i = 0; i < code.Length; i++)
        {
            codeObjects.Add((GameObject)Instantiate(numberTemplates[code[i] - '0']));
        }
        ConfigureCodeObjectsTransforms();
    }


    public void GenerateRandomCode(int codeLength)
    {
        codeObjects.Clear();
        code = "";
        for (int i = 0; i < codeLength; i++)
        {
            int number = Random.Range(0, 9);
            codeObjects.Add((GameObject)Instantiate(numberTemplates[number]));
            code += number.ToString();
        }
        ConfigureCodeObjectsTransforms();
    }

    private void ConfigureCodeObjectsTransforms()
    {
        for (int i = 0; i < code.Length; i++)
        {
            codeObjects[i].transform.SetParent(transform, false);
            codeObjects[i].transform.localPosition = numberSpacing * -(i - (code.Length - 1) / 2f) * Vector3.right;
            codeObjects[i].transform.localScale = numberScale * Vector3.one;
        }
    }
}
