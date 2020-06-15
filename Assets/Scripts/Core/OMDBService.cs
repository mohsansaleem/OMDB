using System.Collections.Generic;
using Newtonsoft.Json;
using OMDB.Model;
using OMDB.Utils;
using RSG;
using UnityEngine;

namespace OMDB.Generic
{
    public class OMDBService:IOMDBService
    {
        private const string _url = "http://www.omdbapi.com/?apikey=6367da97";

        public IPromise<SearchMovieRespose> SearchMovie(string name, int page = 1)
        {
            Promise<SearchMovieRespose> promise = new Promise<SearchMovieRespose>();

            WebUtil.WebRequest(_url + $"&s={name}&page={page}")
                .Done(response =>
                {
                    Debug.Log(response);
                    SearchMovieRespose searchMovieRespose = JsonConvert.DeserializeObject<SearchMovieRespose>(response);
                    if (!searchMovieRespose.Response)
                    {
                        searchMovieRespose.Search = new List<MovieData>();
                    }
                    promise.Resolve(searchMovieRespose);
                },promise.Reject);

            return promise;
        }

        public IPromise<Sprite> GetSprite(string imageAddress)
        {
            return WebUtil.ImageRequest(imageAddress);
        }
    }
}