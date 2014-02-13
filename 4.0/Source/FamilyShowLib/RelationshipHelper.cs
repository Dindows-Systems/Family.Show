using System;
using System.Collections.Generic;

namespace Microsoft.FamilyShowLib
{
    /// <summary>
    /// Contains the relationship logic rules for adding people and how they relate to each other.
    /// 
    /// This class can handle parents, children, spouses and siblings.
    /// 
    /// Parent child relationships can be natural, adopted of foster.
    /// Spouse relationships can be current or former.
    /// 
    /// All types of relationship are removable.
    /// 
    /// The class also handles the update of marriage/divore event information.
    /// 
    /// </summary>
    public static class RelationshipHelper
    {

        #region children

        /// <summary>
        /// Performs the business logic for adding the Child relationship between the person and the child.
        /// </summary>
        public static void AddChild(PeopleCollection family, Person person, Person child, ParentChildModifier modifier)
        {
            if (person != child)
            {
                switch (person.Spouses.Count)
                {
                    // Single parent, add the child to the person
                    case 0:
                        family.AddChild(person, child, modifier);
                        break;

                    // Has existing spouse, add the child to the person's spouse as well.
                    case 1:
                        family.AddChild(person, child, modifier);
                        family.AddChild(person.Spouses[0], child, modifier);
                        break;
                }

                // Add the new child as a sibling to any existing natural children of the natural parents.
                foreach (Person existingSibling in person.NaturalChildren)
                    if (existingSibling != child)
                            family.AddSibling(existingSibling, child);
            }
        }

        /// <summary>
        /// Performs the business logic for adding the Child relationship between the person and the child.
        /// In the case of existing people, do not hook up other siblings etc.  Allow the user to do this manually.
        /// </summary>
        public static void AddExistingChild(PeopleCollection family, Person person, Person child, ParentChildModifier modifier)
        {
            if (person != child)
            {
                family.AddChild(person, child, modifier);
            }
        }

        #endregion

        #region parents

        /// <summary>
        /// Performs the business logic for adding the Parent relationship between the person and the parent.
        /// Once a person has a parent of a given gender, adding another 
        /// parent of the same gender creates an adopted relationship.
        /// </summary>
        public static void AddParent(PeopleCollection family, Person person, Person parent)
        {
            // A person can only have 2 natural parents, so adding a 3rd or more indicates adoption 
            // In this case, just add a simple child parent relationship and don't try to add
            // siblings etc, this should be done manually.

            Gender parentGender = parent.Gender;

            int i = 0;  // number of parents of same gender

            foreach (Person p in person.Parents)
            {
                if (p.Gender == parentGender)
                    i++;
            }

            if (i >= 1) //if a person already has a parent with the same gender, then add as an adopted parent (could be foster)
            {
                family.Add(parent);
                AddExistingParent(family, person, parent, ParentChildModifier.Adopted);
                return;
            }

            else  //adding other parent and siblings, only when a person has one parent set.
            {
                // Add the parent to the main collection of people.
                family.Add(parent); 

                if (person.Parents.Count == 0)// No exisiting parents
                    family.AddChild(parent, person, ParentChildModifier.Natural);
                else
                {
                    // One existing parent
                    family.AddChild(parent, person, ParentChildModifier.Natural);
                    family.AddSpouse(parent, person.Parents[0], SpouseModifier.Current);
                }

                // Handle siblings
                if (person.Siblings.Count > 0)
                {
                    // Make siblings the child of the new parent
                    foreach (Person sibling in person.Siblings)
                    {
                        family.AddChild(parent, sibling, ParentChildModifier.Natural);
                    }
                }
            }

            //Setter for property change notification
            person.HasParents = true;

        }

        /// <summary>
        /// Performs the business logic for adding the Parent relationship between the person and the parent.
        /// In the case of existing people, do not hook up other siblings etc.  Allow the user to do this manually.
        /// </summary>
        public static void AddExistingParent(PeopleCollection family, Person person, Person parent, ParentChildModifier modifier)
        {
            family.AddChild(parent, person, modifier);
            //Setter for property change notification
            person.HasParents = true;
        }

