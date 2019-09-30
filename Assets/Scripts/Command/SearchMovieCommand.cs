using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using OMDB.Generic;
using OMDB.Model;
using UnityEngine;
using Zenject;

namespace OMDB.Command
{
    public class SearchMovieCommand
    {
        [Inject] private DataHub _dataHub;
        
        [Inject] private RemoteDataModel _remoteDataModel;
        
        public void Execute(SearchMovieSignal signal)
        {
            _dataHub.GetMovies(signal.MovieName, signal.Count)
                .Done(response =>
            {
                try
                {
                    if (response.Count == 0)
                    {
                        _remoteDataModel.QueryResult.Clear();
                    }
                    else
                    {
                        List<MovieDataModel> moviesToRemove = new List<MovieDataModel>();
                        foreach (var movieDataModel in _remoteDataModel.QueryResult)
                        {
                            if(!response.Contains(movieDataModel))
                                moviesToRemove.Add(movieDataModel);
                        }

                        foreach (MovieDataModel movieDataModel in moviesToRemove)
                        {
                            _remoteDataModel.QueryResult.Remove(movieDataModel);
                        }
                        
                        response.ForEach(model =>
                        {
                            _remoteDataModel.QueryResult.Add(model);
                        });
                    }

                    Debug.Log("Obj: "+JsonConvert.SerializeObject(response));
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            },exception =>
                {
                    Debug.LogError(exception);
                });
        }
    }
}