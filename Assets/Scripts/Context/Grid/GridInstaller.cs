using UnityEngine;
using Zenject;

namespace OMDB.View
{
    public class GridInstaller : MonoInstaller
    {
        [SerializeField]
        private GridView _view;
        [SerializeField]
        private GridItem _gridItemPrefab;
        
        public override void InstallBindings()
        {
            // TODO: Bind pool 

            Container.BindMemoryPool<GridItem, GridItem.Pool>()
                .WithInitialSize(2)
                .FromComponentInNewPrefab(_gridItemPrefab)
                .UnderTransform(_view.Container);
            
            Container.BindInstance(_view).AsSingle();
            Container.BindInterfacesTo<GridFacade>().AsSingle();
        }
    }
}