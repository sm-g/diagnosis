using Diagnosis.Common;
using MahApps.Metro;
using System;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Linq;
using System.Windows;

namespace Diagnosis.App.Themes
{
    public static class MyThemeManager
    {
        private static IList<AppTheme> _mahThemes;
        private static IList<AppTheme> _fontThemes;
        private static IList<string> MahAppsComponents = new[] { "Controls", "Fonts", "Colors", "Accents/Blue", "Accents/BaseLight" };
        private static IList<string> FontSizes = new[] { "Normal", "Big" };
        private static string MahAppsAssemblyName = "MahApps.Metro";
        private static bool initialized;
        static int resoursesAfterThemes = 2;

        public static IEnumerable<AppTheme> MahThemes
        {
            get
            {
                if (_mahThemes != null)
                    return _mahThemes;

                _mahThemes = new List<AppTheme>();
                foreach (var item in MahAppsComponents)
                {
                    var appTheme = new AppTheme(MahAppsAssemblyName + item, string.Format("Styles/{0}.xaml", item).GetPackUri(MahAppsAssemblyName));
                    _mahThemes.Add(appTheme);
                };

                return _mahThemes;
            }
        }

        public static IList<AppTheme> FontThemes
        {
            get
            {
                if (_fontThemes != null)
                    return _fontThemes;

                _fontThemes = new List<AppTheme>();
                foreach (var size in FontSizes)
                {
                    var appTheme = new AppTheme(size, string.Format("Resources/{0}FontResources.xaml", size).GetPackUri());
                    _fontThemes.Add(appTheme);
                }

                return _fontThemes;
            }
        }

        public static void Initialize()
        {
            if (initialized)
                return;

            typeof(MyThemeManager).Subscribe(Event.ChangeTheme, (e1) =>
            {
                var toMetro = e1.GetValue<bool>(MessageKeys.Boolean);
                SwitchMetro();
            });
            typeof(MyThemeManager).Subscribe(Event.ChangeFont, (e1) =>
            {
                var toBig = e1.GetValue<bool>(MessageKeys.Boolean);
                SwitchFont(toBig ? "Big" : "Normal");
            });
            initialized = true;
        }

        public static void SwitchMetro()
        {
            var resources = Application.Current.Resources;


            var insertTo = resources.MergedDictionaries.Count - resoursesAfterThemes;

            foreach (var th in MahThemes)
            {
                var oldThemeResource = resources.MergedDictionaries.FirstOrDefault(d =>
                    PackUriHelper.ComparePackUri(d.Source, th.Resources.Source) == 0);
                if (oldThemeResource == null)
                {
                    resources.MergedDictionaries.Insert(insertTo, th.Resources);
                }
                else
                {
                    resources.MergedDictionaries.Remove(oldThemeResource);
                }
            }
        }

        public static void SwitchFont(string newSizeName)
        {
            var resources = Application.Current.Resources;

            int insertTo = 0; // in case no FontResources in App.xaml
            var newAppTheme = FontThemes.FirstOrDefault(x => x.Name == newSizeName);

            // first font res
            var oldThemeResource = resources.MergedDictionaries.FirstOrDefault(d =>
                FontThemes.Any(x => PackUriHelper.ComparePackUri(d.Source, x.Resources.Source) == 0));

            if (oldThemeResource != null)
            {
                insertTo = resources.MergedDictionaries.IndexOf(oldThemeResource);
                resources.MergedDictionaries.Remove(oldThemeResource);
            }
            if (newAppTheme != null)
                resources.MergedDictionaries.Insert(insertTo, newAppTheme.Resources);
        }
    }
}