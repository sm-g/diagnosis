using Diagnosis.Common;
using Diagnosis.Models;
using Diagnosis.ViewModels.Screens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

using System.Linq;

namespace Diagnosis.ViewModels.Tests
{
    [TestClass]
    public class OverlayTest : ViewModelTest
    {
        private OverlayServiceViewModel service;

        [TestInitialize]
        public void Init()
        {
            service = new OverlayServiceViewModel();
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (service != null)
                service.Dispose();
        }

        [TestMethod]
        public void AddSame()
        {
            var undoDoActions = new Action[] {
                        () => {},
                        () => {}
                    };
            this.Send(Event.ShowUndoOverlay, new object[] { undoDoActions, typeof(HealthRecord) }.AsParams(MessageKeys.UndoDoActions, MessageKeys.Type));
            this.Send(Event.ShowUndoOverlay, new object[] { undoDoActions, typeof(HealthRecord) }.AsParams(MessageKeys.UndoDoActions, MessageKeys.Type));

            Assert.AreEqual(1, service.Overlays.Count);
            Assert.AreEqual(2, service.Overlays[0].Count);
        }

        [TestMethod]
        public void AddOtherOverlay()
        {
            var undoDoActions = new Action[] {
                        () => {},
                        () => {}
                    };
            this.Send(Event.ShowUndoOverlay, new object[] { undoDoActions, typeof(HealthRecord) }.AsParams(MessageKeys.UndoDoActions, MessageKeys.Type));
            this.Send(Event.ShowMessageOverlay, new object[] { "str", typeof(HealthRecord) }.AsParams(MessageKeys.String, MessageKeys.Type));

            Assert.AreEqual(2, service.Overlays.Count);
        }

        [TestMethod]
        public void AddOtherType()
        {
            var undoDoActions = new Action[] {
                        () => {},
                        () => {}
                    };
            this.Send(Event.ShowUndoOverlay, new object[] { undoDoActions, typeof(HealthRecord) }.AsParams(MessageKeys.UndoDoActions, MessageKeys.Type));
            this.Send(Event.ShowUndoOverlay, new object[] { undoDoActions, typeof(Appointment) }.AsParams(MessageKeys.UndoDoActions, MessageKeys.Type));

            Assert.AreEqual(2, service.Overlays.Count);
        }

        [TestMethod]
        public void AddOtherTypeMeassgeOverlay()
        {
            this.Send(Event.ShowMessageOverlay, new object[] { "str0", typeof(Appointment) }.AsParams(MessageKeys.String, MessageKeys.Type));
            this.Send(Event.ShowMessageOverlay, new object[] { "str1", typeof(HealthRecord) }.AsParams(MessageKeys.String, MessageKeys.Type));

            Assert.AreEqual(2, service.Overlays.Count);
        }

        [TestMethod]
        public void AddSameMeassgeOverlay_MessageChanged()
        {
            this.Send(Event.ShowMessageOverlay, new object[] { "str0", typeof(HealthRecord) }.AsParams(MessageKeys.String, MessageKeys.Type));
            this.Send(Event.ShowMessageOverlay, new object[] { "str1", typeof(HealthRecord) }.AsParams(MessageKeys.String, MessageKeys.Type));

            Assert.AreNotEqual("str0", service.Overlays[0].Message);
        }

        [TestMethod]
        public void Close_RemoveAndExecuteOnClose()
        {
            bool executed = false;
            var undoDoActions = new Action[] {
                        () => {},
                        () => {executed=true;}
                    };

            this.Send(Event.ShowUndoOverlay, new object[] { undoDoActions, typeof(HealthRecord) }.AsParams(MessageKeys.UndoDoActions, MessageKeys.Type));
            service.Overlays[0].CloseCommand.Execute(true);

            Assert.AreEqual(0, service.Overlays.Count);
            Assert.IsTrue(executed);
        }

        [TestMethod]
        public void Hide_RemoveAndExecuteOnClose()
        {
            bool executed = false;
            var undoDoActions = new Action[] {
                        () => {},
                        () => {executed=true;}
                    };

            this.Send(Event.ShowUndoOverlay, new object[] { undoDoActions, typeof(HealthRecord) }.AsParams(MessageKeys.UndoDoActions, MessageKeys.Type));
            this.Send(Event.HideOverlay, new object[] { typeof(HealthRecord), true }.AsParams(MessageKeys.Type, MessageKeys.Boolean));

            Assert.AreEqual(0, service.Overlays.Count);
            Assert.IsTrue(executed);
        }

        [TestMethod]
        public void Hide_RemovedByType()
        {
            bool executed = false;
            var undoDoActions = new Action[] {
                        () => {},
                        () => {executed=true;}
                    };

            this.Send(Event.ShowUndoOverlay, new object[] { undoDoActions, typeof(HealthRecord) }.AsParams(MessageKeys.UndoDoActions, MessageKeys.Type));
            this.Send(Event.ShowMessageOverlay, new object[] { "str0", typeof(HealthRecord) }.AsParams(MessageKeys.String, MessageKeys.Type));
            this.Send(Event.ShowUndoOverlay, new object[] { undoDoActions, typeof(Appointment) }.AsParams(MessageKeys.UndoDoActions, MessageKeys.Type));
            this.Send(Event.HideOverlay, new object[] { typeof(HealthRecord), false }.AsParams(MessageKeys.Type, MessageKeys.Boolean));

            Assert.AreEqual(1, service.Overlays.Count);
            Assert.AreEqual(typeof(Appointment), service.Overlays[0].RelatedType);
            Assert.IsTrue(!executed);
        }
    }
}