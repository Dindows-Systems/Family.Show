/*
 * Exports data from the People collection to a GEDCOM file.
 * 
 * More information on the GEDCOM format is at http://en.wikipedia.org/wiki/Gedcom 
 * and http://homepages.rootsweb.ancestry.com/~pmcbride/gedcom/55gctoc.htm
 * 
 * GedcomExport class
 * Exports data from a Person collection to a GEDCOM file.
 *
 * GedcomIdMap
 * Maps a Person's ID (GUID) to a GEDCOM ID (int).
 * 
 * FamilyMap
 * Creates a list of GEDCOM family groups from the People collection.
 * 
 * Family
 * One family group in the FamilyMap list.
 * 
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;

namespace Microsoft.FamilyShowLib
{
    /// <summary>
    /// Exports data from a People collection to a GEDCOM file.
    /// </summary>
    public class GedcomExport
    {
        #region fields

        // Writes the text (GEDCOM) file.
        private TextWriter writer;

        // Maps GUID IDs (which are too long for GEDCOM) to smaller IDs.
        private GedcomIdMap idMap = new GedcomIdMap();

        // The people collection that is being exported.
        private PeopleCollection people;
        private SourceCollection sources;
        private RepositoryCollection repositories;

        // Family group counter.
        private int familyId = 1;

        #endregion

        /// <summary>
        /// Export the data from the People collection to the specified GEDCOM file.
        /// </summary>
        public void Export(PeopleCollection peopleCollection, SourceCollection sourceCollection, RepositoryCollection repositoryCollection, string gedcomFilePath, string familyxFilePath, string language)
        {
            this.people = peopleCollection;
            this.sources = sourceCollection;
            this.repositories = repositoryCollection;

            using (writer = new StreamWriter(gedcomFilePath))
            {
                WriteLine(0, "HEAD", "");
				ExportSummary(gedcomFilePath,familyxFilePath,language);
                ExportPeople();
                ExportFamilies();
                ExportSources();
                ExportRepositories();
                WriteLine(0, "TRLR", "");
            }
        }
		
		/// <summary>
        /// Export summary to GEDCOM file.
        /// </summary>
        private void ExportSummary(string gedcomFilePath,string familyxFilePath, string language)
		{
                WriteLine(1, "SOUR", "");
				
                Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                string versionlabel = string.Format(CultureInfo.CurrentCulture, "{0}.{1}.{2}", version.Major, version.Minor, version.Build);
                WriteLine(2, "VERS", versionlabel);
                WriteLine(2, "NAME", "Family.Show");
                WriteLine(2, "CORP", "Microsoft");

                if(!string.IsNullOrEmpty(familyxFilePath))
                WriteLine(2, "DATA", Path.GetFileName(familyxFilePath));

                string Date = ExportDate(DateTime.Now);  //GEDCOM dates must be of the form 01 JAN 2009
				string filename = System.IO.Path.GetFileName(gedcomFilePath);
                string Time = DateTime.Now.ToLongTimeString();

				WriteLine(1, "DATE", Date);
				WriteLine(2, "TIME", Time);
				WriteLine(1, "FILE", filename);
				WriteLine(1, "GEDC", "");
				WriteLine(2, "VERS", "5.5");
				WriteLine(2, "FORM", "LINEAGE-LINKED");
				WriteLine(1, "CHAR", "UTF-8");

                switch (language)
                {
                    case "en-US":
                        WriteLine(1, "LANG", "English");
                        break;
                    case "en-GB":
                        WriteLine(1, "LANG", "Anglo-Saxon");
                        break;
                    case "it-IT":
                        WriteLine(1, "LANG", "Italian");
                        break;
                    case "es-ES":
                        WriteLine(1, "LANG", "Spanish");
                        break;
                    case "fr-FR":
                        WriteLine(1, "LANG", "French");
                        break;
                    case "de-DE":
                        WriteLine(1, "LANG", "German");
                        break;
                    default:
                        WriteLine(1, "LANG", "English");
                        break;
                }
	
		}
      
		/// <summary>
        /// Export sources to GEDCOM file.
        /// </summary>
        private void ExportSources()
        {
            foreach (Source source in sources)
            {
                WriteLine(0, string.Format(CultureInfo.InvariantCulture, "@{0}@", source.Id), "SOUR");
                if (!string.IsNullOrEmpty(source.SourceRepository))
                WriteLine(1, "REPO", "@"+source.SourceRepository+"@");
                if (!string.IsNullOrEmpty(source.SourceName))
                WriteLine(1, "TITL", source.SourceName);
                if (!string.IsNullOrEmpty(source.SourceAuthor))
                WriteLine(1, "AUTH", source.SourceAuthor);
                if (!string.IsNullOrEmpty(source.SourcePublisher))
                WriteLine(1, "PUBL", source.SourcePublisher);
                if (!string.IsNullOrEmpty(source.SourceNote))
                WriteLine(1, "NOTE", source.SourceNote);
            }
        }

        /// <summary>
        /// Export sources to GEDCOM file.
        /// </summary>
        private void ExportRepositories()
        {
            foreach (Repository r in repositories)
            {
                WriteLine(0, string.Format(CultureInfo.InvariantCulture, "@{0}@", r.Id), "REPO");
                if (!string.IsNullOrEmpty(r.RepositoryName))
                    WriteLine(1, "NAME", r.RepositoryName);
                if (!string.IsNullOrEmpty(r.RepositoryAddress))
                    WriteLine(1, "ADDR", r.RepositoryAddress);
            }
        }

        /// <summary>
        /// Export each person to the GEDCOM file.
        /// </summary>
        private void ExportPeople()
        {

            FamilyMap map = new FamilyMap();
            map.Create(people);

            foreach (Person person in people)
            {

                string id = idMap.Get(person.Id);

                // Start of a new individual record.
                WriteLine(0, string.Format(CultureInfo.InvariantCulture, "@{0}@", id), "INDI");

                // Export details.

                // Restriction.
                ExportRestriction(person);

                if(person.Restriction ==Restriction.Private)
                    WriteLine(1, "NAME", Microsoft.FamilyShowLib.Properties.Resources.PrivateRecord);
                else
                {

                // Name.
                ExportName(person);
				
				// Surname
				if (!string.IsNullOrEmpty(person.LastName))
                    WriteLine(2, "SURN", person.LastName);

                // Prefix.
                if (!string.IsNullOrEmpty(person.Suffix))
                    WriteLine(2, "NPFX", person.Suffix);

                // Gender.
                ExportGender(person);
                
                // Birth and death info.
                ExportEvent("BIRT", "",person.BirthDateDescriptor ,person.BirthDate, person.BirthPlace, person.BirthCitation, person.BirthCitationNote, person.BirthCitationActualText, person.BirthLink, person.BirthSource);
                ExportEvent("DEAT", "", person.DeathDateDescriptor, person.DeathDate, person.DeathPlace, person.DeathCitation, person.DeathCitationNote, person.DeathCitationActualText, person.BirthLink, person.BirthSource);
                ExportEvent("BURI", "", person.BurialDateDescriptor, person.BurialDate, person.BurialPlace, person.BurialCitation, person.BurialCitationNote, person.BurialCitationActualText, person.BirthLink, person.BirthSource);
                ExportEvent("CREM", "", person.CremationDateDescriptor, person.CremationDate, person.CremationPlace, person.CremationCitation, person.CremationCitationNote, person.CremationCitationActualText, person.BirthLink, person.BirthSource);
                ExportEvent("EDUC", person.Education, "", null, "", person.EducationCitation, person.EducationCitationNote, person.EducationCitationActualText, person.EducationLink, person.EducationSource);
                ExportEvent("OCCU", person.Occupation, "", null, "", person.OccupationCitation, person.OccupationCitationNote, person.OccupationCitationActualText, person.OccupationLink, person.OccupationSource);
                ExportEvent("RELI", person.Religion, "", null , "", person.ReligionCitation, person.ReligionCitationNote, person.ReligionCitationActualText, person.ReligionLink, person.ReligionSource);

                // Photo file names, files themselves cannot be exported as GEDCOM is simply a text file.
                ExportPhotos(person);
                ExportAttachments(person);

                // Notes.	
                if (!string.IsNullOrEmpty(person.Note))
                    WriteLine(1, "NOTE", person.Note);

                int i = 1;  //current family number

                //Write a FAMC or FAMS tag for every family which contains the person
                foreach (Family family in map.Values)
                {

                    //FAMC for children
                    foreach (Person child in family.Children)
                    {
                        if (person.Id == child.Id)
                            WriteLine(1, "FAMC", string.Format(CultureInfo.InvariantCulture, "@F{0}@", i));
                    }

                    //FAMS for parents/spouses
                    if (person.Id == family.ParentLeft.Id)
                        WriteLine(1, "FAMS", string.Format(CultureInfo.InvariantCulture, "@F{0}@", i));

                    if (person.Id == family.ParentRight.Id)
                        WriteLine(1, "FAMS", string.Format(CultureInfo.InvariantCulture, "@F{0}@", i));

                    i++;
                }


                }
            }
        }

        /// <summary>
        /// An exportable citation is one which has a source reference which is contained in the current source list for the family.
        /// </summary>
        private bool ExportableCitation(string sourceID)
        {
            bool exportableSource = false;
            int i = 0;
            foreach (Source s in sources)
            {
                if (s.Id == sourceID)
                    i++;
            }

            if(i>0)
              exportableSource = true; 
            
            return exportableSource;
        }

        /// <summary>
        /// Create the family section (the FAM tags) in the GEDCOM file.
        /// </summary>
        private void ExportFamilies()
        {
            // Exporting families is more difficult since need to export each
            // family group. A family group consists of one or more parents,
            // marriage / divorce information and children. The FamilyMap class
            // creates a list of family groups from the People collection.
            FamilyMap map = new FamilyMap();
            map.Create(people);

            // Created the family groups, now export each family.
            foreach (Family family in map.Values)
                ExportFamily(family);
        }

        /// <summary>
        /// Export one family group to the GEDCOM file.  
        /// GEDCOM has no simple way to accurately define parent child modifiers
        /// so use the non standard _MREL and _FREL tags as many other programs do
        /// (e.g. Family Tree Maker, Ancestry online trees)
        /// </summary>
        private void ExportFamily(Family family)
        {
            // Return right away if this is only a single person without any children.
            if (family.ParentRight == null && family.Children.Count == 0)
                return;

            // Start of new family record.
            WriteLine(0, string.Format(CultureInfo.InvariantCulture, "@F{0}@", familyId++), "FAM");

            // Marriage info.
            ExportMarriage(family.ParentLeft, family.ParentRight, family.Relationship);

            // Children.
            foreach (Person child in family.Children)
            {

                WriteLine(1, "CHIL", string.Format(CultureInfo.InvariantCulture, "@{0}@", idMap.Get(child.Id)));

                // Export the adoption information

                string mrel = string.Empty;  
                string frel = string.Empty; 

                foreach(Relationship rel in child.Relationships)
                {
                   if(rel.RelationshipType == RelationshipType.Parent)
                   {

                    ParentRelationship pRel = (ParentRelationship)rel;

                    if(rel.PersonId==family.ParentLeft.Id && family.ParentLeft.Gender==Gender.Male)
                        mrel = pRel.ParentChildModifier.ToString();
                    if (rel.PersonId == family.ParentLeft.Id && family.ParentLeft.Gender == Gender.Female)
                        frel = pRel.ParentChildModifier.ToString();
                    if (rel.PersonId == family.ParentRight.Id && family.ParentRight.Gender == Gender.Female)
                        frel = pRel.ParentChildModifier.ToString();
                    if (rel.PersonId == family.ParentRight.Id && family.ParentRight.Gender == Gender.Male)
                        mrel = pRel.ParentChildModifier.ToString();
                   }
                }

                //export nothing if natural relationship
                if (frel.Length > 0 && frel!="Natural")
                WriteLine(2, "_FREL", frel);
                if(mrel.Length>0 && mrel!="Natural")
                WriteLine(2, "_MREL", mrel);
            }
        }

        /// <summary>
        /// Export marriage / divorce information.
        /// </summary>
        private void ExportMarriage(Person partnerLeft, Person partnerRight, SpouseRelationship relationship)
        {

            // PartnerLeft.
            if (partnerLeft != null && partnerLeft.Gender == Gender.Male)
                WriteLine(1, "HUSB", string.Format(CultureInfo.InvariantCulture, 
                "@{0}@", idMap.Get(partnerLeft.Id)));
                
            if (partnerLeft != null && partnerLeft.Gender == Gender.Female)
                WriteLine(1, "WIFE", string.Format(CultureInfo.InvariantCulture, 
                "@{0}@", idMap.Get(partnerLeft.Id)));

            if (!partnerLeft.Spouses.Contains(partnerRight))
                return;

            // PartnerRight.
            if (partnerRight != null && partnerRight.Gender == Gender.Male)
                WriteLine(1, "HUSB", string.Format(CultureInfo.InvariantCulture, 
                "@{0}@", idMap.Get(partnerRight.Id)));

            if (partnerRight != null && partnerRight.Gender == Gender.Female)
                WriteLine(1, "WIFE", string.Format(CultureInfo.InvariantCulture, 
                "@{0}@", idMap.Get(partnerRight.Id)));

            if (relationship == null)
                return;


            // Marriage.
            if (relationship.SpouseModifier == SpouseModifier.Current)      
            {
                WriteLine(1, "MARR", "");

                if (relationship.MarriageDate != null)
                {

                    string Date = ExportDate(relationship.MarriageDate);
                    // Date if it exist.
    
                    if (relationship.MarriageDateDescriptor != null && relationship.MarriageDateDescriptor.Length>1)
                        WriteLine(2, "DATE", relationship.MarriageDateDescriptor + Date);
                    else
                        WriteLine(2, "DATE", Date);
 
                }
				//Place if it exist.
                if (relationship.MarriagePlace != null && relationship.MarriagePlace.Length > 0)
                    WriteLine(2, "PLAC", relationship.MarriagePlace);
               
                //Source if it exist.
                if (!string.IsNullOrEmpty(relationship.MarriageSource) && !string.IsNullOrEmpty(relationship.MarriageCitation) && ExportableCitation(relationship.MarriageSource) == true)
                {
                        WriteLine(2, "SOUR", "@" + relationship.MarriageSource + "@");
                        WriteLine(3, "PAGE", relationship.MarriageCitation);
                        WriteLine(3, "DATA", "");
                        if (!string.IsNullOrEmpty(relationship.MarriageCitationActualText))
                            WriteLine(4, "TEXT", relationship.MarriageCitationActualText);
                        //Many programs ignore web links so add web link to note.
                        if (!string.IsNullOrEmpty(relationship.MarriageCitationNote) && string.IsNullOrEmpty(relationship.MarriageLink))
                            WriteLine(3, "NOTE", relationship.MarriageCitationNote);
                        if (!string.IsNullOrEmpty(relationship.MarriageCitationNote) && !string.IsNullOrEmpty(relationship.MarriageLink))
                            WriteLine(3, "NOTE", relationship.MarriageCitationNote + " " + relationship.MarriageLink);
                        if (string.IsNullOrEmpty(relationship.MarriageCitationNote) && !string.IsNullOrEmpty(relationship.MarriageLink))
                            WriteLine(3, "NOTE", relationship.MarriageLink);

                        if (!string.IsNullOrEmpty(relationship.MarriageLink))
                        {
                            WriteLine(3, "OBJE", "");
                            WriteLine(4, "FORM", "URL");
                            WriteLine(4, "TITL", "URL of citation");
                            WriteLine(4, "FILE", relationship.MarriageLink);
                        }
                }	
            }

            // Divorce. 
            if (relationship.SpouseModifier == SpouseModifier.Former)
            {

                WriteLine(1, "MARR", "");

                if (relationship.MarriageDate != null)
                {

                    string Date = ExportDate(relationship.MarriageDate);
                    // Date if it exist.

                    if (relationship.MarriageDateDescriptor != null && relationship.MarriageDateDescriptor.Length>1)
                        WriteLine(2, "DATE", relationship.MarriageDateDescriptor + Date);
                    else
                        WriteLine(2, "DATE", Date);

                }
                //Place if it exist.
                if (relationship.MarriagePlace != null && relationship.MarriagePlace.Length > 0)
                    WriteLine(2, "PLAC", relationship.MarriagePlace);

                //Source if it exist.
                if (!string.IsNullOrEmpty(relationship.MarriageSource) && !string.IsNullOrEmpty(relationship.MarriageCitation) && ExportableCitation(relationship.MarriageSource) == true)
                {
                    WriteLine(2, "SOUR", "@" + relationship.MarriageSource + "@");
                    WriteLine(3, "PAGE", relationship.MarriageCitation);
                    WriteLine(3, "DATA", "");
                    if (!string.IsNullOrEmpty(relationship.MarriageCitationActualText))
                        WriteLine(4, "TEXT", relationship.MarriageCitationActualText);
                    //Many programs ignore web links so add web link to note.
                    if (!string.IsNullOrEmpty(relationship.MarriageCitationNote) && string.IsNullOrEmpty(relationship.MarriageLink))
                        WriteLine(3, "NOTE", relationship.MarriageCitationNote);
                    if (!string.IsNullOrEmpty(relationship.MarriageCitationNote) && !string.IsNullOrEmpty(relationship.MarriageLink))
                        WriteLine(3, "NOTE", relationship.MarriageCitationNote + " " + relationship.MarriageLink);
                    if (string.IsNullOrEmpty(relationship.MarriageCitationNote) && !string.IsNullOrEmpty(relationship.MarriageLink))
                        WriteLine(3, "NOTE", relationship.MarriageLink);

                    if (!string.IsNullOrEmpty(relationship.MarriageLink))
                    {
                        WriteLine(3, "OBJE", "");
                        WriteLine(4, "FORM", "URL");
                        WriteLine(4, "TITL", "URL of citation");
                        WriteLine(4, "FILE", relationship.MarriageLink);
                    }
                }	

                WriteLine(1, "DIV", "");

                if (relationship.DivorceDate != null)
                {

                    string Date = ExportDate(relationship.DivorceDate);
                    // Date if it exist.

                    if (relationship.DivorceDateDescriptor != null && relationship.DivorceDateDescriptor.Length>1)
                        WriteLine(2, "DATE", relationship.DivorceDateDescriptor + Date);
                    else
                        WriteLine(2, "DATE", Date);
 
                }
			
                //Source if it exist.
                if (!string.IsNullOrEmpty(relationship.DivorceSource) && !string.IsNullOrEmpty(relationship.DivorceCitation) && ExportableCitation(relationship.DivorceSource) == true)
                {
                        WriteLine(2, "SOUR", "@" + relationship.DivorceSource + "@");
                        WriteLine(3, "PAGE", relationship.DivorceCitation);
                        WriteLine(3, "DATA", "");
                        if (!string.IsNullOrEmpty(relationship.DivorceCitationActualText))
                            WriteLine(4, "TEXT", relationship.DivorceCitationActualText);
                        //Many programs ignore web links so add web link to note.
                        if (!string.IsNullOrEmpty(relationship.DivorceCitationNote) && string.IsNullOrEmpty(relationship.DivorceLink))
                            WriteLine(4, "NOTE", relationship.DivorceCitationNote);
                        if (!string.IsNullOrEmpty(relationship.DivorceCitationNote) && !string.IsNullOrEmpty(relationship.DivorceLink))
                            WriteLine(4, "NOTE", relationship.DivorceCitationNote + " " + relationship.DivorceLink);
                        if (string.IsNullOrEmpty(relationship.DivorceCitationNote) && !string.IsNullOrEmpty(relationship.DivorceLink))
                            WriteLine(4, "NOTE", relationship.DivorceLink);

                        if (!string.IsNullOrEmpty(relationship.DivorceLink))
                        {
                            WriteLine(3, "OBJE", "");
                            WriteLine(4, "FORM", "URL");
                            WriteLine(4, "TITL", "URL of citation");
                            WriteLine(4, "FILE", relationship.DivorceLink);
                        }
                }	

            }
        }

        private void ExportName(Person person)
        {
            string Space = " ";

            string value = string.Format(CultureInfo.InvariantCulture, 
                "{0}{1}/{2}/", person.FirstName, Space, person.LastName);

            WriteLine(1, "NAME", value);
        }

        private void ExportPhotos(Person person)
        {
            foreach (Photo photo in person.Photos)
            {
                WriteLine(1, "OBJE", "");
                WriteLine(2, "FORM", System.IO.Path.GetExtension(photo.FullyQualifiedPath).Replace(".",""));
                WriteLine(2, "FILE", @"\PathToFile\Images\" + System.IO.Path.GetFileName(photo.FullyQualifiedPath));
            }
        }

        private void ExportAttachments(Person person)
        {
            foreach (Attachment attachment in person.Attachments)
            {
                WriteLine(1, "OBJE", "");
                WriteLine(2, "FORM", System.IO.Path.GetExtension(attachment.FullyQualifiedPath).Replace(".", ""));
                WriteLine(2, "FILE", @"\PathToFile\Attachments\" + System.IO.Path.GetFileName(attachment.FullyQualifiedPath));
            }
        }
        
        private void ExportEvent(string tag, string tagDescription, string descriptor, DateTime? date, string place,string citation, string citationNote, string citationActualText,string link, string source)
        {
            // Return right away if don't have a date or place to export.
            if (date == null && string.IsNullOrEmpty(place))
                return;

            string Date = null;

            // Start the new event tag.
            WriteLine(1, tag, tagDescription);

			if (date != null)
                Date = ExportDate(date);
            
            if (!string.IsNullOrEmpty(Date))
                WriteLine(2, "DATE", descriptor + Date);
	
            // Place.
            if (!string.IsNullOrEmpty(place))
               WriteLine(2, "PLAC", place);

            // Source.
            if (!string.IsNullOrEmpty(source) && !string.IsNullOrEmpty(citation) && ExportableCitation(source) == true)
            {
                WriteLine(2, "SOUR", "@" + source + "@");
                WriteLine(3, "PAGE", citation);
                WriteLine(3, "DATA", "");
                if(!string.IsNullOrEmpty(citationActualText))
                    WriteLine(4, "TEXT", citationActualText);
                //Many programs ignore web links so add web link to note.
                if (!string.IsNullOrEmpty(citationNote) && string.IsNullOrEmpty(link))
                    WriteLine(3, "NOTE", citationNote);
                if (!string.IsNullOrEmpty(citationNote) && !string.IsNullOrEmpty(link))
                    WriteLine(3, "NOTE", citationNote + " " + link );
                if (string.IsNullOrEmpty(citationNote) && !string.IsNullOrEmpty(link))
                    WriteLine(3, "NOTE", link);             

                if (!string.IsNullOrEmpty(link))
                {
                    WriteLine(3, "OBJE", "");
                    WriteLine(4, "FORM", "URL");
                    WriteLine(4, "TITL", "URL of citation");
                    WriteLine(4, "FILE", link);
                }
            }

        }

        private static string ExportDate(DateTime? date)
        {
            if(date==null)
                return string.Empty;
            else
            {

            string day = date.Value.Day.ToString();
            string year = date.Value.Year.ToString();
            int month = date.Value.Month;

            string monthString = string.Empty;

            monthString = GetMMM(month);

            return day + " " + monthString + " " + year;  
            }
        }

		//converts month number to 3 letter month abbreviation as used in GEDCOM
		private static string GetMMM(int month)
		{
		string monthString = string.Empty;
		    if(month==1)
			monthString ="Jan";
			if(month==2)
			monthString ="Feb";
			if(month==3)
			monthString ="Mar";
			if(month==4)
			monthString ="Apr";
			if(month==5)	
			monthString ="May";	
			if(month==6)
			monthString ="Jun";
			if(month==7)
			monthString ="Jul";
			if(month==8)
			monthString ="Aug";
			if(month==9)
			monthString ="Sep";
			if(month==10)
			monthString ="Oct";
			if(month==11)	
			monthString ="Nov";	
			if(month==12)
			monthString ="Dec";
		return monthString;
		}

        private void ExportGender(Person person)
        {
            WriteLine(1, "SEX", (person.Gender == Gender.Female) ? "F" : "M");
        }

        private void ExportRestriction(Person person)
        {
            if(person.Restriction == Restriction.Private)
                WriteLine(1, "RESN", "privacy");
            else if (person.Restriction == Restriction.Locked)
                WriteLine(1, "RESN", "locked");
            else
            {
                //return and do nothing
            }
        }

        // Write a GEDCOM line, this is more involved since the line cannot contain 
        // carriage returns or exceed 255 characters. First, divide the value by carriage 
        // return. Then divide each carriage-return line into chunks of 200 characters. 
        // The first line contains the original tag name and level, carriage returns contain
        // the CONT tag and continue lines contains CONC.
        private void WriteLine(int level, string tag, string value)
        {
            // Trim leading white space
            value = value.Trim();

            // Remove leading carriage returns (these break the level structure)
            if (value.StartsWith("\n") || value.StartsWith("\r"))
            {
                do
                {
                    value = value.Remove(0, 2);
                }
                while (value.StartsWith("\n") || value.StartsWith("\r"));
            }
            
            // The entire line length cannot exceed 255 characters using
            // 200 for the value which should stay below the 255 line length.
            const int ValueLimit = 200;

            // Most lines do not need special processing, export the line if it
            // does not contain carriage returns or exceed the line length.
            if (value.Length < ValueLimit && !value.Contains("\r") && !value.Contains("\n"))
            {
                writer.WriteLine(string.Format(
                    CultureInfo.InvariantCulture, 
                    "{0} {1} {2}", level, tag, value));

                return;
            }

            // First divide the value by carriage returns.
            value = value.Replace("\r\n", "\n");
            value = value.Replace("\r", "\n");
            string[] lines = value.Split('\n');

            // Process each line.
            for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                // The current line processing.
                string line = lines[lineIndex];

                // Write each line but don't exceed the line limit, loop here
                // and write each chunk out at a time.
                int chunkCount = (line.Length + ValueLimit - 1) / ValueLimit;

                for (int chunkIndex = 0; chunkIndex < chunkCount; chunkIndex++)
                {
                    // Current position in the value.
                    int pos = chunkIndex * ValueLimit;

                    // Current value chunk to write.
                    string chunk = line.Substring(pos, Math.Min(line.Length - pos, ValueLimit));

                    // Always use the original level and tag for the first line, but use
                    // the concatenation tag (CONT) for all other lines.
                    if (lineIndex == 0 && chunkIndex == 0)
                    {
                        writer.WriteLine(string.Format(CultureInfo.InvariantCulture, 
                            "{0} {1} {2}", level, tag, chunk));
                    }
                    else
                    {
                        writer.WriteLine(string.Format(CultureInfo.InvariantCulture, 
                            "{0} {1} {2}", level + 1, "CONC", chunk));
                    }
                }

                // All lines except the last line have the continue (CONT) tag.
                if (lineIndex < lines.Length - 1)
                {
                    writer.WriteLine(string.Format(CultureInfo.InvariantCulture, 
                       "{0} {1}", level + 1, "CONT"));
                }
            }
        }
    }

    /// <summary>
    /// Maps Microsoft.FamilyShow Person.Id to a GEDCOM ID. GEDCOM IDs cannot 
    /// exceed 22 characters so GUIDs (Person.Id type) cannot be used
    /// when exporting. 
    /// </summary>
    class GedcomIdMap
    {
        #region fields

        // Quick lookup that maps a GUID to a GEDCOM ID.
        private Dictionary<string, string> map = new Dictionary<string, string>();

        // The next ID to assign.
        private int nextId;

        #endregion

        /// <summary>
        /// Return the mapped ID for the specified GUID.
        /// </summary>
        public string Get(string guid)
        {
            // Return right away if already mapped.
            if (map.ContainsKey(guid))
                return map[guid];

            // Assign a new GEDCOM ID and add to map.
            string id = string.Format(CultureInfo.InvariantCulture, "I{0}", nextId++);
            map[guid] = id;
            return id;
        }
    }

    /// <summary>
    /// One family group. 
    /// </summary>
    class Family
    {
        #region fields

        private Person parentLeft;
        private Person parentRight;
        private SpouseRelationship relationship;
        private List<Person> children = new List<Person>();

        #endregion

        /// <summary>
        /// Get the left-side parent.
        /// </summary>
        public Person ParentLeft
        {
            get { return parentLeft; }
        }

        /// <summary>
        /// Get the right-side parent.
        /// </summary>
        public Person ParentRight
        {
            get { return parentRight; }
        }

        /// <summary>
        /// Get or set the relationship for the two parents.
        /// </summary>
        public SpouseRelationship Relationship
        {
            get { return relationship; }
            set { relationship = value; }
        }

        /// <summary>
        /// Get the list of children.
        /// </summary>
        public List<Person> Children
        {
            get { return children; }
        }

        public Family(Person parentLeft, Person parentRight)
        {
            this.parentLeft = parentLeft;
            this.parentRight = parentRight;
        }
    }

    /// <summary>
    /// Orgainzes the People collection into a list of families. 
    /// </summary>
    class FamilyMap : Dictionary<string, Family>
    {
        /// <summary>
        /// Organize the People collection into a list of families. A family consists of
        /// an wife, husband, children, and married / divorced information.
        /// </summary>
        public void Create(PeopleCollection people)
        {
            this.Clear();

            // First, iterate though the list and create parent groups.
            // A parent group is one or two parents that have one or
            // more children.
            foreach (Person person in people)
            {
                Collection<Person> parents = person.Parents;

                int i = parents.Count;

                for (int j=0; j<i; j=j+2)
                {
                    if (parents.Count > j)
                    {
                        // Use an additional sets if they exist
                        Person parentLeft = parents[j];
                        Person parentRight = new Person();
                        if (parents.Count > j+1)
                            parentRight = (parents.Count > j+1) ? parents[j+1] : null;

                        // See if this parent group has been added to the list yet.
                        string key = GetKey(parentLeft, parentRight);
                        if (!this.ContainsKey(key))
                        {
                            // This parent group does not exist, add it to the list.
                            Family details = new Family(parentLeft, parentRight);
                            details.Relationship = parentLeft.GetSpouseRelationship(parentRight);
                            this[key] = details;
                        }

                        // Add the child to the parent group.
                        this[key].Children.Add(person);
                    }
                }
            }

            // Next, iterate though the list and create marriage groups.
            // A marriage group is current or former marriages that
            // don't have any children.
            foreach (Person person in people)
            {
                Collection<Person> spouses = person.Spouses;
                foreach (Person spouse in spouses)
                {
                    // See if this marriage group is in the list.
                    string key = GetKey(person, spouse);
                    if (!this.ContainsKey(key))
                    {
                        // This marriage group is not in the list, add it to the list.
                        Family details = new Family(person, spouse);
                        details.Relationship = person.GetSpouseRelationship(spouse);
                        this[key] = details;
                    }
                }
            }
        }

        /// <summary>
        /// Return a string for the parent group.
        /// </summary>
        private static string GetKey(Person partnerLeft, Person partnerRight)
        {
            // This is used as the key to the list. This is tricky since parent
            // groups should not be duplicated. For example, the list should
            // not contain the parent groups:
            //
            //  Bob Bee
            //  Bee Bob
            //  
            // The list should only contain the group:
            //
            //  Bob Bee
            //
            // This is accomplished by concatenating the parent
            // ID's together when creating the key.

            string key = partnerLeft.Id;
            if (partnerRight != null)
            {
                if (partnerLeft.Id.CompareTo(partnerRight.Id) < 0)
                    key = partnerLeft.Id + partnerRight.Id;
                else
                    key = partnerRight.Id + partnerLeft.Id;
            }
            return key;
        }
    }

}