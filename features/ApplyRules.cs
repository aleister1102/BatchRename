using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace BatchRename
{
    internal delegate void RenameAction(string oldPath, string newPath);

    public partial class MainWindow : Window
    {
        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            RenameWith(RenameFileAction);
        }

        private void ApplyAndCopyButton_Click(object sender, RoutedEventArgs e)
        {
            var browsingScreen = new CommonOpenFileDialog() { Multiselect = false, IsFolderPicker = true };

            _moveToPath = browsingScreen.ShowDialog() == CommonFileDialogResult.Ok ? browsingScreen.FileName : string.Empty;

            RenameWith(RenameAndCopyFileAction);
        }

        private void RenameWith(RenameAction action)
        {
            List<Tuple<string, string>> renamePairs = MakeRenamePairs();

            bool isValid = Validate(renamePairs);

            if (isValid)
            {
                foreach (var renamePair in renamePairs)
                {
                    string oldPath = renamePair.Item1;
                    string newPath = renamePair.Item2;
                    action.Invoke(oldPath, newPath);
                }
            }
        }

        private List<Tuple<string, string>> MakeRenamePairs()
        {
            var sortedRulesInfo = _viewModel.RulesInfo.Clone().Sort();
            var renamePairs = new List<Tuple<string, string>>();

            foreach (var file in _viewModel.Files)
            {
                string oldPath = file.Path;
                string newPath = file.Path;

                string fileDirectory = UpdateDirectory(file);
                string fileName = file.Name;

                foreach (var ruleInfo in sortedRulesInfo)
                {
                    if (ruleInfo.IsActive())
                    {
                        fileName = ruleInfo.Rule.Rename(fileName);
                        newPath = Path.Combine(fileDirectory, fileName);
                    }
                }

                renamePairs.Add(new Tuple<string, string>(oldPath, newPath));
            }

            return renamePairs;
        }

        private string UpdateDirectory(File file)
        {
            return _moveToPath == string.Empty ? Path.GetDirectoryName(file.Path)! : _moveToPath;
        }

        private static bool Validate(List<Tuple<string, string>> renamePairs)
        {
            bool result = true;

            foreach (var renamePair in renamePairs)
            {
                string newPath = renamePair.Item2;

                if (newPath.Length > 255)
                {
                    result = false;
                    MessageBox.Show("Some of the file name length is greater than 255, please consider to modify your params.");
                }
            }

            return result;
        }

        private static string HandleDuplications(string path)
        {
            int index = 1;

            while (System.IO.File.Exists(path))
            {
                string directory = Path.GetDirectoryName(path)!;
                string fileName = Path.GetFileNameWithoutExtension(path);
                string extension = Path.GetExtension(path);

                path = Path.Combine(directory, $"{fileName} ({index++}){extension}");
            }

            return path;
        }

        private void RenameFileAction(string oldPath, string newPath)
        {
            try
            {
                if (oldPath == newPath) return;

                newPath = HandleDuplications(newPath);
                System.IO.File.Move(oldPath, newPath);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void RenameAndCopyFileAction(string oldPath, string newPath)
        {
            try
            {
                if (oldPath == newPath) return;

                newPath = HandleDuplications(newPath);
                System.IO.File.Copy(oldPath, newPath);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
    }
}