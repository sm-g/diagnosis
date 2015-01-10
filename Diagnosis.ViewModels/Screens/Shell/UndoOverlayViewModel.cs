using Diagnosis.Common;
using Diagnosis.Models;
using System;
using System.Collections.Generic;

namespace Diagnosis.ViewModels.Screens
{
    /// <summary>
    /// Сообщение для отмены удаления нескольких сущностей одного типа.
    /// При отмене выполняется onUndo. 
    /// При закрытии сообщения выполняется onDo.
    /// </summary>
    public class UndoOverlayViewModel : ViewModelBase
    {
        private const string messageTemplate = "Удален{0} {1} {2}";  // удалены 5 записей
        private readonly Action<UndoOverlayViewModel> onClose;
        private readonly List<Action> undos;
        private readonly List<Action> todos;
        private string _message;

        public UndoOverlayViewModel(Action onUndo, Action onDo, Action<UndoOverlayViewModel> onClose, Type objectsType)
        {
            undos = new List<Action>() { onUndo };
            todos = new List<Action>() { onDo };
            ObjectsType = objectsType;

            this.onClose = onClose;
            UpdateMessage();
        }

        public RelayCommand UndoCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    undos.ForEach((onUndo) => onUndo());
                    onClose(this);
                });
            }
        }

        public RelayCommand CloseCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    todos.ForEach((onDo) => onDo());
                    onClose(this);
                });
            }
        }

        public Type ObjectsType { get; private set; }

        public int Count { get { return undos.Count; } }

        public string Message
        {
            get
            {
                return _message;
            }
            set
            {
                if (_message != value)
                {
                    _message = value;
                    OnPropertyChanged(() => Message);
                }
            }
        }

        public static string TypeCountLabel(Type type, int count)
        {
            int ending = Plurals.GetPluralEnding(count);

            var @switch = new Dictionary<Type, Func<string>> {
                { typeof(Patient),() => Plurals.patients[ending] },
                { typeof(Course), () => Plurals.courses[ending] },
                { typeof(Appointment), () => Plurals.apps[ending] },
                { typeof(HealthRecord),() => Plurals.hrs[ending] },
            };
            return @switch[type]();
        }

        public static string DeletedLabel(Type type, int count)
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

        internal void AddActions(Action onUndo, Action onDo)
        {
            undos.Add(onUndo);
            todos.Add(onDo);
            UpdateMessage();
        }

        private void UpdateMessage()
        {
            Message = string.Format(messageTemplate,
                DeletedLabel(ObjectsType, Count),
                Count,
                TypeCountLabel(ObjectsType, Count));
        }
    }
}