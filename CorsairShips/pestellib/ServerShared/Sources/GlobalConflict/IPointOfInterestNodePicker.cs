namespace ServerShared.GlobalConflict
{
    public interface IPointOfInterestNodePicker
    {
        /// <summary>
        /// Метод должен выбрать ноду в которую будет установлена точка интереса
        /// </summary>
        /// <param name="conflictState"></param>
        /// <param name="team"></param>
        /// <param name="point"></param>
        /// <param name="occupiedPoints"></param>
        /// <returns></returns>
        NodeState PickNode(GlobalConflictState conflictState, string team, PointOfInterest point, DeployedPointsOfInterest occupiedPoints);
    }
}