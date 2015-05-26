using Diagnosis.Common;
using EventAggregator;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public class OverlayServiceViewModel : ViewModelBase
    {
        private readonly EventMessageHandlersManager emh;

        public OverlayServiceViewModel()
        {
            Overlays = new ObservableCollection<OverlayViewModel>();
            emh = new EventMessageHandlersManager();

            emh.Add(this.Subscribe(Event.ShowUndoOverlay, (e) =>
            {
                Action[] actions = e.GetValue<Action[]>(MessageKeys.UndoDoActions);
                Type type = e.GetValue<Type>(MessageKeys.Type);

                var existingForType = Overlays.OfType<UndoOverlayViewModel>().SingleOrDefault(o => o.RelatedType == type);
                if (existingForType != null)
                {
                    existingForType.AddActions(actions[0], actions[1]);
                }
                else
                {
                    var vm = new UndoOverlayViewModel(actions[0], actions[1], type);
                    vm.Executed += (s, e1) =>
                    {
                        Overlays.Remove(vm);
                    };
                    Overlays.Add(vm);
                }
            }));

            emh.Add(this.Subscribe(Event.ShowMessageOverlay, (e) =>
            {
                Type type = e.GetValue<Type>(MessageKeys.Type);
                var str = e.GetValue<string>(MessageKeys.String);

                var existingForType = Overlays.OfType<MessageOverlayViewModel>().SingleOrDefault(o => o.RelatedType == type);
                if (existingForType != null)
                {
                    existingForType.AddToMessage(str);
                }
                else
                {
                    var vm = new MessageOverlayViewModel(str, () => { }, type);
                    vm.Executed += (s, e1) =>
                    {
                        Overlays.Remove(vm);
                    };
                    Overlays.Add(vm);
                }
            }));

            emh.Add(this.Subscribe(Event.HideOverlay, (e) =>
             {
                 var type = e.GetValue<Type>(MessageKeys.Type);
                 var withCloseActs = e.GetValue<bool>(MessageKeys.Boolean);
                 Overlays
                     .Where(o => o.RelatedType == type)
                     .ForEach(x => x.CloseCommand.Execute(withCloseActs));
             }));
        }

        public ObservableCollection<OverlayViewModel> Overlays { get; private set; }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                emh.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}