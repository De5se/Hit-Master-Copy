using UnityEngine.AI;

namespace UnityEditor.AI
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(NavMeshModifier))]
    class NavMeshModifierEditor : Editor
    {
        SerializedProperty mAffectedAgents;
        SerializedProperty mArea;
        SerializedProperty mIgnoreFromBuild;
        SerializedProperty mOverrideArea;

        void OnEnable()
        {
            mAffectedAgents = serializedObject.FindProperty("m_AffectedAgents");
            mArea = serializedObject.FindProperty("m_Area");
            mIgnoreFromBuild = serializedObject.FindProperty("m_IgnoreFromBuild");
            mOverrideArea = serializedObject.FindProperty("m_OverrideArea");

            NavMeshVisualizationSettings.showNavigation++;
        }

        void OnDisable()
        {
            NavMeshVisualizationSettings.showNavigation--;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(mIgnoreFromBuild);

            EditorGUILayout.PropertyField(mOverrideArea);
            if (mOverrideArea.boolValue)
            {
                EditorGUI.indentLevel++;
                NavMeshComponentsGUIUtility.AreaPopup("Area Type", mArea);
                EditorGUI.indentLevel--;
            }

            NavMeshComponentsGUIUtility.AgentMaskPopup("Affected Agents", mAffectedAgents);
            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
