using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

[CreateAssetMenu(menuName = "Project/Project Context Settings")]
public class ProjectContextSettingsInstaller : ScriptableObjectInstaller<ProjectContextSettingsInstaller>
{
    public Settings ProjectSettings;

    public override void InstallBindings()
    {
        // Use IfNotBound to allow overriding for eg. from play mode tests
        Container.BindInstance(ProjectSettings).IfNotBound();
    }
    
    [Serializable]
    public class Settings
    {
        [Range(0.03f, 1)]
        public float GridItemMoveSpeed = 0.07f;
        [Range(0.03f, 1)]
        public float GridItemSpawnSpeed = 0.5f;
        [Range(0.03f, 1)]
        public float GridItemDespawnSpeed = 0.8f;
    }
}
