using UniRx;

namespace OMDB.Model
{
    public class RemoteDataModel
    {
        public string CurrentQuery;
        public ReactiveCollection<MovieDataModel> QueryResult;
        public ReactiveCollection<MovieDataModel> MoviesInDetailStack;
        
        public ReactiveProperty<MovieDataModel> SelectedMovie;
        
        public RemoteDataModel()
        {
            QueryResult = new ReactiveCollection<MovieDataModel>();
            MoviesInDetailStack = new ReactiveCollection<MovieDataModel>();
            
            SelectedMovie = new ReactiveProperty<MovieDataModel>();
        }
    }
}
