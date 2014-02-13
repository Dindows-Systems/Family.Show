/*
 * The "People" class creates a family made up of persons, sources, and repositories.
 * 
 * A source/repository is a simple clone of the person object.
 * 
 * 
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Windows.Documents;
using System.Text;
using System.Reflection;
using System.Windows;

namespace Microsoft.FamilyShowLib
{
    /// <summary>
    /// Argument that is passed with the ContentChanged event. Contains the
    /// person that was added to the list. The person can be null.
    /// </summary>
    public class ContentChangedEventArgs : EventArgs
    {
        private Person newPerson;

        public Person NewPerson
        {
            get { return newPerson; }
        }

        public ContentChangedEventArgs(Person newPerson)
        {
            this.newPerson = newPerson;
        }

    }

    /// <summary>
    /// Argument that is passed with the ContentChanged event. Contains the
    /// source that was added to the list. The source can be null.
    /// </summary>
    public class SourceContentChangedEventArgs : EventArgs
    {
        private Source newSource;

        public Source NewSource
        {
            get { return newSource; }
        }

        public SourceContentChangedEventArgs(Source newSource)
        {
            this.newSource = newSource;
        }
    }

    /// <summary>
    /// Argument that is passed with the ContentChanged event. Contains the
    /// source that was added to the list. The repository can be null.
    /// </summary>
    public class RepositoryContentChangedEventArgs : EventArgs
    {
        private Repository newRepository;

        public Repository NewRepository
        {
            get { return newRepository; }
        }

        public RepositoryContentChangedEventArgs(Repository newRepository)
        {
            this.newRepository = newRepository;
        }
    }

    /// <summary>
    /// Contains the collection of person nodes and which person in the list is the currently
    /// selected person. This class exists mainly because of xml serialization limitations.
    /// Properties are not serialized in a class that is derived from a collection class 
    /// (as the PeopleCollection class is). Therefore the People collection is contained in 
    /// this class, along with other important properties that need to be serialized.
    /// </summary>
    [XmlRoot("Family")]
    [XmlInclude(typeof(ParentRelationship))]
    [XmlInclude(typeof(ChildRelationship))]
    [XmlInclude(typeof(SpouseRelationship))]
    [XmlInclude(typeof(SiblingRelationship))]

    public class People
    {
        #region fields and constants

        // The constants specific to this class
        private static class Const
        {
            public const string DataFileName = "default.familyx";
        }

        // Fields
        private PeopleCollection peopleCollection;
        private SourceCollection sourceCollection;
        private RepositoryCollection repositoryCollection;

        //Merge
        private PeopleCollection existingPeopleCollection;
        private PeopleCollection duplicatePeopleCollection;
        private PeopleCollection newPeopleCollection;

        // The current person's Id will be serialized instead of the current person object to avoid
        // circular references during Xml Serialization. When family data is loaded, the corresponding
        // person object will be assigned to the current property (please see app.xaml.cs).
        // The currentPersonId is set in the Save method of this class.
        private string currentPersonId;

        // Store the person's name with the Id to make the xml file more readable.
        // The currentPersonName is set in the Save method of this class.
        private string currentPersonName;

        // The fully qualified path and filename for the family file.
        private string fullyQualifiedFilename;

        // Version of the file. Used to handle previous file formats.
        private string fileVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Major.ToString();

        private string OPCContentFileName = "content.xml";

        private string dateSaved = string.Empty;
        private string dateCreated = DateTime.Now.ToString();

        #endregion

        #region Properties

        /// <summary>
        /// Collection of people.
        /// </summary>
        public PeopleCollection PeopleCollection
        {
            get { return peopleCollection; }
        }

        /// <summary>
        /// Collection of sources.
        /// </summary>
        public SourceCollection SourceCollection
        {
            get { return sourceCollection; }
        }

        /// <summary>
        /// Collection of repositories.
        /// </summary>
        public RepositoryCollection RepositoryCollection
        {
            get { return repositoryCollection; }
        }

        /// <summary>
        /// Collection of existing people used for merging.
        /// </summary>
        [XmlIgnore]
        public PeopleCollection ExistingPeopleCollection
        {
            get { return existingPeopleCollection; }
        }

        /// <summary>
        /// Collection of duplicate people used for merging.
        /// </summary>
        [XmlIgnore]
        public PeopleCollection DuplicatePeopleCollection
        {
            get { return duplicatePeopleCollection; }
        }

        /// <summary>
        /// Collection of new people used for merging.
        /// </summary>
        [XmlIgnore]
        public PeopleCollection NewPeopleCollection
        {
            get { return newPeopleCollection; }
        }


        /// <summary>
        /// Id of currently selected person.
        /// </summary>
        [XmlAttribute(AttributeName = "Current")]
        public string CurrentPersonId
        {
            get { return currentPersonId; }
            set { currentPersonId = value; }
        }

        // Name of current selected person (included for readability in xml file).
        [XmlAttribute(AttributeName = "CurrentName")]
        public string CurrentPersonName
        {
            get { return currentPersonName; }
            set { currentPersonName = value; }
        }

        // Version of the file.
        [XmlAttribute(AttributeName = "FileVersion")]
        public string Version
        {
            get { return fileVersion; }
            set { fileVersion = value; }
        }

        // LastSavedDate date of the file.
        [XmlAttribute(AttributeName = "FileLastSavedDate")]
        public string DateSaved
        {
            get { return dateSaved; }
            set { dateSaved = value; }
        }

        // LastSavedDate date of the file.
        [XmlAttribute(AttributeName = "FileCreated")]
        public string DateCreated
        {
            get { return dateCreated; }
            set { dateCreated = value; }
        }

        [XmlIgnore]
        public static string ApplicationFolderPath
        {
            get
            {
                return Path.Combine(
                  Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                  App.ApplicationFolderName);
            }
        }

        [XmlIgnore]
        public static string DefaultFullyQualifiedFilename
        {
            get
            {
                // Absolute path to the application folder
                string appLocation = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    App.ApplicationFolderName);

                // Create the directory if it doesn't exist
                if (!Directory.Exists(appLocation))
                    Directory.CreateDirectory(appLocation);

                return Path.Combine(appLocation, Const.DataFileName);
            }
        }

        /// <summary>
        /// Fully qualified filename (absolute pathname and filename) for the data file
        /// </summary>
        [XmlIgnore]
        public string FullyQualifiedFilename
        {
            get { return fullyQualifiedFilename; }

            set { fullyQualifiedFilename = value; }
        }

        #endregion

        public People()
        {
            peopleCollection = new PeopleCollection();
            sourceCollection = new SourceCollection();
            repositoryCollection = new RepositoryCollection();

            existingPeopleCollection = new PeopleCollection();
            duplicatePeopleCollection = new PeopleCollection();
            newPeopleCollection = new PeopleCollection();
        }

        #region Loading, saving and merging

        #region save methods

        /// <summary>
        /// Persist the current list of people to disk and remove all living people's 
        /// details depending on the privacy flag.
        /// </summary>
        public void Save(bool privacy)
        {
            // Return right away if nothing to save.
            if (this.PeopleCollection == null || this.PeopleCollection.Count == 0)
                return;

            if (privacy == true)
            {
                foreach (Person p in this.PeopleCollection)
                {
                    if (p.IsLiving)
                    {
                        #region null fields

                        p.FirstName = "Living";
                        p.Note = null;
                        p.Restriction = Restriction.None;

                        p.FirstName = null;
                        p.LastName = null;
                        p.Suffix = null;

                        p.Occupation = null;
                        p.OccupationCitation = null;
                        p.OccupationSource = null;
                        p.OccupationLink = null;
                        p.OccupationCitationNote = null;
                        p.OccupationCitationActualText = null;

                        p.Education = null;
                        p.EducationCitation = null;
                        p.EducationSource = null;
                        p.EducationLink = null;
                        p.EducationCitationNote = null;
                        p.EducationCitationActualText = null;

                        p.Religion = null;
                        p.ReligionCitation = null;
                        p.ReligionSource = null;
                        p.ReligionLink = null;
                        p.ReligionCitationNote = null;
                        p.ReligionCitationActualText = null;

                        p.BirthDate = null;
                        p.BirthDateDescriptor = null;
                        p.BirthPlace = null;
                        p.BirthCitation = null;
                        p.BirthSource = null;
                        p.BirthLink = null;
                        p.BirthCitationNote = null;
                        p.BirthCitationActualText = null;

                        p.DeathDate = null;
                        p.DeathDateDescriptor = null;
                        p.DeathPlace = null;
                        p.DeathCitation = null;
                        p.DeathSource = null;
                        p.DeathLink = null;
                        p.DeathCitationNote = null;
                        p.DeathCitationActualText = null;

                        p.CremationPlace = null;
                        p.CremationDate = null;
                        p.CremationDateDescriptor = null;
                        p.CremationCitation = null;
                        p.CremationSource = null;
                        p.CremationLink = null;
                        p.CremationCitationNote = null;
                        p.CremationCitationActualText = null;

                        p.BurialPlace = null;
                        p.BurialDate = null;
                        p.BurialDateDescriptor = null;
                        p.BurialCitation = null;
                        p.BurialSource = null;
                        p.BurialLink = null;
                        p.BurialCitationNote = null;
                        p.BurialCitationActualText = null;

                        #endregion

                        if (p.HasSpouse == true)
                        {
                            foreach (Relationship rel in p.Relationships)
                            {
                                if (rel.RelationshipType == RelationshipType.Spouse)
                                {
                                    SpouseRelationship spouseRel = rel as SpouseRelationship;
                                    if (spouseRel != null)
                                    {
                                        spouseRel.MarriageDate = null;
                                        spouseRel.MarriageDateDescriptor = null;
                                        spouseRel.MarriagePlace = null;

                                        spouseRel.MarriageCitation = null;
                                        spouseRel.MarriageSource = null;
                                        spouseRel.MarriageLink = null;
                                        spouseRel.MarriageCitationActualText = null;
                                        spouseRel.MarriageCitationNote = null;
       
                                        spouseRel.DivorceDate = null;
                                        spouseRel.DivorceDateDescriptor = null;

                                        spouseRel.DivorceCitation = null;
                                        spouseRel.DivorceSource = null;
                                        spouseRel.DivorceLink = null;
                                        spouseRel.DivorceCitationActualText = null;
                                        spouseRel.DivorceCitationNote = null;
                                        
                                    }
                                }
                            }
                        }

                        foreach (Photo existingPhoto in p.Photos)
                        {
                            existingPhoto.IsAvatar = false;
                        }

                        p.Avatar = string.Empty;

                        if (p.HasPhoto == true)
                        {
                            for (int i = 0; i < p.Photos.Count; i++)
                            {
                                p.Photos[i] = null;
                            }

                            p.OnPropertyChanged("HasPhoto");
                        }

                        if (p.HasAttachments == true)
                        {
                            for (int i = 0; i < p.Attachments.Count; i++)
                            {
                                p.Attachments[i] = null;
                            }

                            p.OnPropertyChanged("HasAttachments");
                        }

                        p.Note = null;
                        p.DeleteStory();

                    }
                }

                foreach (Person p1 in this.PeopleCollection)
                {
                    foreach (Relationship rel in p1.Relationships)
                    {
                        rel.PersonFullName = rel.RelationTo.FirstName + " " + rel.RelationTo.LastName;
                    }
                }
            }

            // Set the current person id and name before serializing
            this.CurrentPersonName = this.PeopleCollection.Current.Name;
            this.CurrentPersonId = this.PeopleCollection.Current.Id;
            this.Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Major.ToString();
            this.DateSaved = DateTime.Now.ToString();
           
            // Use the default path and filename if none was provided
            if (string.IsNullOrEmpty(this.FullyQualifiedFilename))
                this.FullyQualifiedFilename = People.DefaultFullyQualifiedFilename;

            // Setup temp folders for this family to be packaged into OPC later
            string tempFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                App.ApplicationFolderName);
            tempFolder = Path.Combine(tempFolder, App.AppDataFolderName);

            // Create the necessary directories
            Directory.CreateDirectory(tempFolder);

            // Create xml content file
            XmlSerializer xml = new XmlSerializer(typeof(People));
            using (Stream stream = new FileStream(Path.Combine(tempFolder, OPCContentFileName), FileMode.Create, FileAccess.Write, FileShare.None))
            {
                xml.Serialize(stream, this);
            }

            // save to file package
            OPCUtility.CreatePackage(FullyQualifiedFilename, tempFolder);

            this.PeopleCollection.IsDirty = false;

        }

        /// <summary>
        /// Saves the list of people to disk using the specified filename and path
        /// </summary>
        /// <param name="FQFileName">Fully qualified path and filename of family tree file to save</param>
        public void Save(string FQFileName)
        {
            this.fullyQualifiedFilename = FQFileName;
            Save(false);
        }

        /// <summary>
        /// Saves the list of people to disk using the specified filename, path and privacy option
        /// </summary>
        /// <param name="FQFileName">Fully qualified path and filename of family tree file to save</param>
        public void SavePrivacy(string FQFileName, bool privacy)
        {
            this.fullyQualifiedFilename = FQFileName;
            Save(privacy);
        }

        /// <summary>
        /// Saves people related directly to the current person to disk using the specified filename, path and privacy option
        /// </summary>
        /// <param name="FQFileName">Fully qualified path and filename of family tree file to save</param>
        public void SaveDirect(string FQFileName, bool privacy)
        {

            this.fullyQualifiedFilename = FQFileName;

            Person primaryPerson = this.PeopleCollection.Current;
            PeopleCollection keep = new PeopleCollection();
            PeopleCollection delete = new PeopleCollection();

            //add current person and all directly related to the keep people collection
            keep.Add(primaryPerson);
            foreach (Person parent in primaryPerson.Parents)
                keep.Add(parent);
            foreach (Person sibling in primaryPerson.Siblings)
                keep.Add(sibling);
            foreach (Person spouse in primaryPerson.Spouses)
                keep.Add(spouse);
            foreach (Person child in primaryPerson.Children)
                keep.Add(child);

            //remove all people who are not in the keep collection
            foreach (Person q in this.PeopleCollection)
            {
                if (!keep.Contains(q))
                    delete.Add(q);
            }

            //remove relationships of people who are in the keep collection
            foreach (Person q in delete)
            {
                //Remove the relationships
                foreach (Relationship relationship in q.Relationships)
                {
                    foreach (Relationship rel in relationship.RelationTo.Relationships)
                    {
                        if (rel.RelationTo.Equals(q))
                        {
                            relationship.RelationTo.Relationships.Remove(rel);
                            break;
                        }
                    }
                }
                //Then remove the person, their photos and stories
                this.PeopleCollection.Remove(q);
                //q.DeletePhotos();
                q.DeleteStory();

            }
            Save(privacy);

        }

        /// <summary>
        /// Saves current person to disk using the specified filename, path and privacy option
        /// </summary>
        /// <param name="FQFileName">Fully qualified path and filename of family tree file to save</param>
        public void SaveCurrent(string FQFileName, bool privacy)
        {

            this.fullyQualifiedFilename = FQFileName;

            Person primaryPerson = this.PeopleCollection.Current;
            PeopleCollection keep = new PeopleCollection();
            PeopleCollection delete = new PeopleCollection();

            //add current person to the keep people collection
            keep.Add(primaryPerson);

            //remove all people who are not the current person
            foreach (Person q in this.PeopleCollection)
            {
                if (!keep.Contains(q))
                    delete.Add(q);
            }

            //remove relationships of people who are not the current person
            foreach (Person q in delete)
            {
                //Remove the relationships
                foreach (Relationship relationship in q.Relationships)
                {
                    foreach (Relationship rel in relationship.RelationTo.Relationships)
                    {
                        if (rel.RelationTo.Equals(q))
                        {
                            relationship.RelationTo.Relationships.Remove(rel);
                            break;
                        }
                    }
                }
                //Then remove the person, their photos and stories
                this.PeopleCollection.Remove(q);
                //q.DeletePhotos();
                q.DeleteStory();

            }
            Save(privacy);

        }

        /// <summary>
        /// Export current person and given number of ancestor and descendant generations
        /// 1 ancestor generation is parents
        /// 2 ancestor generations is parents and grandparents and so on upto 5 generations
        /// 1 descendent generation is children
        /// 2 descendent generations is grandchildren and so on upto 5 generations
        /// If 0, export current person and their siblings and spouse
        /// In summary, this method exports what it visible in the default tree.
        /// </summary>
        public void SaveGenerations(string FQFileName, decimal ancestors, decimal descendants, bool privacy)
        {

            this.fullyQualifiedFilename = FQFileName;

            Person primaryPerson = this.PeopleCollection.Current;
            PeopleCollection keep = new PeopleCollection();
            PeopleCollection delete = new PeopleCollection();

            //add the current person, their spouses and siblings to the export people collection and then repeat for each specified generation

            keep.Add(primaryPerson);

            //0 generations
            foreach (Person sibling in primaryPerson.Siblings)
                keep.Add(sibling);

            foreach (Person spouse in primaryPerson.Spouses)
                keep.Add(spouse);

            foreach (Person previousSpouse in primaryPerson.PreviousSpouses)
                keep.Add(previousSpouse);


            #region ancestors

            //1 ancestor generation
            if (ancestors >= Convert.ToDecimal(1))
            {
                foreach (Person parent in primaryPerson.Parents)
                {
                    foreach (Person p in ancestorGenerations(parent))
                    {
                        if (!keep.Contains(p))
                            keep.Add(p);
                    }

                    //2 ancestor generations
                    if (ancestors >= Convert.ToDecimal(2))
                    {
                        foreach (Person grandparent in parent.Parents)
                        {

                            foreach (Person p in ancestorGenerations(grandparent))
                            {
                                if (!keep.Contains(p))
                                    keep.Add(p);
                            }

                            //3 ancestor generations
                            if (ancestors >= Convert.ToDecimal(3))
                            {
                                foreach (Person greatgrandparent in grandparent.Parents)
                                {

                                    foreach (Person p in ancestorGenerations(greatgrandparent))
                                    {
                                        if (!keep.Contains(p))
                                            keep.Add(p);
                                    }

                                    //4 ancestor generations
                                    if (ancestors >= Convert.ToDecimal(4))
                                    {
                                        foreach (Person greatgreatgrandparent in greatgrandparent.Parents)
                                        {

                                            foreach (Person p in ancestorGenerations(greatgreatgrandparent))
                                            {
                                                if (!keep.Contains(p))
                                                    keep.Add(p);
                                            }

                                            //5 ancestor generations
                                            if (ancestors >= Convert.ToDecimal(5))
                                            {
                                                foreach (Person greatgreatgreatgrandparent in greatgreatgrandparent.Parents)
                                                {

                                                    foreach (Person p in ancestorGenerations(greatgreatgreatgrandparent))
                                                    {
                                                        if (!keep.Contains(p))
                                                            keep.Add(p);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            #region descendents

            //1 descendant generation
            if (descendants >= Convert.ToDecimal(1))
            {
                foreach (Person child in primaryPerson.Children)
                {
                    foreach (Person p in descendentGenerations(child))
                    {
                        if (!keep.Contains(p))
                            keep.Add(p);
                    }

                    //2 descendant generations
                    if (descendants >= Convert.ToDecimal(2))
                    {
                        foreach (Person grandchild in child.Children)
                        {

                            foreach (Person p in descendentGenerations(grandchild))
                            {
                                if (!keep.Contains(p))
                                    keep.Add(p);
                            }

                            //3 descendent generations
                            if (descendants >= Convert.ToDecimal(3))
                            {
                                foreach (Person greatgrandchild in grandchild.Children)
                                {
                                    foreach (Person p in descendentGenerations(greatgrandchild))
                                    {
                                        if (!keep.Contains(p))
                                            keep.Add(p);
                                    }

                                    //4 descendent generations
                                    if (descendants >= Convert.ToDecimal(4))
                                    {
                                        foreach (Person greatgreatgrandchild in greatgrandchild.Children)
                                        {
                                            foreach (Person p in descendentGenerations(greatgreatgrandchild))
                                            {
                                                if (!keep.Contains(p))
                                                    keep.Add(p);
                                            }

                                            //5 descendent generations
                                            if (descendants >= Convert.ToDecimal(5))
                                            {
                                                foreach (Person greatgreatgreatgrandchild in greatgreatgrandchild.Children)
                                                {
                                                    foreach (Person p in descendentGenerations(greatgreatgreatgrandchild))
                                                    {
                                                        if (!keep.Contains(p))
                                                            keep.Add(p);
                                                    }

                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            //remove all people who are not in the keep collection
            foreach (Person q in this.PeopleCollection)
            {
                if (!keep.Contains(q))
                    delete.Add(q);
            }

            //remove relationships of people who are not in the keep collection
            foreach (Person q in delete)
            {
                //Remove the relationships
                foreach (Relationship relationship in q.Relationships)
                {
                    foreach (Relationship rel in relationship.RelationTo.Relationships)
                    {
                        if (rel.RelationTo.Equals(q))
                        {
                            relationship.RelationTo.Relationships.Remove(rel);
                            break;
                        }
                    }
                }
            }

            foreach (Person q in delete)
            {
                //Then remove the person, their photos and stories
                this.PeopleCollection.Remove(q);
                //q.DeletePhotos();
                q.DeleteStory();

            }

            Save(privacy);

        }

        #endregion

        #region load methods

        /// <summary>
        /// Load the list of people from disk using the Open Package Convention format
        /// Returns true if the file was loaded sucessfully.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public bool LoadOPC()
        {
           
            // Loading, clear existing nodes
            this.PeopleCollection.Clear();
            this.SourceCollection.Clear();
            this.RepositoryCollection.Clear();

            try
            {
                // Use the default path and filename if none were provided
                if (string.IsNullOrEmpty(this.FullyQualifiedFilename))
                    this.FullyQualifiedFilename = People.DefaultFullyQualifiedFilename;

                string tempFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    App.ApplicationFolderName);
                tempFolder = Path.Combine(tempFolder, App.AppDataFolderName + @"\");

                OPCUtility.ExtractPackage(FullyQualifiedFilename, tempFolder,true);

                XmlSerializer xml = new XmlSerializer(typeof(People));

                using (Stream stream = new FileStream(tempFolder + OPCContentFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    People pc = (People)xml.Deserialize(stream);
                    stream.Close();

                    foreach (Person person in pc.PeopleCollection)
                        this.PeopleCollection.Add(person);

                    // To avoid circular references when serializing family data to xml, only the person Id
                    // is seralized to express relationships. When family data is loaded, the correct
                    // person object is found using the person Id and assigned to the appropriate relationship.
                    foreach (Person p in this.PeopleCollection)
                    {

                        RelationshipCollection corruptRelationships = new RelationshipCollection();

                        foreach (Relationship r in p.Relationships)
                        {
                            // If relationships are null remove them.
                            if (this.PeopleCollection.Find(r.PersonId) != null)
                                r.RelationTo = this.PeopleCollection.Find(r.PersonId);
                            else
                                corruptRelationships.Add(r);
                        }

                        foreach (Relationship r in corruptRelationships)
                        {
                            p.Relationships.Remove(r);
                        }

                    }

                    foreach (Source source in pc.SourceCollection)
                        this.SourceCollection.Add(source);

                    foreach (Repository repository in pc.RepositoryCollection)
                        this.RepositoryCollection.Add(repository);

                    // Set the current person in the list
                    this.CurrentPersonId = pc.CurrentPersonId;
                    this.CurrentPersonName = pc.CurrentPersonName;
                    this.PeopleCollection.Current = this.PeopleCollection.Find(this.CurrentPersonId);
                    this.Version = pc.Version;
                    this.PeopleCollection.IsDirty = false;

                    var majorVersion = double.Parse(this.Version);
                    var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
                    // Prompt if old file major version has been opened.
                    if (string.IsNullOrEmpty(this.Version) || majorVersion < assemblyVersion.Major)
                    {
                        MessageBox.Show(Properties.Resources.CompatabilityMessage, Properties.Resources.Compatability, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                    }

                    return true;
                }
            }
            catch
            {
                // Could not load the file. Handle all exceptions
                // the same, ignore and continue.
                this.fullyQualifiedFilename = string.Empty;
                // Warn user of problem with file.
                return false;
            }
        }

        /// <summary>
        /// Load the list of people from an old .family file and convert to .familyx format.
        /// Returns true if the file was converted and loaded sucessfully.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public bool LoadVersion2()
        {

            // Loading, clear existing nodes
            this.PeopleCollection.Clear();
            this.SourceCollection.Clear();
            this.RepositoryCollection.Clear();

            try
            {
                // Use the default path and filename if none were provided
                if (string.IsNullOrEmpty(this.FullyQualifiedFilename))
                    this.FullyQualifiedFilename = People.DefaultFullyQualifiedFilename;

                XmlSerializer xml = new XmlSerializer(typeof(People));
                using (Stream stream = new FileStream(this.FullyQualifiedFilename, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    People pc = (People)xml.Deserialize(stream);
                    stream.Close();

                    foreach (Person person in pc.PeopleCollection)
                        this.PeopleCollection.Add(person);

                    // Setup temp folders for this family to be packaged into OPC later
                    string tempFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), App.ApplicationFolderName);
                    tempFolder = Path.Combine(tempFolder, Microsoft.FamilyShowLib.App.AppDataFolderName);

                    string photoFolder = Path.Combine(tempFolder, Photo.PhotosFolderName);
                    string storyFolder = Path.Combine(tempFolder, Story.StoriesFolderName);

                    People.RecreateDirectory(tempFolder);
                    People.RecreateDirectory(Path.Combine(tempFolder, Photo.PhotosFolderName));
                    People.RecreateDirectory(Path.Combine(tempFolder, Story.StoriesFolderName));
                    People.RecreateDirectory(Path.Combine(tempFolder, Attachment.AttachmentsFolderName));

                    // To avoid circular references when serializing family data to xml, only the person Id
                    // is seralized to express relationships. When family data is loaded, the correct
                    // person object is found using the person Id and assigned to the appropriate relationship.
                    foreach (Person p in this.PeopleCollection)
                    {
                        
                        RelationshipCollection corruptRelationships = new RelationshipCollection();

                        // If relationships are null remove them.
                        foreach (Relationship r in p.Relationships)
                        {
                            if (this.PeopleCollection.Find(r.PersonId) != null)
                                r.RelationTo = this.PeopleCollection.Find(r.PersonId);
                            else
                                corruptRelationships.Add(r);
                        }

                        foreach (Relationship r in corruptRelationships)
                        {
                            p.Relationships.Remove(r);
                        }

                    }

                    foreach (Person p in this.PeopleCollection)
                    {
                        string oldpId = p.Id;
                        p.Id = Guid.NewGuid().ToString();  // Make sure we are using GUIDs.

                        foreach (Relationship r1 in p.Relationships)
                        {
                            foreach (Relationship r2 in r1.RelationTo.Relationships)
                            {
                                if(oldpId == r2.PersonId)
                                    r2.PersonId = p.Id;
                            }
                        }

                        // Store the photos which are present on the users computer
                        PhotoCollection newPhotos = new PhotoCollection();

                        // Store the photos into temp directory to be packaged into OPC later
                        foreach (Photo photo in p.Photos)
                        {
                            string photoOldPath = Path.Combine(Path.Combine(
                                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                                App.ApplicationFolderName), photo.RelativePath);

                            if (File.Exists(photoOldPath) && App.IsPhotoFileSupported(photoOldPath))
                            {

                                string photoFileName = Path.GetFileName(photo.FullyQualifiedPath);
                                // Remove spaces from the filename.
                                photoFileName = App.ReplaceEncodedCharacters(photoFileName);
                                photo.RelativePath = App.ReplaceEncodedCharacters(photo.RelativePath);

                                string photoFile = Path.Combine(photoFolder, photoFileName);

                                File.Copy(photoOldPath, photoFile, true);

                                newPhotos.Add(photo);
                            }

                        }

                        // Clear the old Photo Collection
                        p.Photos.Clear();

                        // Recreate the Photo Collection
                        foreach (Photo newPhoto in newPhotos)
                        {
                            p.Photos.Add(newPhoto);
                            p.OnPropertyChanged("HasAvatar");
                            p.OnPropertyChanged("HasPhoto");
                        }

                        // Store the person's story into temp directory to be packaged into OPC later
                        if (p.Story != null)
                        {
                            if (p.Story.RelativePath != null)
                            {
                                string storyOldPath = Path.Combine(Path.Combine(
                                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                                    App.ApplicationFolderName), p.Story.RelativePath);
                                if (File.Exists(storyOldPath))
                                {

                                    string storyFileName = Path.GetFileName(p.Story.AbsolutePath);
                                    // Remove spaces from the filename.
                                    storyFileName = new StringBuilder(App.ReplaceEncodedCharacters(p.FullName + "(" + p.Id + ")")).Append(".rtf").ToString();
                                    p.Story.RelativePath = Path.Combine(Story.StoriesFolderName,storyFileName);

                                    string storyFile = Path.Combine(storyFolder, storyFileName);

                                    File.Copy(storyOldPath, storyFile, true);

                                    // Update the note
                                    // Convert rft to plain text
                                    // See http://msdn.microsoft.com/en-us/library/cc488002.aspx for details.

                                    System.Windows.Forms.RichTextBox rtBox = new System.Windows.Forms.RichTextBox();

                                    // Get the contents of the RTF file. Note that when it is
                                    // stored in the string, it is encoded as UTF-16.
                                    string s = File.ReadAllText(storyFile);

                                    // Convert the RTF to plain text.
                                    rtBox.Rtf = s;
                                    string plainText = rtBox.Text;

                                    p.Note = rtBox.Text;
                                    p.OnPropertyChanged("HasNote");


                                }
                                else
                                {
                                    p.Story.Delete();
                                    p.OnPropertyChanged("Story");
                                }
                            }
                        }
                    }

                    // Set the current person in the list
                    this.CurrentPersonId = this.PeopleCollection[0].Id;
                    this.CurrentPersonName = this.PeopleCollection[0].FullName;
                    this.PeopleCollection.Current = this.PeopleCollection.Find(this.CurrentPersonId);
                    this.Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Major.ToString();
                    this.PeopleCollection.IsDirty = false;

                }

                this.PeopleCollection.IsDirty = false;
                    return true;
            }
            catch
            {
                // Could not load the file. Handle all exceptions
                // the same, ignore and continue.
                this.fullyQualifiedFilename = string.Empty;
                // Warn user of problem with file.
                return false;
            }
        }

        #endregion

        #region merge methods

        /// <summary>
        /// Merges people from another tree in to the current tree 
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public string[,] MergeOPC(string fileName)
        {

            // Two public people collections for the storing existing people and duplicate people.
            // They are paired  i.e. the ith person in each people collection forms a pair of people
            // who are thought to be the same person.

            this.ExistingPeopleCollection.Clear();
            this.DuplicatePeopleCollection.Clear();
            this.NewPeopleCollection.Clear();

            string[,] summary = new string[3, 1];  //string array for message information

            try
            {
                // Use the default path and filename if none were provided
                if (string.IsNullOrEmpty(fileName))
                    this.FullyQualifiedFilename = People.DefaultFullyQualifiedFilename;
                else
                    this.FullyQualifiedFilename = fileName;

                string tempFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    App.ApplicationFolderName);

                string mergelog = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    App.ApplicationFolderName);

                tempFolder = Path.Combine(tempFolder, App.AppDataFolderName + @"\");

                OPCUtility.ExtractPackage(FullyQualifiedFilename, tempFolder, false);

                Person reselectAfterMerge = this.PeopleCollection.Current;  //get the current person so they can be given focus after the merge

                XmlSerializer xml = new XmlSerializer(typeof(People));
                using (Stream stream = new FileStream(tempFolder + OPCContentFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    People merge = (People)xml.Deserialize(stream);  //add all the people, sources, and repositories from the new file to a people collection for comparison to take place
                    stream.Close();

                    //Get all the relationship information
                    foreach (Person p in merge.PeopleCollection)
                    {
                        foreach (Relationship r in p.Relationships)
                            r.RelationTo = merge.PeopleCollection.Find(r.PersonId);
                    }

                    People duplicates = new People();   //collection for duplicates
                    People imports = new People();      //collection for imports

                    //Update sources and repositories first, then people.

                    #region determine which sources to add

                    //Check two cases:
                    //1.
                    //Sources in the old file with the same name as sources in the new file are duplicates.
                    //In this case, the source Id in the new file needs to be updated with the Id from the old file.
                    //2.
                    //Sometimes different sources have the same Id.  
                    //In this case, update the Id in the new file to a new safe id.
                    //
                    //Any source not falling into the above cases should be added.

                    int y = merge.SourceCollection.Count;

                    //an array to store an old source Id and a new source Id.
                    //each y stores an old id in the 1st dimension and a new id in the second.
                    string[,] sourcesDuplicates = new string[2, y];    //for updating duplicated sources
                    string[,] sourcesDuplicateIds = new string[2, y];  //for updating sources which need new Ids.

                    //counters
                    int i = 0;
                    int w = 0;

                    int contains = 0;
                    int sameId = 0;

                    //collections for determining safeIDs
                    SourceCollection safeIds = new SourceCollection();
                    SourceCollection sameIds = new SourceCollection();

                    //add sources in the old file to the safe id collection
                    foreach (Source s in this.SourceCollection)
                        safeIds.Add(s);

     
                    //duplicates
                    foreach (Source source in merge.SourceCollection)  //for each new source... 
                    {
                        source.SourceName = source.SourceName.Trim();  //trailing spaces trick comparison

                        foreach (Source oldsource in this.SourceCollection) //...compare with the old source
                        {
                                oldsource.SourceName = oldsource.SourceName.Trim();  //trailing spaces trick comparison

                                if (source.Id == oldsource.Id && source.SourceName != oldsource.SourceName)  //a source with a different name but same Id is a dupliateId
                                {
                                    sameIds.Add(source);
                                    contains = 0;
                                    sameId = 1;
                                    
                                }

                                if (source.SourceName == oldsource.SourceName && source.Id != oldsource.Id)  //a source with the same name is a duplicate
                                {
                                    contains = 1;
                                    sourcesDuplicates[0, i] = source.Id;
                                    sourcesDuplicates[1, i] = oldsource.Id;
                                    i++;
                                    break;
                                  
                                }

                                if (source.SourceName == oldsource.SourceName && source.Id == oldsource.Id)  //a source with same name and Id is a duplicate but no action needs to be taken
                                {
                                    contains = 1;
                                    sameId = 1;
                                    break;
                                }
                            
                        }

                        if (contains == 0 && sameId == 0)
                        {
                            imports.SourceCollection.Add(source);
                            safeIds.Add(source);
                        }

                        contains = 0;
                        sameId = 0;
                    }

                    //duplicate ids
                    foreach (Source source in sameIds)
                    {

                        //even if a source was found with a different name but the same id, the original sources may still include that source

                        i = 0;

                        foreach (Source oldSource in this.SourceCollection)
                        {
                            if (oldSource.SourceName.Trim() == source.SourceName.Trim())
                                i++;
                        }

                        if (i == 0)
                        {
                            //note the old id
                            sourcesDuplicateIds[0, w] = source.Id;

                            //get a new safe id
                            source.Id = safeSourceId(safeIds);

                            //note the new id
                            sourcesDuplicateIds[1, w] = source.Id;

                            imports.SourceCollection.Add(source);
                            safeIds.Add(source);

                            w++;
                        }
                    }

                    #region correct source Ids in the merge people collection to reflect duplicated sources

                    foreach (Person p in merge.PeopleCollection)
                    {

                        //only ever replace an id once
                        bool replacedMarriage = false;
                        bool replacedDivorce = false;
                        bool replacedBirth = false;
                        bool replacedDeath = false;
                        bool replacedCremation = false;
                        bool replacedBurial = false;
                        bool replacedOccupation = false;
                        bool replacedEducation = false;
                        bool replacedReligion = false;

                        for (int z = 0; z < y; z++)
                        {

                            string s1 = sourcesDuplicates[0, z];
                            string s2 = sourcesDuplicates[1, z];

                            if (p.HasSpouse )
                            {
                                foreach (Relationship rel in p.Relationships)
                                {
                                    if (rel.RelationshipType == RelationshipType.Spouse)
                                    {
                                        SpouseRelationship spouseRel = (SpouseRelationship)rel;



                                        if (spouseRel.MarriageSource != null && !replacedMarriage)
                                        {
                                            spouseRel.MarriageSource = s2;
                                            replacedMarriage = true;
                                        }

                                        if (spouseRel.DivorceSource != null && !replacedDivorce)
                                        {
                                            spouseRel.DivorceSource = s2;
                                            replacedDivorce = true;
                                        }

                                    }
                                }

                            }

                            if (p.BirthSource == s1)
                            {
                                if (s2 != null && !replacedBirth)
                                {
                                    p.BirthSource = s2;
                                    replacedBirth = true;
                                }
                            }
                            if (p.DeathSource == s1)
                            {
                                if (s2 != null && !replacedDeath)
                                {
                                    p.DeathSource = s2;
                                    replacedDeath = true;
                                }
                            }
                            if (p.OccupationSource == s1)
                            {
                                if (s2 != null && !replacedOccupation)
                                {
                                    p.OccupationSource = s2;
                                    replacedOccupation = true;
                                }
                            }
                            if (p.EducationSource == s1)
                            {
                                if (s2 != null && !replacedEducation)
                                {
                                    p.EducationSource = s2;
                                    replacedEducation = true;
                                }
                            }
                            if (p.ReligionSource == s1)
                            {
                                if (s2 != null && !replacedReligion)
                                {
                                    p.ReligionSource = s2;
                                    replacedReligion = true;
                                }
                            }

                            if (p.CremationSource == s1)
                            {
                                if (s2 != null && !replacedCremation)
                                {
                                    p.CremationSource = s2;
                                    replacedCremation = true;
                                }
                            }

                            if (p.BurialSource == s1)
                            {
                                if (s2 != null && !replacedBurial)
                                {
                                    p.BurialSource = s2;
                                    replacedBurial = true;
                                }
                            }
                        }
                    }

                    #endregion

                    #region correct source Ids in the merge people collection to reflect sources with duplicate Ids.

                    foreach (Person p in merge.PeopleCollection)
                    {

                        //only ever replace an id once
                        bool replacedMarriage = false;
                        bool replacedDivorce = false;
                        bool replacedBirth = false;
                        bool replacedDeath = false;
                        bool replacedCremation = false;
                        bool replacedBurial = false;
                        bool replacedOccupation = false;
                        bool replacedEducation = false;
                        bool replacedReligion = false;

                        for (int z = 0; z < y; z++)
                        {
                            string s1 = sourcesDuplicateIds[0, z];
                            string s2 = sourcesDuplicateIds[1, z];

                            if (p.HasSpouse)
                            {
                                foreach (Relationship rel in p.Relationships)
                                {
                                    if (rel.RelationshipType == RelationshipType.Spouse)
                                    {
                                        SpouseRelationship spouseRel = (SpouseRelationship)rel;



                                        if (spouseRel.MarriageSource != null && !replacedMarriage)
                                        {
                                            spouseRel.MarriageSource = s2;
                                            replacedMarriage = true;
                                        }

                                        if (spouseRel.DivorceSource != null && !replacedDivorce)
                                        {
                                            spouseRel.DivorceSource = s2;
                                            replacedDivorce = true;
                                        }

                                    }
                                }

                            }

                            if (p.BirthSource == s1)
                            {
                                if (s2 != null && !replacedBirth)
                                {
                                    p.BirthSource = s2;
                                    replacedBirth = true;
                                }
                            }
                            if (p.DeathSource == s1)
                            {
                                if (s2 != null && !replacedDeath)
                                {
                                    p.DeathSource = s2;
                                    replacedDeath = true;
                                }
                            }
                            if (p.OccupationSource == s1)
                            {
                                if (s2 != null && !replacedOccupation)
                                {
                                    p.OccupationSource = s2;
                                    replacedOccupation = true;
                                }
                            }
                            if (p.EducationSource == s1)
                            {
                                if (s2 != null && !replacedEducation)
                                {
                                    p.EducationSource = s2;
                                    replacedEducation = true;
                                }
                            }
                            if (p.ReligionSource == s1)
                            {
                                if (s2 != null && !replacedReligion)
                                {
                                    p.ReligionSource = s2;
                                    replacedReligion = true;
                                }
                            }

                            if (p.CremationSource == s1)
                            {
                                if (s2 != null && !replacedCremation)
                                {
                                    p.CremationSource = s2;
                                    replacedCremation = true;
                                }
                            }

                            if (p.BurialSource == s1)
                            {
                                if (s2 != null && !replacedBurial)
                                {
                                    p.BurialSource = s2;
                                    replacedBurial = true;
                                }
                            }
                        }
                    }

                    #endregion

                    //write message information for the source import to an array
                    int duplicateS = merge.SourceCollection.Count - imports.SourceCollection.Count;

                    if (duplicateS > 0 && imports.SourceCollection.Count > 0)
                        summary[1, 0] = "\n\n" + Microsoft.FamilyShowLib.Properties.Resources.ImportedSources + " " + imports.SourceCollection.Count + "\n"
                             + Microsoft.FamilyShowLib.Properties.Resources.MergedSources + " " + duplicateS;
                    else if (imports.SourceCollection.Count > 0 && duplicateS == 0)
                        summary[1, 0] = "\n\n" + Microsoft.FamilyShowLib.Properties.Resources.All + " "+ imports.SourceCollection.Count + " " + Microsoft.FamilyShowLib.Properties.Resources.SourcesImported;
                    else if (imports.SourceCollection.Count == 0)
                        summary[1, 0] = "\n\n" + Microsoft.FamilyShowLib.Properties.Resources.NoSources;

                    #endregion

                    #region determine which repositories to add

                    //Check two cases:
                    //1.
                    //Repositories in the old file with the same name as repository in the new file are duplicates.
                    //In this case, the repository Id in the new file needs to be updated with the Id from the old file.
                    //2.
                    //Sometimes different repositories have the same Id.  
                    //In this case, update the Id in the new file to a new safe id.
                    //
                    //Any repository not falling into the above cases should be added.

                    y = merge.RepositoryCollection.Count;

                    //an array to store an old repository Id and a new repository Id.
                    //each y stores an old id in the 1st dimension and a new id in the second.
                    string[,] repositoriesDuplicates = new string[2, y];       //for updating duplicated repositories
                    string[,] repositoriesDuplicateIds = new string[2, y];     //for updating repositories which need new Ids.

                    //Counters

                    i = 0;
                    w = 0;

                    //collection for determining safeIDs

                    RepositoryCollection safeRIds = new RepositoryCollection();
                    RepositoryCollection sameRIds = new RepositoryCollection();

                    foreach (Repository r in this.RepositoryCollection)
                        safeRIds.Add(r);

                    //duplicates
                    foreach (Repository repository in merge.RepositoryCollection)  //for each new repository... 
                    {
                        repository.RepositoryName = repository.RepositoryName.Trim();  //trailing spaces trick comparison

                        foreach (Repository oldrepository in this.RepositoryCollection) //...compare with the old repository
                        {
                            oldrepository.RepositoryName = oldrepository.RepositoryName.Trim();  //trailing spaces trick comparison

                            if (repository.Id == oldrepository.Id && repository.RepositoryName != oldrepository.RepositoryName)  //a repository with a different name but same Id is a dupliateId
                            {
                                sameRIds.Add(repository);
                                contains = 0;
                                sameId = 1;
                            }

                            if (repository.RepositoryName == oldrepository.RepositoryName)  //a repository with the same name is a duplicate
                            {
                                contains = 1;
                                repositoriesDuplicates[0, i] = repository.Id;
                                repositoriesDuplicates[1, i] = oldrepository.Id;
                                i++;
                                break; 
                            }

                            if (repository.RepositoryName == oldrepository.RepositoryName && repository.Id == oldrepository.Id)  //a repository with same name and Id is a repository but no action needs to be taken
                            {
                                contains = 1;
                                break;  
                            }
                        }

                        if (contains == 0 && sameId == 0)
                        {
                            imports.RepositoryCollection.Add(repository);
                            safeRIds.Add(repository);
                        }

                        contains = 0;
                        sameId = 0;
                    }


                    //duplicate ids
                    foreach (Repository repository in sameRIds)
                    {

                        //even if a repository was found with a different name but the same id, the original repositories may still include that repository

                        i = 0;

                        foreach (Repository r in this.RepositoryCollection)
                        {
                            if (r.RepositoryName == repository.RepositoryName)
                                i++;
                        }

                        if (i == 0)
                        {
                            //note the old id
                            repositoriesDuplicateIds[0, w] = repository.Id;

                            //get a new safe id
                            repository.Id = safeRSourceId(safeRIds);

                            //note the new id
                            repositoriesDuplicateIds[1, w] = repository.Id;

                            imports.RepositoryCollection.Add(repository);
                            safeRIds.Add(repository);

                            w++;
                        }
                    }


                    //correct repositories in merge source collection to reflect the duplicated repository
                    foreach (Source s in merge.SourceCollection)
                    {
                        bool replaced = false;  //only replace an id once!

                        for (int z = 0; z < y; z++)
                        {
                            if (s.SourceRepository == repositoriesDuplicates[0, z])
                            {
                                if (repositoriesDuplicates[1, z] != null && replaced==false)
                                {
                                    s.SourceRepository = repositoriesDuplicates[1, z];
                                    replaced = true;
                                }
                            }
                        }
                    }

                    //correct the repositories in merge source collection to reflect duplicate repository ids.
                    foreach (Source s in merge.SourceCollection)
                    {

                        bool replaced = false;  //only replace an id once!

                        for (int z = 0; z < y; z++)
                        {
                            if (s.SourceRepository == repositoriesDuplicateIds[0, z])
                            {
                                if (repositoriesDuplicateIds[1, z] != null && replaced == false)
                                {
                                    s.SourceRepository = repositoriesDuplicateIds[1, z];
                                    replaced = true;
                                }
                            }
                        }
                    }

                    //write message information for the repository import to an array
                    int duplicateR = merge.RepositoryCollection.Count - imports.RepositoryCollection.Count;

                    if (duplicateR > 0 && imports.RepositoryCollection.Count > 0)
                        summary[2, 0] = "\n\n"+ Microsoft.FamilyShowLib.Properties.Resources.ImportedRepositories + " " + imports.RepositoryCollection.Count + "\n"
                            + Microsoft.FamilyShowLib.Properties.Resources.MergedRepositories + " " + duplicates.RepositoryCollection.Count;
                    else if (imports.RepositoryCollection.Count > 0 && duplicateR == 0)
                        summary[2, 0] = "\n\n" + Microsoft.FamilyShowLib.Properties.Resources.All + " " + imports.RepositoryCollection.Count + " " + Microsoft.FamilyShowLib.Properties.Resources.RepositoriesImported;
                    else if (imports.RepositoryCollection.Count == 0)
                        summary[2, 0] = "\n\n" + Microsoft.FamilyShowLib.Properties.Resources.NoRepositories;

                    #endregion

                    #region determine which people to add

                    foreach (Person person in merge.PeopleCollection)
                    {

                        string personName = RemovedMiddleNames(person);

                        bool duplicated = false;

                        #region name, date and id checks

                        foreach (Person oldperson in this.PeopleCollection)
                        {
                   
                                if (duplicated == false && oldperson.Gender == person.Gender)
                                {

                                    string oldPersonName = RemovedMiddleNames(oldperson);

                                    //don't add people if they have the same Id.  Ids must be unique.
                                    if (duplicated == false && person.Id == oldperson.Id)
                                    {
                                        duplicated = true;
                                        DuplicatePeopleCollection.Add(person);
                                        ExistingPeopleCollection.Add(oldperson);

                                    }

                                    //Don't add people if they have the same name and DeathDate. 
                                    //It is unlikely for people to have same name and DeathDate.  
                                    //Method will not handle twins in the case where the twins names' are "Unknown".
                                    if (duplicated == false && person.FullName != null && oldperson.FullName != null && person.DeathDate != null && oldperson.DeathDate != null)
                                    {
                                        if (person.FullName == oldperson.FullName && person.DeathDate == oldperson.DeathDate)
                                        {
                                            duplicated = true;
                                            DuplicatePeopleCollection.Add(person);
                                            ExistingPeopleCollection.Add(oldperson);
                                        }
                                    }

                                    //Don't add people if they have the same name and DeathDate. 
                                    //It is unlikely for people to have same name and DeathDate.  
                                    //Ignore middle names for this comparison, we have checked for full name matches.
                                    //Method will not handle twins in the case where the twins names' are "Unknown".
                                    if (duplicated == false && personName != null && oldPersonName != null && person.DeathDate != null && oldperson.DeathDate != null)
                                    {
                                        if (personName == oldPersonName && person.DeathDate == oldperson.DeathDate)
                                        {
                                            duplicated = true;
                                            DuplicatePeopleCollection.Add(person);
                                            ExistingPeopleCollection.Add(oldperson);
                                        }
                                    }

                                    //Don't add people if they have the same name and BirthDate. 
                                    //It is unlikely for people to have same name and Birthdate.  
                                    //Don't ignore middle names for this comparison.
                                    //Method will not handle twins in the case where the twins names' are "Unknown".
                                    if (duplicated == false && person.FullName != null && oldperson.FullName != null && person.BirthDate != null && oldperson.BirthDate != null)
                                    {
                                        if (person.FullName == oldperson.FullName && person.BirthDate == oldperson.BirthDate)
                                        {
                                            duplicated = true;
                                            DuplicatePeopleCollection.Add(person);
                                            ExistingPeopleCollection.Add(oldperson);
                                        }
                                    }

                                    //Don't add people if they have the same name and BirthDate. 
                                    //It is unlikely for people to have same name and Birthdate.  
                                    //Ignore middle names for this comparison, we have checked for full name matches.
                                    //Method will not handle twins in the case where the twins names' are "Unknown".
                                    if (duplicated == false && personName != null && oldPersonName != null && person.BirthDate != null && oldperson.BirthDate != null)
                                    {
                                        if (personName == oldPersonName && person.BirthDate == oldperson.BirthDate)
                                        {
                                            duplicated = true;
                                            DuplicatePeopleCollection.Add(person);
                                            ExistingPeopleCollection.Add(oldperson);
                                        }
                                    }

                                    //If person does not have a birth date...
                                    //Don't add a person with with no relatives if an existing person with the same name exists with no relatives.
                                    if (duplicated == false && person.BirthDate == null && oldperson.BirthDate == null)
                                    {

                                        if (person.FullName == oldperson.FullName && person.Relationships.Count == 0 && oldperson.Relationships.Count == 0)
                                        {
                                            duplicated = true;
                                            DuplicatePeopleCollection.Add(person);
                                            ExistingPeopleCollection.Add(oldperson);
                                        }

                                    }
                                }
                            
                        }

                        #endregion

                        #region relationship checks

                        foreach (Person oldperson in this.PeopleCollection)
                        {
                            //Compare full name with relationships
                            if (person.Name == oldperson.Name && duplicated == false && oldperson.Gender == person.Gender)
                            {
                                if (duplicated == false && person.Siblings.Count > 0 && oldperson.Siblings.Count > 0 && person.Siblings.Count == oldperson.Siblings.Count)
                                {
                                    if (CompareStrings(oldperson.SiblingsText, person.SiblingsText) && CompareStrings(oldperson.ParentsText, person.ParentsText))
                                        duplicated = true;
                                }

                                if (duplicated == false && person.Siblings.Count == 0 && oldperson.Siblings.Count == 0 && person.Siblings.Count == oldperson.Siblings.Count)
                                {
                                    if (CompareStrings(oldperson.ParentsText, person.ParentsText))
                                        duplicated = true;
                                }

                                if (duplicated == false && person.Spouses.Count > 0 && oldperson.Spouses.Count > 0 && person.Spouses.Count == oldperson.Spouses.Count)
                                {
                                    if (CompareStrings(oldperson.SpousesText, person.SpousesText))
                                        duplicated = true;
                                }

                                if (duplicated == true && !DuplicatePeopleCollection.Contains(person) && !ExistingPeopleCollection.Contains(oldperson))
                                {
                                    ExistingPeopleCollection.Add(oldperson);
                                    DuplicatePeopleCollection.Add(person);
                                }

                            }
                            //Compare surname and birth date with relationships
                            if (person.LastName == oldperson.LastName && person.BirthDate > DateTime.MinValue && oldperson.BirthDate > DateTime.MinValue  && duplicated == false && oldperson.Gender == person.Gender)
                            {
                                if (person.BirthDate == oldperson.BirthDate)
                                {

                                    if (duplicated == false && person.Siblings.Count > 0 && oldperson.Siblings.Count > 0 && person.Siblings.Count == oldperson.Siblings.Count)
                                    {
                                        if (CompareStrings(oldperson.SiblingsText, person.SiblingsText) && CompareStrings(oldperson.ParentsText, person.ParentsText))
                                            duplicated = true;
                                    }

                                    if (duplicated == false && person.Siblings.Count == 0 && oldperson.Siblings.Count == 0 && person.Siblings.Count == oldperson.Siblings.Count)
                                    {
                                        if (CompareStrings(oldperson.ParentsText, person.ParentsText))
                                            duplicated = true;
                                    }

                                    if (duplicated == false && person.Spouses.Count > 0 && oldperson.Spouses.Count > 0 && person.Spouses.Count == oldperson.Spouses.Count)
                                    {
                                        if (CompareStrings(oldperson.SpousesText, person.SpousesText))
                                            duplicated = true;
                                    }

                                    if (duplicated == true && !DuplicatePeopleCollection.Contains(person) && !ExistingPeopleCollection.Contains(oldperson))
                                    {
                                        ExistingPeopleCollection.Add(oldperson);
                                        DuplicatePeopleCollection.Add(person);
                                    }
                                }
                            }

                            //Compare surname and death date with relationships
                            if (person.LastName == oldperson.LastName && person.DeathDate > DateTime.MinValue && oldperson.DeathDate > DateTime.MinValue && duplicated == false && oldperson.Gender == person.Gender)
                            {
                                if (person.DeathDate == oldperson.DeathDate)
                                {

                                    if (duplicated == false && person.Siblings.Count > 0 && oldperson.Siblings.Count > 0 && person.Siblings.Count == oldperson.Siblings.Count)
                                    {
                                        if (CompareStrings(oldperson.SiblingsText, person.SiblingsText) && CompareStrings(oldperson.ParentsText, person.ParentsText))
                                            duplicated = true;
                                    }

                                    if (duplicated == false && person.Siblings.Count == 0 && oldperson.Siblings.Count == 0 && person.Siblings.Count == oldperson.Siblings.Count)
                                    {
                                        if (CompareStrings(oldperson.ParentsText, person.ParentsText))
                                            duplicated = true;
                                    }

                                    if (duplicated == false && person.Spouses.Count > 0 && oldperson.Spouses.Count > 0 && person.Spouses.Count == oldperson.Spouses.Count)
                                    {
                                        if (CompareStrings(oldperson.SpousesText, person.SpousesText))
                                            duplicated = true;
                                    }

                                    if (duplicated == true && !DuplicatePeopleCollection.Contains(person) && !ExistingPeopleCollection.Contains(oldperson))
                                    {
                                        ExistingPeopleCollection.Add(oldperson);
                                        DuplicatePeopleCollection.Add(person);
                                    }
                                }
                            }

                        }

                        #endregion

                        if (duplicated == false)
                        {
                            imports.PeopleCollection.Add(person);
                            newPeopleCollection.Add(person);
                            
                        }
                        else
                            duplicates.PeopleCollection.Add(person);
                    }

                    
                    if (duplicates.PeopleCollection.Count > 0)
                        summary[0, 0] = Microsoft.FamilyShowLib.Properties.Resources.ImportedPeople + " " + imports.PeopleCollection.Count + "\n" + Microsoft.FamilyShowLib.Properties.Resources.DuplicatePeople + " " + duplicates.PeopleCollection.Count;
                    else
                        summary[0, 0] = Microsoft.FamilyShowLib.Properties.Resources.All + " " + imports.PeopleCollection.Count + " " + Microsoft.FamilyShowLib.Properties.Resources.PeopleImported;

                    #endregion

                    //now add imports to the source collection...
                    foreach (Source source in imports.SourceCollection)
                        this.SourceCollection.Add(source);

                    //now add repositories to the repositories collection...
                    foreach (Repository r in imports.RepositoryCollection)
                        this.RepositoryCollection.Add(r);

                    //Remove the duplicate people from merge
                    foreach (Person duplicate in duplicates.PeopleCollection)
                    {

                        //Remove the relationships involving duplicate people
                        foreach (Relationship relationship in duplicate.Relationships)
                        {
                            foreach (Relationship rel in relationship.RelationTo.Relationships)
                            {
                                if (rel.RelationTo.Equals(duplicate))
                                {
                                    relationship.RelationTo.Relationships.Remove(rel);
                                    break;
                                }
                            }
                        }

                        merge.PeopleCollection.Remove(duplicate);
                    }

                    //Now add non duplicate people to main people collection...
                    foreach (Person p in merge.PeopleCollection)
                        this.PeopleCollection.Add(p);
                }

                //give focus to the person who was selected before the merge
                this.CurrentPersonId = reselectAfterMerge.Id;
                this.CurrentPersonName = reselectAfterMerge.Name;
                this.PeopleCollection.Current = reselectAfterMerge;

                this.PeopleCollection.IsDirty = false;
            }
            catch
            {
                // Merge failed.  Pass summary back to main window as a null on failed load so the user can be prompted.
                summary = null;

            }
            return summary;
        }

        #endregion

        #region helper methods

        /// <summary>
        /// Delete to clear existing files and re-Create the necessary directories
        /// </summary>
        public static void RecreateDirectory(string folderToDelete)
        {
            try
            {
                if (Directory.Exists(folderToDelete))
                    Directory.Delete(folderToDelete, true);
                Directory.CreateDirectory(folderToDelete);
            }
            catch
            {
                // ignore deletion errors
            }
        }

        /// <summary>
        /// Gets a persons name in Firstname + Lastname format taking a person as an argument
        /// </summary>
        private static string RemovedMiddleNames(Person p)
        {

            string surname = string.Empty;
            string firstName = string.Empty;

            if (!string.IsNullOrEmpty(p.LastName))
                surname = p.LastName;

            string[] splitName = p.FirstName.Split(' ');

            if (splitName != null)
            {
                firstName = splitName[0];

                if (firstName != surname)
                    return firstName + " " + surname;
                else
                    return Microsoft.FamilyShowLib.Properties.Resources.Unknown + " " + surname;
            }
            else
                return string.Empty;

        }

        private static bool CompareStrings(string oldString, string newString)
        {
            oldString = oldString.Replace(",", "");
            oldString = oldString.Replace(" and ", " ");
            string[] string1 = oldString.Split();

            newString = newString.Replace(",", "");
            newString = newString.Replace(" and ", " ");
            string[] string2 = newString.Split();

            List<string> list1 = new List<string>();
            List<string> list2 = new List<string>();

            foreach (string s in string1)
            {
                list1.Add(s);
                list1.Sort();
            }

            foreach (string s in string2)
            {
                list2.Add(s);
                list2.Sort();     
            }

            if (list2.ToString() == list1.ToString())
                return true;
            else
                return false;

        }


        /// <summary>
        /// Gets a safe source id in the form S#.
        /// </summary>
        private static string safeSourceId(SourceCollection source)
        {
            int y = 0;

            string oldSourceIDs = string.Empty;

            foreach (Source s in source)
                oldSourceIDs += s.Id + "E";

            do
            {
                y++;
            }
            while (oldSourceIDs.Contains("S" + y.ToString() + "E"));

            return "S" + y.ToString();
        }

        /// <summary>
        /// Gets a safe repository id in the form R#.
        /// </summary>
        private static string safeRSourceId(RepositoryCollection repository)
        {
            int y = 0;

            string oldRIDs = string.Empty;

            foreach (Repository r in repository)
                oldRIDs += r.Id + "E";

            do
            {
                y++;
            }
            while (oldRIDs.Contains("R" + y.ToString() + "E"));

            return "R" + y.ToString();
        }

        /// <summary>
        /// Handles logic for decendent generation
        /// </summary>
        private static PeopleCollection descendentGenerations(Person child)
        {
            PeopleCollection pc = new PeopleCollection();

            if (!pc.Contains(child))
                pc.Add(child);

            foreach (Person p in getSpouses(child))
            {
                if (!pc.Contains(p))
                    pc.Add(p);
            }

            return pc;
        }

        /// <summary>
        /// Handles logic for ancestor generation
        /// </summary>
        private static PeopleCollection ancestorGenerations(Person parent)
        {
            PeopleCollection pc = new PeopleCollection();

            if (!pc.Contains(parent))
                pc.Add(parent);

            foreach (Person p in getSiblingsSpousesChildren(parent))
            {
                if (!pc.Contains(p))
                    pc.Add(p);
            }

            return pc;
        }

        /// <summary>
        /// Get the siblings, spouses, previous spouses and children of a parent
        /// </summary>
        private static PeopleCollection getSiblingsSpousesChildren(Person parent)
        {
            PeopleCollection pc = new PeopleCollection();

            foreach (Person sibling in parent.Siblings)
            {
                if (!pc.Contains(sibling))
                    pc.Add(sibling);
            }
            foreach (Person child in parent.Children)
            {
                if (!pc.Contains(child))
                    pc.Add(child);
            }

            foreach (Person spouse in parent.Spouses)
            {
                if (!pc.Contains(spouse))
                    pc.Add(spouse);
            }

            foreach (Person previousSpouse in parent.PreviousSpouses)
            {
                if (!pc.Contains(previousSpouse))
                    pc.Add(previousSpouse);
            }

            return pc;

        }

        /// <summary>
        /// Get all spouses of a child
        /// </summary>
        private static PeopleCollection getSpouses(Person child)
        {
            PeopleCollection pc = new PeopleCollection();

            foreach (Person spouse in child.Spouses)
            {
                if (!pc.Contains(spouse))
                    pc.Add(spouse);
            }
            foreach (Person previousSpouse in child.PreviousSpouses)
            {
                if (!pc.Contains(previousSpouse))
                    pc.Add(previousSpouse);
            }

            return pc;

        }

        #endregion

        #endregion
    }

    /// <summary>
    /// List of sources.
    /// </summary>
    [Serializable]
    public class SourceCollection : ObservableCollection<Source>, INotifyPropertyChanged
    {
        public SourceCollection() { }

        private Source current;
        private bool dirty;

        ///<summary>
        ///Source currently selected in application
        ///</summary>
        public Source Current
        {
            get { return current; }
            set
            {
                if (current != value)
                {
                    current = value;
                    OnPropertyChanged("Current");
                    OnCurrentChanged();
                }
            }
        }

        ///<summary>
        ///Get or set if the list has been modified.
        ///</summary>
        public bool IsDirty
        {
            get { return dirty; }
            set { dirty = value; }
        }

        public event EventHandler<SourceContentChangedEventArgs> ContentChanged;

        /// <summary>
        /// The details of a source changed.
        /// </summary>

        public void OnContentChanged()
        {
            dirty = true;
            if (ContentChanged != null)
                ContentChanged(this, new SourceContentChangedEventArgs(null));
        }

        /// <summary>
        /// The details of a source changed, and a new source was added to the collection.
        /// </summary>
        public void OnContentChanged(Source newSource)
        {
            dirty = true;
            if (ContentChanged != null)
                ContentChanged(this, new SourceContentChangedEventArgs(newSource));
        }

        /// <summary> 
        /// The primary source changed in the list.
        /// </summary>
        public event EventHandler CurrentChanged;
        protected void OnCurrentChanged()
        {
            if (CurrentChanged != null)
                CurrentChanged(this, EventArgs.Empty);
        }

        #region Add new source

        /// <summary>
        /// Adds source
        /// </summary>
        public void AddSource(Source source)
        {
            //add the source to the main source list
            if (!this.Contains(source))
                this.Add(source);
        }

        #endregion

        public Source Find(string id)
        {
            foreach (Source source in this)
            {
                if (source.Id == id)
                    return source;
            }

            return null;
        }

        #region INotifyPropertyChanged Members

        protected override event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

    }

    /// <summary>
    /// List of repositories.
    /// </summary>
    [Serializable]
    public class RepositoryCollection : ObservableCollection<Repository>, INotifyPropertyChanged
    {
        public RepositoryCollection() { }

        private Repository current;
        private bool dirty;

        /// <summary>
        /// Repository currently selected in application
        /// </summary>
        public Repository Current
        {
            get { return current; }
            set
            {
                if (current != value)
                {
                    current = value;
                    OnPropertyChanged("Current");
                    OnCurrentChanged();
                }
            }
        }

        /// <summary>
        /// Get or set if the list has been modified.
        /// </summary>
        public bool IsDirty
        {
            get { return dirty; }
            set { dirty = value; }
        }

        public event EventHandler<RepositoryContentChangedEventArgs> ContentChanged;

        /// <summary>
        /// The details of a repository changed.
        /// </summary>

        public void OnContentChanged()
        {
            dirty = true;
            if (ContentChanged != null)
                ContentChanged(this, new RepositoryContentChangedEventArgs(null));
        }

        /// <summary>
        /// The details of a repository changed, and a new repository was added to the collection.
        /// </summary>
        public void OnContentChanged(Repository newRepository)
        {
            dirty = true;
            if (ContentChanged != null)
                ContentChanged(this, new RepositoryContentChangedEventArgs(newRepository));
        }

        /// <summary> 
        /// The primary repository changed in the list.
        /// </summary>
        public event EventHandler CurrentChanged;
        protected void OnCurrentChanged()
        {
            if (CurrentChanged != null)
                CurrentChanged(this, EventArgs.Empty);
        }

        #region Add new repository

        /// <summary>
        /// Adds repository
        /// </summary>
        public void AddRepository(Repository repository)
        {
            //add the repository to the main repository list
            if (!this.Contains(repository))
                this.Add(repository);
        }

        #endregion

        public Repository Find(string id)
        {
            foreach (Repository repository in this)
            {
                if (repository.Id == id)
                    return repository;
            }

            return null;
        }

        #region INotifyPropertyChanged Members

        protected override event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

    }

    /// <summary>
    /// List of people.
    /// </summary>
    [Serializable]
    public class PeopleCollection : ObservableCollection<Person>, INotifyPropertyChanged
    {
        public PeopleCollection() { }

        private Person current;
        private bool dirty;

        /// <summary>
        /// Person currently selected in application
        /// </summary>
        public Person Current
        {
            get { return current; }
            set
            {
                if (current != value)
                {
                    current = value;
                    OnPropertyChanged("Current");
                    OnCurrentChanged();
                }
            }
        }

        /// <summary>
        /// Get or set if the list has been modified.
        /// </summary>
        public bool IsDirty
        {
            get { return dirty; }
            set { dirty = value; }
        }

        /// <summary>
        /// A person or relationship was added, removed or modified in the list. This is used
        /// instead of CollectionChanged since CollectionChanged can be raised before the 
        /// relationships are setup (the Person was added to the list, but its Parents, Children,
        /// Sibling and Spouse collections have not been established). This means the subscriber 
        /// (the diagram control) will update before all of the information is available and 
        /// relationships will not be displayed.
        /// 
        /// The ContentChanged event addresses this problem and allows the flexibility to
        /// raise the event after *all* people have been added to the list, and *all* of
        /// their relationships have been established. 
        /// 
        /// Objects that add or remove people from the list, or add or remove relationships
        /// should call OnContentChanged when they want to notify subscribers that all
        /// changes have been made.
        /// </summary>
        public event EventHandler<ContentChangedEventArgs> ContentChanged;

        /// <summary>
        /// The details of a person changed.
        /// </summary>

        public void OnContentChanged()
        {
            dirty = true;
            if (ContentChanged != null)
                ContentChanged(this, new ContentChangedEventArgs(null));
        }

        /// <summary>
        /// The details of a person changed, and a new person was added to the collection.
        /// </summary>
        public void OnContentChanged(Person newPerson)
        {
            dirty = true;
            if (ContentChanged != null)
                ContentChanged(this, new ContentChangedEventArgs(newPerson));
        }

        /// <summary> 
        /// The primary person changed in the list.
        /// </summary>
        public event EventHandler CurrentChanged;
        protected void OnCurrentChanged()
        {
            if (CurrentChanged != null)
                CurrentChanged(this, EventArgs.Empty);
        }

        #region Add new people / relationships

        /// <summary>
        /// Adds Parent-Child relationship between person and child with the provided parent-child relationship type.
        /// </summary>
        public void AddChild(Person person, Person child, ParentChildModifier parentChildType)
        {
            //add child relationship to person
            person.Relationships.Add(new ChildRelationship(child, parentChildType));

            //add person as parent of child
            child.Relationships.Add(new ParentRelationship(person, parentChildType));

            //add the child to the main people list
            if (!this.Contains(child))
                this.Add(child);
        }

        /// <summary>
        /// Add Spouse relationship between the person and the spouse with the provided spouse relationship type.
        /// </summary>
        public void AddSpouse(Person person, Person spouse, SpouseModifier spouseType)
        {
            //assign spouses to each other    
            person.Relationships.Add(new SpouseRelationship(spouse, spouseType));
            spouse.Relationships.Add(new SpouseRelationship(person, spouseType));

            //add the spouse to the main people list
            if (!this.Contains(spouse))
                this.Add(spouse);
        }

        /// <summary>
        /// Adds sibling relation between the person and the sibling
        /// </summary>
        public void AddSibling(Person person, Person sibling)
        {
            //assign sibling to each other    
            person.Relationships.Add(new SiblingRelationship(sibling));
            sibling.Relationships.Add(new SiblingRelationship(person));

            //add the sibling to the main people list
            if (!this.Contains(sibling))
                this.Add(sibling);
        }

        #endregion

        public Person Find(string id)
        {
            foreach (Person person in this)
            {
                if (person.Id == id)
                    return person;
            }

            return null;
        }
        /// <summary>
        /// Gets the next person in the people list.  
        /// Returns null if the current person is the last person in the list.
        /// </summary>
        public Person Next(int i)
        {
            Person p = null;

                foreach (Person person in this)
                {
                    if (this.IndexOf(person) == i+1)
                    {
                        return person;
                    }
                }

            return p;
        }

        /// <summary>
        /// Gets the previous person in the people list.  
        /// Returns null if the current person is the first person in the list.
        /// </summary>
        public Person Previous(int i)
        {
            Person p = null;

                foreach (Person person in this)
                {
                    if (this.IndexOf(person) == i)
                    {
                        return person;
                    }
                }

            return p;
        }

        #region INotifyPropertyChanged Members

        protected override event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {

            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

}
