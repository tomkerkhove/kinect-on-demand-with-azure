
using System;

namespace K4W.KinectVOD.Shared
{
    public class RecordingData
    {
        /// <summary>
        /// Video URL
        /// </summary>
        private string _recordingId = string.Empty;
        public string RecordingId
        {
            get { return _recordingId; }
            set { _recordingId = value; }
        }

        /// <summary>
        /// Caption
        /// </summary>
        private string _caption = string.Empty;
        public string Caption
        {
            get { return _caption; }
            set { _caption = value; }
        }

        /// <summary>
        /// Video URL
        /// </summary>
        private string _smoothStreamUrl = string.Empty;
        public string SmoothStreamUrl
        {
            get { return _smoothStreamUrl; }
            set { _smoothStreamUrl = value; }
        }

        /// <summary>
        /// Timestamp of the recording
        /// </summary>
        private DateTime _recordingStamp = default(DateTime);
        public DateTime RecordingStamp
        {
            get { return _recordingStamp; }
            set { _recordingStamp = value; }
        }


        /// <summary>
        /// Default CTOR
        /// </summary>
        public RecordingData() { }

        /// <summary>
        /// Extended CTOR
        /// </summary>
        /// <param name="caption">Caption</param>
        /// <param name="smoothStreamUrl">Url to the video stream</param>
        public RecordingData(string caption, string smoothStreamUrl, string recordingId, DateTime recordingStamp)
        {
            _caption = caption;
            _smoothStreamUrl = smoothStreamUrl;
            _recordingId = recordingId;
            _recordingStamp = recordingStamp;
        }

        /// <summary>
        /// Override ToString()
        /// </summary>
        public override string ToString()
        {
            return _caption;
        }

        /// <summary>
        /// Override Equals()
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof (RecordingData)) return false;

            return ((obj as RecordingData).Caption == _caption);
        }
    }
}
