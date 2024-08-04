using UnityEngine;
using UnityEngine.UI;
using Klak.TestTools;
using MediaPipe.HandPose;

public sealed class HandAnimator : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] ImageSource _source = null;
    [Space]
    [SerializeField] ResourceSet _resources = null;
    public RenderTexture rt;
    [SerializeField] bool _useAsyncReadback = true;
    [Space]
    [SerializeField] Mesh _jointMesh = null;
    [SerializeField] Mesh _boneMesh = null;
    [Space]
    [SerializeField] Material _jointMaterial = null;
    [SerializeField] Material _boneMaterial = null;
    [Space]
    [SerializeField] RawImage _monitorUI = null;
    [Space]
    [SerializeField, Range(0.1f, 10f)] float _handScale = 1.0f;  // Scale factor for hand visualization

    #endregion

    #region Private members

    HandPipeline _pipeline;

    static readonly (int, int)[] BonePairs =
    {
        (0, 1), (1, 2), (1, 2), (2, 3), (3, 4),     // Thumb
        (5, 6), (6, 7), (7, 8),                     // Index finger
        (9, 10), (10, 11), (11, 12),                // Middle finger
        (13, 14), (14, 15), (15, 16),               // Ring finger
        (17, 18), (18, 19), (19, 20),               // Pinky
        (0, 17), (2, 5), (5, 9), (9, 13), (13, 17)  // Palm
    };

    Matrix4x4 CalculateJointXform(Vector3 pos)
      => Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one * 0.07f);

    Matrix4x4 CalculateBoneXform(Vector3 p1, Vector3 p2)
    {
        var length = Vector3.Distance(p1, p2) / 2;
        var radius = 0.03f;

        var center = (p1 + p2) / 2;
        var rotation = Quaternion.FromToRotation(Vector3.up, p2 - p1);
        var scale = new Vector3(radius, length, radius);

        return Matrix4x4.TRS(center, rotation, scale);
    }

    #endregion

    #region MonoBehaviour implementation

    void Start()
      => _pipeline = new HandPipeline(_resources);

    void OnDestroy()
      => _pipeline.Dispose();

    void LateUpdate()
    {
        // Feed the input image to the Hand pose pipeline.
        _pipeline.UseAsyncReadback = _useAsyncReadback;
        _pipeline.ProcessImage(_source.Texture);

        var layer = gameObject.layer;

        // Joint balls
        for (var i = 0; i < HandPipeline.KeyPointCount; i++)
        {
            var keyPoint = _pipeline.GetKeyPoint(i) * _handScale;
            var xform = CalculateJointXform(keyPoint);
            Graphics.DrawMesh(_jointMesh, xform, _jointMaterial, layer);
        }

        // Bones
        foreach (var pair in BonePairs)
        {
            var p1 = _pipeline.GetKeyPoint(pair.Item1) * _handScale;
            var p2 = _pipeline.GetKeyPoint(pair.Item2) * _handScale;
            var zScaler  = 10;
            //p1 = new Vector3(p1.x, p1.y, p1.z * zScaler);
            //p2 = new Vector3(p2.x, p2.y, p2.z * zScaler);
            print(p1);
            var xform = CalculateBoneXform(p1 , p2 );
            Graphics.DrawMesh(_boneMesh, xform, _boneMaterial, layer);
        }

        // UI update
        _monitorUI.texture = _source.Texture;
    }

    public Vector3[] GetHandKeyPoints()
    {
        Vector3[] keyPoints = new Vector3[HandPipeline.KeyPointCount];
        for (var i = 0; i < HandPipeline.KeyPointCount; i++)
        {
            keyPoints[i] = _pipeline.GetKeyPoint(i) * _handScale;
        }
        return keyPoints;
    }

    #endregion
}
