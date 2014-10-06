using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using EventAggregator;
using Diagnosis.Core;

namespace Diagnosis.ViewModels
{
    public class OverlayServiceViewModel : ViewModelBase
    {
        public OverlayServiceViewModel()
        {
            Overlays = new ObservableCollection<UndoOverlayViewModel>();

            this.Subscribe(Events.ShowUndoOverlay, (e) =>
            {
                Action[] actions = e.GetValue<Action[]>(MessageKeys.UndoOverlay);
                Type type = e.GetValue<Type>(MessageKeys.Type);

                UndoOverlayViewModel existing = Overlays.SingleOrDefault(o => o.ObjectsType == type);

                if (existing != null)
                {
                    existing.AddActions(actions[0], actions[1]);
                }
                else
                {
                    var vm = new UndoOverlayViewModel(actions[0], actions[1], (self) => Overlays.Remove(self), type);
                    Overlays.Add(vm);
                }
            });

            this.Subscribe(Events.HideOverlay, (e) =>
            {
                var type = e.GetValue<Type>(MessageKeys.Type);

                UndoOverlayViewModel existing = Overlays.SingleOrDefault(o => o.ObjectsType == type);
                if (existing != null)
                    existing.CloseCommand.Execute(null);
            });
        }

        public ObservableCollection<UndoOverlayViewModel> Overlays { get; private set; }
    }
}
