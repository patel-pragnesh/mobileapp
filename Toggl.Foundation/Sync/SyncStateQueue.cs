using static Toggl.Foundation.Sync.SyncState;

namespace Toggl.Foundation.Sync
{
    internal sealed class SyncStateQueue : ISyncStateQueue
    {
        private readonly object queueLock = new object();

        private bool pulledLast;
        private bool pullSyncQueued;
        private bool pushSyncQueued;
        
        public void QueuePushSync()
        {
            lock (queueLock)
            {
                pushSyncQueued = true;
            }
        }

        public void QueuePullSync()
        {
            lock (queueLock)
            {
                pullSyncQueued = true;
            }
        }

        public SyncState Dequeue()
        {
            lock (queueLock)
            {
                if (pulledLast)
                    return push();

                if (pullSyncQueued)
                    return pull();

                if (pushSyncQueued)
                    return push();

                return sleep();
            }
        }

        public void Clear()
        {
            lock (queueLock)
            {
                pulledLast = false;
                pullSyncQueued = false;
                pushSyncQueued = false;
            }
        }

        private SyncState pull()
        {
            pullSyncQueued = false;
            pulledLast = true;
            return Pull;
        }

        private SyncState push()
        {
            pushSyncQueued = false;
            pulledLast = false;
            return Push;
        }

        private SyncState sleep()
            => Sleep;
    }
}
