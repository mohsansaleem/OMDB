using System;
using System.Collections.Generic;
using OMDB.Model;
using RSG;
using UnityEngine;
using Zenject;

namespace OMDB.Generic
{
    public class DataHub : IDataHub
    {
        [Inject] private IOMDBService _service;
        [Inject] private MovieDataModel.Factory _movieDataFactory;

        // Cache
        // TODO: Add Size Limit to Cache and also make it consistent by storing.
        // For now just in-memory and without any cap.
        Dictionary<string, Sprite> _spritesCache = new Dictionary<string, Sprite>();
        Dictionary<string, MovieDataModel> _dataModelsCache = new Dictionary<string, MovieDataModel>();
        Dictionary<string, List<MovieDataModel>> _moviesSearchResult = new Dictionary<string, List<MovieDataModel>>();

        public IPromise<List<MovieDataModel>> GetMovies(string name, int count = 10)
        {
            Promise<List<MovieDataModel>> promise = new Promise<List<MovieDataModel>>();
            
            if (!_moviesSearchResult.ContainsKey(name))
            {
                List<MovieDataModel> models = _moviesSearchResult[name] = new List<MovieDataModel>();

                IPromise<List<MovieDataModel>> p = null;

                List<int> page = new List<int>();
                for (int i = models.Count, pageNumber = models.Count/10; i <= count; i += 10)
                {
                    if (p == null)
                        p = GetMoviesInternal(name, ++pageNumber);
                    else
                    {
                        p = p.Then(lst =>
                        {
                            lst.ForEach(models.Add);
                            return GetMoviesInternal(name, ++pageNumber);
                        });
//                        p = GetMoviesInternal(name, ++pageNumber).Then(lst =>
//                        {
//                            lst.ForEach(models.Add);
//                        });
                    }
                }

                if (p == null)
                {
                    promise.Resolve(models);
                }
                else
                {
                    p.Done(lst =>
                    {
                        lst.ForEach(models.Add);
                        promise.Resolve(models);
                    });
                }
            }
            else
            {
                Debug.Log("Result found in Cache.");
                promise.Resolve(_moviesSearchResult[name]);
            }
            
            return promise;
        }

        public IPromise<List<MovieDataModel>> GetMoviesInternal(string name, int pageNumber = 1)
        {
            Promise<List<MovieDataModel>> promise = new Promise<List<MovieDataModel>>();
            List<MovieDataModel> movies = new List<MovieDataModel>();
            
            _service.SearchMovie(name, pageNumber)
                .Done(response =>
                {
                    if (response != null && response.Search != null && response.Search.Count > 0)
                    {
                        try
                        {
                            foreach (MovieData movieData in response.Search)
                            {
                                MovieDataModel model = null;
                                
                                if (_dataModelsCache.ContainsKey(movieData.imdbID))
                                {
                                    Debug.Log("Movie Data Found in Cache.");
                                    model = _dataModelsCache[movieData.imdbID];
                                }
                                else
                                {
                                    model = _movieDataFactory.Create(movieData);
                                    
                                    _dataModelsCache[movieData.imdbID] = model;
                                    
                                    GetSprite(movieData.Poster).Done(sprite =>
                                        {
                                            model.Poster.Value = sprite;
                                        },
                                        exception => Debug.LogError(exception));
                                }

                                movies.Add(model);
                            }
                        }
                        catch (Exception e)
                        {
                            promise.Reject(e);
                            return;
                        }
                    }
                    
                    promise.Resolve(movies);
                },
                promise.Reject);

            return promise;
        }

        public IPromise<Sprite> GetSprite(string imageAddress)
        {
            Promise<Sprite> promise = new Promise<Sprite>();

            if (_spritesCache.ContainsKey(imageAddress))
            {
                promise.Resolve(_spritesCache[imageAddress]);
            }
            else
            {
                _service.GetSprite(imageAddress).Done( sprite =>
                {
                    _spritesCache[imageAddress] = sprite;
                    promise.Resolve(sprite);
                }, promise.Reject);   
            }

            return promise;
        }
    }
}
