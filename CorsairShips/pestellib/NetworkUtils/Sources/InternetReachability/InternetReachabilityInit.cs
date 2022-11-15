using PestelLib.NetworkUtils;
using PestelLib.NetworkUtils.Sources.InternetReachability;
using UnityDI;
using UnityEngine;

public class InternetReachabilityInit : MonoBehaviour
{
    [SerializeField]
    private bool UseStub;
    void Start()
    {
        if (UseStub)
        {
            ContainerHolder.Container.RegisterSingleton<InternetReachabilityStub>();
            ContainerHolder.Container.RegisterCustom<IInternetReachability>(() => ContainerHolder.Container.Resolve<InternetReachabilityStub>());
        }
        else
        {
            ContainerHolder.Container.RegisterUnitySingleton<InternetReachabilityBackendTcpPing>(null, true);
            ContainerHolder.Container.RegisterCustom<IInternetReachability>(() =>
                ContainerHolder.Container.Resolve<InternetReachabilityBackendTcpPing>());
        }
        //ContainerHolder.Container.RegisterUnitySingleton<InternetReachabilityDI>();
    }
}