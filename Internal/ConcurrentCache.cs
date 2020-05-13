﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace BOZHON.TSAMO.DBHelper.Internal
{
    internal class ConcurrentCache<TKey,TValue>
    {
        private readonly Dictionary<TKey, TValue> _map = new Dictionary<TKey, TValue>();

        //线程锁
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        public int Count => _map.Count;

        public TValue GetOrAdd(TKey key,Func<TValue> factory)
        {
            //读取锁定
            _lock.EnterReadLock();

            TValue val;
            try
            {
                if (_map.TryGetValue(key, out val))
                    return val;
            }
            finally
            {
                _lock.ExitReadLock();
            }

            _lock.EnterWriteLock();
            try
            {
                if (_map.TryGetValue(key, out val))
                    return val;
                val = factory();
                _map.Add(key, val);
                return val;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void ForEach(Action<KeyValuePair<TKey,TValue>> item)
        {
            _lock.EnterReadLock();
            try
            {
                foreach(var kv in _map)
                {
                    item(kv);
                }
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void Clear()
        {
            _lock.EnterWriteLock();
            try
            {
                _map.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }
}
