using UnityEngine;
using UnityEngine.Rendering;

namespace PestelLib.DeepWatersPostProcessing
{
    [DisallowMultipleComponent, ExecuteInEditMode, ImageEffectAllowedInSceneView]
    [AddComponentMenu("Rendering/Sky Renderer", -1)]
    [RequireComponent(typeof(Camera))]
    public class SkyRenderer : MonoBehaviour
    {

        public Mesh SkyboxMesh;
        public Material SkyboxMaterial;

        private Camera _Camera;
        private CommandBuffer _CommandBuffer;

        private void OnPreRender()
        {
            Initialize();
            _CommandBuffer.Clear();
            _CommandBuffer.DrawMesh(SkyboxMesh,
                Matrix4x4.TRS(this.transform.position, Quaternion.identity, Vector3.one), SkyboxMaterial);
        }

        private void OnDisable()
        {
            if (_Camera != null && _CommandBuffer != null)
            {
                _Camera.RemoveCommandBuffer(CameraEvent.BeforeForwardOpaque, _CommandBuffer);
            }

            _Camera = null;
            _CommandBuffer = null;
        }

        private void Initialize()
        {
            if (_Camera == null || _CommandBuffer == null)
            {
                _Camera = this.GetComponent<Camera>();
                _CommandBuffer = new CommandBuffer();
                _CommandBuffer.name = "Skybox";
                _Camera.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, _CommandBuffer);
            }
        }
    }
}