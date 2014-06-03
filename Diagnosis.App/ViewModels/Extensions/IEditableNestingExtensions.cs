using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Diagnosis.Core;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Diagnosis.App.ViewModels
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

        public static void SubscribeNesting<T>(this IEditableNesting entity, ObservableCollection<T> inner,
            Action onDeletedBefore = null, Action innerChangedAfter = null, Func<bool> innerChangedAndCondition = null)
            where T : IEditableNesting
        {
            Contract.Requires(entity != null);

            NotifyCollectionChangedEventHandler innerChangedHandler = (s, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    if (innerChangedAndCondition == null || innerChangedAndCondition())
                        entity.Editable.MarkDirty();
                }
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
