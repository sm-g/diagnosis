using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.IO;

namespace Diagnosis.ViewModels.Search
{
    internal class Exporter
    {
        private const string filename = "sample1.xlsx";
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(Exporter));

        public void ExportToXlsx(Statistic stats)
        {
            FileInfo newFile = CreateNewFile(filename);

            ExcelPackage package = new ExcelPackage(newFile);

            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Матрица");

            var patientCols = 3;
            var headerRows = 1;

            var rangeColStart = patientCols + 1;
            var rangeColEnd = patientCols + stats.Words.Count;
            var rangeRowStart = headerRows + 1;
            var rangeRowEnd = headerRows + stats.Patients.Count;

            for (int r = 0; r < stats.Patients.Count; r++)
            {
                for (int c = 0; c < stats.Words.Count; c++)
                {
                    worksheet.Cells[r + 1, c + 1].Value = stats.PatientHasWord[r, c];
                }
            }
            // row headers
            worksheet.InsertColumn(1, patientCols);
            for (int r = 0; r < stats.Patients.Count; r++)
            {
                worksheet.Cells[r + 1, 1].Value = stats.Patients[r].LastName;
                worksheet.Cells[r + 1, 2].Value = stats.Patients[r].IsMale;
                worksheet.Cells[r + 1, 3].Value = stats.Patients[r].Age;
            }
            // column headers
            worksheet.InsertRow(1, headerRows);
            worksheet.Cells[1, 1].Value = "Фамилия";
            worksheet.Cells[1, 2].Value = "Пол";
            worksheet.Cells[1, 3].Value = "Возраст";

            for (int c = 0; c < stats.Words.Count; c++)
            {
                worksheet.Cells[1, c + patientCols + 1].Value = stats.Words[c];
            }

            // format headers
            using (var range = worksheet.Cells[1, 1, headerRows, rangeColEnd])
            {
                range.Style.Font.Bold = true;
            }
            worksheet.Cells[1, patientCols, rangeRowEnd, patientCols].Style.Border.Right.Style = ExcelBorderStyle.Thin;

            worksheet.Cells[1, 1, 1, rangeColEnd].AutoFilter = true;
            worksheet.View.FreezePanes(rangeRowStart, rangeColStart);

            worksheet.Cells[1, 1, rangeRowEnd, rangeColEnd].AutoFitColumns();

            //worksheet.Cells["A5:E5"].Style.Font.Bold = true;

            //worksheet.Cells[5, 3, 5, 5].Formula = string.Format("SUBTOTAL(9,{0})", new ExcelAddress(2, 3, 4, 3).Address);
            //worksheet.Cells["C2:C5"].Style.Numberformat.Format = "#,##0";
            //worksheet.Cells["D2:E5"].Style.Numberformat.Format = "#,##0.00";

            // format page
            worksheet.HeaderFooter.OddHeader.CenteredText = "Матрица";

            worksheet.HeaderFooter.OddFooter.RightAlignedText =
                string.Format("Стр. {0} / {1}", ExcelHeaderFooter.PageNumber, ExcelHeaderFooter.NumberOfPages);
            worksheet.HeaderFooter.OddFooter.CenteredText = ExcelHeaderFooter.SheetName;
            worksheet.HeaderFooter.OddFooter.LeftAlignedText = DateTime.Now.Date.ToShortDateString();

            //worksheet.PrinterSettings.RepeatRows = worksheet.Cells[1, 1, headerRows, rangeColEnd];
            //worksheet.PrinterSettings.RepeatColumns = worksheet.Cells[1, 1, rangeRowEnd, patientCols];

            // Change the sheet view to show it in page layout mode
            // worksheet.View.PageLayoutView = true;

            // set some document properties
            package.Workbook.Properties.Title = "Матрица";

            try
            {
                package.Save();
            }
            catch (Exception e)
            {
                logger.WarnFormat("Error when save {0}: {1}", filename, e);
            }
        }

        private static FileInfo CreateNewFile(string filename)
        {
            FileInfo newFile = new FileInfo(filename);
            for (int i = 0; i < 5; i++)
            {
                if (newFile.Exists)
                {
                    try
                    {
                        newFile.Delete();
                        break;
                    }
                    catch (Exception e)
                    {
                        logger.WarnFormat("Error when delete {0}: {1}", filename, e);
                    }
                }
            }
            newFile = new FileInfo(filename);
            return newFile;
        }
    }
}