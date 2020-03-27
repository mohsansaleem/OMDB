using OMDB.Model;
using RSG;
using UnityEngine;

namespace OMDB.Generic
{
    public interface IOMDBService
    {
        IPromise<SearchMovieRespose> SearchMovie(string name, int page = 1);
        IPromise<Sprite> GetSprite(string imageAddress);
    }
}