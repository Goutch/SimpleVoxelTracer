using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class VoxelRenderer : MonoBehaviour
{
    [SerializeField] private ComputeShader shader;
    [SerializeField] private float maxDistance=500f;
    [SerializeField] private float voxelSize=1f;
    private RenderTexture texture;
    private RawImage image;
    private VoxelData voxelData;
    private ComputeBuffer voxelDataBuffer;

    private void Start()
    {
        image = GetComponent<RawImage>();
        CreateRenderTexture(Screen.width, Screen.height);
        voxelDataBuffer = new ComputeBuffer(VoxelData.SIZE_X * VoxelData.SIZE_Y * VoxelData.SIZE_Z, sizeof(int));
        CreateVoxelData();

        image.texture = texture;
        SetShaderConstants();
    }
    void Update()
    {
        if (Screen.width!=texture.width||Screen.height != texture.height)
        {
            CreateRenderTexture(Screen.width,Screen.height);
        }
        RenderVoxelDataInComputeShader();
    }

    private void OnDisable()
    {
        voxelDataBuffer.Dispose();
    }

    void CreateVoxelData()
    {
        voxelData = new VoxelData();
        for (int x = 0; x < VoxelData.SIZE_X; x++)
        {
            for (int y = 0; y < VoxelData.SIZE_Y; y++)
            {
                for (int z = 0; z < VoxelData.SIZE_Z; z++)
                {
                    voxelData[x, y, z] = 1;
                }
            }
        }

    }

    void CreateRenderTexture(int width, int height)
    {
        texture = new RenderTexture(width, height, 1);
        texture.enableRandomWrite = true;
        texture.Create();
        shader.SetInts("TextureResolution", new int[]{texture.width,texture.height});
    }

    void SetShaderConstants()
    {
        int id = shader.FindKernel("CSMain");
        shader.SetInts("VoxelDataResolution",new int[]{VoxelData.SIZE_X, VoxelData.SIZE_Y, VoxelData.SIZE_Z});
        shader.SetFloat("MaxDistance", maxDistance);
        shader.SetFloat("VoxelSize", voxelSize);
        shader.SetTexture(id, "OutputTexture", texture);
    }
    
    void RenderVoxelDataInComputeShader()
    {
        voxelDataBuffer.SetData(voxelData.Data);
        
        Matrix4x4 viewRotation = Matrix4x4.Inverse(Matrix4x4.Rotate(Camera.main.transform.rotation));
        Vector3 viewPosition=Camera.main.transform.position;
        int id = shader.FindKernel("CSMain");
        SetShaderConstants();
        shader.SetBuffer(id, "VoxelData", voxelDataBuffer);
        shader.SetMatrix("ViewRotation", viewRotation);
        shader.SetVector("ViewPosition", viewPosition);
        shader.Dispatch(id, (int)Mathf.Ceil(texture.width / 8f), (int)Mathf.Ceil(texture.height / 8f), 1);
    }
}