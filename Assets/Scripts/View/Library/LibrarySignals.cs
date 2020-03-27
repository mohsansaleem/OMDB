public class SearchMovieSignal
{
    public string MovieName;
    public int Count;

    public SearchMovieSignal(string movieName, int count = 10)
    {
        MovieName = movieName;
        Count = count;
    }
}
