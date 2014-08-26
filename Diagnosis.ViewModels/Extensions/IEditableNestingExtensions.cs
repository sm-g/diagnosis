using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Diagnosis.Core;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Diagnosis.ViewModels
{
    static class IEditableNestingExtensions
    {
        public static void DeleteEmpty<T>(this IEditableNesting outer, IList<T> inners) where T : IEditableNesting
        {
            var i = 0;
            while (i < inners.Count)
            {
                if (inners[i].IsEmpty)
                {
                    inners[i].Editable.Delete();
                }
                else
                {
                    i++;
                }
            }
        }

        /// <summary>
        /// Сохраняет и удаляет вложенные сущности при сохранении и удалении данной сущности. 
        /// Помечает сущность грязной при удалении в коллекции вложенных сущностей. 
        /// При добавлении вложенная сущность пуста, поэтому данная сущность осаётся чистой.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="inner">Коллекция вложенных сущностей.</param>
        /// <param name="onDeletedBefore">Действие, выполняемое перед удалением сущности.</param>
        /// <param name="innerChangedAfter">Действие, выполняемое после изменения коллекции вложенных сущностей.</param>
        public static void SubscribeEditableNesting<T>(this IEditableNesting entity, ObservableCollection<T> inner,
            Action onDeletedBefore = null, Action innerChangedAfter = null)
            where T : IEditableNesting
        {
            NotifyCollectionChangedEventHandler innerChangedHandler = (s, e) =>
            {
                if (innerChangedAfter != null)
                {
                    innerChangedAfter();
                }
            };

            EditableEventHandler committedHandler = new EditableEventHandler((s, e) =>
            {
                entity.DeleteEmpty(inner);
                inner.ForAll(x => x.Editable.Commit());
            });

            EditableEventHandler deletedHandler = null;
            deletedHandler = (s, e) =>
            {
                if (onDeletedBefore != null)
                    onDeletedBefore();

                while (inner.Count > 0)
                {
                    inner[0].Editable.Delete();
                }

                entity.Editable.Committed -= committedHandler;
                entity.Editable.Deleted -= deletedHandler;
                inner.CollectionChanged -= innerChangedHandler;
            };

            entity.Editable.Committed += committedHandler;
            entity.Editable.Deleted += deletedHandler;
            inner.CollectionChanged += innerChangedHandler;
        }
    }
}
