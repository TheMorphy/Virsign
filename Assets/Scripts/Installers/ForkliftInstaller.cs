using UnityEngine;
using Zenject;

public class ForkliftInstaller : MonoInstaller
{
    [SerializeField] private ForkliftConfig forkliftConfig;

    public override void InstallBindings()
    {
        Container.Bind<ForkliftConfig>()
            .FromInstance(forkliftConfig)
            .AsSingle();

        Container.BindInterfacesAndSelfTo<ForkliftInput>()
            .AsSingle()
            .NonLazy();
    }
}