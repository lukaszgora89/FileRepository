using System;
using System.Security.Cryptography;
using System.IO;
using MessagePackaging;
using XmlMessaging;


namespace RepositoryClient
{
    enum PackageBuilderState
    {
        IDLE,
        WAITING_FOR_HEADER,
        WAITING_FOR_FILE_BEGIN,
        RECEIVE_FILE_DATA
    }

    /*
     * The basic flow is:
     * 
     *    IDLE -------> WAITING_FOR_HEADER -------> WAITING_FOR_FILE_BEGIN -------> RECEIVE_FILE_DATA <----.
     *      ^                                                  ^                            |              |
     *      |                                                  |                            |              |
     *      |                                                {YES}                          |              |
     *      |                                                  |                            v              |
     *      '-----------------------------------{NO}- NEXT FILE EXPECTED? <-----{YES}- IS FILE END? -{NO}--'
     */

    public class FileReciveUpdateEventArgs : EventArgs
    {
        public string FileName { get; set; }
        public long CurrentSize { get; set; }
        public long ExpectedSize { get; set; }
    }


    class PackageBuilder
    {
        public event EventHandler<string> FileReciveBegin;
        public event EventHandler<FileReciveUpdateEventArgs> FileReciveUpdate;
        public event EventHandler<string> FileReciveEnd;


        private string _currentRepositoryPath = String.Empty;
        private PackageBuilderState _state = PackageBuilderState.IDLE;
        private int _numberOfExpectedFiles = 0;
        private int _numberOfFilesReceived = 0;
        private string _fullPackagePath = String.Empty;
        private FileStream _currentFileStrem = null;
        private long _expectedFileSize = 0;
        private string _fileChecksum = String.Empty;

        private bool _verifyChecksum;

        public PackageBuilder(bool verifyChecksum)
        {
            _verifyChecksum = verifyChecksum;
        }

        ~PackageBuilder()
        {
            // should be null but just in case
            if (_currentFileStrem != null)
            {
                _currentFileStrem.Dispose();
            }
        }

        public void StartReceiving(string currentRepositoryPath)
        {
            _currentRepositoryPath = currentRepositoryPath;
            _state = PackageBuilderState.WAITING_FOR_HEADER;
        }

        public bool IsReceiving()
        {
            return _state != PackageBuilderState.IDLE;
        }

        public void OnFileHeaderReceive(object sender, XmlMessagePackageDataHeader message)
        {
            // verify current state
            if (_state != PackageBuilderState.WAITING_FOR_HEADER)
                throw new InvalidOperationException("OnFileHeaderReceive: Invalid internal state - " + _state.ToString());

            // do work
            _numberOfExpectedFiles = message.NumberOfFiles;
            _fullPackagePath = Path.Combine(_currentRepositoryPath, message.RepositoryName, message.PackageName, message.Version);

            if (!Directory.Exists(_fullPackagePath))
            {
                Directory.CreateDirectory(_fullPackagePath);
            }

            // goto next state
            _state = PackageBuilderState.WAITING_FOR_FILE_BEGIN;
        }

        public void OnFileBeginReceive(object sender, XmlMessagePackageFileBegin message)
        {
            // verify current state
            if (_state != PackageBuilderState.WAITING_FOR_FILE_BEGIN)
                throw new InvalidOperationException("OnFileHeaderReceive: Invalid internal state - " + _state.ToString());

            // open file stream  - must be disposed
            string filePath = Path.Combine(_fullPackagePath, message.Name);
            _currentFileStrem = new FileStream(filePath, FileMode.Create);

            _expectedFileSize = message.Size;
            _fileChecksum = message.Checksum;

            // notify observers
            OnFileReciveBegin();

            // goto next state
            _state = PackageBuilderState.RECEIVE_FILE_DATA;
        }

        public void OnFileDataReceive(object sender, PacketMessage message)
        {
            // TODO must be synchonized - file must be written in the right order - block to not allow next data to be written

            // verify current state
            if (_state != PackageBuilderState.RECEIVE_FILE_DATA)
                throw new InvalidOperationException("OnFileHeaderReceive: Invalid internal state - " + _state.ToString());

            if (_currentFileStrem == null)
                throw new InvalidOperationException("OnFileDataReceive: File stream is missing!");

            // add file data
            _currentFileStrem.Write(message.Data, 0, message.Data.Length);

            // notify observers
            OnFileReciveUpdate();
        }

        public void OnFileEndReceive(object sender, EventArgs args)
        {
            // verify current state
            if (_state != PackageBuilderState.RECEIVE_FILE_DATA)
                throw new InvalidOperationException("OnFileHeaderReceive: Invalid internal state - " + _state.ToString());

            if (_currentFileStrem == null)
                throw new InvalidOperationException("OnFileHeaderReceive: File stream is empty!");

            // notify observers
            OnFileReciveEnd();

            // close file and dispose stream
            string fileName = _currentFileStrem.Name;
            _currentFileStrem.Close();
            _currentFileStrem.Dispose();
            _currentFileStrem = null;

            // verify file
            if (_verifyChecksum)
            {
                string reveivedFileChecksum = ComputeFileChecksum(fileName);

                // verify if the check sum is the same as expected
                if (reveivedFileChecksum != _fileChecksum)
                {
                    throw new InvalidOperationException("OnFileHeaderReceive: Invalid checksum - computed(" +
                        reveivedFileChecksum + ") expected(" + _fileChecksum + ")");
                }
            }

            // reset rest of variables
            _expectedFileSize = 0;
            _fileChecksum = String.Empty;

            // check if we expect another file and go to the next state
            _numberOfFilesReceived++;

            if (_numberOfFilesReceived < _numberOfExpectedFiles)
            {
                _state = PackageBuilderState.WAITING_FOR_FILE_BEGIN;
            }
            else
            {
                _state = PackageBuilderState.IDLE;
            }
        }

        public void OnInvalidMessageReceive(object sender, PacketMessage message)
        {
            // stop reciving in case of error
            if (IsReceiving())
            {
                _state = PackageBuilderState.IDLE;
            }
        }

        private string ComputeFileChecksum(string fileName)
        {
            using (FileStream stream = File.OpenRead(fileName))
            {
                SHA256Managed sha = new SHA256Managed();
                byte[] checksum = sha.ComputeHash(stream);
                return BitConverter.ToString(checksum).Replace("-", String.Empty);
            }
        }

        protected virtual void OnFileReciveBegin()
        {
            if (_currentFileStrem == null)
                return;

            if (FileReciveBegin != null)
                FileReciveBegin(this, Path.GetFileName(_currentFileStrem.Name));
        }

        protected virtual void OnFileReciveUpdate()
        {
            if (_currentFileStrem == null)
                return;

            FileReciveUpdateEventArgs args = new FileReciveUpdateEventArgs()
            {
                FileName = Path.GetFileName(_currentFileStrem.Name),
                CurrentSize = _currentFileStrem.Length,
                ExpectedSize = _expectedFileSize
            };

            if (FileReciveUpdate != null)
                FileReciveUpdate(this, args);
        }

        protected virtual void OnFileReciveEnd()
        {
            if (_currentFileStrem == null)
                return;

            if (FileReciveEnd != null)
                FileReciveEnd(this, Path.GetFileName(_currentFileStrem.Name));
        }
    }
}