        /// <summary>
        /// Performs the business logic for updating the Parent relationship between the child and the parent.
        /// </summary>
        public static void UpdateParentChildStatus(PeopleCollection family, Person parent, Person child, ParentChildModifier modifier)
        {
            foreach (Relationship relationship in parent.Relationships)
            {
                if (relationship.RelationshipType == RelationshipType.Child && relationship.RelationTo.Equals(child))
                {
                    ((ChildRelationship)relationship).ParentChildModifier = modifier;
                    break;
                }
            }

            foreach (Relationship relationship in child.Relationships)
            {
                if (relationship.RelationshipType == RelationshipType.Parent && relationship.RelationTo.Equals(parent))
                {
                    ((ParentRelationship)relationship).ParentChildModifier = modifier;
                    break;
                }
            }
        }

        /// <summary>
        /// Performs the business logic for adding the Parent relationship between the person and the parents.
        /// </summary>
        public static void AddParent(PeopleCollection family, Person person, ParentSet parentSet)
        {
            // First add child to parents.
            family.AddChild(parentSet.FirstParent, person, ParentChildModifier.Natural);
            family.AddChild(parentSet.SecondParent, person, ParentChildModifier.Natural);

            // Next update the siblings. Get the list of full siblings for the person. 
            // A full sibling is a sibling that has both parents in common. 
            List<Person> siblings = GetChildren(parentSet);
            foreach (Person sibling in siblings)
            {
                if (sibling != person)
                    family.AddSibling(person, sibling);
            }
        }

        /// <summary>
        /// Return a list of children common to both parents in the parent set.
        /// </summary>
        private static List<Person> GetChildren(ParentSet parentSet)
        {
            // Get list of both parents.
            List<Person> firstParentChildren = new List<Person>(parentSet.FirstParent.Children);
            List<Person> secondParentChildren = new List<Person>(parentSet.SecondParent.Children);

            // Combined children list that is returned.
            List<Person> children = new List<Person>();

            // Go through and add the children that have both parents.            
            foreach (Person child in firstParentChildren)
            {
                if (secondParentChildren.Contains(child))
                    children.Add(child);
            }

            return children;
        }

        #endregion

        #region spouses

        /// <summary>
        /// Performs the business logic for adding the Spousal relationship between the person and the spouse.
        /// </summary>
        public static void AddSpouse(PeopleCollection family, Person person, Person spouse, SpouseModifier modifier)
        {
            // Assume the spouse's gender based on the counterpart of the person's gender
            if (person.Gender == Gender.Male)
                spouse.Gender = Gender.Female;
            else
                spouse.Gender = Gender.Male;

            if (person.Spouses != null)
            {
                switch (person.Spouses.Count)
                {
                    // No existing spouse	
                    case 0:
                        family.AddSpouse(person, spouse, modifier);

                        // Add any of the children as the child of the spouse.
                        if (person.Children != null || person.Children.Count > 0)
                        {
                            foreach (Person child in person.Children)
                            {
                                family.AddChild(spouse, child, ParentChildModifier.Natural);
                            }
                        }
                        break;

                    // Existing spouse(s)
                    default:
                        // If specifying a new married spouse, make existing spouses former.
                        if (modifier == SpouseModifier.Current)
                        {
                            foreach (Relationship relationship in person.Relationships)
                            {
                                if (relationship.RelationshipType == RelationshipType.Spouse)
                                    ((SpouseRelationship)relationship).SpouseModifier = SpouseModifier.Former;
                            }
                        }

                        family.AddSpouse(person, spouse, modifier);
                        break;
                }

                // Setter for property change notification
                person.HasSpouse = true;
            }
        }

        /// <summary>
        /// Performs the business logic for adding the Spousal relationship between the person and the spouse.
        /// In the case of existing people, do not hook up other siblings etc.  Allow the user to do this manually.
        /// </summary>
        public static void AddExistingSpouse(PeopleCollection family, Person person, Person spouse, SpouseModifier modifier)
        {
            // Assume the spouse's gender based on the counterpart of the person's gender
            if (person.Gender == Gender.Male)
                spouse.Gender = Gender.Female;
            else
                spouse.Gender = Gender.Male;

            if (person.Spouses != null)
            {
                // If specifying a new married spouse, make existing spouses former.
                if (modifier == SpouseModifier.Current)
                {
                    foreach (Relationship relationship in person.Relationships)
                    {
                        if (relationship.RelationshipType == RelationshipType.Spouse)
                            ((SpouseRelationship)relationship).SpouseModifier = SpouseModifier.Former;
                    }
                }
                family.AddSpouse(person, spouse, modifier);
            }

            // Setter for property change notification
            person.HasSpouse = true;

        }

