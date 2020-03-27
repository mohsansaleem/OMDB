using System.Collections.Generic;
using OMDB.Model;
using RSG;
using UnityEngine;

namespace OMDB.Generic
{
    public interface IDataHub
    {
        IPromise<List<MovieDataModel>> GetMovies(string name, int count = 10);
        //IPromise<List<MovieDataModel>> GetMoviesInternal(string name, int pageNumber = 1);
        IPromise<Sprite> GetSprite(string imageAddress);
    }
}
