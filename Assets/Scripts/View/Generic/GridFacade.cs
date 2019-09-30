using System;
using System.Collections.Generic;
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
        private SortedList<string, MovieDataModel> _sortedList = new SortedList<string, MovieDataModel>();
        
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
            
            foreach (MovieDataModel model in movies)
            {
                if (p == null)
                {
                    p = AddMovieToGrid(model);
                }
                else
                {
                    p = p.Then(() => AddMovieToGrid(model));
                }
            }
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
                    ExecuteSignal(signal);
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
                    
                    ExecuteSignal(signal);
                }
            }
        }

        private void ExecuteSignal(IGridSignal signal)
        {
            IPromise p = null;
            
            if (signal is AddMovieToGridSignal)
            {
                p = AddMovieToGrid((signal as AddMovieToGridSignal).Model);
            }
            else if (signal is RemoveMovieFromGridSignal)
            {
                p = RemoveMovieFromGrid(signal as RemoveMovieFromGridSignal);
            }
            else
            {
                Debug.LogException(new NotImplementedException("Add Clear Queue case."));
            }
                    
            p.Done(()=>
            {
                lock (_gridSignalsQueue)
                {
                    _gridSignalsQueue.RemoveAt(0);
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
            IPromise tmpP = p;
            
            if (_gridItems.ContainsKey(obj.Model))
            {
                _gridItems[obj.Model].DespawnTween().Done(() =>
                {
                    // Move other to place.
                    int i = _sortedList.IndexOfKey(obj.Model.Title);

                    _gridItemPool.Despawn(_gridItems[obj.Model]);
                    _gridItems.Remove(obj.Model);
                    _sortedList.Remove(obj.Model.Title);
                    
                    var myValueList = _sortedList.Values;
                    for (; i < myValueList.Count; i++)
                    {
                        var gridItem = _gridItems[myValueList[i]];
                        Vector2 pos = GetPositionInGrid(i);
                        
                        tmpP = tmpP.Then(() =>
                        {
                            try
                            {
                                gridItem.ToString();
                            }
                            catch (Exception e)
                            {
                                Debug.LogError(e);
                            }
                            return gridItem.MoveToPositionTween(pos);
                        });
                    }

                    p.Resolve();
                });
            }
            else
            {
                Debug.LogError("SomeThing went Wrong.");
            }
            
            return tmpP;
        }

        private IPromise AddMovieToGrid(MovieDataModel model)
        {
            Promise p = new Promise();
            IPromise tmpP = null;

            try
            {
                _sortedList[model.Title] = model;

                int i = _sortedList.IndexOfKey(model.Title);
                
                GridItem gridItem = _gridItemPool.Spawn(model,
                    GetPositionInGrid(i), true);

                _gridItems[model] = gridItem;
                
                
                var myValueList = _sortedList.Values;
                for (++i; i < myValueList.Count; i++)
                {
                    int j = i;
                    tmpP = _gridItems[myValueList[j]].MoveToPositionTween(
                        GetPositionInGrid(j));
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            if (tmpP == null)
            {
                p.Resolve();
            }
            else
            {
                tmpP?.Done(p.Resolve);
            }
            
            return p;
        }
    }
}
