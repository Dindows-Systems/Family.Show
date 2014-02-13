using System;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using System.Resources;
using System.Reflection;
using System.Globalization;
using System.ComponentModel;


namespace Microsoft.FamilyShowLib
{

    #region Relationship classes

    /// <summary>
    /// Describes the kinship between person objects
    /// </summary>
    [Serializable]
    public abstract class Relationship
    {
        private RelationshipType relationshipType;

        private Person relationTo;

        // The person's Id will be serialized instead of the relationTo person object to avoid
        // circular references during Xml Serialization. When family data is loaded, the corresponding
        // person object will be assigned to the relationTo property (please see app.xaml.cs).
        private string personId;

        // Store the person's name with the Id to make the xml file more readable
        private string personFullName;

        /// <summary>
        /// The Type of relationship.  Parent, child, sibling, or spouse
        /// </summary>
        public RelationshipType RelationshipType
        {
            get { return relationshipType; }
            set { relationshipType = value; }
        }

        /// <summary>
        /// The person id the relationship is to. See comment on personId above.
        /// </summary>
        [XmlIgnore]
        public Person RelationTo
        {
            get { return relationTo; }
            set
            {
                relationTo = value;
                personId = ((Person)value).Id;
                personFullName = ((Person)value).Name;
            }
        }

        public string PersonId
        {
            get { return personId; }
            set { personId = value; }
        }

        public string PersonFullName
        {
            get { return personFullName; }
            set { personFullName = value; }
        }

    }

    /// <summary>
    /// Describes the kinship between a child and parent.
    /// </summary>
    [Serializable]
    public class ParentRelationship : Relationship
    {
        private ParentChildModifier parentChildModifier;
		
        public ParentChildModifier ParentChildModifier
        {
            get { return parentChildModifier; }
            set { parentChildModifier = value; }
        }

        // Paramaterless constructor required for XML serialization
        public ParentRelationship() { }

        public ParentRelationship(Person personId, ParentChildModifier parentChildType)
        {
            RelationshipType = RelationshipType.Parent;
            this.RelationTo = personId;
            this.parentChildModifier = parentChildType;
        }
    }

    /// <summary>
    /// Describes the kinship between a parent and child.
    /// </summary>
    [Serializable]
    public class ChildRelationship : Relationship
    {
        private ParentChildModifier parentChildModifier;
		
        public ParentChildModifier ParentChildModifier
        {
            get { return parentChildModifier; }
            set { parentChildModifier = value; }
        }

        // Paramaterless constructor required for XML serialization
        public ChildRelationship() {}

        public ChildRelationship(Person person, ParentChildModifier parentChildType)
        {
            RelationshipType = RelationshipType.Child;
            this.RelationTo = person;
            this.parentChildModifier = parentChildType;
        }
    }

    /// <summary>
    /// Describes the kindship between a couple
    /// </summary>
    [Serializable]
    public class SpouseRelationship : Relationship
    {
        private SpouseModifier spouseModifier;

        private DateTime? marriageDate;
        private string marriageDateDescriptor;
		private string marriagePlace;

        private string marriageCitation;
        private string marriageSource;
        private string marriageLink;
        private string marriageCitationActualText;
        private string marriageCitationNote;

        private DateTime? divorceDate;
        private string divorceDateDescriptor;

        private string divorceCitation;
        private string divorceSource;
        private string divorceLink;
        private string divorceCitationActualText;
        private string divorceCitationNote;

        public SpouseModifier SpouseModifier
        {
            get { return spouseModifier; }
            set { spouseModifier = value; }
        }

        #region marriage get set methods

        public DateTime? MarriageDate
        {
            get { return marriageDate; }
            set { marriageDate = value; }
        }

        public string MarriageDateDescriptor
        {
            get { return marriageDateDescriptor; }
            set { marriageDateDescriptor = value; }
        }
		
		public string MarriagePlace
        {
            get { return marriagePlace; }
            set { marriagePlace = value; }
        }

        public string MarriageCitation
        {
            get { return marriageCitation; }
            set { marriageCitation = value; }
        }

        public string MarriageSource
        {
            get { return marriageSource; }
            set { marriageSource = value; }
        }

        public string MarriageLink
        {
            get { return marriageLink; }
            set { marriageLink = value; }
        }

        public string MarriageCitationNote
        {
            get { return marriageCitationNote; }
            set { marriageCitationNote = value; }
        }

        public string MarriageCitationActualText
        {
            get { return marriageCitationActualText; }
            set { marriageCitationActualText = value; }
        }

        #endregion

        #region divorce get set methods

        public DateTime? DivorceDate
        {
            get { return divorceDate; }
            set { divorceDate = value; }
        }

        public string DivorceDateDescriptor
        {
            get { return divorceDateDescriptor; }
            set { divorceDateDescriptor = value; }
        }

        public string DivorceCitation
        {
            get { return divorceCitation; }
            set { divorceCitation = value; }
        }

        public string DivorceSource
        {
            get { return divorceSource; }
            set { divorceSource = value; }
        }

        public string DivorceLink
        {
            get { return divorceLink; }
            set { divorceLink = value; }
        }

        public string DivorceCitationNote
        {
            get { return divorceCitationNote; }
            set { divorceCitationNote = value; }
        }

        public string DivorceCitationActualText
        {
            get { return divorceCitationActualText; }
            set { divorceCitationActualText = value; }
        }

        #endregion

        // Paramaterless constructor required for XML serialization
        public SpouseRelationship() {}

        public SpouseRelationship(Person person, SpouseModifier spouseType)
        {
            RelationshipType = RelationshipType.Spouse;
            this.spouseModifier = spouseType;
            this.RelationTo = person;
        }
    }

    /// <summary>
    /// Describes the kindship between a siblings
    /// </summary>
    [Serializable]
    public class SiblingRelationship : Relationship
    {
        // Paramaterless constructor required for XML serialization
        public SiblingRelationship() {}

        public SiblingRelationship(Person person)
        {
            RelationshipType = RelationshipType.Sibling;
            this.RelationTo = person;
        }
    }

    #endregion

    #region Relationships collection

    /// <summary>
    /// Collection of relationship for a person object
    /// </summary>
    [Serializable]
    public class RelationshipCollection : ObservableCollection<Relationship>  { }

    #endregion

    #region Relationship Type/Modifier Enums

    /// <summary>
    /// Enumeration of connection types between person objects
    /// </summary>
    public enum RelationshipType
    {
        Child,
        Parent,
        Sibling,
        Spouse
    }

    public enum SpouseModifier
    {
        Current,
        Former
    }

    public enum ParentChildModifier
    {
        Natural,
        Adopted,
        Foster
    }

    #endregion
}

