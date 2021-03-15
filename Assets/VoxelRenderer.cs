using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class VoxelRenderer : MonoBehaviour
{
    [SerializeField] private ComputeShader shader;
    [SerializeField] private float maxDistance = 500f;
    [SerializeField] private float voxelSize = 1f;
    [SerializeField] private Color skyColor = Color.gray;
    [SerializeField] Color[] materialColors;

    private RenderTexture texture;
    private RawImage image;
    private VoxelData voxelData;
    private ComputeBuffer voxelDataBuffer;
    private ComputeBuffer materialColorsBuffer;

    private void Start()
    {
        image = GetComponent<RawImage>();
        CreateRenderTexture(Screen.width, Screen.height);
        voxelDataBuffer = new ComputeBuffer(VoxelData.SIZE_X * VoxelData.SIZE_Y * VoxelData.SIZE_Z, sizeof(int));
        materialColorsBuffer = new ComputeBuffer(materialColors.Length, sizeof(float) * 4);
        CreateVoxelData();
        image.texture = texture;
        SetShaderConstants();
    }

    void Update()
    {
        if (Screen.width != texture.width || Screen.height != texture.height)
        {
            CreateRenderTexture(Screen.width, Screen.height);
        }

        RenderVoxelDataInComputeShader();
    }

    private void OnDisable()
    {
        voxelDataBuffer.Dispose();
        materialColorsBuffer.Dispose();
    }

    void CreateVoxelData()
    {
        voxelData = new VoxelData();
        int count = 0;
        for (int x = 0; x < VoxelData.SIZE_X; x++)
        {
            for (int y = 0; y < VoxelData.SIZE_Y; y++)
            {
                for (int z = 0; z < VoxelData.SIZE_Z; z++)
                {
                    count++;
                    if (Vector3.Distance(new Vector3( VoxelData.SIZE_X / 2, VoxelData.SIZE_X / 2, VoxelData.SIZE_X / 2),new Vector3(x,y,z)) < VoxelData.SIZE_X / 2)
                        voxelData[x, y, z] = count % (materialColors.Length);
                    else voxelData[x, y, z] = -1;
                }
            }
        }
    }

    void CreateRenderTexture(int width, int height)
    {
        texture = new RenderTexture(width, height, 1);
        texture.enableRandomWrite = true;
        texture.Create();
        shader.SetInts("TextureResolution", new int[] {texture.width, texture.height});
    }

    void SetShaderConstants()
    {
        int id = shader.FindKernel("CSMain");
        shader.SetInts("VoxelDataResolution", new int[] {VoxelData.SIZE_X, VoxelData.SIZE_Y, VoxelData.SIZE_Z});
        shader.SetFloat("MaxDistance", maxDistance);
        shader.SetFloat("VoxelSize", voxelSize);
        shader.SetTexture(id, "OutputTexture", texture);
        shader.SetFloats("SkyColor", skyColor.r, skyColor.g, skyColor.b);
        shader.SetBuffer(id, "VoxelMaterialColor", materialColorsBuffer);
        materialColorsBuffer.SetData(materialColors);
    }

    void RenderVoxelDataInComputeShader()
    {
        voxelDataBuffer.SetData(voxelData.Data);

        Matrix4x4 viewRotation = Matrix4x4.Rotate(Camera.main.transform.rotation);
        Vector3 viewPosition = Camera.main.transform.position;
        int id = shader.FindKernel("CSMain");
        shader.SetBuffer(id, "VoxelData", voxelDataBuffer);
        shader.SetMatrix("ViewRotation", viewRotation);
        shader.SetVector("ViewPosition", viewPosition);
        shader.Dispatch(id, (int) Mathf.Ceil(texture.width / 8f), (int) Mathf.Ceil(texture.height / 8f), 1);
    }
}