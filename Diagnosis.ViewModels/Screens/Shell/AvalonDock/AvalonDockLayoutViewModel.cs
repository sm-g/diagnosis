// using AvalonDock;
// using AvalonDock.Layout.Serialization;
using Diagnosis.Common;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout.Serialization;

// from Edi http://www.codeproject.com/Articles/719143/AvalonDock-Tutorial-Part-Load-Save-Layout

namespace Diagnosis.ViewModels.Screens
{
    /// <summary>
    /// Class implements a viewmodel to support the
    /// <seealso cref="AvalonDockLayoutSerializer"/>
    /// attached behavior which is used to implement
    /// load/save of layout information on application
    /// start and shut-down.
    /// </summary>
    public class AvalonDockLayoutViewModel
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(AvalonDockLayoutViewModel));
        private DockingManager docManager;
        private Action<LayoutSerializationCallbackEventArgs> reloadContent;

        public AvalonDockLayoutViewModel(Action<LayoutSerializationCallbackEventArgs> reloadContentCallback)
        {
            this.reloadContent = reloadContentCallback;
        }

        public event EventHandler LayoutLoaded;

        public event EventHandler LayoutSaved;

        public event EventHandler LayoutLoading;

        /// <summary>
        /// XML with layout. Starting with "LayoutRoot".
        /// </summary>
        public string DefaultLayout { get; set; }

        /// <summary>
        /// File storing user layout.
        /// </summary>
        public string LayoutFileName { get; set; }

        /// <summary>
        /// Implement a command to load the layout of an AvalonDock-DockingManager instance.
        /// This layout defines the position and shape of each document and tool window
        /// displayed in the application.
        ///
        /// Parameter:
        /// The command expects a reference to a <seealso cref="DockingManager"/> instance to
        /// work correctly. Not supplying that reference results in not loading a layout (silent return).
        /// </summary>
        public ICommand LoadLayoutCommand
        {
            get
            {
                return new RelayCommand<FrameworkElement>((p) =>
                 {
                     DockingManager docManager = p as DockingManager;

                     if (docManager == null)
                         return;
                     this.docManager = docManager;

                     this.LoadDockingManagerLayout(docManager);
                 }, (p) => LayoutFileName != null || DefaultLayout != null);
            }
        }

        /// <summary>
        /// Implements a command to save the layout of an AvalonDock-DockingManager instance.
        /// This layout defines the position and shape of each document and tool window
        /// displayed in the application.
        ///
        /// Parameter:
        /// The command expects a reference to a <seealso cref="string"/> instance to
        /// work correctly. The string is supposed to contain the XML layout persisted
        /// from the DockingManager instance. Not supplying that reference to the string
        /// results in not saving a layout (silent return).
        /// </summary>
        public ICommand SaveLayoutCommand
        {
            get
            {
                return new RelayCommand<string>((p) =>
                 {
                     string xmlLayout = p as string;

                     if (xmlLayout == null)
                         return;

                     this.SaveDockingManagerLayout(xmlLayout);
                 }, (p) => LayoutFileName != null);
            }
        }

        protected virtual void OnLayoutLoaded(EventArgs e)
        {
            //logger.DebugFormat("layout loaded");

            var h = LayoutLoaded;
            if (h != null)
            {
                h(this, e);
            }
        }

        protected virtual void OnLayoutSaved(EventArgs e)
        {
            //logger.DebugFormat("layout saved");

            var h = LayoutSaved;
            if (h != null)
            {
                h(this, e);
            }
        }

        protected virtual void OnLayoutLoading(EventArgs e)
        {
            //logger.DebugFormat("layout loading");

            var h = LayoutLoading;
            if (h != null)
            {
                h(this, e);
            }
        }

        /// <summary>
        /// Loads the layout of a particular docking manager instance from persistence
        /// and checks whether a file should really be reloaded (some files may no longer
        /// be available).
        /// </summary>
        /// <param name="docManager"></param>
        private void LoadDockingManagerLayout(DockingManager docManager)
        {
            OnLayoutLoading(EventArgs.Empty);

            var fromFile = System.IO.File.Exists(LayoutFileName);
            var layoutSerializer = new XmlLayoutSerializer(docManager);
            layoutSerializer.LayoutSerializationCallback += (s, args) =>
            {
                //logger.DebugFormat("deserialize {0}, content: {1}", args.Model.ContentId, args.Content);
                // This can happen if the previous session was loading a file
                // but was unable to initialize the view ...
                if (args.Model.ContentId == null)
                {
                    args.Cancel = true;
                    return;
                }

                if (reloadContent != null)
                    reloadContent(args);
            };

            if (fromFile)
                layoutSerializer.Deserialize(LayoutFileName);
            else
                using (var s = DefaultLayout.ToMemoryStream())
                {
                    layoutSerializer.Deserialize(s);
                }

            OnLayoutLoaded(EventArgs.Empty);
        }

        private void SaveDockingManagerLayout(string xmlLayout)
        {
            if (xmlLayout == null)
                return;

            try
            {
                File.WriteAllText(LayoutFileName, xmlLayout);

                OnLayoutSaved(EventArgs.Empty);
            }
            catch (Exception e)
            {
                logger.ErrorFormat("{0}", e);
            }
        }
    }
}