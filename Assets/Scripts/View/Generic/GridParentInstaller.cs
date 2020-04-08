using OMDB.Model;
using UnityEngine;
using Zenject;

namespace OMDB.View
{
    public class GridParentInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.DeclareSignal<AddMovieToGridSignal>().RunAsync();
            Container.DeclareSignal<RemoveMovieFromGridSignal>().RunAsync();
            Container.DeclareSignal<ClearGridSignal>().RunSync();
            Container.DeclareSignal<MovieSelectSignal>().RunSync();
            Container.DeclareSignal<LoadMoviesSignal>().RunSync();
            Container.DeclareSignal<GridInitSignal>().RunAsync();
        }
    }
}