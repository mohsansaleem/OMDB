using OMDB.Model;
using UnityEngine;
using Zenject;

namespace OMDB.View
{
    public class LibraryInstaller : MonoInstaller
    {
        [SerializeField]
        private LibraryView _view;
        
        public override void InstallBindings()
        {
            Container.DeclareSignal<AddMovieToGridSignal>().RunAsync();
            Container.DeclareSignal<RemoveMovieFromGridSignal>().RunAsync();
            Container.DeclareSignal<ClearGridSignal>().RunSync();
            Container.DeclareSignal<MovieSelectSignal>().RunSync();
            Container.DeclareSignal<LoadMoviesSignal>().RunSync();
            Container.DeclareSignal<GridInitSignal>().RunAsync();
            
            Container.BindInstance(_view).AsSingle();
            Container.BindInterfacesTo<LibraryMediator>().AsSingle();
        }
    }
}