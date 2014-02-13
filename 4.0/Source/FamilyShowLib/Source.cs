/*
 * A clone of the photo class which creates a serializable source.
 * 
 * The fields contained in the source are comparable to the GEDCOM 
 * format. Not all fields are currently used.
 * 
*/

using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Microsoft.FamilyShowLib
{
        /// <summary>
        /// Describes a source
        /// </summary>
        [Serializable]
        public class Source : INotifyPropertyChanged, IEquatable<Source>
        {

        #region Fields and Constants

            private string id;
            private string sourceName;
            private string sourceAuthor;
            private string sourcePublisher;
            private string sourceNote;
            private string sourceRepository;

        #endregion

        #region Properties

            [XmlAttribute]
            public string Id
            {
                get { return id; }
                set
                {
                    if (id != value)
                    {
                        id = value;
                        OnPropertyChanged("Id");
                    }
                }
            }

            public string SourceName
            {
                get { return sourceName; }
                set
                {
                    if (sourceName != value)
                    {
                        sourceName = value;
                        OnPropertyChanged("SourceName");
                    }
                }
            }

            public string SourceAuthor
            {
                get { return sourceAuthor; }
                set
                {
                    if (sourceAuthor != value)
                    {
                        sourceAuthor = value;
                        OnPropertyChanged("SourceAuthor");
                    }
                }
            }

            public string SourcePublisher
            {
                get { return sourcePublisher; }
                set
                {
                    if (sourcePublisher != value)
                    {
                        sourcePublisher = value;
                        OnPropertyChanged("SourcePublisher");
                    }
                }
            }

            public string SourceNote
            {
                get { return sourceNote; }
                set
                {
                    if (sourceNote != value)
                    {
                        sourceNote = value;
                        OnPropertyChanged("SourceNote");
                    }
                }
            }

            public string SourceRepository
            {
                get { return sourceRepository; }
                set
                {
                    if (sourceRepository != value)
                    {
                        sourceRepository = value;
                        OnPropertyChanged("SourceRepository");
                    }
                }
            }

            [XmlIgnore]
            public string SourceNameAndId
            {
                get { return id + " " + sourceName; }
                set { }
            }

            #endregion

            #region Constructors

            /// <summary>
            /// Creates a new instance of a sourceobject.
            /// This parameterless constructor is also required for serialization.
            /// </summary>
            public Source()
            {
                this.sourceName = Properties.Resources.Unknown;
            }

            /// <summary>
            /// Creates a new instance of the source class with the id, name, author, publisher, note and repository of the source.  
            /// The calling method must ensure that there are no duplicated ids.
            /// </summary>
            public Source(string sourceId, string sourceName, string sourceAuthor, string sourcePublisher, string sourceNote, string sourceRepository)
                : this()
            {
                if (!string.IsNullOrEmpty(sourceId))
                    this.id = sourceId;
                if (!string.IsNullOrEmpty(sourceName))
                    this.sourceName = sourceName;
                if (!string.IsNullOrEmpty(sourceAuthor))
                    this.sourceAuthor = sourceAuthor;
                if (!string.IsNullOrEmpty(sourcePublisher))
                    this.sourcePublisher = sourcePublisher;
                if (!string.IsNullOrEmpty(sourceNote))
                    this.sourceNote = sourceNote;
                if (!string.IsNullOrEmpty(sourceRepository))
                    this.sourceRepository = sourceRepository;
            }

            #endregion

            #region IEquatable Members

            /// <summary>
            /// Determine equality between two source classes
            /// </summary>
            public bool Equals(Source other)
            {
                return (this.Id == other.Id);
            }

            #endregion

            #region INotifyPropertyChanged Members

            /// <summary>
            /// INotifyPropertyChanged requires a property called PropertyChanged.
            /// </summary>
            public event PropertyChangedEventHandler PropertyChanged;

            /// <summary>
            /// Fires the event for the property when it changes.
            /// </summary>
            public virtual void OnPropertyChanged(string propertyName)
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }

            #endregion

        }
 
}
