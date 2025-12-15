#region

using UnityEngine;

#endregion

namespace FailCake
{
    [DisallowMultipleComponent]
    public class TickController : MonoBehaviour
    {
        public void OnDestroy() {
            util_timer.Clear();
            util_fade_timer.Clear();
        }

        public void FixedUpdate() {
            util_timer.Tick();
            util_fade_timer.Tick(Time.fixedDeltaTime);
        }
    }
}