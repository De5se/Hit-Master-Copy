using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace UnityEngine.AI
{
    public enum CollectObjects
    {
        All = 0,
        Volume = 1,
        Children = 2,
    }

    [ExecuteAlways]
    [DefaultExecutionOrder(-102)]
    [AddComponentMenu("Navigation/NavMeshSurface", 30)]
    [HelpURL("https://github.com/Unity-Technologies/NavMeshComponents#documentation-draft")]
    public class NavMeshSurface : MonoBehaviour
    {
        [SerializeField]
        int mAgentTypeID;
        public int agentTypeID { get { return mAgentTypeID; } set { mAgentTypeID = value; } }

        [SerializeField]
        CollectObjects mCollectObjects = CollectObjects.All;
        public CollectObjects collectObjects { get { return mCollectObjects; } set { mCollectObjects = value; } }

        [SerializeField]
        Vector3 mSize = new Vector3(10.0f, 10.0f, 10.0f);
        public Vector3 size { get { return mSize; } set { mSize = value; } }

        [SerializeField]
        Vector3 mCenter = new Vector3(0, 2.0f, 0);
        public Vector3 center { get { return mCenter; } set { mCenter = value; } }

        [SerializeField]
        LayerMask mLayerMask = ~0;
        public LayerMask layerMask { get { return mLayerMask; } set { mLayerMask = value; } }

        [SerializeField]
        NavMeshCollectGeometry mUseGeometry = NavMeshCollectGeometry.RenderMeshes;
        public NavMeshCollectGeometry useGeometry { get { return mUseGeometry; } set { mUseGeometry = value; } }

        [SerializeField]
        int mDefaultArea;
        public int defaultArea { get { return mDefaultArea; } set { mDefaultArea = value; } }

        [SerializeField]
        bool mIgnoreNavMeshAgent = true;
        public bool ignoreNavMeshAgent { get { return mIgnoreNavMeshAgent; } set { mIgnoreNavMeshAgent = value; } }

        [SerializeField]
        bool mIgnoreNavMeshObstacle = true;
        public bool ignoreNavMeshObstacle { get { return mIgnoreNavMeshObstacle; } set { mIgnoreNavMeshObstacle = value; } }

        [SerializeField]
        bool mOverrideTileSize;
        public bool overrideTileSize { get { return mOverrideTileSize; } set { mOverrideTileSize = value; } }
        [SerializeField]
        int mTileSize = 256;
        public int tileSize { get { return mTileSize; } set { mTileSize = value; } }
        [SerializeField]
        bool mOverrideVoxelSize;
        public bool overrideVoxelSize { get { return mOverrideVoxelSize; } set { mOverrideVoxelSize = value; } }
        [SerializeField]
        float mVoxelSize;
        public float voxelSize { get { return mVoxelSize; } set { mVoxelSize = value; } }

        // Currently not supported advanced options
        [SerializeField]
        bool mBuildHeightMesh;
        public bool buildHeightMesh { get { return mBuildHeightMesh; } set { mBuildHeightMesh = value; } }

        // Reference to whole scene navmesh data asset.
        [UnityEngine.Serialization.FormerlySerializedAs("m_BakedNavMeshData")]
        [SerializeField]
        NavMeshData mNavMeshData;
        public NavMeshData navMeshData { get { return mNavMeshData; } set { mNavMeshData = value; } }

        // Do not serialize - runtime only state.
        NavMeshDataInstance mNavMeshDataInstance;
        Vector3 mLastPosition = Vector3.zero;
        Quaternion mLastRotation = Quaternion.identity;

        static readonly List<NavMeshSurface> SNavMeshSurfaces = new List<NavMeshSurface>();

        public static List<NavMeshSurface> activeSurfaces
        {
            get { return SNavMeshSurfaces; }
        }

        void OnEnable()
        {
            Register(this);
            AddData();
        }

        void OnDisable()
        {
            RemoveData();
            Unregister(this);
        }

        public void AddData()
        {
#if UNITY_EDITOR
            var isInPreviewScene = EditorSceneManager.IsPreviewSceneObject(this);
            var isPrefab = isInPreviewScene || EditorUtility.IsPersistent(this);
            if (isPrefab)
            {
                //Debug.LogFormat("NavMeshData from {0}.{1} will not be added to the NavMesh world because the gameObject is a prefab.",
                //    gameObject.name, name);
                return;
            }
#endif
            if (mNavMeshDataInstance.valid)
                return;

            if (mNavMeshData != null)
            {
                mNavMeshDataInstance = NavMesh.AddNavMeshData(mNavMeshData, transform.position, transform.rotation);
                mNavMeshDataInstance.owner = this;
            }

            mLastPosition = transform.position;
            mLastRotation = transform.rotation;
        }

        public void RemoveData()
        {
            mNavMeshDataInstance.Remove();
            mNavMeshDataInstance = new NavMeshDataInstance();
        }

        public NavMeshBuildSettings GetBuildSettings()
        {
            var buildSettings = NavMesh.GetSettingsByID(mAgentTypeID);
            if (buildSettings.agentTypeID == -1)
            {
                Debug.LogWarning("No build settings for agent type ID " + agentTypeID, this);
                buildSettings.agentTypeID = mAgentTypeID;
            }

            if (overrideTileSize)
            {
                buildSettings.overrideTileSize = true;
                buildSettings.tileSize = tileSize;
            }
            if (overrideVoxelSize)
            {
                buildSettings.overrideVoxelSize = true;
                buildSettings.voxelSize = voxelSize;
            }
            return buildSettings;
        }

        public void BuildNavMesh()
        {
            var sources = CollectSources();

            // Use unscaled bounds - this differs in behaviour from e.g. collider components.
            // But is similar to reflection probe - and since navmesh data has no scaling support - it is the right choice here.
            var sourcesBounds = new Bounds(mCenter, Abs(mSize));
            if (mCollectObjects == CollectObjects.All || mCollectObjects == CollectObjects.Children)
            {
                sourcesBounds = CalculateWorldBounds(sources);
            }

            var data = NavMeshBuilder.BuildNavMeshData(GetBuildSettings(),
                    sources, sourcesBounds, transform.position, transform.rotation);

            if (data != null)
            {
                data.name = gameObject.name;
                RemoveData();
                mNavMeshData = data;
                if (isActiveAndEnabled)
                    AddData();
            }
        }

        public AsyncOperation UpdateNavMesh(NavMeshData data)
        {
            var sources = CollectSources();

            // Use unscaled bounds - this differs in behaviour from e.g. collider components.
            // But is similar to reflection probe - and since navmesh data has no scaling support - it is the right choice here.
            var sourcesBounds = new Bounds(mCenter, Abs(mSize));
            if (mCollectObjects == CollectObjects.All || mCollectObjects == CollectObjects.Children)
                sourcesBounds = CalculateWorldBounds(sources);

            return NavMeshBuilder.UpdateNavMeshDataAsync(data, GetBuildSettings(), sources, sourcesBounds);
        }

        static void Register(NavMeshSurface surface)
        {
#if UNITY_EDITOR
            var isInPreviewScene = EditorSceneManager.IsPreviewSceneObject(surface);
            var isPrefab = isInPreviewScene || EditorUtility.IsPersistent(surface);
            if (isPrefab)
            {
                //Debug.LogFormat("NavMeshData from {0}.{1} will not be added to the NavMesh world because the gameObject is a prefab.",
                //    surface.gameObject.name, surface.name);
                return;
            }
#endif
            if (SNavMeshSurfaces.Count == 0)
                NavMesh.onPreUpdate += UpdateActive;

            if (!SNavMeshSurfaces.Contains(surface))
                SNavMeshSurfaces.Add(surface);
        }

        static void Unregister(NavMeshSurface surface)
        {
            SNavMeshSurfaces.Remove(surface);

            if (SNavMeshSurfaces.Count == 0)
                NavMesh.onPreUpdate -= UpdateActive;
        }

        static void UpdateActive()
        {
            for (var i = 0; i < SNavMeshSurfaces.Count; ++i)
                SNavMeshSurfaces[i].UpdateDataIfTransformChanged();
        }

        void AppendModifierVolumes(ref List<NavMeshBuildSource> sources)
        {
#if UNITY_EDITOR
            var myStage = StageUtility.GetStageHandle(gameObject);
            if (!myStage.IsValid())
                return;
#endif
            // Modifiers
            List<NavMeshModifierVolume> modifiers;
            if (mCollectObjects == CollectObjects.Children)
            {
                modifiers = new List<NavMeshModifierVolume>(GetComponentsInChildren<NavMeshModifierVolume>());
                modifiers.RemoveAll(x => !x.isActiveAndEnabled);
            }
            else
            {
                modifiers = NavMeshModifierVolume.activeModifiers;
            }

            foreach (var m in modifiers)
            {
                if ((mLayerMask & (1 << m.gameObject.layer)) == 0)
                    continue;
                if (!m.AffectsAgentType(mAgentTypeID))
                    continue;
#if UNITY_EDITOR
                if (!myStage.Contains(m.gameObject))
                    continue;
#endif
                var mcenter = m.transform.TransformPoint(m.center);
                var scale = m.transform.lossyScale;
                var msize = new Vector3(m.size.x * Mathf.Abs(scale.x), m.size.y * Mathf.Abs(scale.y), m.size.z * Mathf.Abs(scale.z));

                var src = new NavMeshBuildSource();
                src.shape = NavMeshBuildSourceShape.ModifierBox;
                src.transform = Matrix4x4.TRS(mcenter, m.transform.rotation, Vector3.one);
                src.size = msize;
                src.area = m.area;
                sources.Add(src);
            }
        }

        List<NavMeshBuildSource> CollectSources()
        {
            var sources = new List<NavMeshBuildSource>();
            var markups = new List<NavMeshBuildMarkup>();

            List<NavMeshModifier> modifiers;
            if (mCollectObjects == CollectObjects.Children)
            {
                modifiers = new List<NavMeshModifier>(GetComponentsInChildren<NavMeshModifier>());
                modifiers.RemoveAll(x => !x.isActiveAndEnabled);
            }
            else
            {
                modifiers = NavMeshModifier.activeModifiers;
            }

            foreach (var m in modifiers)
            {
                if ((mLayerMask & (1 << m.gameObject.layer)) == 0)
                    continue;
                if (!m.AffectsAgentType(mAgentTypeID))
                    continue;
                var markup = new NavMeshBuildMarkup();
                markup.root = m.transform;
                markup.overrideArea = m.overrideArea;
                markup.area = m.area;
                markup.ignoreFromBuild = m.ignoreFromBuild;
                markups.Add(markup);
            }

#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
            {
                if (mCollectObjects == CollectObjects.All)
                {
                    UnityEditor.AI.NavMeshBuilder.CollectSourcesInStage(
                        null, mLayerMask, mUseGeometry, mDefaultArea, markups, gameObject.scene, sources);
                }
                else if (mCollectObjects == CollectObjects.Children)
                {
                    UnityEditor.AI.NavMeshBuilder.CollectSourcesInStage(
                        transform, mLayerMask, mUseGeometry, mDefaultArea, markups, gameObject.scene, sources);
                }
                else if (mCollectObjects == CollectObjects.Volume)
                {
                    Matrix4x4 localToWorld = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
                    var worldBounds = GetWorldBounds(localToWorld, new Bounds(mCenter, mSize));

                    UnityEditor.AI.NavMeshBuilder.CollectSourcesInStage(
                        worldBounds, mLayerMask, mUseGeometry, mDefaultArea, markups, gameObject.scene, sources);
                }
            }
            else
#endif
            {
                if (mCollectObjects == CollectObjects.All)
                {
                    NavMeshBuilder.CollectSources(null, mLayerMask, mUseGeometry, mDefaultArea, markups, sources);
                }
                else if (mCollectObjects == CollectObjects.Children)
                {
                    NavMeshBuilder.CollectSources(transform, mLayerMask, mUseGeometry, mDefaultArea, markups, sources);
                }
                else if (mCollectObjects == CollectObjects.Volume)
                {
                    Matrix4x4 localToWorld = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
                    var worldBounds = GetWorldBounds(localToWorld, new Bounds(mCenter, mSize));
                    NavMeshBuilder.CollectSources(worldBounds, mLayerMask, mUseGeometry, mDefaultArea, markups, sources);
                }
            }

            if (mIgnoreNavMeshAgent)
                sources.RemoveAll((x) => (x.component != null && x.component.gameObject.GetComponent<NavMeshAgent>() != null));

            if (mIgnoreNavMeshObstacle)
                sources.RemoveAll((x) => (x.component != null && x.component.gameObject.GetComponent<NavMeshObstacle>() != null));

            AppendModifierVolumes(ref sources);

            return sources;
        }

        static Vector3 Abs(Vector3 v)
        {
            return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
        }

        static Bounds GetWorldBounds(Matrix4x4 mat, Bounds bounds)
        {
            var absAxisX = Abs(mat.MultiplyVector(Vector3.right));
            var absAxisY = Abs(mat.MultiplyVector(Vector3.up));
            var absAxisZ = Abs(mat.MultiplyVector(Vector3.forward));
            var worldPosition = mat.MultiplyPoint(bounds.center);
            var worldSize = absAxisX * bounds.size.x + absAxisY * bounds.size.y + absAxisZ * bounds.size.z;
            return new Bounds(worldPosition, worldSize);
        }

        Bounds CalculateWorldBounds(List<NavMeshBuildSource> sources)
        {
            // Use the unscaled matrix for the NavMeshSurface
            Matrix4x4 worldToLocal = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            worldToLocal = worldToLocal.inverse;

            var result = new Bounds();
            foreach (var src in sources)
            {
                switch (src.shape)
                {
                    case NavMeshBuildSourceShape.Mesh:
                    {
                        var m = src.sourceObject as Mesh;
                        result.Encapsulate(GetWorldBounds(worldToLocal * src.transform, m.bounds));
                        break;
                    }
                    case NavMeshBuildSourceShape.Terrain:
                    {
                        // Terrain pivot is lower/left corner - shift bounds accordingly
                        var t = src.sourceObject as TerrainData;
                        result.Encapsulate(GetWorldBounds(worldToLocal * src.transform, new Bounds(0.5f * t.size, t.size)));
                        break;
                    }
                    case NavMeshBuildSourceShape.Box:
                    case NavMeshBuildSourceShape.Sphere:
                    case NavMeshBuildSourceShape.Capsule:
                    case NavMeshBuildSourceShape.ModifierBox:
                        result.Encapsulate(GetWorldBounds(worldToLocal * src.transform, new Bounds(Vector3.zero, src.size)));
                        break;
                }
            }
            // Inflate the bounds a bit to avoid clipping co-planar sources
            result.Expand(0.1f);
            return result;
        }

        bool HasTransformChanged()
        {
            if (mLastPosition != transform.position) return true;
            if (mLastRotation != transform.rotation) return true;
            return false;
        }

        void UpdateDataIfTransformChanged()
        {
            if (HasTransformChanged())
            {
                RemoveData();
                AddData();
            }
        }

#if UNITY_EDITOR
        bool UnshareNavMeshAsset()
        {
            // Nothing to unshare
            if (mNavMeshData == null)
                return false;

            // Prefab parent owns the asset reference
            var isInPreviewScene = EditorSceneManager.IsPreviewSceneObject(this);
            var isPersistentObject = EditorUtility.IsPersistent(this);
            if (isInPreviewScene || isPersistentObject)
                return false;

            // An instance can share asset reference only with its prefab parent
            var prefab = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(this) as NavMeshSurface;
            if (prefab != null && prefab.navMeshData == navMeshData)
                return false;

            // Don't allow referencing an asset that's assigned to another surface
            for (var i = 0; i < SNavMeshSurfaces.Count; ++i)
            {
                var surface = SNavMeshSurfaces[i];
                if (surface != this && surface.mNavMeshData == mNavMeshData)
                    return true;
            }

            // Asset is not referenced by known surfaces
            return false;
        }

        void OnValidate()
        {
            if (UnshareNavMeshAsset())
            {
                Debug.LogWarning("Duplicating NavMeshSurface does not duplicate the referenced navmesh data", this);
                mNavMeshData = null;
            }

            var settings = NavMesh.GetSettingsByID(mAgentTypeID);
            if (settings.agentTypeID != -1)
            {
                // When unchecking the override control, revert to automatic value.
                const float kMinVoxelSize = 0.01f;
                if (!mOverrideVoxelSize)
                    mVoxelSize = settings.agentRadius / 3.0f;
                if (mVoxelSize < kMinVoxelSize)
                    mVoxelSize = kMinVoxelSize;

                // When unchecking the override control, revert to default value.
                const int kMinTileSize = 16;
                const int kMaxTileSize = 1024;
                const int kDefaultTileSize = 256;

                if (!mOverrideTileSize)
                    mTileSize = kDefaultTileSize;
                // Make sure tilesize is in sane range.
                if (mTileSize < kMinTileSize)
                    mTileSize = kMinTileSize;
                if (mTileSize > kMaxTileSize)
                    mTileSize = kMaxTileSize;
            }
        }
#endif
    }
}
