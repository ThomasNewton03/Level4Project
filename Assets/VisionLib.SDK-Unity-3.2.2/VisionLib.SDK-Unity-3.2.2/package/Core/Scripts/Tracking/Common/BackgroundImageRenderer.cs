using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Visometry.VisionLib.SDK.Core.Details;
using Visometry.VisionLib.SDK.Core.API.Native;

#if VL_USE_UNIVERSAL_RENDER_PIPELINE
using UnityEngine.Rendering.Universal;
#endif

namespace Visometry.VisionLib.SDK.Core
{
    /// <summary>
    ///  Used for rendering the camera image in the background.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   The necessary GameObjects for rending the camera image will be added to
    ///   the scene at runtime.
    ///  </para>
    /// </remarks>
    /// @ingroup Core
    [HelpURL(DocumentationLink.APIReferenceURI.Core + "background_image_renderer.html")]
    [AddComponentMenu("VisionLib/Core/Background Image Renderer")]
    [RequireComponent(typeof(ImageStreamFilter))]
    public class BackgroundImageRenderer : MonoBehaviour
    {
        /// <summary>
        ///  Layer for rending the background image.
        /// </summary>
        /// <remarks>
        ///  This can't be changed at runtime.
        /// </remarks>
        public int backgroundLayer = 8;

        /// <summary>
        ///  Color used to clear the screen.
        /// </summary>
        /// <remarks>
        ///  This can't be changed at runtime.
        /// </remarks>
        public Color clearColor = Color.cyan;

        /// <summary>
        ///  Material used for rendering the background image.
        /// </summary>
        /// <remarks>
        ///  This can't be changed at runtime.
        /// </remarks>
        public Material imageMaterial = null;

        private GameObject backgroundGO;

        private ImageStreamFilter imageStreamFilter = null;

        private class BackgroundCameraData
        {
            public GameObject go = null;
            public Camera camera = null;
        }

        private class BackgroundMeshData
        {
            public GameObject go = null;
            public Mesh mesh = null;
            public MeshFilter meshFilter = null;
            public MeshRenderer meshRenderer = null;
            public Material material = null;
        }

        private BackgroundCameraData backgroundCameraData0 = null;
        private BackgroundMeshData backgroundMeshData0 = null;

        private RenderRotation renderRotation = RenderRotation.CCW0;
        private int screenWidth = -1;
        private int screenHeight = -1;

        // UV coordinates for different screen orientations
        // (the v-axis is always flipped, because the image is copied into the
        // texture upside down)
        private static readonly Vector2[] UV0 =
        {
            new Vector2(0.0f, 1.0f), // Bottom-left
            new Vector2(1.0f, 1.0f), // Bottom-right
            new Vector2(1.0f, 0.0f), // Top-right
            new Vector2(0.0f, 0.0f) // Top-left
        };
        private static readonly Vector2[] UV90 =
        {
            new Vector2(0.0f, 0.0f), // Bottom-left
            new Vector2(0.0f, 1.0f), // Bottom-right
            new Vector2(1.0f, 1.0f), // Top-right
            new Vector2(1.0f, 0.0f) // Top-left
        };
        private static readonly Vector2[] UV180 =
        {
            new Vector2(1.0f, 0.0f), // Bottom-left
            new Vector2(0.0f, 0.0f), // Bottom-right
            new Vector2(0.0f, 1.0f), // Top-right
            new Vector2(1.0f, 1.0f) // Top-left
        };
        private static readonly Vector2[] UV270 =
        {
            new Vector2(1.0f, 1.0f), // Bottom-left
            new Vector2(1.0f, 0.0f), // Bottom-right
            new Vector2(0.0f, 0.0f), // Top-right
            new Vector2(0.0f, 1.0f) // Top-left
        };

        private GameObject CreateBackgroundObject()
        {
            GameObject backgroundObject = new GameObject("VLBackground");

            // If a VL scene is loaded additively to another scene, the background object
            // would appear in the first scene instead of the VL scene.
            // To fix that, the object needs to be moved to the VL scene here.
            SceneManager.MoveGameObjectToScene(backgroundObject, this.gameObject.scene);

            return backgroundObject;
        }

