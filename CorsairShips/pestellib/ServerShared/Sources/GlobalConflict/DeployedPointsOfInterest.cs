using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerShared.GlobalConflict
{
    public class DeployedPointsOfInterest
    {
        private object _sync = new object();
        private List<PointOfInterest> _pois = new List<PointOfInterest>();

        public void AddDeployed(PointOfInterest poi, bool skipExpired)
        {
            if (poi == null)
                return;

            if (skipExpired && poi.NextDeploy < DateTime.UtcNow)
                return;
            lock (_sync)
            {
                _pois.Add(poi);
            }
        }

        /// <summary>
        /// Возвращает точку конкретную интереса для команды если она установлена
        /// </summary>
        /// <param name="poiId"></param>
        /// <param name="team"></param>
        /// <returns></returns>
        public PointOfInterest GetTeamPoi(string poiId, string team)
        {
            lock (_sync)
            {
                return _pois.FirstOrDefault(_ => _.Id == poiId && _.OwnerTeam == team);
            }
        }

        /// <summary>
        /// Проверяет установлена ли точка инетреса с указанным id для указанной команды
        /// </summary>
        /// <param name="poiId"></param>
        /// <param name="team"></param>
        /// <returns></returns>
        public bool HasTeamPoi(string poiId, string team)
        {
            return GetTeamPoi(poiId, team) != null;
        }

        /// <summary>
        /// Возвращает все точки интереса установенные для конкретной команды.
        /// По этим точкам можно узнать в каких нодах они установлены например
        /// </summary>
        /// <param name="team"></param>
        /// <returns></returns>
        public IEnumerable<PointOfInterest> GetTeamPois(string team)
        {
            lock (_sync)
            {
                return _pois.Where(_ => _.OwnerTeam == team);
            }
        }

        public int Count
        {
            get
            {
                return _pois.Count;
            }
        }
    }
}
