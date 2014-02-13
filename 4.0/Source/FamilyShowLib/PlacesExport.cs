/*
* Exports time encoded place information to a kml file.
* 
* Three sections are exported: Events and People
* 1. Events are births, marriages and deaths and the year that the event occurs is included.
* 2. People are exported with a timespan from date of birth to date of death.
* 3. All places with no time information.
* 
* The format is based on the open kml standard for map information.
* The recommended software for reading the file is Google Earth as
* this program will search for coordinate information of places 
* specified in the file. Other similar services such as Bing maps 
* require coordinates to be specifed in the file.  Google Earth can also
* read the time information and allows the user to specify a time period to
* display e.g. 1900-1950.
* 
* Places export supports restrictions and quick filter for living people.
*/

//burials

//cremations

using System.IO;

namespace Microsoft.FamilyShowLib
{
    public class PlacesExport
    {

        #region export methods

        public string[] ExportPlaces(PeopleCollection peopleCollection, string fileName, bool hideliving, bool times, bool lifespans, bool places,bool burials, bool deaths, bool cremations, bool births, bool marriages)
        {
            string PlacesFileName = Path.GetFileNameWithoutExtension(fileName);

            TextWriter tw = new StreamWriter(fileName);

            #region styles

            // Write text necessary for kml file.  
            // Uses Google Earths's male and female icons.
            tw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                         "<kml xmlns=\"http://www.opengis.net/kml/2.2\" xmlns:gx=\"http://www.google.com/kml/ext/2.2\" xmlns:kml=\"http://www.opengis.net/kml/2.2\" xmlns:atom=\"http://www.w3.org/2005/Atom\">\n" +
                         "<Document>\n" +
                         "<name>" + PlacesFileName + "</name>\n" +

                         "<Style id=\"sn_man2\">\n" +
                                "<IconStyle>\n" +
                                    "<scale>0.9</scale>\n" +
                                    "<Icon>\n" +
                                        "<href>http://maps.google.com/mapfiles/kml/shapes/man.png</href>\n" +
                                    "</Icon>\n" +
                                "</IconStyle>\n" +
                                "<LabelStyle>\n" +
                                    "<scale>0.9</scale>\n" +
                                "</LabelStyle>\n" +
                         "</Style>\n" +

                         "<Style id=\"sh_man0\">\n" +
                                "<IconStyle>\n" +
                                "	<scale>0.9</scale>\n" +
                                    "<Icon>\n" +
                                    "	<href>http://maps.google.com/mapfiles/kml/shapes/man.png</href>\n" +
                                    "</Icon>\n" +
                                "</IconStyle>\n" +
                                "<LabelStyle>\n" +
                                "	<scale>0.9</scale>\n" +
                                "</LabelStyle>\n" +
                         "</Style>\n" +

                         "<StyleMap id=\"msn_man\">\n" +
                            "<Pair>\n" +
                            "	<key>normal</key>\n" +
                            "	<styleUrl>#sn_man2</styleUrl>\n" +
                            "</Pair>\n" +
                            "<Pair>\n" +
                            "	<key>highlight</key>\n" +
                            "	<styleUrl>#sh_man0</styleUrl>\n" +
                            "</Pair>\n" +
                         "</StyleMap>\n" +


                         "<Style id=\"sn_woman1\">\n" +
                         "	<IconStyle>\n" +
                         "		<scale>0.9</scale>\n" +
                         "		<Icon>\n" +
                         "			<href>http://maps.google.com/mapfiles/kml/shapes/woman.png</href>\n" +
                         "		</Icon>\n" +
                         "	</IconStyle>\n" +
                         "  <LabelStyle>\n" +
                         "  	<scale>0.9</scale>\n" +
                         "  </LabelStyle>\n" +
                         "</Style>\n" +


                         "<Style id=\"sh_woman0\">\n" +
                         "	 <IconStyle>\n" +
                         "		<scale>0.9</scale>\n" +
                         "      <Icon>\n" +
                         "			<href>http://maps.google.com/mapfiles/kml/shapes/woman.png</href>\n" +
                         "		</Icon>\n" +
                         "	 </IconStyle>\n" +
                         "	 <LabelStyle>\n" +
                         "		<scale>0.9</scale>\n" +
                         "	 </LabelStyle>\n" +
                         "</Style>\n" +

                         "<StyleMap id=\"msn_woman\">\n" +
                         "<Pair>\n" +
                         "		<key>normal</key>\n" +
                         "		<styleUrl>#sn_woman1</styleUrl>\n" +
                         "</Pair>\n" +
                         "<Pair>\n" +
                         "		<key>highlight</key>\n" +
                         "		<styleUrl>#sh_woman0</styleUrl>\n" +
                         "</Pair>\n" +
                         "</StyleMap>");

