using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServerShared;
using ServerShared.PlayerProfile;

namespace BackendTest.PlayerProfile
{
    class SimpleUpdate : IUpdateProvider
    {
        public event Action OnUpdate = () => { };
        public void Update() => OnUpdate();
    }

    [TestClass]
    public class ProfileCommandRunnerTest
    {

        //can't compile CommandRunner 
        /*
        [TestMethod]
        public void TestExecSequence()
        {
            var updater = new SimpleUpdate();
            var runner = new CommandRunner(updater, 1);
            var execOrder = "";
            var com1 = new Mock<ICommand>();
            var com2 = new Mock<ICommand>();
            com1.Setup(_ => _.Init()).Returns(() =>
            {
                com1.Setup(_ => _.Initialized).Returns(true);
                com1.Setup(_ => _.Started).Returns(true);
                com1.Setup(_ => _.Completed).Returns(true);
                return true;
            });
            com2.Setup(_ => _.Init()).Returns(() =>
            {
                com2.Setup(_ => _.Initialized).Returns(true);
                com2.Setup(_ => _.Started).Returns(true);
                com2.Setup(_ => _.Completed).Returns(true);
                return true;
            });

            com1.Setup(_ => _.Run()).Callback(() =>
            {
                execOrder += "1";
            });
            com2.Setup(_ => _.Run()).Callback(() =>
            {
                execOrder += "2";
            });

            runner.Add(com1.Object);
            runner.Add(com2.Object);

            updater.Update();

            com1.Verify(_ => _.Init(), Times.Once);
            com2.Verify(_ => _.Init(), Times.Once);
            com1.Verify(_ => _.Run(), Times.Once);
            com2.Verify(_ => _.Run(), Times.Once);
            Assert.AreEqual("12", execOrder);
        }

        [TestMethod]
        public void TestUninitBlocksExec()
        {
            var updater = new SimpleUpdate();
            var runner = new CommandRunner(updater, 1);
            var com1 = new Mock<ICommand>();
            var com2 = new Mock<ICommand>();

            runner.Add(com1.Object);
            runner.Add(com2.Object);

            updater.Update();
            updater.Update();
            updater.Update();

            com1.Verify(_ => _.Init(), Times.Exactly(3));
            com2.Verify(_ => _.Init(), Times.Never);
            com1.Verify(_ => _.Run(), Times.Never);
            com2.Verify(_ => _.Run(), Times.Never);

            com1.Setup(_ => _.Init()).Returns(() =>
            {
                com1.Setup(_ => _.Initialized).Returns(true);
                com1.Setup(_ => _.Started).Returns(true);
                return true;
            });
            com2.Setup(_ => _.Init()).Returns(() =>
            {
                com2.Setup(_ => _.Initialized).Returns(true);
                com2.Setup(_ => _.Started).Returns(true);
                return true;
            });

            updater.Update();

            com1.Verify(_ => _.Init(), Times.Exactly(4));
            com2.Verify(_ => _.Init(), Times.Never);
            com1.Verify(_ => _.Run(), Times.Once);
            com2.Verify(_ => _.Run(), Times.Never);

            com1.Setup(_ => _.Completed).Returns(true);

            updater.Update();

            com1.Verify(_ => _.Init(), Times.Exactly(4));
            com2.Verify(_ => _.Init(), Times.Once);
            com1.Verify(_ => _.Run(), Times.Once);
            com2.Verify(_ => _.Run(), Times.Once);
        }

        [TestMethod]
        public void TestParallelExec()
        {
            var updater = new SimpleUpdate();
            var runner = new CommandRunner(updater, 2);
            var com1 = new Mock<ICommand>();
            var com2 = new Mock<ICommand>();

            runner.Add(com1.Object);
            runner.Add(com2.Object);

            // uninit state still must block queue
            updater.Update();
            updater.Update();
            updater.Update();

            com1.Verify(_ => _.Init(), Times.Exactly(3));
            com2.Verify(_ => _.Init(), Times.Never);
            com1.Verify(_ => _.Run(), Times.Never);
            com2.Verify(_ => _.Run(), Times.Never);

            com1.Setup(_ => _.Init()).Returns(() =>
            {
                com1.Setup(_ => _.Initialized).Returns(true);
                com1.Setup(_ => _.Started).Returns(true);
                return true;
            });
            com2.Setup(_ => _.Init()).Returns(() =>
            {
                com2.Setup(_ => _.Initialized).Returns(true);
                com2.Setup(_ => _.Started).Returns(true);
                return true;
            });

            updater.Update();

            com1.Verify(_ => _.Init(), Times.Exactly(4));
            com2.Verify(_ => _.Init(), Times.Once);
            com1.Verify(_ => _.Run(), Times.Once);
            com2.Verify(_ => _.Run(), Times.Once);
        }
        */
    }
}
