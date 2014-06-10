using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace Diagnosis.App.Controls
{
    static class DataGridHelper
    {
        public static DataGridCell GetDataGridCell(DataGridCellInfo cellInfo)
        {
            var cellContent = cellInfo.Column.GetCellContent(cellInfo.Item);

            if (cellContent != null)
                return ((DataGridCell)cellContent.Parent);

            return (null);
        }
    }
}
