using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServerShared.GlobalConflict;
using ServerShared.Sources.GlobalConflict;

namespace GlobalConflictModuleTest
{
    [TestClass]
    public class ExtensionsTest
    {
        [TestMethod]
        public void TestIsDeployable()
        {
            Assert.IsTrue(new PointOfInterest()
            {
                NextDeploy = DateTime.UtcNow.AddMinutes(-1)
            }.IsDeployable(), 
                "Deploy time passed");

            Assert.IsFalse(new PointOfInterest()
            {
                NextDeploy = DateTime.UtcNow.AddMinutes(1)
            }.IsDeployable(), 
                "Deploy time not reached");

            Assert.IsTrue(new PointOfInterest()
            {
                NextDeploy = DateTime.UtcNow.AddMinutes(1)
            }.IsDeployable(dt: DateTime.UtcNow.AddMinutes(2)), 
                "Deploy time passed with custom DateTime");

            Assert.IsFalse(new PointOfInterest()
            {
                NextDeploy = DateTime.UtcNow.AddMinutes(-1),
                GeneralLevel = 1
            }.IsDeployable(), "General POI can be deployed by player with same general level only");

            Assert.IsFalse(new PointOfInterest()
            {
                NextDeploy = DateTime.UtcNow.AddMinutes(-1),
                GeneralLevel = 1
            }.IsDeployable(new PlayerState()), "General POI can be deployed by player with same general level only");

            Assert.IsTrue(new PointOfInterest()
            {
                NextDeploy = DateTime.UtcNow.AddMinutes(-1),
                GeneralLevel = 1
            }.IsDeployable(new PlayerState() {GeneralLevel = 1}), "Deploy general's poi");

            Assert.IsFalse(new PointOfInterest()
            {
                NextDeploy = DateTime.UtcNow.AddMinutes(-1),
                OwnerTeam = "team1"
            }.IsDeployable(), "Team's POI can be deployed only by team player");

            Assert.IsFalse(new PointOfInterest()
            {
                NextDeploy = DateTime.UtcNow.AddMinutes(-1),
                OwnerTeam = "team1"
            }.IsDeployable(new PlayerState() {TeamId = "team2"}), "Team's POI can be deployed only by team player");

            Assert.IsTrue(new PointOfInterest()
            {
                NextDeploy = DateTime.UtcNow.AddMinutes(-1),
                OwnerTeam = "team1"
            }.IsDeployable(new PlayerState() { TeamId = "team1" }), "Team's POI can be deployed only by team player");

            Assert.IsFalse(new PointOfInterest()
                {
                    Auto = true,
                    NextDeploy = DateTime.UtcNow.AddMinutes(-1),
            }.IsDeployable(), "Deplying auto POIs not allowed in manual mode");
        }
    }
}