        /// <summary>
        /// Performs the business logic for updating the spouse relationship status
        /// </summary>
        public static void UpdateSpouseStatus(Person person, Person spouse, SpouseModifier modifier)
        {
            foreach (Relationship relationship in person.Relationships)
            {
                if (relationship.RelationshipType == RelationshipType.Spouse && relationship.RelationTo.Equals(spouse))
                {
                    ((SpouseRelationship)relationship).SpouseModifier = modifier;
                    break;
                }
            }

            foreach (Relationship relationship in spouse.Relationships)
            {
                if (relationship.RelationshipType == RelationshipType.Spouse && relationship.RelationTo.Equals(person))
                {
                    ((SpouseRelationship)relationship).SpouseModifier = modifier;
                    break;
                }
            }
        }

        #endregion

        #region siblings

        /// <summary>
        /// Performs the business logic for adding the Sibling relationship between the person and the sibling.
        /// </summary>
        public static void AddSibling(PeopleCollection family, Person person, Person sibling)
        {
            // Handle siblings

            if (person.Siblings.Count > 0)
            {
                // Make the siblings siblings to each other.
                foreach (Person existingSibling in person.Siblings)
                {
                    if(existingSibling!=sibling)
                        family.AddSibling(existingSibling, sibling);
                }
            }

            if (person.NaturalParents != null)
            {

                foreach (Person parent in person.NaturalParents)
                {
                    family.AddChild(parent, sibling, ParentChildModifier.Natural);
                }

                family.AddSibling(person, sibling);
            }
            person.HasSiblings = true;


        }

        /// <summary>
        /// Performs the business logic for adding the Sibling relationship between the person and the sibling.
        /// In the case of existing people, do not hook up other siblings etc.  Allow the user to do this manually.
        /// </summary>
        public static void AddExistingSibling(PeopleCollection family, Person person, Person sibling)
        {
            if (person.Parents != null)
                family.AddSibling(person, sibling);
        }

        /// <summary>
        /// Performs the business logic for updating the natural siblings of a child.
        /// </summary>
        public static void UpdateSiblings(PeopleCollection family, Person child)
        {
            foreach (Person p in child.NaturalParents)
            {

                foreach (Person c in p.NaturalChildren)
                {
                    if (c != child && !family.Current.Siblings.Contains(c))
                        RelationshipHelper.AddExistingSibling(family, family.Current, c);
                }

            }
        }

        #endregion

        #region update marriage and divorce fields

        /// <summary>
        /// Performs the business logic for updating the marriage date
        /// </summary>
        public static void UpdateMarriageDate(Person person, Person spouse, DateTime? dateTime)
        {
            foreach (Relationship relationship in person.Relationships)
            {
                if (relationship.RelationshipType == RelationshipType.Spouse && relationship.RelationTo.Equals(spouse))
                {
                    ((SpouseRelationship)relationship).MarriageDate = dateTime;
                    break;
                }
            }

            foreach (Relationship relationship in spouse.Relationships)
            {
                if (relationship.RelationshipType == RelationshipType.Spouse && relationship.RelationTo.Equals(person))
                {
                    ((SpouseRelationship)relationship).MarriageDate = dateTime;
                    break;
                }
            }
        }

        /// <summary>
        /// Performs the business logic for updating the marriage place
        /// </summary>
        public static void UpdateMarriagePlace(Person person, Person spouse, string marriagePlace)
        {
            foreach (Relationship relationship in person.Relationships)
            {
                if (relationship.RelationshipType == RelationshipType.Spouse && relationship.RelationTo.Equals(spouse))
                {
                    ((SpouseRelationship)relationship).MarriagePlace = marriagePlace;
                    break;
                }
            }

            foreach (Relationship relationship in spouse.Relationships)
            {
                if (relationship.RelationshipType == RelationshipType.Spouse && relationship.RelationTo.Equals(person))
                {
                    ((SpouseRelationship)relationship).MarriagePlace = marriagePlace;
                    break;
                }
            }
        }

        /// <summary>
        /// Performs the business logic for updating the marriage source
        /// </summary>
        public static void UpdateMarriageSource(Person person, Person spouse, string marriageSource)
        {
            foreach (Relationship relationship in person.Relationships)
            {
                if (relationship.RelationshipType == RelationshipType.Spouse && relationship.RelationTo.Equals(spouse))
                {
                    ((SpouseRelationship)relationship).MarriageSource = marriageSource;
                    break;
                }
            }

            foreach (Relationship relationship in spouse.Relationships)
            {
                if (relationship.RelationshipType == RelationshipType.Spouse && relationship.RelationTo.Equals(person))
                {
                    ((SpouseRelationship)relationship).MarriageSource = marriageSource;
                    break;
                }
            }
        }