        private static BackgroundCameraData CreateBackgroundCamera(
            string name,
            Transform parentTransform,
            int backgroundLayer,
            Color clearColor)
        {
            BackgroundCameraData data = new BackgroundCameraData();

            data.go = GetOrAddChild(parentTransform, name);

            data.camera = data.go.GetOrAddComponent<Camera>();
            data.camera.depth = 0;
            data.camera.cullingMask = 1 << backgroundLayer;

            data.camera.clearFlags = CameraClearFlags.SolidColor;
            data.camera.backgroundColor = clearColor;

            // Use an orthographic projection
            data.camera.orthographic = true;
            data.camera.orthographicSize = 0.375f;
            data.camera.nearClipPlane = -1.0f;
            data.camera.farClipPlane = 1.0f;

            return data;
        }

        private static Mesh CreateQuadMesh()
        {
            Mesh mesh = new Mesh();

            mesh.vertices = new Vector3[]
            {
                new Vector3(-0.5f, -0.5f, 0), // Bottom-left
                new Vector3(0.5f, -0.5f, 0), // Bottom-right
                new Vector3(0.5f, 0.5f, 0), // Top-right
                new Vector3(-0.5f, 0.5f, 0) // Top-left
            };

            mesh.normals = new Vector3[] {Vector3.back, Vector3.back, Vector3.back, Vector3.back};

            mesh.uv = UV0;

            mesh.triangles = new int[]
            {
                // Clock-wise, because Unity is left-handed
                0, 2, 1, 0, 3, 2
            };
            
            return mesh;
        }

        private static BackgroundMeshData CreateBackgroundMesh(
            string name,
            Transform parentTransform,
            int backgroundLayer,
            Material material)
        {
            BackgroundMeshData data = new BackgroundMeshData();

            // Create GameObject
            data.go = GetOrAddChild(parentTransform, name);
            data.go.transform.localScale = new Vector3(1.0f, 0.75f, 1.0f);
            data.go.layer = backgroundLayer;

            // Create MeshFilter
            data.meshFilter = data.go.GetOrAddComponent<MeshFilter>();
            data.mesh = CreateQuadMesh();
            data.meshFilter.mesh = data.mesh;

            // Create MeshRenderer
            data.meshRenderer = data.go.GetOrAddComponent<MeshRenderer>();
            data.meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            data.meshRenderer.receiveShadows = false;
            data.meshRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            data.meshRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;

            // Create Material
            data.material = new Material(material);

            // Use texture
            data.material.mainTexture = Texture2D.blackTexture;

            // Use material
            data.meshRenderer.material = data.material;

            return data;
        }

        /// <summary>
        ///  Update the size (scale) of the background mesh and the orthographic
        ///  size of the background camera
        /// </summary>
        private void UpdateBackgroundSize()
        {
            if (!this.backgroundMeshData0.material.mainTexture)
            {
                return;
            }

            float iw;
            float ih;
            if (this.renderRotation == RenderRotation.CCW0 ||
                this.renderRotation == RenderRotation.CCW180)
            {
                iw = System.Convert.ToSingle(this.backgroundMeshData0.material.mainTexture.width);
                ih = System.Convert.ToSingle(this.backgroundMeshData0.material.mainTexture.height);
            }
            else
            {
                // Swap width and height, because the texture will be rotated,
                // but the VisionLib image stays unchanged
                iw = System.Convert.ToSingle(this.backgroundMeshData0.material.mainTexture.height);
                ih = System.Convert.ToSingle(this.backgroundMeshData0.material.mainTexture.width);
            }
            float imageAspectRatio = iw / ih;

            float sw = System.Convert.ToSingle(this.screenWidth);
            float sh = System.Convert.ToSingle(this.screenHeight);
            float screenAspectRatio = sw / sh;

            // Scale the image up until it covers the whole screen
            float targetWidth;
            float targetHeight;
            // Is the relative height larger?
            if (screenAspectRatio < imageAspectRatio)
            {
                // Keep the height and adapt the width accordingly
                targetWidth = sh * imageAspectRatio;
                targetHeight = sh;
            }
            else
            {
                // Keep the width and adapt the height accordingly
                targetWidth = sw;
                targetHeight = sw / imageAspectRatio;
            }

            // Rotate the UV-coordinates, because the size of the background mesh
            // will be adapted to the screen orientation, but the background image
            // always has the same orientation
            if (this.renderRotation == RenderRotation.CCW0)
            {
                this.backgroundMeshData0.mesh.uv = UV0;
            }
            else if (this.renderRotation == RenderRotation.CCW180)
            {
                this.backgroundMeshData0.mesh.uv = UV180;
            }
            else if (this.renderRotation == RenderRotation.CCW90)
            {
                this.backgroundMeshData0.mesh.uv = UV90;
            }
            else if (this.renderRotation == RenderRotation.CCW270)
            {
                this.backgroundMeshData0.mesh.uv = UV270;
            }

            this.backgroundMeshData0.go.transform.localScale =
                new Vector3(targetWidth, targetHeight, 1.0f);

            this.backgroundCameraData0.camera.orthographicSize = sh / 2.0f;
        }

