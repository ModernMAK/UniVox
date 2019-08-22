using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rendering
{
    public abstract class PipelineV2<TKey, TJob> : IDisposable where TJob : IPipelineHandle
    {
        public PipelineV2()
        {
            _jobLookup = new Dictionary<TKey, TJob>();
            _jobsToRemove = new Queue<KeyValuePair<TKey, TJob>>();
        }

        private readonly Dictionary<TKey, TJob> _jobLookup;
        private readonly Queue<KeyValuePair<TKey, TJob>> _jobsToRemove;
        private event EventHandler<TKey> _completionEvent;

        public event EventHandler<TKey> Completed
        {
            add => _completionEvent += value;
            remove => _completionEvent -= value;
        }


        public void AddJob(TKey key, TJob job)
        {
            if (job.IsCompleted())
            {
                Debug.LogWarning($"Cant add a completed job!\n{job.ToString()}");
                return;
            }

            if (_jobLookup.TryGetValue(key, out var oldJob))
            {
                Debug.LogWarning($"An old job is being completed and disposed!\n{oldJob.ToString()}");
                oldJob.CompleteAndDispose();
            }

            _jobLookup[key] = job;
        }

        public bool HasJob(TKey key) => _jobLookup.ContainsKey(key);


        /// <summary>
        /// Removes the job after ensuring its completed and disposed. Does not raise a completion event.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool RemoveJob(TKey key)
        {
            if (!_jobLookup.TryGetValue(key, out var oldJob))
                return false;
            RemoveLogic(key, oldJob);
            return true;
        }


        private void RemoveLogic(TKey key, TJob job)
        {
            job.CompleteAndDispose();
            _jobLookup.Remove(key);
        }

        public void UpdateEvents()
        {
            foreach (var pair in _jobLookup)
            {
                if (pair.Value.IsCompleted())
                    _jobsToRemove.Enqueue(pair);
            }

            while (_jobsToRemove.Count > 0)
            {
                var pair = _jobsToRemove.Dequeue();
                RemoveLogic(pair.Key, pair.Value);
                _completionEvent?.Invoke(this, pair.Key);
            }
        }

        public void Dispose()
        {
            foreach (var pair in _jobLookup)
                pair.Value.CompleteAndDispose();
        }
    }
}