        /// <summary>
        /// Performs the business logic for updating the marriage citation
        /// </summary>
        public static void UpdateMarriageCitation(Person person, Person spouse, string marriageCitation)
        {
            foreach (Relationship relationship in person.Relationships)
            {
                if (relationship.RelationshipType == RelationshipType.Spouse && relationship.RelationTo.Equals(spouse))
                {
                    ((SpouseRelationship)relationship).MarriageCitation = marriageCitation;
                    break;
                }
            }

            foreach (Relationship relationship in spouse.Relationships)
            {
                if (relationship.RelationshipType == RelationshipType.Spouse && relationship.RelationTo.Equals(person))
                {
                    ((SpouseRelationship)relationship).MarriageCitation = marriageCitation;
                    break;
                }
            }
        }

        /// <summary>
        /// Performs the business logic for updating the marriage link
        /// </summary>
        public static void UpdateMarriageLink(Person person, Person spouse, string marriageLink)
        {
            foreach (Relationship relationship in person.Relationships)
            {
                if (relationship.RelationshipType == RelationshipType.Spouse && relationship.RelationTo.Equals(spouse))
                {
                    ((SpouseRelationship)relationship).MarriageLink = marriageLink;
                    break;
                }
            }

            foreach (Relationship relationship in spouse.Relationships)
            {
                if (relationship.RelationshipType == RelationshipType.Spouse && relationship.RelationTo.Equals(person))
                {
                    ((SpouseRelationship)relationship).MarriageLink = marriageLink;
                    break;
                }
            }
        }

        /// <summary>
        /// Performs the business logic for updating the marriage citation actual text
        /// </summary>
        public static void UpdateMarriageCitationActualText(Person person, Person spouse, string marriageCitationActualText)
        {
            foreach (Relationship relationship in person.Relationships)
            {
                if (relationship.RelationshipType == RelationshipType.Spouse && relationship.RelationTo.Equals(spouse))
                {
                    ((SpouseRelationship)relationship).MarriageCitationActualText = marriageCitationActualText;
                    break;
                }
            }

            foreach (Relationship relationship in spouse.Relationships)
            {
                if (relationship.RelationshipType == RelationshipType.Spouse && relationship.RelationTo.Equals(person))
                {
                    ((SpouseRelationship)relationship).MarriageCitationActualText = marriageCitationActualText;
                    break;
                }
            }
        }

        /// <summary>
        /// Performs the business logic for updating the marriage citation note
        /// </summary>
        public static void UpdateMarriageCitationNote(Person person, Person spouse, string marriageCitationNote)
        {
            foreach (Relationship relationship in person.Relationships)
            {
                if (relationship.RelationshipType == RelationshipType.Spouse && relationship.RelationTo.Equals(spouse))
                {
                    ((SpouseRelationship)relationship).MarriageCitationNote = marriageCitationNote;
                    break;
                }
            }

            foreach (Relationship relationship in spouse.Relationships)
            {
                if (relationship.RelationshipType == RelationshipType.Spouse && relationship.RelationTo.Equals(person))
                {
                    ((SpouseRelationship)relationship).MarriageCitationNote = marriageCitationNote;
                    break;
                }
            }
        }

        /// <summary>
        /// Performs the business logic for updating the marriage descriptor
        /// </summary>
        public static void UpdateMarriageDateDescriptor(Person person, Person spouse, string marriageDateDescriptor)
        {
            foreach (Relationship relationship in person.Relationships)
            {
                if (relationship.RelationshipType == RelationshipType.Spouse && relationship.RelationTo.Equals(spouse))
                {
                    ((SpouseRelationship)relationship).MarriageDateDescriptor = marriageDateDescriptor;
                    break;
                }
            }

            foreach (Relationship relationship in spouse.Relationships)
            {
                if (relationship.RelationshipType == RelationshipType.Spouse && relationship.RelationTo.Equals(person))
                {
                    ((SpouseRelationship)relationship).MarriageDateDescriptor = marriageDateDescriptor;
                    break;
                }
            }
        }