            #endregion

            int i = 0;  // Counter for all the places exported.

            if (places)
            {
                #region places with no time information

                tw.WriteLine("<Folder>\n" +
                             "<name>" + Microsoft.FamilyShowLib.Properties.Resources.People + "</name>\n" +
                             "<open>0</open>\n" +
                             "<Folder>\n" +
                             "<name>" + Microsoft.FamilyShowLib.Properties.Resources.Births + "</name>\n" +
                             "<open>0</open>");

                if (births)
                {

                    foreach (Person p in peopleCollection)
                    {
                        if (!(hideliving && p.IsLiving))
                        {
                            if (!string.IsNullOrEmpty(p.BirthPlace) && p.Restriction != Restriction.Private)
                            {
                                string name = string.Empty;

                                tw.WriteLine("<Placemark>\n" +
                                            "	<name>" + p.FullName + "</name>\n" +
                                            "	<address>" + p.BirthPlace + "</address>\n" +
                                            "	<description>" + p.BirthPlace + "</description>");

                                if (p.Gender == Gender.Male)
                                    tw.WriteLine("<styleUrl>#msn_man</styleUrl>\n</Placemark>");
                                else
                                    tw.WriteLine("<styleUrl>#msn_woman</styleUrl>\n</Placemark>");

                                i++;
                            }
                        }

                    }

                    tw.WriteLine("</Folder>");
                }

                if (deaths)
                {

                    tw.WriteLine("<Folder>\n<name>" + Microsoft.FamilyShowLib.Properties.Resources.Deaths + "</name>\n" +
                                 "<open>0</open>");

                    foreach (Person p in peopleCollection)
                    {
                        if (!(hideliving && p.IsLiving))
                        {
                            if (!string.IsNullOrEmpty(p.DeathPlace) && p.Restriction != Restriction.Private)
                            {

                                tw.WriteLine("<Placemark>\n" +
                                             "<name>" + p.FullName + "</name>\n" +
                                             "<address>" + p.DeathPlace + "</address>\n" +
                                             "<description>" + p.DeathPlace + "</description>");

                                if (p.Gender == Gender.Male)
                                    tw.WriteLine("<styleUrl>#msn_man</styleUrl>\n</Placemark>");
                                else
                                    tw.WriteLine("<styleUrl>#msn_woman</styleUrl>\n</Placemark>");

                                i++;
                            }
                        }
                    }

                    tw.WriteLine("</Folder>");
                }

                if (burials)
                {
                    tw.WriteLine("<Folder>\n" +
                                 "<name>" + Microsoft.FamilyShowLib.Properties.Resources.Burials + "</name>\n" +
                                 "<open>0</open>");

                    foreach (Person p in peopleCollection)
                    {
                        if (!(hideliving && p.IsLiving))
                        {
                            if (!string.IsNullOrEmpty(p.BurialPlace) && p.Restriction != Restriction.Private)
                            {

                                tw.WriteLine("<Placemark>\n" +
                                             "<name>" + p.FullName + "</name>\n" +
                                             "<address>" + p.BurialPlace + "</address>\n" +
                                             "<description>" + p.BurialPlace + "</description>");

                                if (p.Gender == Gender.Male)
                                    tw.WriteLine("<styleUrl>#msn_man</styleUrl>\n</Placemark>");
                                else
                                    tw.WriteLine("<styleUrl>#msn_woman</styleUrl>\n</Placemark>");

                                i++;
                            }
                        }
                    }

                    tw.WriteLine("</Folder>");
                }

                if (cremations)
                {

                    tw.WriteLine("<Folder>\n" +
                                 "<name>" + Microsoft.FamilyShowLib.Properties.Resources.Cremations + "</name>\n" +
                                 "<open>0</open>");

                    foreach (Person p in peopleCollection)
                    {
                        if (!(hideliving && p.IsLiving))
                        {
                            if (!string.IsNullOrEmpty(p.CremationPlace) && p.Restriction != Restriction.Private)
                            {

                                tw.WriteLine("<Placemark>\n" +
                                             "<name>" + p.FullName + "</name>\n" +
                                             "<address>" + p.CremationPlace + "</address>\n" +
                                             "<description>" + p.CremationPlace + "</description>");

                                if (p.Gender == Gender.Male)
                                    tw.WriteLine("<styleUrl>#msn_man</styleUrl>\n</Placemark>");
                                else
                                    tw.WriteLine("<styleUrl>#msn_woman</styleUrl>\n</Placemark>");

                                i++;
                            }
                        }
                    }

                    tw.WriteLine("</Folder>");

                }

                if (marriages)
                {

                    tw.WriteLine("<Folder>\n" +
                                 "<name>" + Microsoft.FamilyShowLib.Properties.Resources.Marriages + "</name>\n" +
                                 "<open>0</open>");

                    foreach (Person p in peopleCollection)
                    {
                        if (!(hideliving && p.IsLiving))
                        {
                            if (p.Restriction != Restriction.Private)
                            {
                                foreach (Relationship rel in p.Relationships)
                                {

                                    if (rel.RelationshipType == RelationshipType.Spouse)
                                    {

                                        SpouseRelationship spouseRel = ((SpouseRelationship)rel);

                                        if (!string.IsNullOrEmpty(spouseRel.MarriagePlace))
                                        {
                                            tw.WriteLine("<Placemark>\n" +
                                                         "<name>" + p.FullName + "</name>\n" +
                                                         "<address>" + spouseRel.MarriagePlace + "</address>\n" +
                                                         "<description>" + spouseRel.MarriagePlace + "</description>");


                                            if (p.Gender == Gender.Male)
                                                tw.WriteLine("<styleUrl>#msn_man</styleUrl>\n</Placemark>");
                                            else
                                                tw.WriteLine("<styleUrl>#msn_woman</styleUrl>\n</Placemark>");

                                            i++;
                                        }
                                    }
                                }
                            }
                        }
                    }
                        
                    tw.WriteLine("</Folder>");
                }

                #endregion
            }
            else if(times)
            {
                #region place with time information

                tw.WriteLine("<Folder>\n" +
                             "<name>" + Microsoft.FamilyShowLib.Properties.Resources.Events + "</name>\n" +
                             "<open>0</open>");

                if (births)
                {

                    tw.WriteLine("<Folder>\n" +
                                 "<name>" + Microsoft.FamilyShowLib.Properties.Resources.Births + "</name>\n" +
                                 "<open>0</open>");

                    foreach (Person p in peopleCollection)
                    {
                        if (!(hideliving && p.IsLiving))
                        {

                            if (!string.IsNullOrEmpty(p.BirthPlace) && p.Restriction != Restriction.Private)
                            {

                                tw.WriteLine("<Placemark>\n" +
                                            "	<name>" + p.FullName + "</name>\n" +
                                            "	<address>" + p.BirthPlace + "</address>\n" +
                                            "	<description>" + p.BirthPlace + "</description>\n" +
                                            "   <TimeStamp>\n<when>" + p.YearOfBirth + "</when>\n</TimeStamp>");

                                if (p.Gender == Gender.Male)
                                    tw.WriteLine("<styleUrl>#msn_man</styleUrl>\n</Placemark>");
                                else
                                    tw.WriteLine("<styleUrl>#msn_woman</styleUrl>\n</Placemark>");

                                i++;
                            }
                        }

                    }

                    tw.WriteLine("</Folder>");
                }

                if (deaths)
                {

                    tw.WriteLine("<Folder>\n" +
                                 "<name>" + Microsoft.FamilyShowLib.Properties.Resources.Deaths + "</name>\n" +
                                 "<open>0</open>");

                    foreach (Person p in peopleCollection)
                    {
                        if (!(hideliving && p.IsLiving))
                        {
                            if (!string.IsNullOrEmpty(p.DeathPlace) && p.Restriction != Restriction.Private)
                            {

                                tw.WriteLine("<Placemark>\n" +
                                             "<name>" + p.FullName + "</name>\n" +
                                             "<address>" + p.DeathPlace + "</address>\n" +
                                             "<description>" + p.DeathPlace + "</description>\n" +
                                             "<TimeStamp>\n<when>" + p.YearOfDeath + "</when>\n</TimeStamp>");

                                if (p.Gender == Gender.Male)
                                    tw.WriteLine("<styleUrl>#msn_man</styleUrl>\n</Placemark>");
                                else
                                    tw.WriteLine("<styleUrl>#msn_woman</styleUrl>\n</Placemark>");

                                i++;
                            }
                        }
                    }

                    tw.WriteLine("</Folder>");

                }

                if (deaths)
                {

                    tw.WriteLine("<Folder>\n" +
                                 "<name>" + Microsoft.FamilyShowLib.Properties.Resources.Burials + "</name>\n" +
                                 "<open>0</open>");

                    foreach (Person p in peopleCollection)
                    {
                        if (!(hideliving && p.IsLiving))
                        {
                            string year = string.Empty;

                            if (p.BurialDate != null)
                                year = p.BurialDate.Value.Year.ToString();

                            if (!string.IsNullOrEmpty(p.BurialPlace) && p.Restriction != Restriction.Private && !string.IsNullOrEmpty(year))
                            {

                                tw.WriteLine("<Placemark>\n" +
                                             "<name>" + p.FullName + "</name>\n" +
                                             "<address>" + p.BurialPlace + "</address>\n" +
                                             "<description>" + p.BurialPlace + "</description>\n" +
                                             "<TimeStamp>\n<when>" + year + "</when>\n</TimeStamp>");

                                if (p.Gender == Gender.Male)
                                    tw.WriteLine("<styleUrl>#msn_man</styleUrl>\n</Placemark>");
                                else
                                    tw.WriteLine("<styleUrl>#msn_woman</styleUrl>\n</Placemark>");

                                i++;
                            }
                        }
                    }

                    tw.WriteLine("</Folder>");

                }

                if (cremations)
                {

                    tw.WriteLine("<Folder>\n" +
                                 "<name>" + Microsoft.FamilyShowLib.Properties.Resources.Cremations + "</name>\n" +
                                 "<open>0</open>");

                    foreach (Person p in peopleCollection)
                    {
                        if (!(hideliving && p.IsLiving))
                        {
                            string year = string.Empty;

                            if(p.CremationDate!=null)
                                year = p.CremationDate.Value.Year.ToString();

                            if (!string.IsNullOrEmpty(p.CremationPlace) && p.Restriction != Restriction.Private && !string.IsNullOrEmpty(year))
                            {

                                tw.WriteLine("<Placemark>\n" +
                                             "<name>" + p.FullName + "</name>\n" +
                                             "<address>" + p.CremationPlace + "</address>\n" +
                                             "<description>" + p.CremationPlace + "</description>\n" +
                                             "<TimeStamp>\n<when>" + year + "</when>\n</TimeStamp>");

                                if (p.Gender == Gender.Male)
                                    tw.WriteLine("<styleUrl>#msn_man</styleUrl>\n</Placemark>");
                                else
                                    tw.WriteLine("<styleUrl>#msn_woman</styleUrl>\n</Placemark>");

                                i++;
                            }
                        }
                    }

                    tw.WriteLine("</Folder>");

                }

                if (marriages)
                {

                    tw.WriteLine("<Folder>\n" +
                                 "<name>" + Microsoft.FamilyShowLib.Properties.Resources.Marriages + "</name>\n" +
                                 "<open>0</open>");

                    foreach (Person p in peopleCollection)
                    {
                        if (!(hideliving && p.IsLiving))
                        {
                            if (p.Restriction != Restriction.Private)
                            {
                                foreach (Relationship rel in p.Relationships)
                                {

                                    if (rel.RelationshipType == RelationshipType.Spouse)
                                    {

                                        SpouseRelationship spouseRel = ((SpouseRelationship)rel);

                                        if (!string.IsNullOrEmpty(spouseRel.MarriagePlace))
                                        {
                                            string date = string.Empty;

                                            if (spouseRel.MarriageDate != null)
                                                date = spouseRel.MarriageDate.Value.Year.ToString();

                                            tw.WriteLine("<Placemark>\n" +
                                                         "<name>" + p.FullName + "</name>\n" +
                                                         "<address>" + spouseRel.MarriagePlace + "</address>\n" +
                                                         "<description>" + spouseRel.MarriagePlace + "</description>\n" +
                                                         "<TimeStamp>\n<when>" + date + "</when>\n</TimeStamp>");


                                            if (p.Gender == Gender.Male)
                                                tw.WriteLine("<styleUrl>#msn_man</styleUrl>\n</Placemark>");
                                            else
                                                tw.WriteLine("<styleUrl>#msn_woman</styleUrl>\n</Placemark>");

                                            i++;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    tw.WriteLine("</Folder>");
                }
                #endregion
            }
            else if (lifespans)
            {
                #region lifespans

                tw.WriteLine("<Folder>\n" +
                             "<name>" + Microsoft.FamilyShowLib.Properties.Resources.People + "</name>\n" +
                             "<open>0</open>");

                foreach (Person p in peopleCollection)
                {
                    if (!(hideliving && p.IsLiving))
                    {
                        if (p.Restriction != Restriction.Private)
                        {
                            string place = string.Empty;

                            if (!string.IsNullOrEmpty(p.BirthPlace) && string.IsNullOrEmpty(place))
                                place = p.BirthPlace;

                            if (!string.IsNullOrEmpty(p.DeathPlace) && string.IsNullOrEmpty(place))
                                place = p.DeathPlace;

                            tw.WriteLine("<Placemark>\n" +
                                         "<name>" + p.FullName + "</name>\n" +
                                         "<address>" + place + "</address>\n" +
                                         "<description>" + place + "</description>\n" +
                                         "<TimeSpan>");
                            if (!string.IsNullOrEmpty(p.YearOfBirth))
                                tw.WriteLine("<begin>" + p.YearOfBirth + "</begin>");
                            if (!string.IsNullOrEmpty(p.YearOfBirth))
                                tw.WriteLine("<end>" + p.YearOfDeath + "</end>");

                            tw.WriteLine("</TimeSpan>");

                            if (p.Gender == Gender.Male)
                                tw.WriteLine("<styleUrl>#msn_man</styleUrl>\n</Placemark>");
                            else
                                tw.WriteLine("<styleUrl>#msn_woman</styleUrl>\n</Placemark>");

                            i++;
                        }
                    }

                }

                #endregion
            }

            tw.WriteLine("</Folder>\n" +
                         "</Document>\n" +
                         "</kml>");

            tw.Close();

            string[] summary = new string[2];

            summary[0] = i.ToString() + " " + Microsoft.FamilyShowLib.Properties.Resources.PlacesExported;
            summary[1] = fileName.ToString();

            if (i == 0)
            {
                File.Delete(fileName);
                summary[0] = Microsoft.FamilyShowLib.Properties.Resources.NoPlaces;
                summary[1] = "No file";
            }

            return summary;
        }

        #endregion

    }
}

