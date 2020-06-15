using System;
using System.Collections.Generic;
using OMDB.Generic;
using OMDB.Model;
using UniRx;
using UnityEngine.SceneManagement;
using Zenject;

namespace OMDB.View
{
    public class LibraryMediator : Mediator
    {
        [Inject] private LibraryView _view;
        [Inject] private RemoteDataModel _remoteDataModel;
        [Inject] private ZenjectSceneLoader _zenjectSceneLoader;

        // Local caching
        private Dictionary<MovieDataModel, IDisposable> _reactiveDisposible = new Dictionary<MovieDataModel, IDisposable>();
        
        public override void Initialize()
        {
            _remoteDataModel.QueryResult.ObserveAdd().Subscribe(OnMovieAdd).AddTo(Disposables);
            _remoteDataModel.QueryResult.ObserveRemove().Subscribe(OnMovieRemove).AddTo(Disposables);

            _remoteDataModel.QueryResult.ObserveReset().Subscribe(OnGridReset).AddTo(Disposables);


            _view.InputField.onValueChanged.AddListener(str =>
            {
                if (str != null)
                {
                    Search(str);
                }
            });
            
            SignalBus.Subscribe<MovieSelectSignal>(signal =>
            {
                _remoteDataModel.SelectedMovie.Value = signal.Model;
                
                // Lame. Refactor through command and Async Load.
                _zenjectSceneLoader.LoadScene(1, LoadSceneMode.Additive);
            });
            
            //Observable.Timer(TimeSpan.FromSeconds(2)).Subscribe(t => Search("sun")).AddTo(Disposables);
        }

        private void OnMovieAdd(CollectionAddEvent<MovieDataModel> addEvent)
        {
            if (!_reactiveDisposible.ContainsKey(addEvent.Value))
            {
                _reactiveDisposible.Add(addEvent.Value,
                    addEvent.Value.Poster.Subscribe(sprite =>
                    {
                        if (sprite != null)
                        {
                            SignalBus.Fire(new AddMovieToGridSignal(addEvent.Value));

                            // Test
//                            if (addEvent.Index == 1)
//                            {
//                                Observable.Timer(TimeSpan.FromSeconds(3)).Subscribe((l =>
//                                {
//                                    _remoteDataModel.QueryResult.Remove(addEvent.Value);
//                                })).AddTo(Disposables);
//
//                            }
                        }
                    }));
            }
        }
        
        private void OnMovieRemove(CollectionRemoveEvent<MovieDataModel> removeEvent)
        {
            if (_reactiveDisposible.ContainsKey(removeEvent.Value))
            {
                _reactiveDisposible[removeEvent.Value].Dispose();
                _reactiveDisposible.Remove(removeEvent.Value);
            }
            
            SignalBus.Fire(new RemoveMovieFromGridSignal(removeEvent.Value));
        }

        public void OnGridReset(Unit observer)
        {
            foreach (var reactiveDisposibleValue in _reactiveDisposible.Values)
            {
                reactiveDisposibleValue.Dispose();
            }
            _reactiveDisposible.Clear();

            SignalBus.Fire<ClearGridSignal>();
        }
        
        void Search(string name)
        {
            SignalBus.Fire<SearchMovieSignal>(new SearchMovieSignal(name));
        }
    }
}