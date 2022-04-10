using System;
using Fow.Utils;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Rendering;

namespace Fow
{
    public class FowEffectController : MonoBehaviour
    {
        [SerializeField]
        private float ppu = 16f;

        [LayerProperty]
        [SerializeField]
        private int layerInput;
        [LayerProperty]
        [SerializeField]
        private int layerFxChain;
        [LayerProperty]
        [SerializeField]
        private int layerOutput = 0;
        
        [Space]
        
        [SerializeField]
        private RenderTexture textureIn;
        [SerializeField]
        private RenderTexture textureOut;

        [SerializeField]
        private Material matFxChain;
        [SerializeField]
        private Material matOutput;
        
        private MeshRenderer surfaceFxChain;
        private MeshRenderer surfaceOut;
        
        private Camera camInput;
        private Camera camFxChain;

        private float lastCamWidth = 0f;
        private float lastCamHeight = 0f;

        public RenderTexture TextureIn => textureIn;

        public RenderTexture TextureOut => textureOut;

        public Material MatFxChain => matFxChain;

        public Material MatOutput => matOutput;

        public MeshRenderer SurfaceFxChain => surfaceFxChain;

        public MeshRenderer SurfaceOut => surfaceOut;

        public Camera CamInput => camInput;

        public Camera CamFxChain => camFxChain;

        private void Awake()
        {
            Camera camMain = Camera.main;

            this.surfaceFxChain = CreateMeshRenderer(transform, layerFxChain, "FowFxChainSurface", matFxChain);
            this.surfaceOut = CreateMeshRenderer(camMain.transform, layerOutput, "FowOutputSurface", matOutput);

            this.camInput = CreateCamera(camMain.transform, "FowInputCamera", 0, -100, layerInput, textureIn);
            this.camFxChain = CreateCamera(transform, "FowFxChainCamera", -10, -99, layerFxChain, textureOut);
        }

        private void Start()
        {
            Rebind();
        }

        private void LateUpdate()
        {
            Vector2 camSize = EvalCamSize(Camera.main);

            if (Math.Abs(lastCamWidth - camSize.x) > 0.01f ||
                Math.Abs(lastCamHeight - camSize.y) > 0.01f)
            {
                lastCamWidth = camSize.x;
                lastCamHeight = camSize.y;

                Rebind();
            }
        }

        private void Rebind()
        {
            Vector2 camSize = EvalCamSize(Camera.main);

            int bufferWidth = Mathf.CeilToInt(camSize.x * ppu);
            int bufferHeight = Mathf.CeilToInt(camSize.y * ppu);

            textureIn.Release();
            textureIn.width = bufferWidth;
            textureIn.height = bufferHeight;
            textureIn.Create();
            
            textureOut.Release();
            textureOut.width = bufferWidth;
            textureOut.height = bufferHeight;
            textureOut.Create();

            Vector3 surfaceScale = new Vector3(camSize.x, camSize.y, 1f);
            surfaceFxChain.transform.localScale = surfaceScale;
            surfaceOut.transform.localScale = surfaceScale;

            camInput.orthographicSize = Camera.main.orthographicSize;
            camInput.projectionMatrix = Camera.main.projectionMatrix;
            
            camFxChain.orthographicSize = Camera.main.orthographicSize;
            camFxChain.projectionMatrix = Camera.main.projectionMatrix;

            camInput.targetTexture = textureIn;
            camFxChain.targetTexture = textureOut;
        }

        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                Rebind();
            }
        }

        private static Vector2 EvalCamSize(Camera cam)
        {
            Vector3 bottomLeft = cam.ViewportToWorldPoint(Vector3.zero);
            Vector3 topRight = cam.ViewportToWorldPoint(Vector3.one);

            return new Vector2(
                topRight.x - bottomLeft.x, 
                topRight.y - bottomLeft.y);
        }

        private static Camera CreateCamera(Transform parent, string name, float localPozZ, int camDepth, int captureLayer, [CanBeNull] RenderTexture targetTexture)
        {
            GameObject go = new GameObject();
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0f, 0f, localPozZ);

            Camera cam = go.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0f, 0f, 0f, 0f);
            cam.cullingMask = LayerToMask(captureLayer);
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.targetTexture = targetTexture;
            cam.depth = camDepth;
            cam.useOcclusionCulling = false;

            return cam;
        }

        private static MeshRenderer CreateMeshRenderer(Transform parent, int layer, string name, Material material)
        {
            GameObject go = new GameObject();
            go.name = name;
            go.transform.parent = parent;
            go.layer = layer;
            
            Mesh mesh = new Mesh
            {
                vertices = new[]
                {
                    new Vector3(-0.5f, -0.5f),
                    new Vector3(-0.5f, +0.5f),
                    new Vector3(+0.5f, -0.5f),
                    new Vector3(+0.5f, +0.5f),
                },
                uv = new[]
                {
                    new Vector2(0, 0),
                    new Vector2(0, 1),
                    new Vector2(1, 0),
                    new Vector2(1, 1),
                },
                triangles = new[] {0, 1, 3, 3, 2, 0}
            };
            MeshFilter meshFilter = go.AddComponent<MeshFilter>();
            meshFilter.mesh = mesh;

            MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
            meshRenderer.material = material;
            meshRenderer.receiveShadows = false;
            meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            meshRenderer.allowOcclusionWhenDynamic = false;

            return meshRenderer;
        }

        private static int LayerToMask(int layer) => 
            layer != -1 ? 1 << layer : 0;
    }
}