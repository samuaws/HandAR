using UnityEngine;
using UnityEngine.UI;
using Klak.TestTools;
using MediaPipe.HandPose;

public sealed class HandVisualizer : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] ImageSource _source = null;
    [Space]
    [SerializeField] ResourceSet _resources = null;
    [SerializeField] Shader _keyPointShader = null;
    [SerializeField] Shader _handRegionShader = null;
    [Space]
    [SerializeField] RawImage _mainUI = null;
    [SerializeField] RawImage _cropUI = null;
    [Space]
    [SerializeField, Range(0.1f, 10f)] float _handScale = 1.0f;  // Scale factor for hand visualization

    #endregion

    #region Private members

    HandPipeline _pipeline;
    (Material keys, Material region) _material;
    ComputeBuffer _scaledKeyPointsBuffer;

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        _pipeline = new HandPipeline(_resources);
        _material = (new Material(_keyPointShader),
                     new Material(_handRegionShader));

        // Material initial setup
        _scaledKeyPointsBuffer = new ComputeBuffer(_pipeline.KeyPointBuffer.count, sizeof(float) * 4); // Assuming key points are stored as float4
        _material.keys.SetBuffer("_KeyPoints", _scaledKeyPointsBuffer);
        _material.region.SetBuffer("_Image", _pipeline.HandRegionCropBuffer);

        // UI setup
        _cropUI.material = _material.region;
    }

    void OnDestroy()
    {
        _pipeline.Dispose();
        Destroy(_material.keys);
        Destroy(_material.region);
        _scaledKeyPointsBuffer.Release();
    }

    void LateUpdate()
    {
        // Feed the input image to the Hand pose pipeline.
        _pipeline.ProcessImage(_source.Texture);

        // Scale the key points
        ScaleKeyPoints();

        // UI update
        _mainUI.texture = _source.Texture;
        _cropUI.texture = _source.Texture;
    }

    void ScaleKeyPoints()
    {
        var keyPoints = new Vector4[_pipeline.KeyPointBuffer.count];
        _pipeline.KeyPointBuffer.GetData(keyPoints);
        print(keyPoints[1]);

        for (int i = 0; i < keyPoints.Length; i++)
        {
            keyPoints[i] *= _handScale;
        }

        _scaledKeyPointsBuffer.SetData(keyPoints);
    }

    void OnRenderObject()
    {
        // Key point circles
        _material.keys.SetPass(0);
        Graphics.DrawProceduralNow(MeshTopology.Triangles, 96, 21);

        // Skeleton lines
        _material.keys.SetPass(1);
        Graphics.DrawProceduralNow(MeshTopology.Lines, 2, 4 * 5 + 1);
    }

    #endregion
}
