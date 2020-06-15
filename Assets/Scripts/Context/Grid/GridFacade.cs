using System;
using System.Collections.Generic;
using System.Linq;
using OMDB.Generic;
using OMDB.Model;
using RSG;
using UnityEngine;
using Zenject;

namespace OMDB.View
{
    public class GridFacade : Mediator
    {
        [Inject] private GridView _view;
        [Inject] private GridItem.Pool _gridItemPool;

        // Locals
        private int x = 50, y = -50;
        private Dictionary<MovieDataModel, GridItem> _gridItems = new Dictionary<MovieDataModel, GridItem>();
        private List<MovieDataModel> _sortedList = new List<MovieDataModel>();
        
        private List<IGridSignal> _gridSignalsQueue = new List<IGridSignal>();
        
        public override void Initialize()
        {
            SignalBus.Subscribe<AddMovieToGridSignal>(EnqueueSignal);
            SignalBus.Subscribe<RemoveMovieFromGridSignal>(EnqueueSignal);
            SignalBus.Subscribe<ClearGridSignal>(ClearGrid);
            SignalBus.Subscribe<LoadMoviesSignal>((signal)=> LoadGrid(signal.Movies));
            
            SignalBus.TryFire<GridInitSignal>();
        }

        private Vector2 GetPositionInGrid(int index)
        {
            return new Vector2(50 + 100 * (index % _view.GridSize.x), -50 - 100*((int)((float)index/_view.GridSize.x)));
        }

        private void LoadGrid(List<MovieDataModel> movies)
        {
            IPromise p = null;
            List<IPromise> promises = new List<IPromise>();
            
            foreach (MovieDataModel model in movies)
            {
                promises.Add(AddMovieToGrid(model, false));
            }

            Promise.All(promises).Done(() =>
            {
                int i = 0;
                foreach (var model in _sortedList)
                {
                    //Debug.Log($"Index: {i++}, {model.Title}");
                }
            });
        }
        
        private void ClearGrid()
        {
            lock (_gridSignalsQueue)
            {
                _gridSignalsQueue.Clear();
                _sortedList.Clear();

                foreach (var keyValuePair in _gridItems)
                {
                    _gridItemPool.Despawn(keyValuePair.Value);
                }

                _gridItems.Clear();
            }
        }

        private void EnqueueSignal(IGridSignal signal)
        {
            lock (_gridSignalsQueue)
            {
                _gridSignalsQueue.Add(signal);
                
                if (_gridSignalsQueue.Count == 1)
                {
                    ExecuteSignalAsync(signal);
                }
            }
        }

        private void CheckAndExecuteQueue()
        {
            lock (_gridSignalsQueue)
            {
                if (_gridSignalsQueue.Count > 0)
                {
                    var signal = _gridSignalsQueue[0];
                    
                    ExecuteSignalAsync(signal);
                }
            }
        }

        private void ExecuteSignalAsync(IGridSignal signal)
        {
            IPromise p = null;

            if (signal is AddMovieToGridSignal addMovieToGridSignal)
            {
                p = AddMovieToGrid(addMovieToGridSignal.Model);
            }
            else if (signal is RemoveMovieFromGridSignal removeMovieFromGridSignal)
            {
                p = RemoveMovieFromGrid(removeMovieFromGridSignal);
            }
            else
            {
                Debug.LogException(new NotImplementedException("Add Clear Queue case."));
            }

            var tmpSignal = signal;
            p.Done(()=>
            {
                lock (_gridSignalsQueue)
                {
                    _gridSignalsQueue.Remove(tmpSignal);
                }
                CheckAndExecuteQueue();
            });
        }

        private void DeQueueSignal(IGridSignal signal)
        {
            
        }

        private IPromise RemoveMovieFromGrid(RemoveMovieFromGridSignal obj)
        {
            Promise p = new Promise();
            IPromise tmpP = null;
            List<IPromise> promises = new List<IPromise>();
            
            if (_gridItems.ContainsKey(obj.Model))
            {
                _gridItems[obj.Model].DespawnTween().Done(() =>
                {
                    int i = _sortedList.IndexOf(obj.Model);

                    _gridItemPool.Despawn(_gridItems[obj.Model]);
                    _gridItems.Remove(obj.Model);
                    _sortedList.Remove(obj.Model);
                    
                    // Move other to place.
                    var myValueList = _sortedList;
                    
                    var ps = myValueList.Select(model => (Func<IPromise>)(() =>
                    {
                        var gridItem = _gridItems[model];
                        Vector2 pos = GetPositionInGrid(_sortedList.IndexOf(model));
                        
                        return gridItem.MoveToPositionTween(pos);
                    }));
                    
                    Promise.Sequence(ps).Done(p.Resolve);
                });
                
                return p;
            }
            else
            {
                p.Reject(new Exception($"RemoveMovieFromGrid: GridItems doesn't have item: {obj.Model.Title}"));
            }

            p.Resolve();
            
            return p;
        }

        private IPromise AddMovieToGrid(MovieDataModel model, bool animate = true)
        {
            Promise p = new Promise();
            List<Promise> promises = new List<Promise>();

            try
            {
                if (!_sortedList.Contains(model))
                {
                    _sortedList.Add(model);
                    _sortedList.Sort();
                    
                    int i = _sortedList.IndexOf(model);
                
                    GridItem gridItem = _gridItemPool.Spawn(model,
                        GetPositionInGrid(i), true);

                    _gridItems[model] = gridItem;

                    if (animate)
                    {
                        var myValueList = _sortedList;

                        for (int j = ++i; j < myValueList.Count; j++)
                        {
                            promises.Add(
                                _gridItems[myValueList[j]].MoveToPositionTween(GetPositionInGrid(j)) as Promise);
                        }
                    }
                    
                    p.Resolve();
                }
                else
                {
                    Debug.LogError($"Model already exist for {model.Title}.");
                    p.Reject(new Exception($"Model already exist for {model.Title}."));
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Exception");
                Debug.LogError(e);
            }

            promises.Add(p);
            
            return Promise.All(promises);
        }
    }
}
