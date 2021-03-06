﻿using Diagnosis.Common;
using Diagnosis.Models;
using Diagnosis.ViewModels.Framework;
using Diagnosis.ViewModels.Screens;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Diagnosis.ViewModels
{
    internal class Exporter
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(Exporter));
        private readonly static string defName = "diagnosis export";
        private readonly static double hrIdColWidth = 13;
        private readonly static string uomHeader = "ед. изм.";

        // колонок на заголовок строки
        private readonly static int patientCols = 4;

        // строк на заголовок колонок
        private readonly static int headerRows = 1;

        public void ExportToXlsx(HrsStatistic stats)
        {
            var result = new FileDialogService().ShowSaveFileDialog(null,
                  FileType.Xlsx.ToEnumerable(),
                  FileType.Xlsx,
                  string.Format("{0} {1:yyyy.MM.dd HH.mm}", defName, DateTime.Now));

            if (result.IsValid)
            {
                var newFile = FileHelper.CreateNewFile(result.FileName);
                ExcelPackage package = MakeExcelPackage(stats);
                SaveTo(package, newFile);
            }
        }

        private static ExcelPackage MakeExcelPackage(HrsStatistic stats)
        {
            ExcelPackage package = new ExcelPackage();
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Матрица");

            // доп. строки на запись
            var additionalRows = new Dictionary<HealthRecord, int>();

            SetValues(stats, worksheet, additionalRows);

            SetHeaders(stats, worksheet, additionalRows);

            FormatSheet(stats, worksheet, additionalRows);

            // set some document properties
            package.Workbook.Properties.Title = "Diagnosis Эскпорт";

            return package;
        }

        private static void SetValues(HrsStatistic stats, ExcelWorksheet worksheet, Dictionary<HealthRecord, int> additionalRows)
        {
            int cIndex = 1, rIndex = 1; // индексация с 1

            foreach (var hr in stats.HealthRecords)
            {
                cIndex = 1;
                foreach (var icd in stats.Icds)
                {
                    if (stats.GridValues[hr][icd].Bool.HasValue)
                        worksheet.Cells[rIndex, cIndex].Value = stats.GridValues[hr][icd].Bool;
                    cIndex++;
                }
                foreach (var word in stats.Words)
                {
                    int additionalRow = 0;

                    if (stats.GridValues[hr][word].Bool.HasValue)
                        worksheet.Cells[rIndex, cIndex].Value = stats.GridValues[hr][word].Bool;
                    else
                    {
                        foreach (var measure in stats.GridValues[hr][word].Measures)
                        {
                            // При повторе слова с числом в одной записи — только это число в дополнительной строке.
                            worksheet.Cells[rIndex + additionalRow, cIndex].Value = measure.Value;
                            worksheet.Cells[rIndex + additionalRow, cIndex + 1].Value = measure.Uom != null ? measure.Uom.Abbr : null;
                            additionalRow++;
                        }
                    }

                    // есть измерение со словом - доп. колонка
                    if (HasMeasuresFor(stats.GridValues, word))
                        cIndex++;

                    cIndex++;
                    additionalRows[hr] = Math.Max(additionalRows.GetValueOrDefault(hr), additionalRow - 1);
                }
                rIndex = rIndex + 1 + additionalRows[hr];
            }
        }

        private static void SetHeaders(HrsStatistic stats, ExcelWorksheet worksheet, Dictionary<HealthRecord, int> additionalRows)
        {
            // заголовки строк
            worksheet.InsertColumn(1, patientCols);

            var rIndex = 1;
            foreach (var hr in stats.HealthRecords)
            {
                var pat = hr.GetPatient();
                worksheet.Cells[rIndex, 1].Value = pat.FullNameOrCreatedAt;
                worksheet.Cells[rIndex, 2].Value = pat.IsMale;
                worksheet.Cells[rIndex, 3].Value = pat.Age;
                worksheet.Cells[rIndex, 4].Value = hr.CreatedAt;
                // доп. строки
                rIndex += 1 + additionalRows[hr];
            }
            // заголовки колонок на заголовок строки
            worksheet.InsertRow(1, headerRows);
            worksheet.Cells[1, 1].Value = "Имя";
            worksheet.Cells[1, 2].Value = "Пол";
            worksheet.Cells[1, 3].Value = "Возраст";
            worksheet.Cells[1, 4].Value = "Дата создания записи";

            var cIndex = patientCols + 1;
            // заголовки колонок со значениями
            foreach (var icd in stats.Icds)
            {
                worksheet.Cells[1, cIndex++].Value = icd.Code;
            }
            foreach (var word in stats.Words)
            {
                worksheet.Cells[1, cIndex++].Value = word.Title;

                // есть измерение со словом - доп. колонка
                if (HasMeasuresFor(stats.GridValues, word))
                    worksheet.Cells[1, cIndex++].Value = uomHeader;
            }
        }

        private static void FormatSheet(HrsStatistic stats, ExcelWorksheet worksheet, Dictionary<HealthRecord, int> additionalRows)
        {
            // область значений
            var rangeColStart = patientCols + 1;
            var rangeColEnd = patientCols + stats.Icds.Count + stats.Words.Count + stats.WordsWithMeasure.Count;
            var rangeRowStart = headerRows + 1;
            var rangeRowEnd = headerRows + stats.HealthRecords.Count + additionalRows.Values.Sum();

            // format headers
            worksheet.Cells[1, 1, headerRows, rangeColEnd].Style.Font.Bold = true;

            worksheet.Cells[1, patientCols, rangeRowEnd, patientCols].Style.Border.Right.Style = ExcelBorderStyle.Thin;

            worksheet.Cells[1, 1, 1, rangeColEnd].AutoFilter = true;
            worksheet.Cells[1, 1, rangeRowEnd, rangeColEnd].AutoFitColumns();
            worksheet.View.FreezePanes(rangeRowStart, rangeColStart);

            // hr id column
            worksheet.Cells[rangeRowStart, 4, rangeRowEnd, 4].Style.Numberformat.Format = "dd/mm/yy h:mm";
            worksheet.Column(4).Width = hrIdColWidth;

            // format page
            worksheet.HeaderFooter.OddHeader.CenteredText = "Эскпорт";

            worksheet.HeaderFooter.OddFooter.RightAlignedText =
                string.Format("Стр. {0} / {1}", ExcelHeaderFooter.PageNumber, ExcelHeaderFooter.NumberOfPages);
            worksheet.HeaderFooter.OddFooter.CenteredText = ExcelHeaderFooter.SheetName;
            worksheet.HeaderFooter.OddFooter.LeftAlignedText = DateTime.Now.Date.ToShortDateString();

            //worksheet.PrinterSettings.RepeatRows = worksheet.Cells[1, 1, headerRows, rangeColEnd];
            //worksheet.PrinterSettings.RepeatColumns = worksheet.Cells[1, 1, rangeRowEnd, patientCols];
        }

        private static bool HasMeasuresFor(
            Dictionary<HealthRecord, Dictionary<IHrItemObject, Diagnosis.ViewModels.Screens.HrsStatistic.GridValue>> GridValues,
            Word word)
        {
            return GridValues.Values.Any(y => y[word].Measures.Any());
        }

        private static void SaveTo(ExcelPackage package, FileInfo file)
        {
            try
            {
                package.SaveAs(file);
            }
            catch (Exception e)
            {
                logger.WarnFormat("Error when save {0}: {1}", file.FullName, e);
            }
        }
    }
}