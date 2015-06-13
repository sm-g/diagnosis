using Diagnosis.Common;
using Diagnosis.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Diagnosis.ViewModels.Screens
{
    public class OverlayViewModel : ViewModelBase
    {
        private readonly List<Action> acts;
        private readonly List<Action> closeActs;
        private readonly Type type;
        private string _message;
        private string _actText;

        public OverlayViewModel(Action act, Action closeAct, Type objectsType)
        {
            Contract.Requires(act != null);
            Contract.Requires(closeAct != null);
            Contract.Requires(objectsType != null);

            acts = new List<Action>() { act };
            closeActs = new List<Action>() { closeAct };
            type = objectsType;

            UpdateMessage();
        }

        /// <summary>
        /// Возникает после выполнения любой команды.
        /// </summary>
        public event EventHandler Executed;

        /// <summary>
        /// Выполняет acts.
        /// </summary>
        public RelayCommand ActionCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    acts.ForEach(a => a());

                    OnExecuted(EventArgs.Empty);
                }, () => !string.IsNullOrEmpty(ActionText));
            }
        }

        /// <summary>
        /// Выполняет closeActs, если параметр true.
        /// </summary>
        public RelayCommand<bool> CloseCommand
        {
            get
            {
                return new RelayCommand<bool>((executeCloseAct) =>
                {
                    if (executeCloseAct)
                        closeActs.ForEach(a => a());

                    OnExecuted(EventArgs.Empty);
                });
            }
        }

        /// <summary>
        /// Тип объектов, к которыым относится сообщение.
        /// </summary>
        public Type RelatedType { get { return type; } }

        /// <summary>
        /// Количество добавленных команд.
        /// </summary>
        public int Count { get { return acts.Count; } }

        public string Message
        {
            get
            {
                return _message;
            }
            protected set
            {
                if (_message != value)
                {
                    _message = value;
                    OnPropertyChanged(() => Message);
                }
            }
        }

        public string ActionText
        {
            get
            {
                return _actText;
            }
            protected set
            {
                if (_actText != value)
                {
                    _actText = value;
                    OnPropertyChanged(() => ActionText);
                }
            }
        }

        public void AddActions(Action act, Action closeAct)
        {
            Contract.Requires(act != null);
            Contract.Requires(closeAct != null);

            acts.Add(act);
            closeActs.Add(closeAct);
            UpdateMessage();
        }

        public void AddToMessage(string str)
        {
            Message += Environment.NewLine + str;
        }

        protected virtual void OnExecuted(EventArgs e)
        {
            var h = Executed;
            if (h != null)
            {
                h(this, e);
            }
        }

        protected virtual void UpdateMessage()
        {
        }
    }

    /// <summary>
    /// Сообщение для отмены удаления нескольких сущностей одного типа.
    /// При отмене выполняется onUndo.
    /// При закрытии сообщения выполняется onDo.
    /// </summary>
    public class UndoOverlayViewModel : OverlayViewModel
    {
        private static string messageTemplate = "Удален{0} {1} {2}.";  // удалены 5 записей

        public UndoOverlayViewModel(Action onUndo, Action onDo, Type objectsType)
            : base(onUndo, onDo, objectsType)
        {
            Contract.Requires(new[] { typeof(Patient), typeof(Course), typeof(Appointment), typeof(HealthRecord) }.Contains(objectsType));

            ActionText = "Отменить?";
        }

        protected override void UpdateMessage()
        {
            Message = string.Format(messageTemplate,
                DeletedLabel(RelatedType, Count),
                Count,
                TypeCountLabel(RelatedType, Count));
        }

        private static string TypeCountLabel(Type type, int count)
        {
            int ending = PluralsHelper.GetPluralEnding(count);

            var @switch = new Dictionary<Type, Func<string>> {
                { typeof(Patient),() => Plurals.patients[ending] },
                { typeof(Course), () => Plurals.courses[ending] },
                { typeof(Appointment), () => Plurals.apps[ending] },
                { typeof(HealthRecord),() => Plurals.hrs[ending] },
            };
            return @switch[type]();
        }

        private static string DeletedLabel(Type type, int count)
        {
            // удалена запись, удалено 3 записи, удален осмотр
            var s = "";

            if (type == typeof(HealthRecord) && count == 1)
            {
                s += "a";
            }
            if (count > 1)
            {
                s += "о";
            }
            return s;
        }
    }

    public class MessageOverlayViewModel : OverlayViewModel
    {
        public MessageOverlayViewModel(string message, Action onClose, Type objectsType)
            : base(() => { }, onClose, objectsType)
        {
            Message = message;
        }
    }
}