using System.Collections.Generic;

namespace KioskBrains.Kiosk.Helpers.Collections
{
    public class ConcurrentHashSet<TItem>
    {
        private readonly HashSet<TItem> _hashSet = new HashSet<TItem>();

        private readonly object _hashSetLock = new object();

        public bool Add(TItem item)
        {
            lock (_hashSetLock)
            {
                return _hashSet.Add(item);
            }
        }

        public bool Remove(TItem item)
        {
            lock (_hashSetLock)
            {
                return _hashSet.Remove(item);
            }
        }

        public bool Contains(TItem item)
        {
            lock (_hashSetLock)
            {
                return _hashSet.Contains(item);
            }
        }
    }
}