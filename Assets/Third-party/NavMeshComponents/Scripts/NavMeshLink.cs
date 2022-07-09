using System.Collections.Generic;

namespace UnityEngine.AI
{
    [ExecuteInEditMode]
    [DefaultExecutionOrder(-101)]
    [AddComponentMenu("Navigation/NavMeshLink", 33)]
    [HelpURL("https://github.com/Unity-Technologies/NavMeshComponents#documentation-draft")]
    public class NavMeshLink : MonoBehaviour
    {
        [SerializeField]
        int mAgentTypeID;
        public int agentTypeID { get { return mAgentTypeID; } set { mAgentTypeID = value; UpdateLink(); } }

        [SerializeField]
        Vector3 mStartPoint = new Vector3(0.0f, 0.0f, -2.5f);
        public Vector3 startPoint { get { return mStartPoint; } set { mStartPoint = value; UpdateLink(); } }

        [SerializeField]
        Vector3 mEndPoint = new Vector3(0.0f, 0.0f, 2.5f);
        public Vector3 endPoint { get { return mEndPoint; } set { mEndPoint = value; UpdateLink(); } }

        [SerializeField]
        float mWidth;
        public float width { get { return mWidth; } set { mWidth = value; UpdateLink(); } }

        [SerializeField]
        int mCostModifier = -1;
        public int costModifier { get { return mCostModifier; } set { mCostModifier = value; UpdateLink(); } }

        [SerializeField]
        bool mBidirectional = true;
        public bool bidirectional { get { return mBidirectional; } set { mBidirectional = value; UpdateLink(); } }

        [SerializeField]
        bool mAutoUpdatePosition;
        public bool autoUpdate { get { return mAutoUpdatePosition; } set { SetAutoUpdate(value); } }

        [SerializeField]
        int mArea;
        public int area { get { return mArea; } set { mArea = value; UpdateLink(); } }

        NavMeshLinkInstance mLinkInstance = new NavMeshLinkInstance();

        Vector3 mLastPosition = Vector3.zero;
        Quaternion mLastRotation = Quaternion.identity;

        static readonly List<NavMeshLink> STracked = new List<NavMeshLink>();

        void OnEnable()
        {
            AddLink();
            if (mAutoUpdatePosition && mLinkInstance.valid)
                AddTracking(this);
        }

        void OnDisable()
        {
            RemoveTracking(this);
            mLinkInstance.Remove();
        }

        public void UpdateLink()
        {
            mLinkInstance.Remove();
            AddLink();
        }

        static void AddTracking(NavMeshLink link)
        {
#if UNITY_EDITOR
            if (STracked.Contains(link))
            {
                Debug.LogError("Link is already tracked: " + link);
                return;
            }
#endif

            if (STracked.Count == 0)
                NavMesh.onPreUpdate += UpdateTrackedInstances;

            STracked.Add(link);
        }

        static void RemoveTracking(NavMeshLink link)
        {
            STracked.Remove(link);

            if (STracked.Count == 0)
                NavMesh.onPreUpdate -= UpdateTrackedInstances;
        }

        void SetAutoUpdate(bool value)
        {
            if (mAutoUpdatePosition == value)
                return;
            mAutoUpdatePosition = value;
            if (value)
                AddTracking(this);
            else
                RemoveTracking(this);
        }

        void AddLink()
        {
#if UNITY_EDITOR
            if (mLinkInstance.valid)
            {
                Debug.LogError("Link is already added: " + this);
                return;
            }
#endif

            var link = new NavMeshLinkData();
            link.startPosition = mStartPoint;
            link.endPosition = mEndPoint;
            link.width = mWidth;
            link.costModifier = mCostModifier;
            link.bidirectional = mBidirectional;
            link.area = mArea;
            link.agentTypeID = mAgentTypeID;
            mLinkInstance = NavMesh.AddLink(link, transform.position, transform.rotation);
            if (mLinkInstance.valid)
                mLinkInstance.owner = this;

            mLastPosition = transform.position;
            mLastRotation = transform.rotation;
        }

        bool HasTransformChanged()
        {
            if (mLastPosition != transform.position) return true;
            if (mLastRotation != transform.rotation) return true;
            return false;
        }

        void OnDidApplyAnimationProperties()
        {
            UpdateLink();
        }

        static void UpdateTrackedInstances()
        {
            foreach (var instance in STracked)
            {
                if (instance.HasTransformChanged())
                    instance.UpdateLink();
            }
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            mWidth = Mathf.Max(0.0f, mWidth);

            if (!mLinkInstance.valid)
                return;

            UpdateLink();

            if (!mAutoUpdatePosition)
            {
                RemoveTracking(this);
            }
            else if (!STracked.Contains(this))
            {
                AddTracking(this);
            }
        }
#endif
    }
}
