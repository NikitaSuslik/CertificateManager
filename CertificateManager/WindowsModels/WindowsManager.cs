using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

using CertificateManager.Windows;
using System.ComponentModel;

namespace CertificateManager.WindowsModels
{
    class WindowsManager
    {
        private Stack<Window> WindowStack = new Stack<Window>();
        static private WindowsManager shared = null;

        private WindowsManager()
        {

        }

        static public WindowsManager Shared
        {
            get
            {
                return shared ?? (shared = new WindowsManager());
            }
        }

        public void ShowMessage(string title, string message, bool error)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, error ? MessageBoxImage.Error : MessageBoxImage.Information);
        }

        public bool ShowQuestion(string title, string message)
        {
            return MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }

        public void ShowMainWindow()
        {
            Window w = new MainWindow();
            WindowStack.Push(w);
            (w.DataContext as MainWindowModel).WindowUpdate();

            MyWindowModel context = (w.DataContext as MyWindowModel);
            w.Closed += _WindowsClosed;

            w.Show();
        }

        public void ShowWindow(Window window, params object[] prop)
        {
            if(WindowStack.Count != 0)
                WindowStack.Peek().Hide();
            WindowStack.Push(window);

            MyWindowModel context = (window.DataContext as MyWindowModel);
            context.props = prop;
            window.Closed += _WindowsClosed;

            window.Show();
        }

        private void _WindowsClosed(object sender, EventArgs e)
        {
            WindowStack.Pop();
            if (WindowStack.Count == 0)
                Environment.Exit(0);
            else
            {
                WindowStack.Peek().Show();
            }
        }

        public void CloseCurrentWindow()
        {
            Window win = WindowStack.Pop();
            win.Closed -= _WindowsClosed;
            win.Close();
            Window w = WindowStack.Peek();
            if (w is MainWindow)
            {
                (w.DataContext as MainWindowModel).WindowUpdate();
                w.Show();
            }
        }

    }



    class MyWindowModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected object[] _props = null;
        public object[] props
        {
            get
            {
                return _props;
            }
            set
            {
                _props = value;
                _PropsChanged();
            }
        }

        virtual protected void _PropsChanged()
        {
            
        }

        protected void OnPropertyChanged(string prop)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
