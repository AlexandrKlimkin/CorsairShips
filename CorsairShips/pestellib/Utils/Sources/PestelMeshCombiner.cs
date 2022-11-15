using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter))]
public class PestelMeshCombiner : MonoBehaviour
{
    [SerializeField] private bool _useOnlyMeshesWithSameMaterial = false;
    [SerializeField] private List<Transform> _excludedTransforms = new List<Transform>();
    [SerializeField] ReflectionProbeUsage _reflectionProbeUsabe = ReflectionProbeUsage.Off;
    //[SerializeField] private ShadowCastingMode _shadowCasting = ShadowCastingMode.Off;

    public event Action OnCombined = () => { };
    public SkinnedMeshRenderer SkinnedMeshRenderer { get { return _skinnedMeshRenderer; } }
    private SkinnedMeshRenderer _skinnedMeshRenderer;

    public bool UseOnlyMeshesWithSameMaterial
    {
        get { return _useOnlyMeshesWithSameMaterial; }
        set { _useOnlyMeshesWithSameMaterial = value; }
    }
    
    public List<Transform> ExcludedTransform
    {
        get { return _excludedTransforms; }
    }

    public bool IncludeThisTransformToCombine;

    [ContextMenu("Combine")]
    public void Start()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>().Where(IsMeshToInclude).ToArray();

        if (meshFilters.Length == 0) return;

        if (_useOnlyMeshesWithSameMaterial)
        {
            meshFilters = meshFilters.Where(x => x.GetComponent<MeshRenderer>().sharedMaterial == meshFilters[0].GetComponent<MeshRenderer>().sharedMaterial).ToArray();
        }

        var materialForSkinnedMeshRenderer = meshFilters[0].GetComponent<MeshRenderer>().material;

        _skinnedMeshRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();
        if (!_skinnedMeshRenderer)
            _skinnedMeshRenderer = gameObject.AddComponent<SkinnedMeshRenderer>();
        Transform[] bones = meshFilters.Select(x => x.GetComponent<Transform>()).ToArray();

        var totalVertices = meshFilters.Sum(x => x.mesh.vertexCount);
        BoneWeight[] weights = new BoneWeight[totalVertices];

        Matrix4x4[] bindPoses = new Matrix4x4[bones.Length];

        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        int meshIndex = 0;
        int vertexIndex = 0;
        while (meshIndex < meshFilters.Length)
        {
            bindPoses[meshIndex] = bones[meshIndex].worldToLocalMatrix;

            var currentMesh = meshFilters[meshIndex].sharedMesh;

            var lastVertexIndex = vertexIndex + currentMesh.vertexCount;
            for (; vertexIndex < lastVertexIndex; vertexIndex++)
            {
                weights[vertexIndex].boneIndex0 = meshIndex;
                weights[vertexIndex].weight0 = 1;
            }

            combine[meshIndex].mesh = currentMesh;
            combine[meshIndex].transform = meshFilters[meshIndex].transform.localToWorldMatrix;// * transform.worldToLocalMatrix;
            var meshRenderer = meshFilters[meshIndex].GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.enabled = false;
            }
            meshIndex++;
        }

        var combined = new Mesh();
        combined.CombineMeshes(combine);
        combined.boneWeights = weights;
        combined.bindposes = bindPoses;
        combined.UploadMeshData(false);
        combined.RecalculateBounds();

        //skinnedMeshRenderer.rootBone = transform;
        _skinnedMeshRenderer.bones = bones;
        _skinnedMeshRenderer.sharedMesh = combined;
        _skinnedMeshRenderer.shadowCastingMode = ShadowCastingMode.Off;
        _skinnedMeshRenderer.receiveShadows = false;
        _skinnedMeshRenderer.skinnedMotionVectors = false;
        _skinnedMeshRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
        _skinnedMeshRenderer.lightProbeUsage = LightProbeUsage.Off;
        _skinnedMeshRenderer.reflectionProbeUsage = _reflectionProbeUsabe;
        _skinnedMeshRenderer.updateWhenOffscreen = true;
        _skinnedMeshRenderer.quality = SkinQuality.Bone1;

        // Debug.Log("visible: " + skinnedMeshRenderer.isVisible);

        transform.GetComponent<MeshFilter>().mesh = combined;

        _skinnedMeshRenderer.material = materialForSkinnedMeshRenderer;

        OnCombined();
    }

    bool IsMeshToInclude(MeshFilter mesh)
    {
        var filtredByFlagIncludeThis = IncludeThisTransformToCombine || mesh.transform != this.transform;
        return !Excluded(mesh.transform) && filtredByFlagIncludeThis;
    }

    bool Excluded(Transform tr)
    {
        while (tr.parent != null)
        {
            if (_excludedTransforms.Contains(tr))
            {
                return true;
            }

            tr = tr.parent;
        }

        return false;
    }
}
