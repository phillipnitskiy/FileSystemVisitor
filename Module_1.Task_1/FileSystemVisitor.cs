using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Module_1.Task_1
{
    public class FileSystemVisitor : IEnumerable<string>
    {
        #region Fields

        private readonly string _root;
        private readonly Predicate<string> _filter = (x) => true;

        #endregion

        #region Constructors

        public FileSystemVisitor(string root)
        {
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }
            if (!Directory.Exists(root))
            {
                throw new DirectoryNotFoundException();
            }
            _root = root;
        }

        public FileSystemVisitor(string root, Predicate<string> filter): this(root)
        {
            _filter = filter ?? throw new ArgumentNullException(nameof(filter));
        }

        #endregion

        #region Private Methods

        private IEnumerable<string> VisitFileSystem(string root)
        {
            if (root == null)
            {
                throw new ArgumentNullException($"{nameof(root)}");
            }

            OnVisitStarted(new EventArgs());

            foreach (var item in VisitDirectory(root))
            {
                yield return item;
            }

            OnVisitFinished(new EventArgs());
        }

        private IEnumerable<string> VisitDirectory(string root)
        {
            foreach (var file in VisitFiles(root))
            {
                yield return file;
            }
            
            foreach (var subDirectory in Directory.EnumerateDirectories(root))
            {
                var directoryEventArgs = new DirectoryEventArgs { Path = subDirectory };
                OnDirectoryFound(directoryEventArgs);
                if (directoryEventArgs.Stop)
                {
                    yield break;
                }

                if (!directoryEventArgs.Exclude && _filter(subDirectory))
                {
                    var filteredDirectoryEventArgs = new DirectoryEventArgs { Path = subDirectory };
                    OnFilteredDirectoryFound(directoryEventArgs);
                    if (filteredDirectoryEventArgs.Stop)
                    {
                        yield break;
                    }

                    if (!filteredDirectoryEventArgs.Exclude)
                    {
                        yield return subDirectory; 
                    }
                } 
                
                foreach (var item in VisitDirectory(subDirectory))
                {
                    yield return item;
                }
            }
        }

        private IEnumerable<string> VisitFiles(string root)
        {
            foreach (var file in Directory.EnumerateFiles(root))
            {
                var fileEventArgs = new FileEventArgs { Path = file };
                OnFileFound(fileEventArgs);
                if (fileEventArgs.Stop)
                {
                    yield break;
                }
                if (!fileEventArgs.Exclude && _filter(file))
                {
                    var filteredFileEventArgs = new FileEventArgs { Path = file };
                    OnFilteredFileFound(filteredFileEventArgs);
                    if (filteredFileEventArgs.Stop)
                    {
                        yield break;
                    }

                    if (!filteredFileEventArgs.Exclude)
                    {
                        yield return file;
                    }
                }
            }
        }

        #endregion

        #region Events

        public event EventHandler<EventArgs> VisitStarted;
        public event EventHandler<EventArgs> VisitFinished;
        public event EventHandler<FileEventArgs> FileFound;
        public event EventHandler<DirectoryEventArgs> DirectoryFound;
        public event EventHandler<FileEventArgs> FilteredFileFound;
        public event EventHandler<DirectoryEventArgs> FilteredDirectoryFound;

        #endregion

        #region Event Risers

        protected virtual void OnVisitStarted(EventArgs args)
        {
            VisitStarted?.Invoke(this, args);
        }

        protected virtual void OnVisitFinished(EventArgs args)
        {
            VisitFinished?.Invoke(this, args);
        }

        protected virtual void OnFileFound(FileEventArgs args)
        {
            FileFound?.Invoke(this, args);
        }

        protected virtual void OnDirectoryFound(DirectoryEventArgs args)
        {
            DirectoryFound?.Invoke(this, args);
        }

        protected virtual void OnFilteredFileFound(FileEventArgs args)
        {
            FilteredFileFound?.Invoke(this, args);
        }

        protected virtual void OnFilteredDirectoryFound(DirectoryEventArgs args)
        {
            FilteredDirectoryFound?.Invoke(this, args);
        }

        #endregion

        #region IEnumerable implementation

        public IEnumerator<string> GetEnumerator()
        {
            return VisitFileSystem(_root).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
