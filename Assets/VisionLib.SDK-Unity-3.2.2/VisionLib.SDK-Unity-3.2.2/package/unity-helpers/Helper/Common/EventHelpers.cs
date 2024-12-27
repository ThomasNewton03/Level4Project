using System.Threading.Tasks;
using UnityEngine.Events;

namespace Visometry.Helpers
{
    public static class EventHelpers
    {
        /// <summary>
        /// Returns a task that is completed when the event is invoked the next time.
        /// </summary>
        public static Task AsTask(this UnityEvent e)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            void Fun()
            {
                tcs.SetResult(true);
                e.RemoveListener(Fun);
            }
            e.AddListener(Fun);
            return tcs.Task;
        }
    }
}
