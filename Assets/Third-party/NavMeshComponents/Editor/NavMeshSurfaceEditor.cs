#define NAVMESHCOMPONENTS_SHOW_NAVMESHDATA_REF

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.IMGUI.Controls;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine.AI;
using UnityEngine;

namespace UnityEditor.AI
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(NavMeshSurface))]
    class NavMeshSurfaceEditor : Editor
    {
        SerializedProperty mAgentTypeID;
        SerializedProperty mBuildHeightMesh;
        SerializedProperty mCenter;
        SerializedProperty mCollectObjects;
        SerializedProperty mDefaultArea;
        SerializedProperty mLayerMask;
        SerializedProperty mOverrideTileSize;
        SerializedProperty mOverrideVoxelSize;
        SerializedProperty mSize;
        SerializedProperty mTileSize;
        SerializedProperty mUseGeometry;
        SerializedProperty mVoxelSize;

#if NAVMESHCOMPONENTS_SHOW_NAVMESHDATA_REF
        SerializedProperty mNavMeshData;
#endif
        class Styles
        {
            public readonly GUIContent MLayerMask = new GUIContent("Include Layers");

            public readonly GUIContent MShowInputGeom = new GUIContent("Show Input Geom");
            public readonly GUIContent MShowVoxels = new GUIContent("Show Voxels");
            public readonly GUIContent MShowRegions = new GUIContent("Show Regions");
            public readonly GUIContent MShowRawContours = new GUIContent("Show Raw Contours");
            public readonly GUIContent MShowContours = new GUIContent("Show Contours");
            public readonly GUIContent MShowPolyMesh = new GUIContent("Show Poly Mesh");
            public readonly GUIContent MShowPolyMeshDetail = new GUIContent("Show Poly Mesh Detail");
        }

        static Styles _sStyles;

        static bool _sShowDebugOptions;

        static Color _sHandleColor = new Color(127f, 214f, 244f, 100f) / 255;
        static Color _sHandleColorSelected = new Color(127f, 214f, 244f, 210f) / 255;
        static Color _sHandleColorDisabled = new Color(127f * 0.75f, 214f * 0.75f, 244f * 0.75f, 100f) / 255;

        BoxBoundsHandle mBoundsHandle = new BoxBoundsHandle();

        bool editingCollider
        {
            get { return EditMode.editMode == EditMode.SceneViewEditMode.Collider && EditMode.IsOwner(this); }
        }

        void OnEnable()
        {
            mAgentTypeID = serializedObject.FindProperty("m_AgentTypeID");
            mBuildHeightMesh = serializedObject.FindProperty("m_BuildHeightMesh");
            mCenter = serializedObject.FindProperty("m_Center");
            mCollectObjects = serializedObject.FindProperty("m_CollectObjects");
            mDefaultArea = serializedObject.FindProperty("m_DefaultArea");
            mLayerMask = serializedObject.FindProperty("m_LayerMask");
            mOverrideTileSize = serializedObject.FindProperty("m_OverrideTileSize");
            mOverrideVoxelSize = serializedObject.FindProperty("m_OverrideVoxelSize");
            mSize = serializedObject.FindProperty("m_Size");
            mTileSize = serializedObject.FindProperty("m_TileSize");
            mUseGeometry = serializedObject.FindProperty("m_UseGeometry");
            mVoxelSize = serializedObject.FindProperty("m_VoxelSize");

#if NAVMESHCOMPONENTS_SHOW_NAVMESHDATA_REF
            mNavMeshData = serializedObject.FindProperty("m_NavMeshData");
#endif
            NavMeshVisualizationSettings.showNavigation++;
        }

        void OnDisable()
        {
            NavMeshVisualizationSettings.showNavigation--;
        }

        Bounds GetBounds()
        {
            var navSurface = (NavMeshSurface)target;
            return new Bounds(navSurface.transform.position, navSurface.size);
        }

        public override void OnInspectorGUI()
        {
            if (_sStyles == null)
                _sStyles = new Styles();

            serializedObject.Update();

            var bs = NavMesh.GetSettingsByID(mAgentTypeID.intValue);

            if (bs.agentTypeID != -1)
            {
                // Draw image
                const float diagramHeight = 80.0f;
                Rect agentDiagramRect = EditorGUILayout.GetControlRect(false, diagramHeight);
                NavMeshEditorHelpers.DrawAgentDiagram(agentDiagramRect, bs.agentRadius, bs.agentHeight, bs.agentClimb, bs.agentSlope);
            }
            NavMeshComponentsGUIUtility.AgentTypePopup("Agent Type", mAgentTypeID);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(mCollectObjects);
            if ((CollectObjects)mCollectObjects.enumValueIndex == CollectObjects.Volume)
            {
                EditorGUI.indentLevel++;

                EditMode.DoEditModeInspectorModeButton(EditMode.SceneViewEditMode.Collider, "Edit Volume",
                    EditorGUIUtility.IconContent("EditCollider"), GetBounds, this);
                EditorGUILayout.PropertyField(mSize);
                EditorGUILayout.PropertyField(mCenter);

                EditorGUI.indentLevel--;
            }
            else
            {
                if (editingCollider)
                    EditMode.QuitEditMode();
            }

            EditorGUILayout.PropertyField(mLayerMask, _sStyles.MLayerMask);
            EditorGUILayout.PropertyField(mUseGeometry);

            EditorGUILayout.Space();

            mOverrideVoxelSize.isExpanded = EditorGUILayout.Foldout(mOverrideVoxelSize.isExpanded, "Advanced");
            if (mOverrideVoxelSize.isExpanded)
            {
                EditorGUI.indentLevel++;

                NavMeshComponentsGUIUtility.AreaPopup("Default Area", mDefaultArea);

                // Override voxel size.
                EditorGUILayout.PropertyField(mOverrideVoxelSize);

                using (new EditorGUI.DisabledScope(!mOverrideVoxelSize.boolValue || mOverrideVoxelSize.hasMultipleDifferentValues))
                {
                    EditorGUI.indentLevel++;

                    EditorGUILayout.PropertyField(mVoxelSize);

                    if (!mOverrideVoxelSize.hasMultipleDifferentValues)
                    {
                        if (!mAgentTypeID.hasMultipleDifferentValues)
                        {
                            float voxelsPerRadius = mVoxelSize.floatValue > 0.0f ? (bs.agentRadius / mVoxelSize.floatValue) : 0.0f;
                            EditorGUILayout.LabelField(" ", voxelsPerRadius.ToString("0.00") + " voxels per agent radius", EditorStyles.miniLabel);
                        }
                        if (mOverrideVoxelSize.boolValue)
                            EditorGUILayout.HelpBox("Voxel size controls how accurately the navigation mesh is generated from the level geometry. A good voxel size is 2-4 voxels per agent radius. Making voxel size smaller will increase build time.", MessageType.None);
                    }
                    EditorGUI.indentLevel--;
                }

                // Override tile size
                EditorGUILayout.PropertyField(mOverrideTileSize);

                using (new EditorGUI.DisabledScope(!mOverrideTileSize.boolValue || mOverrideTileSize.hasMultipleDifferentValues))
                {
                    EditorGUI.indentLevel++;

                    EditorGUILayout.PropertyField(mTileSize);

                    if (!mTileSize.hasMultipleDifferentValues && !mVoxelSize.hasMultipleDifferentValues)
                    {
                        float tileWorldSize = mTileSize.intValue * mVoxelSize.floatValue;
                        EditorGUILayout.LabelField(" ", tileWorldSize.ToString("0.00") + " world units", EditorStyles.miniLabel);
                    }

                    if (!mOverrideTileSize.hasMultipleDifferentValues)
                    {
                        if (mOverrideTileSize.boolValue)
                            EditorGUILayout.HelpBox("Tile size controls the how local the changes to the world are (rebuild or carve). Small tile size allows more local changes, while potentially generating more data overall.", MessageType.None);
                    }
                    EditorGUI.indentLevel--;
                }


                // Height mesh
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.PropertyField(mBuildHeightMesh);
                }

                EditorGUILayout.Space();
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();

            var hadError = false;
            var multipleTargets = targets.Length > 1;
            foreach (NavMeshSurface navSurface in targets)
            {
                var settings = navSurface.GetBuildSettings();
                // Calculating bounds is potentially expensive when unbounded - so here we just use the center/size.
                // It means the validation is not checking vertical voxel limit correctly when the surface is set to something else than "in volume".
                var bounds = new Bounds(Vector3.zero, Vector3.zero);
                if (navSurface.collectObjects == CollectObjects.Volume)
                {
                    bounds = new Bounds(navSurface.center, navSurface.size);
                }

                var errors = settings.ValidationReport(bounds);
                if (errors.Length > 0)
                {
                    if (multipleTargets)
                        EditorGUILayout.LabelField(navSurface.name);
                    foreach (var err in errors)
                    {
                        EditorGUILayout.HelpBox(err, MessageType.Warning);
                    }
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(EditorGUIUtility.labelWidth);
                    if (GUILayout.Button("Open Agent Settings...", EditorStyles.miniButton))
                        NavMeshEditorHelpers.OpenAgentSettings(navSurface.agentTypeID);
                    GUILayout.EndHorizontal();
                    hadError = true;
                }
            }

            if (hadError)
                EditorGUILayout.Space();

#if NAVMESHCOMPONENTS_SHOW_NAVMESHDATA_REF
            var nmdRect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);

            EditorGUI.BeginProperty(nmdRect, GUIContent.none, mNavMeshData);
            var rectLabel = EditorGUI.PrefixLabel(nmdRect, GUIUtility.GetControlID(FocusType.Passive), new GUIContent(mNavMeshData.displayName));
            EditorGUI.EndProperty();

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUI.BeginProperty(nmdRect, GUIContent.none, mNavMeshData);
                EditorGUI.ObjectField(rectLabel, mNavMeshData, GUIContent.none);
                EditorGUI.EndProperty();
            }
