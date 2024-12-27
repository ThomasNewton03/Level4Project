using System;
using System.Collections.Generic;
using System.Linq;
using Visometry.VisionLib.SDK.Core.API.Native;

namespace Visometry.VisionLib.SDK.Core.API
{
    /// @ingroup API
    internal class AnchorObservableMap
    {
        private Dictionary<string, EventWrapper<SimilarityTransform>> nameToObservable =
            new Dictionary<string, EventWrapper<SimilarityTransform>>();

        private object nameToObservableLock = new object();

        public EventWrapper<SimilarityTransform> GetOrCreate(string anchorName)
        {
            lock (this.nameToObservableLock)
            {
                if (!this.nameToObservable.ContainsKey(anchorName))
                {
                    this.nameToObservable.Add(anchorName, new EventWrapper<SimilarityTransform>());
                }
                return this.nameToObservable[anchorName];
            }
        }

        public void NotifyAll(Dictionary<string, SimilarityTransform> anchorTransforms)
        {
            lock (this.nameToObservableLock)
            {
                foreach (var nameAndAnchorTransform in anchorTransforms)
                {
                    var anchorName = nameAndAnchorTransform.Key;
                    var anchorTransform = nameAndAnchorTransform.Value;
                    if (!this.nameToObservable.ContainsKey(anchorName))
                    {
                        continue;
                    }
                    this.nameToObservable[anchorName].Notify(anchorTransform);
                }
            }
        }

        public List<string> GetAnchorNames()
        {
            lock (this.nameToObservableLock)
            {
                return this.nameToObservable.ToList().Where(pair => pair.Value.IsUsed())
                    .Select(pair => pair.Key).ToList();
            }
        }

        private class PseudoAnchorHandler : IDisposable
        {
            public void Dispose() {}
        }

        // This adds new AnchorHandlers to the given map for each non-empty observable
        public void SynchronizeHandler(Dictionary<string, IDisposable> nameToHandler, Worker worker)
        {
            lock (this.nameToObservableLock)
            {
                foreach (var observableKV in this.nameToObservable)
                {
                    var anchorName = observableKV.Key;
                    var observable = observableKV.Value;

                    // Create new handler for used observers
                    if (observable.IsUsed() && !nameToHandler.ContainsKey(anchorName))
                    {
                        try
                        {
                            nameToHandler.Add(
                                anchorName,
                                new AnchorHandler(worker, anchorName, observable));
                        }
                        catch (InvalidOperationException)
                        {
                            nameToHandler.Add(anchorName, new PseudoAnchorHandler());
                        }
                    }

                    // Remove handler for unused observers
                    if (!observable.IsUsed() && nameToHandler.ContainsKey(anchorName))
                    {
                        nameToHandler[anchorName].Dispose();
                        nameToHandler.Remove(anchorName);
                    }
                }

                // Remove all unused observers
                this.nameToObservable = this.nameToObservable
                    .Where(observableKV => observableKV.Value.IsUsed()).ToDictionary(
                        observableKV => observableKV.Key,
                        observableKV => observableKV.Value);
            }
        }
    }
}
