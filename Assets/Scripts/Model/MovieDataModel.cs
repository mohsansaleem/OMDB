using UniRx;
using UnityEngine;
using Zenject;

namespace OMDB.Model
{
    public class MovieDataModel
    {
        public MovieData MovieData;

        public string Title => MovieData.Title;
        public string ImdbId => MovieData.imdbID;
        public ReactiveProperty<Sprite> Poster;

        public MovieDataModel(MovieData movieData)
        {
            Poster = new ReactiveProperty<Sprite>();
            MovieData = movieData;
        }
        
        public class Factory : PlaceholderFactory<MovieData, MovieDataModel>
        {
        }
    }
}