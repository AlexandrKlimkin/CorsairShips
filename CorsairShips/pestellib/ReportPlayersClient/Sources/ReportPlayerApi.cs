using System;
using PestelLib.ServerClientUtils;
using PestelLib.UniversalSerializer;
using ReportPlayersProtocol;
using S;
using UnityEngine;

namespace PestelLib.ReportPlayersClient
{
    public class ReportPlayerApi
    {
        private const int MaxDescriptionLength = 160;
        private const int MaxTypeLength = 60;
        private readonly RequestQueue _requestQueue;

        public ReportPlayerApi(RequestQueue requestQueue)
        {
            _requestQueue = requestQueue;
        }

        /*
         * Отправка сообщения о подозрении в читерстве игрока
         *
         * suspectPlayerId - тот игрок, которого мы хотим зарепортить.
         * type - одно из фиксированных текстовых значений, которые стоит давать выбрать игроку в диалоговом окне репорта
         * Например это могут быть wallhack / aimbot / speedhack / other
         * description - произвольная информация, которую пользователь может указать в репорте
         *
         * Вы можете так же репортить из кода игры игроков, в тех случаях, когда они сильно похожи на читеров.
         * Например, если игрок за бой ни разу не умер и набил больше 20 фрагов - это может быть поводом зарепортить его.
         * Для таких репортов нужно ставить флаг reportedBySystem в true
         * Эти репорты лучше слать "самому на себя" т.к. в противном случае, их будет приходить каждый раз несколько от разных
         * игроков (например от каждого оппонента в конце боя)
         *
         * Т.к. бан по репортам происходит в ручном режиме, лучше не спамить слишком автоматическими репортами,
         * и слать их в том случае, если подозрения в читерстве очень серьезные
         *
         * Примеры использования:
         *
         * var api = new ReportPlayerApi(_requestQueue);
         *
         * //отправка репорта от игрока
         * api.SendReport(otherPlayerGUID, dialog.Type.text, dialog.Description.text, "", false)
         *
         * //отправка репорта от автоматического отслеживания результатов боя
         * api.SendReport(_requestQueue.PlayerId, "AutoTooManyFrags", $"Frags {frags} with {deaths} deaths", "", true);
         */
        public void SendReport(Guid suspectPlayerId, string type, string description, string gamePayload, bool reportedBySystem)
        {
            var report = new PlayerReportData
            {
                Sender = reportedBySystem ? Guid.NewGuid() :_requestQueue.PlayerId,
                Type = type,
                Description = description,
                ReportedBySystem = reportedBySystem,
                Reported = suspectPlayerId, 
                GamePayload = gamePayload
            };

            if (report.Description.Length > MaxDescriptionLength)
            {
                report.Description = report.Description.Substring(0, MaxDescriptionLength);
            }

            if (report.Type.Length > MaxTypeLength)
            {
                report.Type = report.Type.Substring(0, MaxTypeLength);
            }

            _requestQueue.SendRequest("ReportPlayerModule", new Request
            {
                ExtensionModuleRequest = new ExtensionModuleRequest
                {
                    ModuleType = "ReportPlayerModule",
                    Request = Serializer.Serialize<BaseReportRequest>(report)
                }
            }, (resp, dataCollection) =>
            {
                Log("Report was sent");
            });
        }

        /*
         * Ключевой параметр для определения читер игрок или нет - соотношение его игровых сессий к количеству репортов на него
         * Для учета кол-ва сессий нужно вызвать этот метод, либо на каждом запуске игры, либо на каждом запуске боя
         * (второе предпочтительнее - это даст более точные результаты)
         */
        public void IncrementSessionCounter(string playerNickName)
        {
            _requestQueue.SendRequest("ReportPlayerModule", new Request
            {
                ExtensionModuleRequest = new ExtensionModuleRequest
                {
                    ModuleType = "ReportPlayerModule",
                    Request = Serializer.Serialize<BaseReportRequest>(new RegisterNewSession
                    {
                        PlayerId = _requestQueue.PlayerId,
                        PlayerName = playerNickName
                    })
                }
            }, (resp, dataCollection) =>
            {
                Log("Session counter incremented");
            });
        }

        private void Log(string message)
        {
            if (Application.isEditor)
            {
                Debug.Log(nameof(ReportPlayerApi) + ": " + message);
            }
        }
    }
}
