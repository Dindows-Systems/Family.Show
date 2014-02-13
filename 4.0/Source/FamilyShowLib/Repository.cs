/*
 * A clone of the photo class which creates a serializable repository.
 * 
 * The fields contained in the repository are comparable to the GEDCOM 
 * format.
 * 
*/

using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Microsoft.FamilyShowLib
{
        /// <summary>
        /// Describes a repository
        /// </summary>
        [Serializable]
        public class Repository : INotifyPropertyChanged, IEquatable<Repository>
        {

        #region Fields and Constants

            private string id;
            private string repositoryName;
            private string repositoryAddress;

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

            public string RepositoryName
            {
                get { return repositoryName; }
                set
                {
                    if (repositoryName != value)
                    {
                        repositoryName = value;
                        OnPropertyChanged("RepositoryeName");
                    }
                }
            }

            public string RepositoryAddress
            {
                get { return repositoryAddress; }
                set
                {
                    if (repositoryAddress != value)
                    {
                        repositoryAddress = value;
                        OnPropertyChanged("RepositoryAddress");
                    }
                }
            }

            [XmlIgnore]
            public string RepositoryNameAndId
            {
                get { return id + " " + repositoryName; }
                set { }
            }

            #endregion

            #region Constructors

            /// <summary>
            /// Creates a new instance of a repository object.
            /// This parameterless constructor is also required for serialization.
            /// </summary>
            public Repository()
            {
                this.repositoryName = Properties.Resources.Unknown;
            }

            /// <summary>
            /// Creates a new instance of the repository class with the id, name and address of the repository.  
            /// The calling method must ensure that there are no duplicated ids.
            /// </summary>
            public Repository(string repositoryId, string repositoryName, string repositoryAddress) : this()
            {
                if (!string.IsNullOrEmpty(repositoryId))
                this.id = repositoryId;
                if (!string.IsNullOrEmpty(repositoryName))
                this.repositoryName = repositoryName;
                if (!string.IsNullOrEmpty(repositoryAddress))
                this.repositoryAddress = repositoryAddress;
            }

            #endregion

            #region IEquatable Members

            /// <summary>
            /// Determine equality between two repository classes
            /// </summary>
            public bool Equals(Repository other)
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
