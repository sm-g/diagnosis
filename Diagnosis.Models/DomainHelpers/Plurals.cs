using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace Diagnosis.Models
{
    public class Plurals
    {
        public static string[] minutes = new string[3] { "минута", "минуты", "минут" };
        public static string[] hours = new string[3] { "час", "часа", "часов" };
        public static string[] days = new string[3] { "день", "дня", "дней" };
        public static string[] weeks = new string[3] { "неделя", "недели", "недель" };
        public static string[] months = new string[3] { "месяц", "месяца", "месяцев" };
        public static string[] years = new string[3] { "год", "года", "лет" };
        public static string[] patients = new string[3] { "пациент", "пациента", "пациентов" };
        public static string[] courses = new string[3] { "курс", "курса", "курсов" };
        public static string[] apps = new string[3] { "осмотр", "осмотра", "осмотров" };
        public static string[] hrs = new string[3] { "запись", "записи", "записей" };
    }
}
