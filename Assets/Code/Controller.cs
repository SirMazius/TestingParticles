using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Controller : MonoBehaviour
{

    public Mesh mesh;
    public Material material;
    public ComputeShader cShader;
    public int instanceCount;
    public int rows, cols, depth;
    public int nBuckets, sizeOfBuckets;

    private int subMeshIndex = 0;
    private int cachedInstanceCount = -1;
    private int cachedSubMeshIndex = -1;

    private int kernelId;
    private int kernelId2;
    private int kernelId3;
    private int kernelId4;

    private ComputeBuffer positionBuffer, velocityBuffer, forceBuffer, auxBuffer;
    private ComputeBuffer hTable, indexBuffer;
    private ComputeBuffer argsBuffer;

    private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
    int[] hTableData;
    int[] indexBufferData;

    Vector4[] positions;
    void Start()
    {
        instanceCount = rows * cols * depth;
        nBuckets = Utilidades.Next_prime(2*instanceCount);
        sizeOfBuckets = 15;

        kernelId = cShader.FindKernel("CSMain");
        kernelId2 = cShader.FindKernel("CleanTable");
        kernelId3 = cShader.FindKernel("HashParticles");
        kernelId4 = cShader.FindKernel("InsertParticles");

        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        positionBuffer = new ComputeBuffer(instanceCount, 16);
        forceBuffer = new ComputeBuffer(instanceCount, 12);
        velocityBuffer = new ComputeBuffer(instanceCount, 12);
        hTable = new ComputeBuffer(nBuckets * sizeOfBuckets, sizeof(int));
        indexBuffer = new ComputeBuffer(instanceCount, sizeof(int));

        auxBuffer = new ComputeBuffer(instanceCount, sizeof(int)); // Para el debug y esas mierdas

        positions = new Vector4[instanceCount];
        Vector3[] velocities = new Vector3[instanceCount];
        Vector3[] forces = new Vector3[instanceCount];
        hTableData = new int[(nBuckets * sizeOfBuckets)];
        indexBufferData = new int[instanceCount];
        for (int i = 0; i < instanceCount; i++)
        {
            float angle = 0.0f;//Random.Range(0.0f, Mathf.PI * 2.0f);
            float distance = Random.Range(-20.0f, 20.0f);
            float height = Random.Range(-2.0f, 2.0f);
            float size = 0.2f;//Random.Range(0.05f, 0.25f);
            //positions[i] = new Vector4(Mathf.Sin(angle) * distance, height, Mathf.Cos(angle) * distance, size);

            float range = 1000f;
            float vX = Random.Range(-range, range);
            float vY = Random.Range(-range, range);
            float vZ = Random.Range(-range, range);

            forces[i].x = vX;
            forces[i].y = vY;
            forces[i].z = vZ;

            velocities[i] = new Vector3();
        }
        DefineVolume();
        positionBuffer.SetData(positions);
        velocityBuffer.SetData(velocities);
        forceBuffer.SetData(forces);
        material.SetBuffer("positionBuffer", positionBuffer);

        // Main
        cShader.SetBuffer(kernelId, "positionBuffer", positionBuffer);
        cShader.SetBuffer(kernelId, "auxBuffer", auxBuffer);
        cShader.SetBuffer(kernelId, "velocityBuffer", velocityBuffer);
        cShader.SetBuffer(kernelId, "forceBuffer", forceBuffer);
        cShader.SetBuffer(kernelId, "hTable", hTable);

        // CleanTable
        cShader.SetBuffer(kernelId2, "hTable", hTable);
        cShader.SetBuffer(kernelId2, "indexBuffer", indexBuffer);

        // HashParticles
        cShader.SetBuffer(kernelId3, "indexBuffer", indexBuffer);
        cShader.SetBuffer(kernelId3, "positionBuffer", positionBuffer);

        
        cShader.SetInt("nParticles", instanceCount);
        cShader.SetInt("width", Mathf.CeilToInt(instanceCount / 1024f));
        cShader.SetInt("sizeBuckets", sizeOfBuckets);
        cShader.SetInt("nBuckets", nBuckets);
        cShader.SetFloat("l", 0.5f);

        if (mesh != null)
        {
            args[0] = (uint)mesh.GetIndexCount(subMeshIndex);
            args[1] = (uint)instanceCount;
            args[2] = (uint)mesh.GetIndexStart(subMeshIndex);
            args[3] = (uint)mesh.GetBaseVertex(subMeshIndex);
        }

        argsBuffer.SetData(args);
        cShader.Dispatch(kernelId, Mathf.CeilToInt((instanceCount) / 1024f), 1, 1);
        
        cShader.Dispatch(kernelId2, Mathf.CeilToInt((nBuckets * sizeOfBuckets) / 1024f), 1, 1);
        hTable.GetData(hTableData);
        cShader.Dispatch(kernelId3, Mathf.CeilToInt((instanceCount) / 1024f), 1, 1);
        indexBuffer.GetData(indexBufferData);

        Parallel.For(0, instanceCount, i => {
            int auxIndex = indexBufferData[i];
            int auxCounter = 0;
            while (auxCounter < sizeOfBuckets)
            {
                if (hTableData[auxIndex + auxCounter] == -1) {
                    hTableData[auxIndex + auxCounter] = i;
                    break;
                }
                else
                        auxCounter++;
            }
        });
        //InsertCpuSide();
        
        int auxIndex2 = indexBufferData[1100];
        Debug.Log("AuxIndex ->>" + auxIndex2);
        Debug.Log("hTable ->>" + hTableData[auxIndex2]);

        hTable.SetData(hTableData);

        // int[] count = new int[instanceCount];
        // auxBuffer.GetData(count);
        // Debug.Log(count[501]);
        //PerformComprobation(count, nBuckets * sizeOfBuckets);
        //cShader.Dispatch(kernelId, Mathf.CeilToInt(instanceCount / 1024f), 1, 1);

        // cShader.Dispatch(kernelId2, Mathf.CeilToInt((nBuckets * sizeOfBuckets) / 1024f), 1, 1);
        // hTable.GetData(hTableData);
        // cShader.Dispatch(kernelId3, Mathf.CeilToInt((instanceCount) / 1024f), 1, 1);
        // indexBuffer.GetData(indexBufferData);

        // Parallel.For(0, instanceCount, i => {
        //     int index = indexBufferData[i];
        //     int auxCounter = 0;
        //     while (auxCounter < sizeOfBuckets)
        //     {
        //         if (hTableData[index + auxCounter] == -1)
        //             hTableData[index + auxCounter] = i;
        //         else
        //                 auxCounter++;
        //     }
        // });

        // hTable.SetData(hTableData);
        // cShader.Dispatch(kernelId, Mathf.CeilToInt((instanceCount) / 1024f), 1, 1);
    }

    void FixedUpdate()
    {
        cShader.Dispatch(kernelId2, Mathf.CeilToInt((nBuckets * sizeOfBuckets) / 1024f), 1, 1);
        hTable.GetData(hTableData);
        cShader.Dispatch(kernelId3, Mathf.CeilToInt((instanceCount) / 1024f), 1, 1);
        indexBuffer.GetData(indexBufferData);

        Parallel.For(0, instanceCount, i => {
            int index = indexBufferData[i];
            int auxCounter = 0;
            while (auxCounter < sizeOfBuckets)
            {
                if (hTableData[index + auxCounter] == -1)
                    hTableData[index + auxCounter] = i;
                else
                        auxCounter++;
            }
        });

        hTable.SetData(hTableData);
        cShader.Dispatch(kernelId, Mathf.CeilToInt((instanceCount) / 1024f), 1, 1);

        // indexBufferData[10]
        // InsertCpuSide();
        //cShader.Dispatch(kernelId4, Mathf.CeilToInt((nBuckets) / 1024f), 1, 1);
    }

     void InsertCpuSide() {
         
        for (int i = 0; i < instanceCount; i++)
        {
            int auxIndex = indexBufferData[i];
            int auxCounter = 0;
            while (auxCounter < sizeOfBuckets)
            {
                if (hTableData[auxIndex + auxCounter] == -1) {
                    hTableData[auxIndex + auxCounter] = i;
                    break;
                }
                else
                        auxCounter++;
            }
        }
    }

   

    void Update()
    {
        Graphics.DrawMeshInstancedIndirect(mesh, subMeshIndex, material, new Bounds(Vector3.zero, new Vector3(100.0f, 100.0f, 100.0f)), argsBuffer);
    }

    void OnDisable()
    {
        if (positionBuffer != null)
            positionBuffer.Release();
        positionBuffer = null;

        if (argsBuffer != null)
            argsBuffer.Release();
        argsBuffer = null;

        if (auxBuffer != null)
            auxBuffer.Release();
        auxBuffer = null;

        if (velocityBuffer != null)
            velocityBuffer.Release();
        velocityBuffer = null;

        if (forceBuffer != null)
            forceBuffer.Release();
        forceBuffer = null;

        if (indexBuffer != null)
            indexBuffer.Release();
        indexBuffer = null;

        if (hTable != null)
            hTable.Release();
        indexBuffer = null;
    }

    void UpdatePositions()
    {
        for (int i = 0; i < instanceCount; i++)
        {
            positions[i] = positions[i] + new Vector4(0.1f, 0.1f, 0.1f, 0);
        }

        positionBuffer.SetData(positions);
    }

    private void DefineVolume()
    {
        int index = 0;
        float spacing = 0.5f;
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                for (int k = 0; k < depth; k++)
                {
                    positions[index] = new Vector4(spacing * i, spacing * j, spacing * k, 0.2f);
                    index++;
                }
            }
        }
    }

    private void PerformComprobation(int[] a, int size)
    {
        for (int i = 0; i < size; i++)
        {
            if (a[i] != -1)
               return;
        }

        Debug.Log("EXITO EN LA VIDA");
    }
}
