using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseTextureGenerator : MonoBehaviour
{
    private readonly int randomSeedPropertyId = Shader.PropertyToID("_RandomSeed");

    [SerializeField]
    private RenderTexture renderTexture;

    [SerializeField]
    private Material noiseMaterial;

    // Update is called once per frame
    void Update()
    {
        noiseMaterial.SetInt(randomSeedPropertyId, Random.Range(0, 1000));
        Graphics.Blit(renderTexture, renderTexture, noiseMaterial);
    }
}
