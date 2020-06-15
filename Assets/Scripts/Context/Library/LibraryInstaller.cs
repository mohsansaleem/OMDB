using OMDB.Model;
using UnityEngine;
using Zenject;

namespace OMDB.View
{
    public class LibraryInstaller : GridParentInstaller
    {
        [SerializeField]
        private LibraryView _view;
        
        public override void InstallBindings()
        {
            base.InstallBindings();
            
            Container.BindInstance(_view).AsSingle();
            Container.BindInterfacesTo<LibraryMediator>().AsSingle();
        }
    }
}