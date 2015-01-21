using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MahApps.Metro;
using System.Windows;

namespace Diagnosis.App.Themes
{
    public class MyThemeManager
    {
        private static IList<AppTheme> _mahThemes;

        private static AppTheme _mahControls = new AppTheme("MahControls", new Uri("pack://application:,,,/MahApps.Metro;component/Styles/Controls.xaml"));
        private static AppTheme _mahFonts = new AppTheme("MahFonts", new Uri("pack://application:,,,/MahApps.Metro;component/Styles/Fonts.xaml"));
        private static AppTheme _mahColors = new AppTheme("MahColors", new Uri("pack://application:,,,/MahApps.Metro;component/Styles/Colors.xaml"));
        private static AppTheme _mahAcc = new AppTheme("MahAcc", new Uri("pack://application:,,,/MahApps.Metro;component/Styles/Accents/Blue.xaml"));
        public static IEnumerable<AppTheme> MahThemes
        {
            get
            {
                if (_mahThemes != null)
                    return _mahThemes;

                var themes = new[] { "BaseLight", "BaseDark" };

                _mahThemes = new List<AppTheme>()
                {
                    _mahControls,
                    _mahFonts,
                    _mahColors,
                    _mahAcc
                };

                return _mahThemes;
            }
        }

        public void Switch(bool toMetro)
        {
            var mahDicts = new[] { "MahControls", "MahFonts", "MahColors", "MahAcc" };

            var resources = Application.Current.Resources;
            var resBeforeThemes = 5;
            var resAfterThemes = 2;
            var insertTo = resources.MergedDictionaries.Count - resAfterThemes;


            foreach (var th in MahThemes)
            {
                var oldThemeResource = resources.MergedDictionaries.FirstOrDefault(d => d.Source == th.Resources.Source);
                if (toMetro)
                {
                    resources.MergedDictionaries.Insert(insertTo, th.Resources);

                }
                else
                    if (oldThemeResource != null)
                    {
                        {
                            resources.MergedDictionaries.Remove(oldThemeResource);
                        }

                    }
            }
        }
    }
}