        /// <summary>
        /// Performs the business logic for updating the divorce descriptor
        /// </summary>
        public static void UpdateDivorceDateDescriptor(Person person, Person spouse, string divorceDateDescriptor)
        {
            foreach (Relationship relationship in person.Relationships)
            {
                if (relationship.RelationshipType == RelationshipType.Spouse && relationship.RelationTo.Equals(spouse))
                {
                    ((SpouseRelationship)relationship).DivorceDateDescriptor = divorceDateDescriptor;
                    break;
                }
            }

            foreach (Relationship relationship in spouse.Relationships)
            {
                if (relationship.RelationshipType == RelationshipType.Spouse && relationship.RelationTo.Equals(person))
                {
                    ((SpouseRelationship)relationship).DivorceDateDescriptor = divorceDateDescriptor;
                    break;
                }
            }
        }

        /// <summary>
        /// Performs the business logic for updating the divorce date
        /// </summary>
        public static void UpdateDivorceDate(Person person, Person spouse, DateTime? dateTime)
        {
            foreach (Relationship relationship in person.Relationships)
            {
                if (relationship.RelationshipType == RelationshipType.Spouse && relationship.RelationTo.Equals(spouse))
                {
                    ((SpouseRelationship)relationship).DivorceDate = dateTime;
                    break;
                }
            }

            foreach (Relationship relationship in spouse.Relationships)
            {
                if (relationship.RelationshipType == RelationshipType.Spouse && relationship.RelationTo.Equals(person))
                {
                    ((SpouseRelationship)relationship).DivorceDate = dateTime;
                    break;
                }
            }
        }

        /// <summary>
        /// Performs the business logic for updating the divorce source
        /// </summary>
        public static void UpdateDivorceSource(Person person, Person spouse, string divorceSource)
        {
            foreach (Relationship relationship in person.Relationships)
            {
                if (relationship.RelationshipType == RelationshipType.Spouse && relationship.RelationTo.Equals(spouse))
                {
                    ((SpouseRelationship)relationship).DivorceSource = divorceSource;
                    break;
                }
            }

            foreach (Relationship relationship in spouse.Relationships)
            {
                if (relationship.RelationshipType == RelationshipType.Spouse && relationship.RelationTo.Equals(person))
                {
                    ((SpouseRelationship)relationship).DivorceSource = divorceSource;
                    break;
                }
            }
        }

        /// <summary>
        /// Performs the business logic for updating the divorce citation
        /// </summary>
        public static void UpdateDivorceCitation(Person person, Person spouse, string divorceCitation)
        {
            foreach (Relationship relationship in person.Relationships)
            {
                if (relationship.RelationshipType == RelationshipType.Spouse && relationship.RelationTo.Equals(spouse))
                {
                    ((SpouseRelationship)relationship).DivorceCitation = divorceCitation;
                    break;
                }
            }

            foreach (Relationship relationship in spouse.Relationships)
            {
                if (relationship.RelationshipType == RelationshipType.Spouse && relationship.RelationTo.Equals(person))
                {
                    ((SpouseRelationship)relationship).DivorceCitation = divorceCitation;
                    break;
                }
            }
        }

        /// <summary>
        /// Performs the business logic for updating the divorce link
        /// </summary>
        public static void UpdateDivorceLink(Person person, Person spouse, string divorceLink)
        {
            foreach (Relationship relationship in person.Relationships)
            {
                if (relationship.RelationshipType == RelationshipType.Spouse && relationship.RelationTo.Equals(spouse))
                {
                    ((SpouseRelationship)relationship).DivorceLink = divorceLink;
                    break;
                }
            }

            foreach (Relationship relationship in spouse.Relationships)
            {
                if (relationship.RelationshipType == RelationshipType.Spouse && relationship.RelationTo.Equals(person))
                {
                    ((SpouseRelationship)relationship).DivorceLink = divorceLink;
                    break;
                }
            }
        }

        /// <summary>
        /// Performs the business logic for updating the divorce citation actual text
        /// </summary>
        public static void UpdateDivorceCitationActualText(Person person, Person spouse, string divorceCitationActualText)
        {
            foreach (Relationship relationship in person.Relationships)
            {
                if (relationship.RelationshipType == RelationshipType.Spouse && relationship.RelationTo.Equals(spouse))
                {
                    ((SpouseRelationship)relationship).DivorceCitationActualText = divorceCitationActualText;
                    break;
                }
            }

            foreach (Relationship relationship in spouse.Relationships)
            {
                if (relationship.RelationshipType == RelationshipType.Spouse && relationship.RelationTo.Equals(person))
                {
                    ((SpouseRelationship)relationship).DivorceCitationActualText = divorceCitationActualText;
                    break;
                }
            }
        }

