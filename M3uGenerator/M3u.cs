﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;
using System.Collections.ObjectModel;

namespace M3uGenerator
{
    public class M3u : INotifyPropertyChanged
    {
        #region [Private Fields]

        private string _path;
        private ObservableCollection<Tag> _fileList;
        private bool _changed;

        #endregion

        #region [Event Handler]
        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        void OnPropertyChanged(params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public string FilePath {
            get => _path;
            set { _path = value; OnPropertyChanged(nameof(FilePath), nameof(Title), nameof(CanSaveAs)); }
        }

        public bool Changed {
            get => _changed;
            set { if (_changed != value) { _changed = value; OnPropertyChanged(nameof(Changed), nameof(Title)); } }
        }

        public ObservableCollection<Tag> FileList {
            get => _fileList;
            set
            {
                _fileList = value;
                Changed = true;
                OnPropertyChanged(nameof(FileList));
                _fileList.CollectionChanged += (s, e) => Changed = true;
            }
        }

        public string Title => (Path.GetFileNameWithoutExtension(FilePath) ?? "Untitled") + (Changed ? "*" : "");

        public string FileName => Path.GetFileName(FilePath);

        public bool CanSaveAs => FilePath != null;

        public M3u()
        {
            FileList = new ObservableCollection<Tag>();
        }

        public M3u(IEnumerable<Tag> fileList)
        {
            FileList = new ObservableCollection<Tag>(fileList);
        }

        public static M3u Load(string filePath)
        {
            var folder = Path.GetDirectoryName(filePath);
            var collection = new Collection<Tag>();
            foreach(var relativePath in File.ReadAllLines(filePath))
            {
                if (string.IsNullOrWhiteSpace(relativePath)) continue;
                var fullPath = Path.GetFullPath(Path.Combine(folder, relativePath));
                collection.Add(Tag.ReadFrom(fullPath));
            }
            return new M3u(collection) { FilePath = filePath, Changed = false };
        }

        public void Save(string filePath=null)
        {
            filePath = filePath ?? FilePath
                ?? throw new ArgumentNullException("filePath", "未指定保存位置");
            var folder = Path.GetDirectoryName(filePath);
            var builder = new StringBuilder();
            foreach (var tag in FileList)
                builder.AppendLine(Util.GetRelativePath(tag.Path, folder));
            File.WriteAllText(filePath, builder.ToString());
            if (FilePath != filePath) FilePath = filePath;
        }
    }
}
