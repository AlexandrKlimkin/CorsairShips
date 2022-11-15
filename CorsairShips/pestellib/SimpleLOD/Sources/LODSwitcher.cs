/* SimpleLOD 1.6     */
/* By Orbcreation BV */
/* Richard Knol      */
/* Mar 11, 2016      */

using UnityEngine;
using System;
using System.Collections;
using OrbCreationExtensions;

public class LODSwitcher : MonoBehaviour
{
	// Analogous to mip mapping, lod viewport sizes differ by the factor of two
	private readonly float[] _lodSwitchSizes = {0.5f, 0.25f, 0.125f, 0.0625f};
	
	public Mesh[] lodMeshes;
	public GameObject[] lodGameObjects;
	public float[] lodScreenSizes;
	public float deactivateAtDistance = 0f;
	public Camera customCamera = null;

	private Vector3 centerOffset;
	private float objectSize;
	private int fixedLODLevel = -1;
	private  int lodLevel = 0;
	
	private int _frameOffset = 0;
	private const int FrameInterval = 5;

	[SerializeField]
	private float _lodBias = 0.5f;

	[SerializeField]
	private MeshFilter _meshFilter;
	
	[SerializeField]
	private SkinnedMeshRenderer _skinnedMeshRenderer;

	private Renderer _renderer;

	void Start()
	{
		_frameOffset = Mathf.Abs(GetInstanceID()) % FrameInterval;
		
		if ((lodMeshes == null || lodMeshes.Length <= 0) && (lodGameObjects == null || lodGameObjects.Length <= 0))
		{
			Debug.LogWarning(gameObject.name + ".LODSwitcher: No lodMeshes/lodGameObjects set. LODSwitcher is now disabled.");
			enabled = false;
		}
		var nrOfLevels = 0;
		if (lodMeshes != null) nrOfLevels = lodMeshes.Length - 1;
		if (lodGameObjects != null) nrOfLevels = Mathf.Max(nrOfLevels, lodGameObjects.Length - 1);
		if (enabled && (lodScreenSizes == null || lodScreenSizes.Length != nrOfLevels))
		{
			Debug.LogWarning(gameObject.name + ".LODSwitcher: lodScreenSizes should have a length of " + nrOfLevels +
			                 ". LODSwitcher is now disabled.");
			enabled = false;
		}

		_renderer = GetComponent<Renderer>();
		
		SetLODLevel(0);
		lodLevel = -1;
		ComputeLODLevel();
	}

	[ContextMenu("Hook components")]
	void Reset()
	{
		_meshFilter = GetComponent<MeshFilter>();
		_skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
	}

	void OnGUI_Debug()
	{
		var bounds = GetComponent<Renderer>().bounds;

		var camera = Camera.main;
		var worldToScreen = Matrix4x4.TRS(new Vector3(camera.pixelWidth * 0.5f, camera.pixelHeight * 0.5f), Quaternion.identity, new Vector3(camera.pixelWidth * 0.5f, camera.pixelHeight * -0.5f, 1)) * camera.projectionMatrix * camera.worldToCameraMatrix;
		worldToScreen = GL.GetGPUProjectionMatrix(worldToScreen, renderIntoTexture: false);

		var point1 = (Vector2) worldToScreen.MultiplyPoint(bounds.min);
		var point2 = (Vector2) worldToScreen.MultiplyPoint(bounds.max);
		
		var inverseExtents = bounds.extents;
		inverseExtents.z *= -1;

		var point3 = (Vector2) worldToScreen.MultiplyPoint(bounds.center - inverseExtents);
		var point4 = (Vector2) worldToScreen.MultiplyPoint(bounds.center + inverseExtents);

		var minX = Min(point1.x, point2.x, point3.x, point4.x);
		var minY = Min(point1.y, point2.y, point3.y, point4.y);

		var maxX = Max(point1.x, point2.x, point3.x, point4.x);
		var maxY = Max(point1.y, point2.y, point3.y, point4.y);
		
		var rect = new Rect(minX, minY, maxX - minX, maxY - minY);
		
		GUI.color = new Color(1, 1, 1, 0.5f);
		GUI.DrawTexture(rect, Texture2D.whiteTexture);
	}

	void OnDrawGizmosSelected()
	{
		var bounds = GetComponent<Renderer>().bounds;

		Gizmos.DrawWireCube(bounds.center, bounds.size);
		Gizmos.DrawSphere(bounds.min, 1f);
		Gizmos.DrawSphere(bounds.max, 1f);
	}

	public void UpdateBounds()
    {
        lodLevel = -1;
        ComputeLODLevel();
    }
	
	public void SetCustomCamera(Camera aCamera) 
	{
		customCamera = aCamera;
	}

	public void SetFixedLODLevel(int aLevel) {
		fixedLODLevel = Mathf.Max(0, Mathf.Min(MaxLODLevel(), aLevel));
	}
	public void ReleaseFixedLODLevel() {
		fixedLODLevel = -1;
	}

	public int GetLODLevel() {
		return lodLevel;
	}

	public int MaxLODLevel() {
		if(lodScreenSizes == null) return 0;
		return lodScreenSizes.Length;
	}

	public Mesh GetMesh(int aLevel) {
		if(lodMeshes != null && lodMeshes.Length >= aLevel) return lodMeshes[aLevel];
		return null;
	}
	
