using System.Collections.Generic;

namespace OMDB.Model
{
    public class SearchMovieRespose
    {
        public bool Response;
        public List<MovieData> Search = new List<MovieData>();
        public int totalResults;
    }
}