#endif
            using (new EditorGUI.DisabledScope(Application.isPlaying || mAgentTypeID.intValue == -1))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(EditorGUIUtility.labelWidth);
                if (GUILayout.Button("Clear"))
                {
                    NavMeshAssetManager.instance.ClearSurfaces(targets);
                    SceneView.RepaintAll();
                }

                if (GUILayout.Button("Bake"))
                {
                    NavMeshAssetManager.instance.StartBakingSurfaces(targets);
                }

                GUILayout.EndHorizontal();
            }

            // Show progress for the selected targets
            var bakeOperations = NavMeshAssetManager.instance.GetBakeOperations();
            for (int i = bakeOperations.Count - 1; i >= 0; --i)
            {
                if (!targets.Contains(bakeOperations[i].Surface))
                    continue;

                var oper = bakeOperations[i].BakeOperation;
                if (oper == null)
                    continue;

                var p = oper.progress;
                if (oper.isDone)
                {
                    SceneView.RepaintAll();
                    continue;
                }

                GUILayout.BeginHorizontal();

                if (GUILayout.Button("Cancel", EditorStyles.miniButton))
                {
                    var bakeData = bakeOperations[i].BakeData;
                    UnityEngine.AI.NavMeshBuilder.Cancel(bakeData);
                    bakeOperations.RemoveAt(i);
                }

                EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), p, "Baking: " + (int)(100 * p) + "%");
                if (p <= 1)
                    Repaint();

                GUILayout.EndHorizontal();
            }
        }

        [DrawGizmo(GizmoType.Selected | GizmoType.Active | GizmoType.Pickable)]
        static void RenderBoxGizmoSelected(NavMeshSurface navSurface, GizmoType gizmoType)
        {
            RenderBoxGizmo(navSurface, gizmoType, true);
        }

        [DrawGizmo(GizmoType.NotInSelectionHierarchy | GizmoType.Pickable)]
        static void RenderBoxGizmoNotSelected(NavMeshSurface navSurface, GizmoType gizmoType)
        {
            if (NavMeshVisualizationSettings.showNavigation > 0)
                RenderBoxGizmo(navSurface, gizmoType, false);
            else
                Gizmos.DrawIcon(navSurface.transform.position, "NavMeshSurface Icon", true);
        }

        static void RenderBoxGizmo(NavMeshSurface navSurface, GizmoType gizmoType, bool selected)
        {
            var color = selected ? _sHandleColorSelected : _sHandleColor;
            if (!navSurface.enabled)
                color = _sHandleColorDisabled;

            var oldColor = Gizmos.color;
            var oldMatrix = Gizmos.matrix;

            // Use the unscaled matrix for the NavMeshSurface
            var localToWorld = Matrix4x4.TRS(navSurface.transform.position, navSurface.transform.rotation, Vector3.one);
            Gizmos.matrix = localToWorld;

            if (navSurface.collectObjects == CollectObjects.Volume)
            {
                Gizmos.color = color;
                Gizmos.DrawWireCube(navSurface.center, navSurface.size);

                if (selected && navSurface.enabled)
                {
                    var colorTrans = new Color(color.r * 0.75f, color.g * 0.75f, color.b * 0.75f, color.a * 0.15f);
                    Gizmos.color = colorTrans;
                    Gizmos.DrawCube(navSurface.center, navSurface.size);
                }
            }
            else
            {
                if (navSurface.navMeshData != null)
                {
                    var bounds = navSurface.navMeshData.sourceBounds;
                    Gizmos.color = Color.grey;
                    Gizmos.DrawWireCube(bounds.center, bounds.size);
                }
            }

            Gizmos.matrix = oldMatrix;
            Gizmos.color = oldColor;

            Gizmos.DrawIcon(navSurface.transform.position, "NavMeshSurface Icon", true);
        }

        void OnSceneGUI()
        {
            if (!editingCollider)
                return;

            var navSurface = (NavMeshSurface)target;
            var color = navSurface.enabled ? _sHandleColor : _sHandleColorDisabled;
            var localToWorld = Matrix4x4.TRS(navSurface.transform.position, navSurface.transform.rotation, Vector3.one);
            using (new Handles.DrawingScope(color, localToWorld))
            {
                mBoundsHandle.center = navSurface.center;
                mBoundsHandle.size = navSurface.size;

                EditorGUI.BeginChangeCheck();
                mBoundsHandle.DrawHandle();
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(navSurface, "Modified NavMesh Surface");
                    Vector3 center = mBoundsHandle.center;
                    Vector3 size = mBoundsHandle.size;
                    navSurface.center = center;
                    navSurface.size = size;
                    EditorUtility.SetDirty(target);
                }
            }
        }

        [MenuItem("GameObject/AI/NavMesh Surface", false, 2000)]
        public static void CreateNavMeshSurface(MenuCommand menuCommand)
        {
            var parent = menuCommand.context as GameObject;
            var go = NavMeshComponentsGUIUtility.CreateAndSelectGameObject("NavMesh Surface", parent);
            go.AddComponent<NavMeshSurface>();
            var view = SceneView.lastActiveSceneView;
            if (view != null)
                view.MoveToView(go.transform);
        }
    }
}
