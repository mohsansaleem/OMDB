﻿using System;
using System.Collections.Generic;
using OMDB.Generic;
using OMDB.Model;
using UniRx;
using UnityEngine.SceneManagement;
using Zenject;

namespace OMDB.View
{
    public class MovieDetailMediator : Mediator
    {
        [Inject] private MovieDetailView _view;
        [Inject] private RemoteDataModel _remoteDataModel;

        // Local caching
        private Dictionary<MovieDataModel, IDisposable> _reactiveDisposible = new Dictionary<MovieDataModel, IDisposable>();
        
        public override void Initialize()
        {
            _remoteDataModel.SelectedMovie.Subscribe(model =>
            {
                _view.Image.sprite = model.Poster.Value;
                _view.Text.text = model.Title;
            }).AddTo(Disposables);
            
            _remoteDataModel.QueryResult.ObserveAdd().Subscribe(e => OnMovieRemove(e.Value)).AddTo(Disposables);
            _remoteDataModel.QueryResult.ObserveRemove().Subscribe(e => OnMovieRemove(e.Value)).AddTo(Disposables);

            _remoteDataModel.QueryResult.ObserveReset().Subscribe(OnGridReset).AddTo(Disposables);
            
            SignalBus.Subscribe<GridInitSignal>(AddData);
            
            // For Back
            _remoteDataModel.MoviesInDetailStack.ObserveAdd().Subscribe(OnMovieAddToStack).AddTo(Disposables);
            _remoteDataModel.MoviesInDetailStack.ObserveRemove().Subscribe(OnMovieRemoveFromStack).AddTo(Disposables);
            
            _view.BackButton.onClick.AddListener((() =>
            {
                _remoteDataModel.MoviesInDetailStack.RemoveAt(_remoteDataModel.MoviesInDetailStack.Count-1);

                if (_remoteDataModel.MoviesInDetailStack.Count > 0)
                {
                    _remoteDataModel.SelectedMovie.Value = _remoteDataModel.MoviesInDetailStack[_remoteDataModel.MoviesInDetailStack.Count-1];
                }
                else
                {
                    Exit();
                }
            }));
            
            _view.ExitButton.onClick.AddListener(Exit);
            
            SignalBus.Subscribe<MovieSelectSignal>(signal =>
            {
                _remoteDataModel.SelectedMovie.Value = signal.MovieDataModel;
                _remoteDataModel.MoviesInDetailStack.Add(signal.MovieDataModel);
            });
        }

        private void AddData()
        {
            List<MovieDataModel> movies = new List<MovieDataModel>();
            foreach (var model in _remoteDataModel.QueryResult)
            {
                if (!_remoteDataModel.MoviesInDetailStack.Contains(model))
                {
                    OnMovieAdd(model);
                    //movies.Add(model);
                }
            }
            
            //SignalBus.Fire(new LoadMoviesSignal(movies));
        }

        private void Exit()
        {
            _remoteDataModel.MoviesInDetailStack.Clear();
                
            // Lame. Use Command.
            SceneManager.UnloadSceneAsync(1);
        }

        private void OnMovieAdd(MovieDataModel model)
        {
            if (!_reactiveDisposible.ContainsKey(model))
            {
                _reactiveDisposible.Add(model,
                    model.Poster.Subscribe(sprite =>
                    {
                        if (sprite != null)
                        {
                            SignalBus.Fire(new AddMovieToGridSignal(model));

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
        
        private void OnMovieRemove(MovieDataModel model)
        {
            if (_reactiveDisposible.ContainsKey(model))
            {
                _reactiveDisposible[model].Dispose();
                _reactiveDisposible.Remove(model);
            }
            
            SignalBus.Fire(new RemoveMovieFromGridSignal(model));
        }

        private void OnMovieAddToStack(CollectionAddEvent<MovieDataModel> addEvent)
        {
            OnMovieRemove(addEvent.Value);
        }

        private void OnMovieRemoveFromStack(CollectionRemoveEvent<MovieDataModel> removeEvent)
        {
            OnMovieAdd(removeEvent.Value);
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