	public void SetMesh(Mesh aMesh, int aLevel) {
		if(lodMeshes == null) lodMeshes = new Mesh[aLevel+1];
		if(lodMeshes.Length <= aLevel) Array.Resize(ref lodMeshes, aLevel + 1);
		if(aLevel > 0) {
			if(lodScreenSizes == null) lodScreenSizes = new float[aLevel];
			if(lodScreenSizes.Length < aLevel) Array.Resize(ref lodScreenSizes, aLevel);
		}
		lodMeshes[aLevel] = aMesh;
		if(aLevel == lodLevel) {
			lodLevel = -1;
			SetLODLevel(aLevel);  // ensure we use the new model
		}
	}
	
	public void SetRelativeScreenSize(float aValue, int aLevel) {
		if(lodScreenSizes == null) lodScreenSizes = new float[aLevel];
		if(lodScreenSizes.Length < aLevel) Array.Resize(ref lodScreenSizes, aLevel);
		for(var i=0;i<lodScreenSizes.Length;i++) {
			if(i + 1 == aLevel) lodScreenSizes[i] = aValue;
			else if(lodScreenSizes[i] == 0f) {
				if(i == 0) lodScreenSizes[i] = 0.6f;
				else lodScreenSizes[i] = lodScreenSizes[i - 1] * 0.5f;
			}
		}
	}

	void Update () 
	{
		if ((Time.frameCount + _frameOffset) % FrameInterval != 0)
		{
			return; // no need to do this every frame for every object in the scene
		}
	
		ComputeLODLevel();
	}

	public void ForceUpdateLodLevel()
	{
		ComputeLODLevel();
	}

	public Vector3 NearestCameraPositionForLOD(int aLevel)
	{
		var cam = GetCamera();
		
		if(aLevel > 0 && aLevel <= lodScreenSizes.Length)
		{
			var pixelSize = 0;//objectSize * pixelsPerMeter;
			var distance = pixelSize / Screen.width / lodScreenSizes[aLevel-1];
			return transform.position;// + centerOffset + (cam.transform.rotation * (Vector3.back * distance));
		}
		return cam.transform.position;
	}

	private Camera GetCamera()
	{
		var result = customCamera;
		if (result == null)
		{
			result = Camera.main;
		}

		return result;
	}

	private float ComputeViewportSize()
	{
		var bounds = _renderer.bounds;

		var camera = GetCamera();

		if (camera == null)
		{
			return 0f;
		}
		
		var worldToScreen = camera.projectionMatrix * camera.worldToCameraMatrix;
		worldToScreen = GL.GetGPUProjectionMatrix(worldToScreen, renderIntoTexture: false);

		var point1 = (Vector2) worldToScreen.MultiplyPoint(bounds.min);
		var point2 = (Vector2) worldToScreen.MultiplyPoint(bounds.max);
		
		var inverseExtents = bounds.extents;
		inverseExtents.z *= -1;

		var point3 = (Vector2) worldToScreen.MultiplyPoint(bounds.center - inverseExtents);
		var point4 = (Vector2) worldToScreen.MultiplyPoint(bounds.center + inverseExtents);

		var minX = Min(point1.x, point2.x, point3.x, point4.x);
		var minY = Min(point1.y, point2.y, point3.y, point4.y);

		var maxX = Max(point1.x, point2.x, point3.x, point4.x);
		var maxY = Max(point1.y, point2.y, point3.y, point4.y);
		
		return new Vector2(maxX - minX, maxY - minY).magnitude * 0.5f;
	}

	private float Min(float x1, float x2, float x3, float x4)
	{
		return Mathf.Min(x1, Mathf.Min(x2, Mathf.Min(x3, x4)));
	}
	
	private float Max(float x1, float x2, float x3, float x4)
	{
		return Mathf.Max(x1, Mathf.Max(x2, Mathf.Max(x3, x4)));
	}

	private void ComputeLODLevel()
	{
		var newLodLevel = 0;
		if (fixedLODLevel >= 0)
		{
			newLodLevel = fixedLODLevel;
		}
		else
		{
			var objectViewportSize = ComputeViewportSize() * _lodBias;
			if (objectViewportSize >= 0f)
			{
				for (var i = 0; i < _lodSwitchSizes.Length; i++)
				{
					if (objectViewportSize < _lodSwitchSizes[i]) newLodLevel++;
				}
			}
			else newLodLevel = -1;
		}
		
		if (newLodLevel != lodLevel)
		{
			SetLODLevel(newLodLevel);
		}
	}

	public void SetLODLevel(int newLodLevel)
	{
		if (newLodLevel != lodLevel)
		{
			newLodLevel = Mathf.Min(MaxLODLevel(), newLodLevel);
			if (newLodLevel < 0)
			{
				_renderer.enabled = false;
			}
			else
			{
				if (lodMeshes != null && lodMeshes.Length > 0)
				{
					_renderer.enabled = true;
				}
				
				if (lodMeshes != null && lodMeshes.Length > newLodLevel)
				{
					SetLodMesh(newLodLevel);
				}

				for (var i = 0; lodGameObjects != null && i < lodGameObjects.Length; i++)
				{
					lodGameObjects[i].SetActive(i == newLodLevel);
				}
			}
			lodLevel = newLodLevel;
		}
	}

	private void SetLodMesh(int lodLevel)
	{
		if (_skinnedMeshRenderer != null)
		{
			_skinnedMeshRenderer.sharedMesh = lodMeshes[lodLevel];
		}
		else if (_meshFilter != null)
		{
			_meshFilter.sharedMesh = lodMeshes[lodLevel];
		}
	}
}