        /// <summary>
        /// Performs the business logic for updating the divorce citation note
        /// </summary>
        public static void UpdateDivorceCitationNote(Person person, Person spouse, string divorceCitationNote)
        {
            foreach (Relationship relationship in person.Relationships)
            {
                if (relationship.RelationshipType == RelationshipType.Spouse && relationship.RelationTo.Equals(spouse))
                {
                    ((SpouseRelationship)relationship).DivorceCitationNote = divorceCitationNote;
                    break;
                }
            }

            foreach (Relationship relationship in spouse.Relationships)
            {
                if (relationship.RelationshipType == RelationshipType.Spouse && relationship.RelationTo.Equals(person))
                {
                    ((SpouseRelationship)relationship).DivorceCitationNote = divorceCitationNote;
                    break;
                }
            }
        }


        #endregion

        #region remove methods

        /// <summary>
        /// Helper function for removing sibling relationships.
        /// Only remove siblings from a person if they are contained in the specified parent.
        /// </summary>
        public static void RemoveSiblingRelationships(Person person, Person parent)
        {
            foreach (Person p in parent.Children)
            {
                foreach (Relationship relationship in p.Relationships)
                {
                    if (relationship.RelationshipType == RelationshipType.Sibling && relationship.RelationTo.Equals(person))
                    {
                        p.Relationships.Remove(relationship);
                        break;
                    }
                }

                foreach (Relationship relationship in person.Relationships)
                {
                    if (relationship.RelationshipType == RelationshipType.Sibling && relationship.RelationTo.Equals(p))
                    {
                        person.Relationships.Remove(relationship);
                        break;
                    }
                }

            }
        }

        /// <summary>
        /// Helper function for removing a one to one sibling relationship between a person and a sibling
        /// </summary>
        public static void RemoveSiblingRelationshipsOneToOne(Person person, Person sibling)
        {

            foreach (Relationship relationship in person.Relationships)
            {
                if (relationship.RelationshipType == RelationshipType.Sibling && relationship.RelationTo.Equals(sibling))
                {
                    person.Relationships.Remove(relationship);
                    break;
                }
            }

            foreach (Relationship relationship in sibling.Relationships)
            {
                if (relationship.RelationshipType == RelationshipType.Sibling && relationship.RelationTo.Equals(person))
                {
                    sibling.Relationships.Remove(relationship);
                    break;
                }
            }
        }

        /// <summary>
        /// Helper function for removing a parent relationship
        /// </summary>
        public static void RemoveParentChildRelationship(Person person, Person parent)
        {
            foreach (Relationship relationship in person.Relationships)
            {
                if (relationship.RelationshipType == RelationshipType.Parent && relationship.RelationTo.Equals(parent))
                {
                    person.Relationships.Remove(relationship);
                    break;
                }
            }

            foreach (Relationship relationship in parent.Relationships)
            {
                if (relationship.RelationshipType == RelationshipType.Child && relationship.RelationTo.Equals(person))
                {
                    parent.Relationships.Remove(relationship);
                    break;
                }
            }
        }

        /// <summary>
        /// Helper function for removing a spouse relationship
        /// </summary>
        public static void RemoveSpouseRelationship(Person person, Person spouse)
        {
            foreach (Relationship relationship in person.Relationships)
            {
                if (relationship.RelationshipType == RelationshipType.Spouse && relationship.RelationTo.Equals(spouse))
                {
                    person.Relationships.Remove(relationship);
                    break;
                }
            }

            foreach (Relationship relationship in spouse.Relationships)
            {
                if (relationship.RelationshipType == RelationshipType.Spouse && relationship.RelationTo.Equals(person))
                {
                    spouse.Relationships.Remove(relationship);
                    break;
                }
            }
        }

        /// <summary>
        /// Performs the business logic for deleting a person
        /// </summary>
        public static void DeletePerson(PeopleCollection family, Person personToDelete)
        {
            if (!personToDelete.IsDeletable)
                return;

            // Remove the personToDelete from the relationships that contains the personToDelete.
            foreach (Relationship relationship in personToDelete.Relationships)
            {
                foreach (Relationship rel in relationship.RelationTo.Relationships)
                {
                    if (rel.RelationTo.Equals(personToDelete))
                    {
                        relationship.RelationTo.Relationships.Remove(rel);
                        break;
                    }
                }
            }

            // Delete the person's photos and story
            //personToDelete.DeletePhotos();
            personToDelete.DeleteStory();

            family.Remove(personToDelete);
        }

        #endregion
   
    }
}
