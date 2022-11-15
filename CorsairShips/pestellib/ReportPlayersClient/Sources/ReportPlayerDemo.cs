using System;
using PestelLib.ClientConfig;
using PestelLib.ServerClientUtils;
using PestelLib.Utils;
using UnityDI;
using UnityEngine;

namespace PestelLib.ReportPlayersClient
{
    public class ReportPlayerDemo : MonoBehaviour
    {
        public string GamePayload = "Lvl:23,Gold:21312";
        public string ReportType = "TestReportType";
        public string ReportMessage = "TestDescription";
        public string PlayerNickName = "TestPlayerName";
        public Guid ReportedGuid;
    
        [Dependency] private ReportPlayerApi _reportPlayerApi;
        [Dependency] private RequestQueue _requestQueue;

        void Start()
        {
            InitDependencies();

            ContainerHolder.Container.BuildUp(this);

            SetupDependencyInjection();
        }

        private static void InitDependencies()
        {
            var container = ContainerHolder.Container;

            if (!container.IsRegistred<ReportPlayerApi>())
            {
                //регистрация RequestQueue и необходимых ему зависимостей - это скорее всего уже и так у вас есть
                container.RegisterUnitySingleton<UpdateProvider>(null, true);
                container.RegisterUnitySingleton<SharedTime>(null, true);
                container.RegisterUnityScriptableObject<Config>(null);
                container.RegisterCustomSingleton(() => new RequestQueue());

                //регистрация Api для репортов, это можно добавить в TaskInitDependencyInjection, а можно
                //создавать ReportPlayerApi через его конструктор в том месте, где он нужен, если это удобнее
                container.RegisterCustomSingleton(() => new ReportPlayerApi(container.Resolve<RequestQueue>()));
            }
        }

        [ContextMenu("Set new random reporter GUID")]
        void SetupDependencyInjection()
        {
            ReportedGuid = Guid.NewGuid();
        }

        [ContextMenu("Report player")]
        void ReportPlayer()
        {
            _reportPlayerApi.SendReport(ReportedGuid, ReportType, ReportMessage, GamePayload, false);
        }

        [ContextMenu("Increment sessions counter")]
        void IncrementSessionsConter()
        {
            _reportPlayerApi.IncrementSessionCounter(PlayerNickName);
        }

        [ContextMenu("Report yourself by system")]
        void ReportYourself()
        {
            _reportPlayerApi.SendReport(_requestQueue.PlayerId, ReportType, ReportMessage, GamePayload, true);
        }
    }
}
