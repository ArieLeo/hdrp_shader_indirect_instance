using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public sealed class Scatter : MonoBehaviour
{
    [SerializeField] Mesh _sourceMesh;
    [SerializeField] float _extent = 1;
    float curExtent;
    float prevExtent;
    [Range(1, 40000)]
    [SerializeField] int _instanceCount = 100;
    int curCount;
    int prevCount;
    [SerializeField] Color _color = Color.white;
    [SerializeField] float _scale = 1;
    [SerializeField][Range(0, 1)] int _shape;
    [SerializeField] ShadowCastingMode shadowCastingMode;
    int curShape;
    int prevShape;

    [SerializeField, HideInInspector] Shader _shader;

    ComputeBuffer IndirectShaderData;
    private List<Vector2> _positions;
    private RenderParams _renderParams;
    public Material _material;
    MaterialPropertyBlock _sheet;
    Vector2 TempPosition;
    static readonly int PerInstanceData = Shader.PropertyToID("PositionsBuffer");


    void OnValidate()
    {
        _instanceCount = Mathf.Max(1, _instanceCount);
        _positions = new List<Vector2>();
        _renderParams = new RenderParams(_material);
        _renderParams.shadowCastingMode = shadowCastingMode;

    }

    void OnDisable()
    {
        IndirectShaderData?.Release();
        IndirectShaderData = null;
    }

    void OnDestroy()
    {
        IndirectShaderData?.Release();
        IndirectShaderData = null;
        // if (_material)
        // {
        //     if (Application.isPlaying)
        //         Destroy(_material);
        //     else
        //         DestroyImmediate(_material);
        // }
    }

    void Update()
    {
        if (_sourceMesh == null) return;

        // if (IndirectShaderData == null)
        //     IndirectShaderData = new ComputeBuffer(
        //         1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);

        if (_material == null) _material = new Material(_shader);
        if (_sheet == null) _sheet = new MaterialPropertyBlock();

        var bounds = new Bounds(
            transform.position,
            new Vector3(_extent, _sourceMesh.bounds.size.magnitude, _extent)
        );
        _renderParams.worldBounds = bounds;
        curCount = _instanceCount;
        curShape = _shape;
        curExtent = _extent;
        // _sheet.SetColor("_Color", _color);
        // _sheet.SetFloat("_Scale", _scale);
        // _sheet.SetFloat("_Area", _extent);
        // _sheet.SetInt("_Shape", _shape);
        // _sheet.SetFloat("_Time", Application.isPlaying ? Time.time : 0);
        // _sheet.SetMatrix("_LocalToWorld", transform.localToWorldMatrix);
        // _sheet.SetMatrix("_WorldToLocal", transform.worldToLocalMatrix);


        // IndirectShaderData.SetData(new uint[5] {
        //     _sourceMesh.GetIndexCount(0), (uint)_instanceCount, 0, 0, 0 });
        // _sheet.SetBuffer(PerInstanceData, IndirectShaderData);

        // Graphics.DrawMeshInstancedIndirect(
        //     _sourceMesh, 0, _material, bounds, IndirectShaderData, 0, _sheet
        // );
        if (curCount != prevCount || curShape != prevShape || curExtent != prevExtent)
        {
            GeneratePosition();
            prevCount = curCount;
            prevShape = curShape;
            prevExtent = curExtent;
        }
        IndirectShaderData?.Release();
        if (_instanceCount == 0) return;
        IndirectShaderData = new ComputeBuffer(_instanceCount, 8);
        IndirectShaderData.SetData(_positions);
        _sheet.SetBuffer(PerInstanceData, IndirectShaderData);
        _renderParams.matProps = _sheet;
        Graphics.RenderMeshPrimitives(_renderParams, _sourceMesh, 0, _instanceCount);
    }

    void GeneratePosition()
    {
        _positions.Clear();
        Vector3 Wp = transform.position;
        for (var i = 0; i < _instanceCount; i++)
        {
            int seed = i * 6;
            Random.InitState(seed);
            TempPosition = new Vector2(Random.value, Random.value + 1);
            if (_shape == 1)
            {
                TempPosition = Random.insideUnitCircle * (_extent * 0.5f);
                TempPosition = new Vector2(TempPosition.x - Wp.x, TempPosition.y - Wp.z);
            }
            else
            {
                TempPosition = new Vector3(TempPosition.x - 0.5f, TempPosition.y - 0.5f) * _extent;
                TempPosition.y = TempPosition.y - _extent;
                TempPosition = new Vector2(TempPosition.x - Wp.x, TempPosition.y - Wp.z);

            }
            _positions.Add(TempPosition);
        }
    }
    private void OnDrawGizmos()
    {
        if (_shape == 0)
        {
            Handles.color = Color.blue;
            Handles.matrix = this.transform.localToWorldMatrix;
            Handles.DrawWireCube(Vector3.zero, new Vector3(_extent, 0, _extent));
        }
        if (_shape == 1)
        {
            Handles.color = Color.blue;
            Handles.matrix = this.transform.localToWorldMatrix;
            Handles.DrawWireDisc(Vector3.zero, Vector3.up, _extent / 2);
        }

    }
}
