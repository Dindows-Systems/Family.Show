/*
 * Derived class that filters data in the family data view.
*/

using Microsoft.FamilyShowLib;
namespace Microsoft.FamilyShow
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    class FamilyEditListView : FilterSortListView
    {
        /// <summary>
        /// Called for each item in the list. Return true if the item should be in
        /// the current result set, otherwise return false to exclude the item.
        /// </summary>    
        override protected bool FilterCallback(object item)
        {
            Person person = item as Person;
            if (person == null)
                return false;

            // Check for match.
			// Additional filters to search other columns
            if (this.Filter.Matches(person.FirstName) ||
                this.Filter.Matches(person.LastName) ||
                this.Filter.Matches(person.Suffix) ||
                this.Filter.Matches(person.Name) ||
                this.Filter.Matches(person.BurialPlace) ||
                this.Filter.Matches(person.BurialDate) ||
                this.Filter.Matches(person.Occupation) ||
                this.Filter.Matches(person.Education) ||
                this.Filter.Matches(person.Religion) ||
                this.Filter.Matches(person.CremationPlace) ||
                this.Filter.Matches(person.CremationDate) ||
                this.Filter.Matches(person.DeathPlace) ||
                this.Filter.Matches(person.DeathDate) ||
                this.Filter.Matches(person.BirthDate) ||
                this.Filter.Matches(person.BirthPlace) ||
                this.Filter.Matches(person.Age))
                return true;
    
            // Check for the special case of birthdays, if
            // matches the month and day, but don't check year.
            if (this.Filter.MatchesMonth(person.BirthDate) &&
                this.Filter.MatchesDay(person.BirthDate))
                return true;

            //Special filters
            if (this.Filter.MatchesImages(person.HasAvatar) ||
                this.Filter.MatchesRestrictions(person.HasRestriction) ||
                this.Filter.MatchesPhotos(person.HasPhoto) ||
                this.Filter.MatchesCitations(person.HasCitations) ||
                this.Filter.MatchesLiving(person.IsLiving) ||
                this.Filter.MatchesNotes(person.HasNote) ||
                this.Filter.MatchesAttachments(person.HasAttachments))
                return true;

            //filter for gender
            if (person.Gender == Gender.Male)
            {
                if(this.Filter.MatchesGender(Properties.Resources.Male.ToLower()))
                    return true;
            }
            if(person.Gender == Gender.Female)
            {
                if(this.Filter.MatchesGender(Properties.Resources.Female.ToLower()))
                    return true;
            }

            return false;                
        }
    }

}
