using System.Collections.Generic;
using OMDB.Model;

namespace OMDB.View
{
    public interface IGridSignal
    {
    }

    public class MovieSignal
    {
        public MovieDataModel Model;

        protected MovieSignal(MovieDataModel model)
        {
            Model = model;
        }
    }
    
    public class AddMovieToGridSignal : MovieSignal, IGridSignal
    {
        public bool Animate;
        public AddMovieToGridSignal(MovieDataModel model, bool animate = true):base(model)
        {
            Animate = animate;
        }
    }

    public class RemoveMovieFromGridSignal : MovieSignal, IGridSignal
    {
        public RemoveMovieFromGridSignal(MovieDataModel model):base(model)
        {
        }
    }

    public class MovieSelectSignal : MovieSignal
    {
        public MovieSelectSignal(MovieDataModel model): base(model)
        {
        }
    }
    
    public class LoadMoviesSignal
    {
        public List<MovieDataModel> Movies;

        public LoadMoviesSignal(List<MovieDataModel> movies)
        {
            Movies = movies;
        }
    }

    public class GridInitSignal
    {
        
    }

    public class ClearGridSignal : IGridSignal
    {
        
    }
}
