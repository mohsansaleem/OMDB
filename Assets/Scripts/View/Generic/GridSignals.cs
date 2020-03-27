using System.Collections.Generic;
using OMDB.Model;

namespace OMDB.View
{
    public interface IGridSignal
    {
    }
    
    public class AddMovieToGridSignal:IGridSignal
    {
        public MovieDataModel Model;

        public AddMovieToGridSignal(MovieDataModel model)
        {
            Model = model;
        }
    }

    public class RemoveMovieFromGridSignal:IGridSignal
    {
        public MovieDataModel Model;

        public RemoveMovieFromGridSignal(MovieDataModel model)
        {
            Model = model;
        }
    }

    public class MovieSelectSignal
    {
        public MovieDataModel MovieDataModel;

        public MovieSelectSignal(MovieDataModel model)
        {
            MovieDataModel = model;
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
    
    public class ClearGridSignal:IGridSignal
    {
        
    }
}