        /// <summary>
        /// Returns the child GameObject with the given name if the GameObject has one attached,
        /// adds and returns it otherwise.
        /// </summary>
        private static GameObject GetOrAddChild(Transform parentTransform, string name)
        {
            Transform t = parentTransform.Find(name);
            if (t != null)
            {
                return t.gameObject;
            }

            var go = new GameObject(name);
            go.transform.parent = parentTransform;
            return go;
        }

        private void Update()
        {
            Texture2D imageStreamTexture = this.imageStreamFilter.GetTexture();
            if (imageStreamTexture != null)
            {
                this.backgroundMeshData0.material.mainTexture = imageStreamTexture;
            }
            else
            {
                this.backgroundMeshData0.material.mainTexture = Texture2D.blackTexture;
            }
            this.UpdateBackgroundSize();
        }

        private void OnOrientationChange(ScreenOrientation orientation)
        {
            this.renderRotation = orientation.GetRenderRotation();
            this.UpdateBackgroundSize();
        }

        private void OnScreenSizeChange(int screenWidth, int screenHeight)
        {
            this.screenWidth = screenWidth;
            this.screenHeight = screenHeight;
            this.UpdateBackgroundSize();
        }

        private Camera GetBackgroundCamera()
        {
            if (this.backgroundCameraData0 == null)
            {
                Initialize();
            }
            return this.backgroundCameraData0.camera;
        }

        private void Initialize()
        {
            if (this.backgroundMeshData0 != null)
            {
                return;
            }

            if (this.backgroundGO == null)
            {
                this.backgroundGO = CreateBackgroundObject();
            }

            this.backgroundCameraData0 = CreateBackgroundCamera(
                "VLBackgroundCamera0",
                this.backgroundGO.transform,
                this.backgroundLayer,
                this.clearColor);

            this.backgroundMeshData0 = CreateBackgroundMesh(
                "VLBackgroundMesh0",
                this.backgroundCameraData0.go.transform,
                this.backgroundLayer,
                this.imageMaterial);

            this.imageStreamFilter = GetComponent<ImageStreamFilter>();
        }

        private void Awake()
        {
#if VL_USE_UNIVERSAL_RENDER_PIPELINE
            // URP does not support the traditional Camera stacking approach through Depth Only.
            // Background Camera is used as Base Camera and VLCamera as Overlay Camera.
            if (GraphicsSettings.renderPipelineAsset != null)
            {
                string srpType = GraphicsSettings.renderPipelineAsset.GetType().ToString();
                if (srpType.Contains("UniversalRenderPipelineAsset") ||
                    srpType.Contains("LightweightRenderPipelineAsset"))
                {
                    var camera = GetComponent<Camera>();
                    camera.GetUniversalAdditionalCameraData().renderType =
                        CameraRenderType.Overlay;
                    GetBackgroundCamera().GetUniversalAdditionalCameraData().cameraStack.Add(camera);
                }
            }
#endif
            Initialize();
        }

        private void OnEnable()
        {
            Initialize();

            ScreenOrientation orientation = ScreenOrientationObserver.GetScreenOrientation();

            this.OnOrientationChange(orientation);
            this.OnScreenSizeChange(Screen.width, Screen.height);

            ScreenOrientationObserver.OnOrientationChange += OnOrientationChange;
            ScreenOrientationObserver.OnSizeChange += OnScreenSizeChange;

            this.backgroundGO.SetActive(true);
        }

        private void OnDisable()
        {
            // GameObject not destroyed already?
            if (this.backgroundGO != null)
            {
                this.backgroundGO.SetActive(false);
            }

            ScreenOrientationObserver.OnSizeChange -= OnScreenSizeChange;
            ScreenOrientationObserver.OnOrientationChange -= OnOrientationChange;
        }

        private void OnDestroy()
        {
            if (this.backgroundGO != null)
            {
                Destroy(this.backgroundGO);
                this.backgroundGO = null;
                // All other GameObject are children of backgroundGO and get
                // destroyed with the parent.
            }
        }
    }
}
