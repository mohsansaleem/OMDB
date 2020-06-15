using UnityEngine;
using Zenject;

namespace OMDB.View
{
    public class MovieDetailInstaller : GridParentInstaller
    {
        [SerializeField]
        private MovieDetailView _view;
        
        public override void InstallBindings()
        {
            base.InstallBindings();
            
            Container.BindInstance(_view).AsSingle();
            Container.BindInterfacesTo<MovieDetailMediator>().AsSingle();
        }
    }
}