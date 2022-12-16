using BatchRename.Rules;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace BatchRename
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private ObservableCollection<File> _sourceFiles = new ObservableCollection<File>();

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var browsingScreen = new OpenFileDialog { Multiselect = true };

            if (browsingScreen.ShowDialog() == true)
            {
                var filePaths = browsingScreen.FileNames;

                foreach (var filePath in filePaths)
                {
                    var fileInfo = new FileInfo(filePath);
                    var fileName = fileInfo.Name;

                    _sourceFiles.Add(new File() { Path = filePath, Name = fileName });
                }
            }

            FileListView.ItemsSource = _sourceFiles;
            PreviewListView.ItemsSource = _sourceFiles;
        }

        private void LoadPresetButton_Click(object sender, RoutedEventArgs e)
        {
            //var screen = new OpenFileDialog();

            //if (screen.ShowDialog() == true)
            //{
            //    var configFilePath = screen.FileName;
            //}

            IRule rule1 = new AddPrefixRule() { Prefix = "DTO_" };
            IRule rule2 = new ReplaceSpecialCharsRule() { SpecialChars = "_", Replacement = " " };

            var rules = new List<IRule>() { rule1, rule2 };

            foreach (var file in _sourceFiles)
            {
                foreach (var rule in rules)
                {
                    file.Name = rule.Rename(file.Name);
                }
            }
        }
    }
}