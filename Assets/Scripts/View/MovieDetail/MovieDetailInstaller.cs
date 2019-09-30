using UnityEngine;
using Zenject;

namespace OMDB.View
{
    public class MovieDetailInstaller : MonoInstaller
    {
        [SerializeField]
        private MovieDetailView _view;
        
        public override void InstallBindings()
        {
            Container.DeclareSignal<AddMovieToGridSignal>().RunSync();
            Container.DeclareSignal<RemoveMovieFromGridSignal>().RunSync();
            Container.DeclareSignal<ClearGridSignal>().RunSync();
            Container.DeclareSignal<MovieSelectSignal>().RunSync();
            Container.DeclareSignal<LoadMoviesSignal>().RunSync();
            Container.DeclareSignal<GridInitSignal>().RunAsync();
            
            Container.BindInstance(_view).AsSingle();
            Container.BindInterfacesTo<MovieDetailMediator>().AsSingle();
        }
    }
}