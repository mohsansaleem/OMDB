using OMDB.Command;
using OMDB.Model;
using Zenject;

namespace OMDB.Generic
{
    public class ProjectContextInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            // Installing Signal Bus Once and for all.
            SignalBusInstaller.Install(Container);

            Container.BindFactory<MovieData, MovieDataModel, MovieDataModel.Factory>();
            
            Container.Bind<RemoteDataModel>().AsSingle();
            
            Container.BindInterfacesAndSelfTo<OMDBService>().AsSingle();
            Container.BindInterfacesAndSelfTo<DataHub>().AsSingle();
            
            Container.DeclareSignal<SearchMovieSignal>().RunAsync();
            Container.BindSignal<SearchMovieSignal>()
                .ToMethod<SearchMovieCommand>(arg => arg.Execute)
                .FromNew();
        }
    }
    
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
}