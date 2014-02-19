using System;
namespace Diagnosis.Controls
{
    interface IEditableItem
    {
        void BeginEdit();
        void CommitChanges();
        void RevertChanges();
    }
}
