using jp.ootr.common;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace jp.ootr.CustomScaler
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CustomScaler : BaseClass
    {
        private int _customScale = 10;
        [SerializeField] internal int minScale = 1;
        [SerializeField] internal int maxScale = 20;
        [SerializeField] internal int defaultScale = 10;
        [SerializeField] internal int scaleResolution = 10;
        [SerializeField] internal GameObject[] scaleTargetGameObject;
        [SerializeField] internal GameObject[] reverseScaleTargetGameObject;
        [UdonSynced] private int _syncedScale = 10;
        [SerializeField] internal bool isSynced = false;
        internal UdonSharpBehaviour[] ScaleEventReceivers = new UdonSharpBehaviour[0];
        
        public void ScaleUp()
        {
            _customScale += 1;
            ApplyScale();
        }
        
        public void ScaleDown()
        {
            _customScale -= 1;
            ApplyScale();
        }
        
        public void ResetScale()
        {
            _customScale = defaultScale;
            ApplyScale();
        }

        private void ApplyScale()
        {
            _customScale = Mathf.Clamp(_customScale, minScale, maxScale);
            var scale = _customScale / (float)scaleResolution;
            var reverseScale = 1 / scale;
            foreach (var obj in scaleTargetGameObject)
            {
                if (obj == null) continue;
                obj.transform.localScale = new Vector3(scale, scale, scale);
            }

            foreach (var obj in reverseScaleTargetGameObject)
            {
                if (obj == null) continue;
                obj.transform.localScale = new Vector3(reverseScale, reverseScale, reverseScale);
            }
            
            foreach (var receiver in ScaleEventReceivers)
            {
                if (receiver == null) continue;
                receiver.SendCustomEvent("OnScaleChanged");
            }

            if (!isSynced) return;
            _syncedScale = _customScale;
            Sync();
        }

        public override void _OnDeserialization()
        {
            base._OnDeserialization();
            if (!isSynced) return;
            _customScale = _syncedScale;
            ApplyScale();
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            base.OnPlayerJoined(player);
            if (!isSynced || !Networking.IsOwner(gameObject)) return;
            _syncedScale = _customScale;
            Sync();
        }

        public void AddEventListener(UdonSharpBehaviour listener)
        {
            if (listener == null) return;
            ScaleEventReceivers = ScaleEventReceivers.Append(listener);
        }
    }
}
