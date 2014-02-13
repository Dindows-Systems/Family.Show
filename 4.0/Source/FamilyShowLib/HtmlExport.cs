/*
 * Exports data from the People collection to a Html based report.
 * 
 * There are 5 different export methods:
 * 1. Export all
 * 2. Export immediate family
 * 3. Export current person
 * 4. Export based on a filter
 * 5. Export based on generations
 * 
 * All the html containers are in the html document structures region and 
 * are written to the XHtml 1.0 Transitional Standard and CSS2 standard.
 * For further information see the http://www.w3.org/
 * 
 * tw.WriteLine(Microsoft.FamilyShowLib.Properties.Resources.Tree + " <b>" + Path.GetFileNameWithoutExtension(familyxFileName) + "</b><br/>"); is utilised to provide some helper funtions for reports.  
 * for example to hide/show a note.  For users who have Java disabled, 
 * the Html file is written so that all notes will be shown in the file.
 * 
 * CSS is used to style the report.  CSS hides any helper buttons during 
 * print.
*/

using System;
using System.Globalization;
using System.IO;

namespace Microsoft.FamilyShowLib
{
    public class HtmlExport
    {

        #region fields

        private TextWriter tw;

        #endregion

        #region export methods

        /// <summary>
        /// Export all the data from the People collection to the specified html file.
        /// </summary>
        public void ExportAll(PeopleCollection peopleCollection, SourceCollection sourceCollection, RepositoryCollection repositoryCollection, string htmlFilePath, string familyxFileName, bool privacy, bool sourcesbool)
        {
            string filename = Path.GetFileName(htmlFilePath);
            tw = new StreamWriter(filename);
            //write the necessary html code for a html document
            tw.WriteLine(Header());
            tw.WriteLine(JavaScripts());
            tw.WriteLine(CSS());
            tw.WriteLine(CSSprinting(15));

            tw.WriteLine("</head><body onload=\"javascript:showhideall('hide_all')\">");
            tw.WriteLine(Buttons());

            tw.WriteLine("<h2>"+ Microsoft.FamilyShowLib.Properties.Resources.FamilyShow + "</h2>");
            tw.WriteLine("<i>" + Microsoft.FamilyShowLib.Properties.Resources.SummaryOfPeople + "</i><br/><br/>");

            if (!string.IsNullOrEmpty(familyxFileName))
                tw.WriteLine("<b>" + Path.GetFileNameWithoutExtension(familyxFileName) + " " + Microsoft.FamilyShowLib.Properties.Resources.FamilyTree + "</b><br/>");

            tw.WriteLine(Microsoft.FamilyShowLib.Properties.Resources.NumberOfPeople + " <b>" + peopleCollection.Count + "</b><br/><br/>");

            tw.WriteLine(NormalTableColumns());

            //write all people to the html file
            foreach (Person p in peopleCollection)
            {
                string[,] sourceArray = new string[1, 7];

                if (sourcesbool == true)
                    sourceArray = Sources(p);

                if (p.IsLiving == true && privacy == true && p.Restriction != Restriction.Private) //quick privacy option
                    tw.WriteLine("<tr id=\"id_" + p.Id + "\" class=\"person\"><td>" + Microsoft.FamilyShowLib.Properties.Resources.Living + "</td><td>" + p.LastName + "</td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td></tr>");
                else if (p.Restriction == Restriction.Private) //a private record should not be exported
                    tw.WriteLine("<tr id=\"id_" + p.Id + "\" class=\"person\"><td>" + Microsoft.FamilyShowLib.Properties.Resources.PrivateRecord + "</td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td></tr>");
                else
                {
                    if (p.Note != null)
                        tw.WriteLine(dataWithNote(p,sourceArray));
                    else
                        tw.WriteLine(dataWithoutNote(p, sourceArray));
                }
            }

            if (sourcesbool == true)
            {
                if(sourceCollection.Count>0)
                {
                tw.WriteLine("</table>");
                tw.WriteLine(NormalSourceColumns());
                foreach (Source s in sourceCollection)
                    tw.WriteLine("<tr><td><a name=\"" + s.Id + "\"></a>" + s.Id + "</td><td>" + s.SourceName + "</td><td>" + s.SourceAuthor + "</td><td>" + s.SourcePublisher + "</td><td>" + s.SourceNote + "</td><td>" + s.SourceRepository + "</td></tr>");
                }
                if (repositoryCollection.Count > 0)
                {
                    tw.WriteLine("</table>");
                    tw.WriteLine(NormalRepositoryColumns());
                    foreach (Repository r in repositoryCollection)
                        tw.WriteLine("<tr><td><a name=\"" + r.Id + "\"></a>" + r.Id + "</td><td>" + r.RepositoryName + "</td><td>" + r.RepositoryAddress + "</td></tr>");
                }
            }

            tw.WriteLine(Footer());
            tw.Close();
        }

        /// <summary>
        /// Export current person and immediate family data from the People collection to the specified html file.
        /// </summary>
        public void ExportDirect(PeopleCollection peopleCollection, SourceCollection sourceCollection, RepositoryCollection repositoryCollection, string htmlFilePath, string familyxFileName, bool privacy, bool sourcesbool)
        {
            PeopleCollection pc = new PeopleCollection();
            string filename = Path.GetFileName(htmlFilePath);
            tw = new StreamWriter(filename);
            
            //add the current person, their parents, spouses, previous spouses, children and siblings to the export people collection

            Person primaryPerson = peopleCollection.Current;
                pc.Add(primaryPerson);
            foreach (Person parent in primaryPerson.Parents)
                pc.Add(parent);
            foreach (Person sibling in primaryPerson.Siblings)
                pc.Add(sibling);
            foreach (Person spouse in primaryPerson.Spouses)
                pc.Add(spouse);
            foreach (Person previousSpouse in primaryPerson.PreviousSpouses)
                pc.Add(previousSpouse);
            foreach (Person child in primaryPerson.Children)
                pc.Add(child);

            //write the necessary html code for a html document

            tw.WriteLine(Header());
           tw.WriteLine(JavaScripts());
            tw.WriteLine(CSS());
            tw.WriteLine(CSSprinting(16));

            tw.WriteLine("</head><body onload=\"javascripts\">");
            tw.WriteLine(Buttons());
            tw.WriteLine("<h2>" + Microsoft.FamilyShowLib.Properties.Resources.FamilyShow + "</h2>");
            tw.WriteLine("<i>" + Microsoft.FamilyShowLib.Properties.Resources.SummaryOfPeople + "</i><br/><br/>");

            if (!string.IsNullOrEmpty(familyxFileName))
                tw.WriteLine("<b>" + Path.GetFileNameWithoutExtension(familyxFileName) + " " + Microsoft.FamilyShowLib.Properties.Resources.FamilyTree + "</b><br/>");
            
            int i = pc.Count; 

            if (i < 0)
                i = 0;

            tw.WriteLine(CurrentPersonString(peopleCollection.Current,privacy));
            tw.WriteLine(Microsoft.FamilyShowLib.Properties.Resources.SummaryOfPeople + " <b>" + i + "</b><br/><br/>");

            tw.WriteLine("<table id=\"qstable\" border=\"1\" rules=\"all\" frame=\"box\">\n" +
            "<thead>\n" +
            "<tr>\n" +
            "<th width=\"5%\">" + Microsoft.FamilyShowLib.Properties.Resources.RelationshipToCurrentPerson + "</th> \n" +
            "<th width=\"5%\">" + Microsoft.FamilyShowLib.Properties.Resources.Names + "</th> \n" +
            "<th width=\"5%\">" + Microsoft.FamilyShowLib.Properties.Resources.Surname + "</th>\n" +
            "<th width=\"5%\">" + Microsoft.FamilyShowLib.Properties.Resources.Age + "</th>\n" +
            "<th width=\"5%\">" + Microsoft.FamilyShowLib.Properties.Resources.BirthDate + "</th>\n" +
            "<th width=\"8.5%\">" + Microsoft.FamilyShowLib.Properties.Resources.BirthPlace + "</th>\n" +
            "<th width=\"5%\">" + Microsoft.FamilyShowLib.Properties.Resources.DeathDate + "</th> \n" +
            "<th width=\"8.5%\">" + Microsoft.FamilyShowLib.Properties.Resources.DeathPlace + "</th>\n" +
            "<th width=\"8.5%\">" + Microsoft.FamilyShowLib.Properties.Resources.Occupation + "</th>\n" +
            "<th width=\"8.5%\">" + Microsoft.FamilyShowLib.Properties.Resources.Education + "</th>\n" +
            "<th width=\"8.5%\">" + Microsoft.FamilyShowLib.Properties.Resources.Religion + "</th>\n" +
            "<th width=\"8.5%\">" + Microsoft.FamilyShowLib.Properties.Resources.BurialPlace + "</th>\n" +
            "<th width=\"5%\">" + Microsoft.FamilyShowLib.Properties.Resources.BurialDate + "</th>\n" +
            "<th width=\"8.5%\">" + Microsoft.FamilyShowLib.Properties.Resources.CremationPlace + "</th>\n" +
            "<th width=\"5%\">" + Microsoft.FamilyShowLib.Properties.Resources.CremationDate + "</th>\n" +
            "<th width=\"5%\">" + Microsoft.FamilyShowLib.Properties.Resources.AdditionalInfo + "</th>\n" +
            "</tr>\n" +
            "</thead>");

            //write all the appropriate people to the html file including relationship to current person
            foreach (Person p in pc)
            {
                string[,] sourceArray = new string[1, 7];

                if (sourcesbool == true)
                    sourceArray = Sources(p);

                foreach (Relationship rel in peopleCollection.Current.Relationships)
                {
                    if (rel.RelationTo == p)
                    {
                        string relationship = string.Empty;
                        string relstring = string.Empty;
                        string relstringmodifier = string.Empty;

                        //for parent relationships, also specify if natural or adopted parent
                        if (rel.RelationshipType == RelationshipType.Parent)
                        {
                            ParentRelationship parentRel = ((ParentRelationship)rel);

                            relstring = Microsoft.FamilyShowLib.Properties.Resources.Parent;

                            if(parentRel.ParentChildModifier == ParentChildModifier.Natural)
                                relstringmodifier = Microsoft.FamilyShowLib.Properties.Resources.Natural;
                            if (parentRel.ParentChildModifier == ParentChildModifier.Adopted)
                                relstringmodifier = Microsoft.FamilyShowLib.Properties.Resources.Adopted;
                            if (parentRel.ParentChildModifier == ParentChildModifier.Foster)
                                relstringmodifier = Microsoft.FamilyShowLib.Properties.Resources.Foster;

                            relationship = relstring + " (" + relstringmodifier + ")";

                        }
                        else if (rel.RelationshipType == RelationshipType.Child)
                            relationship = Microsoft.FamilyShowLib.Properties.Resources.Child;
                        else if (rel.RelationshipType == RelationshipType.Spouse)
                            relationship = Microsoft.FamilyShowLib.Properties.Resources.Spouse;
                        else if (rel.RelationshipType == RelationshipType.Sibling)
                            relationship = Microsoft.FamilyShowLib.Properties.Resources.Sibling;

                        if (p.IsLiving == true && privacy == true && p.Restriction != Restriction.Private) //quick privacy option
                            tw.WriteLine("<tr id=\"id_" + p.Id + "\" class=\"person\"><td>" + relationship + "</td><td>" + Microsoft.FamilyShowLib.Properties.Resources.Living + "</td><td>" + p.LastName + "</td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td></tr>");
                        else if (p.Restriction == Restriction.Private) //a private record should not be exported
                            tw.WriteLine("<tr id=\"id_" + p.Id + "\" class=\"person\"><td>" + Microsoft.FamilyShowLib.Properties.Resources.PrivateRecord + "</td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td></tr>");
                        else
                        {
                            if (p.Note != null)
                            {
                                tw.WriteLine("<tr id=\"id_" + p.Id + "\" class=\"personhighlight\"><td>" + relationship + "</td><td>" + p.FirstName + "</td><td>"
                                    + p.LastName + "</td><td>" + p.Age + "</td><td>"
                                    + p.BirthDateDescriptor + " " + dateformat(p.BirthDate) + sourceArray[0, 0] + "</td><td>"
                                    + p.BirthPlace + sourceArray[0, 0] + "</td><td>"
                                    + p.DeathDateDescriptor + " " + dateformat(p.DeathDate) + sourceArray[0, 1] + "</td><td>"
                                    + p.DeathPlace + sourceArray[0, 1] + "</td><td>"
                                    + p.Occupation + sourceArray[0, 2] + "</td><td>"
                                    + p.Education + sourceArray[0, 3] + "</td><td>"
                                    + p.Religion + sourceArray[0, 4] + "</td><td>"
                                    + p.BurialPlace + sourceArray[0, 5] + "</td><td>"
                                    + p.BurialDateDescriptor + " " + dateformat(p.BurialDate) + sourceArray[0, 5] + "</td><td>"
                                    + p.CremationPlace + sourceArray[0, 6] + "</td><td>"
                                    + p.CremationDateDescriptor + " " + dateformat(p.CremationDate) + sourceArray[0, 6] + "</td>"
                                    + "</td><td><p class=\"notelink\">[<a href=\"javascript:showhide('id_" + p.Id + "')\">Note</a>]</p></td></tr>");

                                tw.WriteLine("<tr id=\"note_id_" + p.Id + "\" class=\"noteshown\"><td colspan=\"16\"><b>"+Microsoft.FamilyShowLib.Properties.Resources.Note + "</b>: <pre>" + p.Note + "</pre></td></tr>");

                            }
                            else
                            {
                                tw.WriteLine("<tr id=\"id_" + p.Id + "\" class=\"person\"><td>" + relationship + "</td><td>" + p.FirstName + "</td><td>"   
                                + p.LastName + "</td><td>" + p.Age + "</td><td>"
                                + p.BirthDateDescriptor + " " + dateformat(p.BirthDate) + sourceArray[0, 0] + "</td><td>"
                                + p.BirthPlace + sourceArray[0, 0] + "</td><td>"
                                + p.DeathDateDescriptor + " " + dateformat(p.DeathDate) + sourceArray[0, 1] + "</td><td>"
                                + p.DeathPlace + sourceArray[0, 1] + "</td><td>"
                                + p.Occupation + sourceArray[0, 2] + "</td><td>"
                                + p.Education + sourceArray[0, 3] + "</td><td>"
                                + p.Religion + sourceArray[0, 4] + "</td><td>"
                                + p.BurialPlace + sourceArray[0, 5] + "</td><td>"
                                + p.BurialDateDescriptor + " " + dateformat(p.BurialDate) + sourceArray[0, 5] + "</td><td>"
                                + p.CremationPlace + sourceArray[0, 6] + "</td><td>"
                                + p.CremationDateDescriptor + " " + dateformat(p.CremationDate) + sourceArray[0, 6] + "</td><td></td></tr>");
                            }
                        }
                    }
                }
            }
            if (sourcesbool == true)
            {
                if (sourceCollection.Count > 0)
                {
                    tw.WriteLine("</table>");
                    tw.WriteLine(NormalSourceColumns());
                    foreach (Source s in sourceCollection)
                        tw.WriteLine("<tr><td><a name=\"" + s.Id + "\"></a>" + s.Id + "</td><td>" + s.SourceName + "</td><td>" + s.SourceAuthor + "</td><td>" + s.SourcePublisher + "</td><td>" + s.SourceNote + "</td><td>" + s.SourceRepository + "</td></tr>");
                }
                if (repositoryCollection.Count > 0)
                {
                    tw.WriteLine("</table>");
                    tw.WriteLine(NormalRepositoryColumns());
                    foreach (Repository r in repositoryCollection)
                        tw.WriteLine("<tr><td><a name=\"" + r.Id + "\"></a>" + r.Id + "</td><td>" + r.RepositoryName + "</td><td>" + r.RepositoryAddress + "</td></tr>");
                }
            }
            
            tw.WriteLine(Footer());
            tw.Close();
        }

        /// <summary>
        /// Export current person to the specified html file.
        /// </summary>
        public void ExportCurrent(PeopleCollection peopleCollection, SourceCollection sourceCollection, RepositoryCollection repositoryCollection, string htmlFilePath, string familyxFileName, bool privacy, bool sourcesbool)
        {
            PeopleCollection pc = new PeopleCollection();
            string filename = Path.GetFileName(htmlFilePath);
            tw = new StreamWriter(filename);

            //add the current person, their parents, spouses, children and siblings to the export people collection

            Person primaryPerson = peopleCollection.Current;
            pc.Add(primaryPerson);

            //write the necessary html code for a html document

            tw.WriteLine(Header());
           tw.WriteLine(JavaScripts());
            tw.WriteLine(CSS());
            tw.WriteLine(CSSprinting(15));

            tw.WriteLine("</head><body>");
            tw.WriteLine("<h2>" + Microsoft.FamilyShowLib.Properties.Resources.FamilyShow + "</h2>");
            tw.WriteLine("<i>" + Microsoft.FamilyShowLib.Properties.Resources.SummaryOfPeople + "</i><br/><br/>");

            if (!string.IsNullOrEmpty(familyxFileName))
                tw.WriteLine("<b>" + Path.GetFileNameWithoutExtension(familyxFileName) + " " + Microsoft.FamilyShowLib.Properties.Resources.FamilyTree + "</b><br/>");

            tw.WriteLine(CurrentPersonString(peopleCollection.Current, privacy));
            tw.WriteLine(Microsoft.FamilyShowLib.Properties.Resources.NumberOfPeople + " <b>" + pc.Count + "</b><br/><br/>");
            tw.WriteLine(NormalTableColumns());


            //write all the appropriate people to the html file including relationship to current person
            foreach (Person p in pc)
            {

                string[,] sourceArray = new string[1, 7];

                if (sourcesbool == true)
                    sourceArray = Sources(p);

                if (p.IsLiving == true && privacy == true && p.Restriction != Restriction.Private) //quick privacy option
                    tw.WriteLine("<tr id=\"id_" + p.Id + "\" class=\"person\"><td>" + Microsoft.FamilyShowLib.Properties.Resources.Living + "</td><td>" + p.LastName + "</td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td></tr>");
                else if (p.Restriction == Restriction.Private) //a private record should not be exported
                    tw.WriteLine("<tr id=\"id_" + p.Id + "\" class=\"person\"><td>" + Microsoft.FamilyShowLib.Properties.Resources.PrivateRecord + "</td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td></tr>");
                else
                {
                        if (p.Note != null)
                            tw.WriteLine(dataWithNote(p, sourceArray));
                        else
                            tw.WriteLine(dataWithoutNote(p, sourceArray));
                }
            }

            if (sourcesbool == true)
            {
                if (sourceCollection.Count > 0)
                {
                    tw.WriteLine("</table>");
                    tw.WriteLine(NormalSourceColumns());
                    foreach (Source s in sourceCollection)
                        tw.WriteLine("<tr><td><a name=\"" + s.Id + "\"></a>" + s.Id + "</td><td>" + s.SourceName + "</td><td>" + s.SourceAuthor + "</td><td>" + s.SourcePublisher + "</td><td>" + s.SourceNote + "</td><td>" + s.SourceRepository + "</td></tr>");
                }
                if (repositoryCollection.Count > 0)
                {
                    tw.WriteLine("</table>");
                    tw.WriteLine(NormalRepositoryColumns());
                    foreach (Repository r in repositoryCollection)
                        tw.WriteLine("<tr><td><a name=\"" + r.Id + "\"></a>" + r.Id + "</td><td>" + r.RepositoryName + "</td><td>" + r.RepositoryAddress + "</td></tr>");
                }
            }
            tw.WriteLine(Footer());
            tw.Close();
        }

        /// <summary>
        /// Export people to the report based on a filter to the specified html file.
        /// </summary>
        public void ExportFilter(PeopleCollection peopleCollection, SourceCollection sourceCollection, RepositoryCollection repositoryCollection, string searchtext,string searchfield, int searchfieldindex, string htmlFilePath, string familyxFileName, bool privacy, bool sourcesbool)
        {
            PeopleCollection pc = new PeopleCollection();
            string filename = Path.GetFileName(htmlFilePath);
            tw = new StreamWriter(filename);

            #region filter logic

            if (searchtext!=null)
            {
                switch (searchfieldindex)
                {
                    case 0:
                        foreach (Person p in peopleCollection)
                        {
                            if (p.FullName.Contains(searchtext))
                                pc.Add(p);
                        }
                        break;
                    case 1:
                            string searchOperator = string.Empty;
                            string searchtext1 = searchtext;
                            //allow for operators = > < and combinations
                            if (searchtext.StartsWith(">"))
                            {
                                searchOperator = ">";
                                searchtext1 = searchtext.Replace(">", "");
                            }

                            else if (searchtext.StartsWith("<"))
                            {
                                searchOperator = "<";
                                searchtext1 = searchtext.Replace("<", "");
                            }

                            else if (searchtext.StartsWith("="))
                            {
                                searchOperator = "=";
                                searchtext1 = searchtext.Replace("=", "");
                            }

                            else if (searchtext.StartsWith(">="))
                            {
                                searchOperator = ">=";
                                searchtext1 = searchtext.Replace(">=", "");
                            }

                            else if (searchtext.StartsWith("<="))
                            {
                                searchOperator = "<=";
                                searchtext1 = searchtext.Replace("<=", "");
                            }
                            
                            int i = -1;

                            try
                            {
                            i = Convert.ToInt32(searchtext1);
                            }
                            catch { }

                            foreach (Person p in peopleCollection)
                        {

                            if (i != -1 && string.IsNullOrEmpty(searchOperator))
                            {
                                if (p.Age==i)
                                    pc.Add(p);
                            }

                            else if (i != -1 && searchOperator == "=")
                            {
                                if (p.Age == i)
                                    pc.Add(p);
                            }

                            else if (i != -1 && searchOperator == "<")
                            {
                                if (p.Age < i)
                                    pc.Add(p);
                            }

                            else if (i != -1 && searchOperator == ">")
                            {
                                if (p.Age > i)
                                    pc.Add(p);
                            }

                            else if (i != -1 && searchOperator == "<=")
                            {
                                if (p.Age <= i)
                                    pc.Add(p);
                            }

                            else if (i != -1 && searchOperator == ">=")
                            {
                                if (p.Age >= i)
                                    pc.Add(p);
                            }

                        }
                        break;
                    case 2:
                        foreach (Person p in peopleCollection)
                        {
                            if (p.DeathDate.ToString().Contains(searchtext))
                                pc.Add(p);
                        }
                        break;
                    case 3:
                        foreach (Person p in peopleCollection)
                        {
                            if (p.BirthDate.ToString().Contains(searchtext))
                                pc.Add(p);
                        }
                        break;
                    case 4:
                        foreach (Person p in peopleCollection)
                        {
                            if (p.BirthPlace.Contains(searchtext))
                                pc.Add(p);
                        }
                        break;
                    case 5:
                        foreach (Person p in peopleCollection)
                        {
                            if (p.DeathPlace.Contains(searchtext))
                                pc.Add(p);
                        }
                        break;
                    case 6:
                        foreach (Person p in peopleCollection)
                        {
                            if (p.Occupation.Contains(searchtext))
                                pc.Add(p);
                        }
                        break;
                    case 7:
                        foreach (Person p in peopleCollection)
                        {
                            if (p.Education.Contains(searchtext))
                                pc.Add(p);
                        }
                        break;
                    case 8:
                        foreach (Person p in peopleCollection)
                        {
                            if (p.Religion.Contains(searchtext))
                                pc.Add(p);
                        }
                        break;
                }
            }

            #endregion

            //write the necessary html code for a html document
            
            tw.WriteLine(Header());
            tw.WriteLine(JavaScripts());
            tw.WriteLine(CSS());
            tw.WriteLine(CSSprinting(15));

            tw.WriteLine("</head><body onload=\"javascript:showhideall('hide_all')\">");
            tw.WriteLine(Buttons());
            tw.WriteLine("<h2>" + Microsoft.FamilyShowLib.Properties.Resources.FamilyShow + "</h2>");
            tw.WriteLine("<i>" + Microsoft.FamilyShowLib.Properties.Resources.SummaryOfPeople + "</i><br/><br/>");

            if (!string.IsNullOrEmpty(familyxFileName))
                tw.WriteLine("<b>" + Path.GetFileNameWithoutExtension(familyxFileName) + " " + Microsoft.FamilyShowLib.Properties.Resources.FamilyTree + "</b><br/>");

            tw.WriteLine(Microsoft.FamilyShowLib.Properties.Resources.FilteredOn + " <b>" + searchfield + "</b><br/>");
            tw.WriteLine(searchfield + ": <b>" + searchtext + "</b><br/>");
            tw.WriteLine(Microsoft.FamilyShowLib.Properties.Resources.NumberOfPeople + " <b>" + pc.Count + "</b><br/><br/>");
            tw.WriteLine(NormalTableColumns());

            //write all the appropriate people to the html file including relationship to current person
            foreach (Person p in pc)
            {
                string[,] sourceArray = new string[1, 7];

                if (sourcesbool == true)
                    sourceArray = Sources(p);

                if (p.IsLiving == true && privacy == true && p.Restriction != Restriction.Private) //quick privacy option
                    tw.WriteLine("<tr id=\"id_" + p.Id + "\" class=\"person\"><td>" + Microsoft.FamilyShowLib.Properties.Resources.Living + "</td><td>" + p.LastName + "</td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td></tr>");
                else if (p.Restriction == Restriction.Private) //a private record should not be exported
                    tw.WriteLine("<tr id=\"id_" + p.Id + "\" class=\"person\"><td>" + Microsoft.FamilyShowLib.Properties.Resources.PrivateRecord + "</td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td></tr>");
                else
                {
                        if (p.Note != null)
                            tw.WriteLine(dataWithNote(p, sourceArray));
                        else
                            tw.WriteLine(dataWithoutNote(p, sourceArray));
                }
            }

            if (sourcesbool == true)
            {
                if (sourceCollection.Count > 0)
                {
                    tw.WriteLine("</table>");
                    tw.WriteLine(NormalSourceColumns());
                    foreach (Source s in sourceCollection)
                        tw.WriteLine("<tr><td><a name=\"" + s.Id + "\"></a>" + s.Id + "</td><td>" + s.SourceName + "</td><td>" + s.SourceAuthor + "</td><td>" + s.SourcePublisher + "</td><td>" + s.SourceNote + "</td><td>" + s.SourceRepository + "</td></tr>");
                }
                if (repositoryCollection.Count > 0)
                {
                    tw.WriteLine("</table>");
                    tw.WriteLine(NormalRepositoryColumns());
                    foreach (Repository r in repositoryCollection)
                        tw.WriteLine("<tr><td><a name=\"" + r.Id + "\"></a>" + r.Id + "</td><td>" + r.RepositoryName + "</td><td>" + r.RepositoryAddress + "</td></tr>");
                }
            }
            tw.WriteLine(Footer());
            tw.Close();
        }

        /// <summary>
        /// Export current person and given number of ancestor and descendant generations
        /// 1 ancestor generation is parents
        /// 2 ancestor generations is parents and grandparents and so on up to 5 generations.
        /// 1 descendent generation is children
        /// 2 descendent generations is grandchildren and so on up to 5 generations.
        /// If 0, export current person and their siblings and spouse
        /// In summary, this method exports what it visible in the default tree.
        /// </summary>
        public void ExportGenerations(PeopleCollection peopleCollection, SourceCollection sourceCollection, RepositoryCollection repositoryCollection, decimal ancestors, decimal descendants, string htmlFilePath, string familyxFileName, bool privacy, bool sourcesbool)
        {
            PeopleCollection pc = new PeopleCollection();
            string filename = Path.GetFileName(htmlFilePath);
            tw = new StreamWriter(filename);

            //add the current person, their spouses and siblings to the export people collection and then repeat for each specified generation

            Person primaryPerson = peopleCollection.Current;
            pc.Add(primaryPerson);

            //0 generations
            foreach (Person sibling in primaryPerson.Siblings)
                pc.Add(sibling);

            foreach (Person spouse in primaryPerson.Spouses)
                pc.Add(spouse);

            foreach (Person previousSpouse in primaryPerson.PreviousSpouses)
                pc.Add(previousSpouse);

            #region ancestors

            //1 ancestor generation
            if (ancestors >= Convert.ToDecimal(1))
            {
                foreach (Person parent in primaryPerson.Parents)
                {
                    foreach (Person p in ancestorGenerations(parent))
                    {
                        if (!pc.Contains(p))
                            pc.Add(p);
                    }

                    //2 ancestor generations
                    if (ancestors >= Convert.ToDecimal(2))
                    {
                        foreach (Person grandparent in parent.Parents)
                        {

                            foreach (Person p in ancestorGenerations(grandparent))
                            {
                                if (!pc.Contains(p))
                                    pc.Add(p);
                            }

                            //3 ancestor generations
                            if (ancestors >= Convert.ToDecimal(3))
                            {
                                foreach (Person greatgrandparent in grandparent.Parents)
                                {

                                    foreach (Person p in ancestorGenerations(greatgrandparent))
                                    {
                                        if (!pc.Contains(p))
                                            pc.Add(p);
                                    }

                                    //4 ancestor generations
                                    if (ancestors >= Convert.ToDecimal(4))
                                    {
                                        foreach (Person greatgreatgrandparent in greatgrandparent.Parents)
                                        {

                                            foreach (Person p in ancestorGenerations(greatgreatgrandparent))
                                            {
                                                if (!pc.Contains(p))
                                                    pc.Add(p);
                                            }

                                            //5 ancestor generations
                                            if (ancestors >= Convert.ToDecimal(5))
                                            {
                                                foreach (Person greatgreatgreatgrandparent in greatgreatgrandparent.Parents)
                                                {

                                                    foreach (Person p in ancestorGenerations(greatgreatgreatgrandparent))
                                                    {
                                                        if (!pc.Contains(p))
                                                            pc.Add(p);
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
                        if (!pc.Contains(p))
                            pc.Add(p);
                    }

                    //2 descendant generations
                    if (descendants >= Convert.ToDecimal(2))
                    {
                        foreach (Person grandchild in child.Children)
                        {

                            foreach (Person p in descendentGenerations(grandchild))
                            {
                                if (!pc.Contains(p))
                                    pc.Add(p);
                            }

                            //3 descendent generations
                            if (descendants >= Convert.ToDecimal(3))
                            {
                                foreach (Person greatgrandchild in grandchild.Children)
                                {
                                    foreach (Person p in descendentGenerations(greatgrandchild))
                                    {
                                        if (!pc.Contains(p))
                                            pc.Add(p);
                                    }

                                    //4 descendent generations
                                    if (descendants >= Convert.ToDecimal(4))
                                    {
                                        foreach (Person greatgreatgrandchild in greatgrandchild.Children)
                                        {
                                            foreach (Person p in descendentGenerations(greatgreatgrandchild))
                                            {
                                                if (!pc.Contains(p))
                                                    pc.Add(p);
                                            }

                                            //5 descendent generations
                                            if (descendants >= Convert.ToDecimal(5))
                                            {
                                                foreach (Person greatgreatgreatgrandchild in greatgreatgrandchild.Children)
                                                {
                                                    foreach (Person p in descendentGenerations(greatgreatgreatgrandchild))
                                                    {
                                                        if (!pc.Contains(p))
                                                            pc.Add(p);
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

            //write the necessary html code for a html document

            tw.WriteLine(Header());
            tw.WriteLine(JavaScripts());
            tw.WriteLine(CSS());
            tw.WriteLine(CSSprinting(15));

            tw.WriteLine("</head><body onload=\"javascript:showhideall('hide_all')\">");
            tw.WriteLine(Buttons());
            tw.WriteLine("<h2>" + Microsoft.FamilyShowLib.Properties.Resources.FamilyShow + "</h2>");
            tw.WriteLine("<i>" + Microsoft.FamilyShowLib.Properties.Resources.SummaryOfPeople + "</i><br/><br/>");

            if (!string.IsNullOrEmpty(familyxFileName))
                tw.WriteLine("<b>" + Path.GetFileNameWithoutExtension(familyxFileName) + " " + Microsoft.FamilyShowLib.Properties.Resources.FamilyTree + "</b><br/>");

            tw.WriteLine(CurrentPersonString(peopleCollection.Current, privacy));
            tw.WriteLine(Microsoft.FamilyShowLib.Properties.Resources.AncestralGenerations + " <b>" + ancestors + "</b><br/>");
            tw.WriteLine(Microsoft.FamilyShowLib.Properties.Resources.DescendantGenerations + " <b>" + descendants + "</b><br/>");
            tw.WriteLine(Microsoft.FamilyShowLib.Properties.Resources.FamilyShow + " <b>" + pc.Count + "</b><br/><br/>");

            tw.WriteLine(NormalTableColumns());

            //write all the appropriate people to the html file including relationship to current person
            foreach (Person p in pc)
            {
                string[,] sourceArray = new string[1, 7];

                if (sourcesbool == true)
                    sourceArray = Sources(p);

                if (p.IsLiving == true && privacy == true && p.Restriction != Restriction.Private) //quick privacy option
                    tw.WriteLine("<tr id=\"id_" + p.Id + "\" class=\"person\"><td>" + Microsoft.FamilyShowLib.Properties.Resources.Living + "</td><td>" + p.LastName + "</td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td></tr>");
                else if (p.Restriction == Restriction.Private) //a private record should not be exported
                    tw.WriteLine("<tr id=\"id_" + p.Id + "\" class=\"person\"><td>" + Microsoft.FamilyShowLib.Properties.Resources.PrivateRecord + "</td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td></tr>");
                else
                {
                    if (p.Note != null)
                        tw.WriteLine(dataWithNote(p, sourceArray));
                    else
                        tw.WriteLine(dataWithoutNote(p, sourceArray));
                }
            }

            if (sourcesbool == true)
            {
                if (sourceCollection.Count > 0)
                {
                    tw.WriteLine("</table>");
                    tw.WriteLine(NormalSourceColumns());
                    foreach (Source s in sourceCollection)
                        tw.WriteLine("<tr><td><a name=\"" + s.Id + "\"></a>" + s.Id + "</td><td>" + s.SourceName + "</td><td>" + s.SourceAuthor + "</td><td>" + s.SourcePublisher + "</td><td>" + s.SourceNote + "</td><td>" + s.SourceRepository + "</td></tr>");
                }
                if (repositoryCollection.Count > 0)
                {
                    tw.WriteLine("</table>");
                    tw.WriteLine(NormalRepositoryColumns());
                    foreach (Repository r in repositoryCollection)
                        tw.WriteLine("<tr><td><a name=\"" + r.Id + "\"></a>" + r.Id + "</td><td>" + r.RepositoryName + "</td><td>" + r.RepositoryAddress + "</td></tr>");
                }
            }
            tw.WriteLine(Footer());
            tw.Close();
        }

        /// <summary>
        /// Export events by year to html file.
        /// </summary>
        public void ExportEventsByDecade(PeopleCollection peopleCollection, SourceCollection sourceCollection, RepositoryCollection repositoryCollection, string htmlFilePath, string familyxFileName, bool privacy, int startYear, int endYear)
        {
            PeopleCollection pc = new PeopleCollection();
            string filename = Path.GetFileName(htmlFilePath);
            tw = new StreamWriter(filename);

            //write the necessary html code for a html document

            tw.WriteLine(Header());
            tw.WriteLine(CSSevents());
 
            tw.WriteLine("</head><body>");
            tw.WriteLine("<h2>" + Microsoft.FamilyShowLib.Properties.Resources.FamilyShow + "</h2>");
            tw.WriteLine("<i>" + Microsoft.FamilyShowLib.Properties.Resources.Events + " " + startYear.ToString() + " - " + endYear.ToString() + "</i><br/><br/>");
            if (!string.IsNullOrEmpty(familyxFileName))
                tw.WriteLine("<b>" + Path.GetFileNameWithoutExtension(familyxFileName) + " " + Microsoft.FamilyShowLib.Properties.Resources.FamilyTree + "</b><br/>");
            tw.WriteLine("<div class=\"timeline\">");

            // Ensure we use actual decades and not just 10 year periods
            do
            {
                startYear = startYear - 1;
            }
            while (!startYear.ToString().EndsWith("0"));

            do
            {
                endYear = endYear + 1;
            }
            while (!endYear.ToString().EndsWith("0"));

            for (int i = startYear; i <= endYear; i = i + 10)
            {
                int ii = i + 9;

                if (i < DateTime.Now.Year) // Only export events which have happened.
                {
                    
                    tw.WriteLine("<p class=\"decade\"><span class=\"tick\">&#8212;</span><b>" + i.ToString() + "-" + ii.ToString() + "</b></p>");

                    int year = 0;

                    //write all the events split by decade
                    foreach (Person p in peopleCollection)
                    {
                        if (!((p.IsLiving && privacy) || p.Restriction == Restriction.Private))
                        {
                            if (p.BirthDate != null)
                                year = p.BirthDate.Value.Year;

                            string place = p.BirthPlace;

                            if (year >= i && year <= ii)
                            {

                                if (!string.IsNullOrEmpty(p.BirthPlace))
                                    tw.WriteLine("<p class=\"event\"><b>" + p.FullName + "</b> " + Properties.Resources.WasBornOn + " " + dateformat(p.BirthDate) + " " + Properties.Resources.In + " " + p.BirthPlace + ".</p>");
                                else
                                    tw.WriteLine("<p class=\"event\"><b>" + p.FullName + "</b> " + Properties.Resources.WasBornOn + " " + dateformat(p.BirthDate) + ".</p>");
                            }
                        }

                        year = 0;

                    }

                    foreach (Person p in peopleCollection)
                    {
                        if (!((p.IsLiving && privacy) || p.Restriction == Restriction.Private))
                        {
                            if (p.DeathDate != null)
                                year = p.DeathDate.Value.Year;

                            if (year >= i && year <= ii)
                            {
                                if (!string.IsNullOrEmpty(p.DeathPlace))
                                    tw.WriteLine("<p class=\"event\"><b>" + p.FullName + "</b> " + Properties.Resources.DiedOn + " " + dateformat(p.DeathDate) + " " + Properties.Resources.In + " " + p.DeathPlace + ".</p>");
                                else
                                    tw.WriteLine("<p class=\"event\"><b>" + p.FullName + "</b> " + Properties.Resources.DiedOn + " " + dateformat(p.DeathDate) + ".</p>");
                            }
                            year = 0;
                        }
                    }

                    foreach (Person p in peopleCollection)
                    {
                        if (!((p.IsLiving && privacy) || p.Restriction == Restriction.Private))
                        {
                            if (p.BurialDate != null)
                                year = p.BurialDate.Value.Year;

                            if (year >= i && year <= ii)
                            {
                                if (!string.IsNullOrEmpty(p.BurialPlace))
                                    tw.WriteLine("<p class=\"event\"><b>" + p.FullName + "</b> " + Properties.Resources.WasBuriedOn + " " + dateformat(p.BurialDate) + " " + Properties.Resources.At + " " + p.BurialPlace + ".</p>");
                                else
                                    tw.WriteLine("<p class=\"event\"><b>" + p.FullName + "</b> " + Properties.Resources.WasBuriedOn + " " + dateformat(p.BurialDate) + ".</p>");
                            }
                            year = 0;
                        }
                    }

                    foreach (Person p in peopleCollection)
                    {
                        if (!((p.IsLiving && privacy) || p.Restriction == Restriction.Private))
                        {
                            if (p.CremationDate != null)
                                year = p.CremationDate.Value.Year;

                            if (year >= i && year <= ii)
                            {
                                if (!string.IsNullOrEmpty(p.CremationPlace))
                                    tw.WriteLine("<p class=\"event\"><b>" + p.FullName + "</b> " + Properties.Resources.WasCrematedOn + " " + dateformat(p.CremationDate) + " " + Properties.Resources.At + " " + p.CremationPlace + ".</p>");
                                else
                                    tw.WriteLine("<p class=\"event\"><b>" + p.FullName + "</b> " + Properties.Resources.WasCrematedOn + " " + dateformat(p.CremationDate) + ".</p>");
                            }
                            year = 0;
                        }
                    }
                }
            }
            tw.WriteLine("</div>");
            tw.WriteLine(Footer());
            tw.Close();

        }

        #endregion

        #region data output methods

        /// <summary>
        /// Writes a row in the table for people with a note
        /// </summary>
        private static string dataWithNote(Person p, string[,] sourceArray)
        {

            return "<tr id=\"id_" + p.Id + "\" class=\"personhighlight\"><td>" + p.FirstName + "</td><td>"
                            + p.LastName + "</td><td>" + p.Age + "</td><td>"
                            + p.BirthDateDescriptor + " " + dateformat(p.BirthDate) + sourceArray[0, 0] + "</td><td>"
                            + p.BirthPlace + sourceArray[0, 0] + "</td><td>"
                            + p.DeathDateDescriptor + " " + dateformat(p.DeathDate) + sourceArray[0, 1] + "</td><td>"
                            + p.DeathPlace + sourceArray[0, 1] + "</td><td>"
                            + p.Occupation + sourceArray[0, 2] + "</td><td>"
                            + p.Education + sourceArray[0, 3] + "</td><td>"
                            + p.Religion + sourceArray[0, 4] + "</td><td>" 
                            + p.BurialPlace + sourceArray[0, 5] + "</td><td>"
                            + p.BurialDateDescriptor + " " + dateformat(p.BurialDate) + sourceArray[0, 5] + "</td><td>"
                            + p.CremationPlace + sourceArray[0, 6] + "</td><td>"
                            + p.CremationDateDescriptor + " " + dateformat(p.CremationDate) + sourceArray[0, 6]
                            + "</td><td><p class=\"notelink\">[<a href=\"javascript:showhide('id_" + p.Id + "')\">Note</a>]</p></td></tr>"
                            + "<tr id=\"note_id_" + p.Id + "\" class=\"noteshown\"><td colspan=\"15\"><b>"+Microsoft.FamilyShowLib.Properties.Resources.Note + "</b>: <pre>" + p.Note + "</pre></td></tr>";

        }

        /// <summary>
        /// Writes a row in the table for people without a note
        /// </summary>
        private static string dataWithoutNote(Person p, string[,] sourceArray)
        {
            return "<tr id=\"id_" + p.Id + "\" class=\"person\"><td>" + p.FirstName + "</td><td>"
                            + p.LastName + "</td><td>" + p.Age + "</td><td>"
                            + p.BirthDateDescriptor + " " + dateformat(p.BirthDate) + sourceArray[0, 0] + "</td><td>"
                            + p.BirthPlace + sourceArray[0, 0] + "</td><td>"
                            + p.DeathDateDescriptor + " " + dateformat(p.DeathDate) + sourceArray[0, 1] + "</td><td>"
                            + p.DeathPlace + sourceArray[0, 1] + "</td><td>"
                            + p.Occupation + sourceArray[0, 2] + "</td><td>"
                            + p.Education + sourceArray[0, 3] + "</td><td>"
                            + p.Religion + sourceArray[0, 4] + "</td><td>"
                            + p.BurialPlace + sourceArray[0, 5] + "</td><td>"
                            + p.BurialDateDescriptor + " " + dateformat(p.BurialDate) + sourceArray[0, 5] + "</td><td>"
                            + p.CremationPlace + sourceArray[0, 6] + "</td><td>"
                            + p.CremationDateDescriptor + " " + dateformat(p.CremationDate) + sourceArray[0, 6]+ "</td><td></td></tr>";                   
        }

        /// <summary>
        /// Write the column headers for sources
        /// </summary>
        private static string NormalSourceColumns()
        {
            return "<br/><br/><i>" + Microsoft.FamilyShowLib.Properties.Resources.SummaryOfSources + "</i><br/><br/>\n" +

            "<table id=\"sourcetable\" border=\"1\" rules=\"all\" frame=\"box\">\n" +
            "<thead>\n" +
            "<tr>\n" +
            "<th width=\"10%\">" + Microsoft.FamilyShowLib.Properties.Resources.Source + "</th>\n" +
            "<th width=\"15%\">" + Microsoft.FamilyShowLib.Properties.Resources.Name + "</th>\n" +
            "<th width=\"15%\">" + Microsoft.FamilyShowLib.Properties.Resources.Author + "</th>\n" +
            "<th width=\"15%\">" + Microsoft.FamilyShowLib.Properties.Resources.Publisher + "</th>\n" +
            "<th width=\"15%\">" + Microsoft.FamilyShowLib.Properties.Resources.Note + "</th>\n" +
            "<th width=\"10%\">" + Microsoft.FamilyShowLib.Properties.Resources.Repository + "</th>\n" +
            "</tr>\n" +
            "</thead>";

        }

        /// <summary>
        /// Write the column headers for the people
        /// </summary>
        private static string NormalTableColumns()
        {
        return "<table id=\"peopletable\" border=\"1\" rules=\"all\" frame=\"box\">\n" +
            "<thead>\n" +
            "<tr>\n" +
            "<th width=\"5%\">" + Microsoft.FamilyShowLib.Properties.Resources.Names + "</th> \n" +
            "<th width=\"5%\">" + Microsoft.FamilyShowLib.Properties.Resources.Surname + "</th>\n" +
            "<th width=\"5%\">" + Microsoft.FamilyShowLib.Properties.Resources.Age + "</th>\n" +
            "<th width=\"5%\">" + Microsoft.FamilyShowLib.Properties.Resources.BirthDate + "</th>\n" +
            "<th width=\"8.5%\">" + Microsoft.FamilyShowLib.Properties.Resources.BirthPlace + "</th>\n" +
            "<th width=\"5%\">" + Microsoft.FamilyShowLib.Properties.Resources.DeathDate + "</th> \n" +
            "<th width=\"8.5%\">" + Microsoft.FamilyShowLib.Properties.Resources.DeathPlace + "</th>\n" +
            "<th width=\"8.5%\">" + Microsoft.FamilyShowLib.Properties.Resources.Occupation + "</th>\n" +
            "<th width=\"8.5%\">" + Microsoft.FamilyShowLib.Properties.Resources.Education + "</th>\n" +
            "<th width=\"8.5%\">" + Microsoft.FamilyShowLib.Properties.Resources.Religion + "</th>\n" +
            "<th width=\"8.5%\">" + Microsoft.FamilyShowLib.Properties.Resources.BurialPlace + "</th>\n" +
            "<th width=\"5%\">" + Microsoft.FamilyShowLib.Properties.Resources.BurialDate + "</th>\n" +
            "<th width=\"8.5%\">" + Microsoft.FamilyShowLib.Properties.Resources.CremationPlace + "</th>\n" +
            "<th width=\"5%\">" + Microsoft.FamilyShowLib.Properties.Resources.CremationDate+ "</th>\n" +
            "<th width=\"5%\">" + Microsoft.FamilyShowLib.Properties.Resources.AdditionalInfo + "</th>\n" +
            "</tr>\n" +
            "</thead>";
        }

        /// <summary>
        /// Write column headers for repositories
        /// </summary>
        private static string NormalRepositoryColumns()
        {
            return "<br/><br/><i>" + Microsoft.FamilyShowLib.Properties.Resources.SummaryOfRepositories + "</i><br/><br/>\n" +
                
            "<table id=\"reositorytable\" border=\"1\" rules=\"all\" frame=\"box\">\n" +
            "<thead>\n" +
            "<tr>\n" +
            "<th width=\"10%\">" + Microsoft.FamilyShowLib.Properties.Resources.Repository + "</th>\n" +
            "<th width=\"15%\">" + Microsoft.FamilyShowLib.Properties.Resources.Name + "</th>\n" +
            "<th width=\"15%\">" + Microsoft.FamilyShowLib.Properties.Resources.Address + "</th>\n" +
            "</tr>\n" +
            "</thead>";

        }

        /// <summary>
        /// Write the source information to a string array
        /// </summary>
        private static string[,] Sources(Person p)
        {
            string[,] sourceArray = new string[1, 7];

            if (!string.IsNullOrEmpty(p.BirthCitation) && !string.IsNullOrEmpty(p.BirthSource))
            {
                sourceArray[0, 0] = "<br/><br/><i>" + p.BirthCitation + "</i> [<a href=\"#" + p.BirthSource + "\">" + p.BirthSource + "</a>]";
                if (!string.IsNullOrEmpty(p.BirthLink) &&( p.BirthLink.StartsWith("www.") || p.BirthLink.StartsWith("http://")))
                    sourceArray[0, 0]+= " [<a href=\"" + p.BirthLink + "\">Link</a>]";
            }
            else
                sourceArray[0, 0] = "";
            if (!string.IsNullOrEmpty(p.DeathCitation) && !string.IsNullOrEmpty(p.DeathSource))
            {
                sourceArray[0, 1] = "<br/><br/><i>" + p.DeathCitation + "</i> [<a href=\"#" + p.DeathSource + "\">" + p.DeathSource + "</a>]";
                if (!string.IsNullOrEmpty(p.DeathLink) && (p.DeathLink.StartsWith("www.") || p.DeathLink.StartsWith("http://") ))
                    sourceArray[0, 1]+= " [<a href=\"" + p.DeathLink + "\">Link</a>]";
            }
            else
                sourceArray[0, 1] = "";
            if (!string.IsNullOrEmpty(p.OccupationCitation) && !string.IsNullOrEmpty(p.OccupationSource))
            {
                sourceArray[0, 2] = "<br/><br/><i>" + p.OccupationCitation + "</i> [<a href=\"#" + p.OccupationSource + "\">" + p.OccupationSource + "</a>]";
                if (!string.IsNullOrEmpty(p.OccupationLink) && (p.OccupationLink.StartsWith("www.") || p.OccupationLink.StartsWith("http://") ))
                    sourceArray[0, 2] += " [<a href=\"" + p.OccupationLink + "\">Link</a>]";
            }
            else
                sourceArray[0, 2] = "";
            if (!string.IsNullOrEmpty(p.EducationCitation) && !string.IsNullOrEmpty(p.EducationSource))
            {
                sourceArray[0, 3] = "<br/><br/><i>" + p.EducationCitation + "</i> [<a href=\"#" + p.EducationSource + "\">" + p.EducationSource + "</a>]";
                if (!string.IsNullOrEmpty(p.EducationLink) && (p.EducationLink.StartsWith("www.") || p.EducationLink.StartsWith("http://") ))
                    sourceArray[0, 3] += " [<a href=\"" + p.EducationLink + "\">Link</a>]";
            }
            else
                sourceArray[0, 3] = "";
            if (!string.IsNullOrEmpty(p.ReligionCitation) && !string.IsNullOrEmpty(p.ReligionSource))
            {
                sourceArray[0, 4] = "<br/><br/><i>" + p.ReligionCitation + "</i> [<a href=\"#" + p.ReligionSource + "\">" + p.ReligionSource + "</a>]";
                if (!string.IsNullOrEmpty(p.ReligionLink) && (p.ReligionLink.StartsWith("www.") || p.ReligionLink.StartsWith("http://") ))
                    sourceArray[0, 4] += " [<a href=\"" + p.ReligionLink + "\">Link</a>]";
            }
            else
                sourceArray[0, 4] = "";
            if (!string.IsNullOrEmpty(p.BurialCitation) && !string.IsNullOrEmpty(p.BurialSource))
            {
                sourceArray[0, 5] = "<br/><br/><i>" + p.BurialCitation + "</i> [<a href=\"#" + p.BurialSource + "\">" + p.BurialSource + "</a>]";
                if (!string.IsNullOrEmpty(p.BurialLink) && (p.BurialLink.StartsWith("www.") || p.BurialLink.StartsWith("http://") ))
                    sourceArray[0, 5] += " [<a href=\"" + p.BurialLink + "\">Link</a>]";
            }
            else
                sourceArray[0, 5] = "";
            if (!string.IsNullOrEmpty(p.CremationCitation) && !string.IsNullOrEmpty(p.CremationSource))
            {
                sourceArray[0, 6] = "<br/><br/><i>" + p.CremationCitation + "</i> [<a href=\"#" + p.CremationSource + "\">" + p.CremationSource + "</a>]";
                if (!string.IsNullOrEmpty(p.CremationLink) && (p.CremationLink.StartsWith("www.") || p.CremationLink.StartsWith("http://") ))
                    sourceArray[0, 6] += " [<a href=\"" + p.CremationLink + "\">Link</a>]";
            }
            else
                sourceArray[0, 6] = "";

        return sourceArray;
        }

        #endregion

        #region html document structure methods

        /// <summary>
        /// Write the header information
        /// </summary>
        private static string Header()
        {
            return  "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                    "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">\n" +
                    "<html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"en\" lang=\"en\">\n" +
                    "<head>\n" +
                    "<title>" + Microsoft.FamilyShowLib.Properties.Resources.FamilyShow + "</title>";
        }

        /// <summary>
        /// Write the CSS information
        /// </summary>
        private static string CSS()
        {
            return  "<style type=\"text/css\">\n" +

                    "body { background-color: white; font-family: Calibri, Arial, sans-serif; font-size: 12px; line-height: 1.2; padding: 1em; color: #2E2E2E; }\n" +

                    "table { border: 0.5px gray solid; width: 100%; empty-cells: show; }\n" +
                    "th, td { border: 0.5px gray solid; padding: 0.5em; vertical-align: top; }\n" +
                    "td { text-align: left; }\n" +
                    "th { background-color: #F0F8FF; }\n" +
                    "td a { color: navy; text-decoration: none; }\n" +
                    "td a:hover  { text-decoration: underline; }\n" +

                    "tr.notehidden { display: none;}\n" +
                    "tr.personhighlight {border-left: 1px #2E2E2E solid;}\n" +
                    "tr.person {border-left: 0.5px gray solid;}\n" +
                    "tr.noteshown {border-left: 1px #2E2E2E solid;}\n" +
                    "tr.noteshown pre {width: 98%; white-space: pre-wrap;}";    
        }

        /// <summary>
        /// Write the CSS information
        /// </summary>
        private static string CSSevents()
        {
            return "<style type=\"text/css\">\n" +
                    "body { background-color: white; font-family: Calibri, Arial, sans-serif; font-size: 12px; line-height: 1.2; padding: 1em; color: #2E2E2E; }\n" +
                    ".timeline { border-left-color: gray; border-left-style: solid; border-left-width: 1px; }\n" + 
                    ".tick {color: gray; margin: 0px 2px 0px -1px; }\n" + 
                    "p.decade { line-height: 1; margin: 8px 0px 8px 0px; }\n" +
                    "p.event { margin: 3px 0px 3px 20px;}</style>";
        }

        /// <summary>
        /// Write the CSS printing information.  This method ensures that the correct number of columns is printed.
        /// </summary>
        private static string CSSprinting(int i)
        {
            string printstyle = "@media print {\n" +
                                "p.notelink, noscript, input  { display: none; }\n" +
                                "table { border-width: 0px; }\n" +
                                "tr { page-break-inside: avoid; }\n" +
                                "tr >";

            for (int j=1;j<=i;j++)
            {
                if(i!=j)
                printstyle  += "*+";
                else
                printstyle  += "*";
            }

            printstyle  +=  "{display: none; }\n" +
                            "}\n" +
                            "</style>";

            return printstyle;
        }

        /// <summary>
        /// Write show/hide all notes buttons
        /// </summary>
        private static string Buttons()
        {

            return "<input type=\"button\" onclick=\"showhideall('hide_all')\" value=\"" + Microsoft.FamilyShowLib.Properties.Resources.HideNotes + "\" />\n" +
               "<input type=\"button\" onclick=\"showhideall('show_all')\" value=\"" + Microsoft.FamilyShowLib.Properties.Resources.ShowNotes + "\" />\n" +
               "<noscript>\n" +
               "<p>[" + Microsoft.FamilyShowLib.Properties.Resources.HidingNotes + "]</p>\n" +
               "</noscript>";
        }

        /// <summary>
        /// Write the tw.WriteLine(Microsoft.FamilyShowLib.Properties.Resources.Tree + " <b>" + Path.GetFileNameWithoutExtension(familyxFileName) + "</b><br/>");s
        /// </summary>
        private static string JavaScripts()
        {
            return "<script type=\"text/javascript\">\n" +

                    "function showhide(id) {\n" +
	                "var person = document.getElementById(id);\n" +
	                "var note = document.getElementById('note_'+id);\n" +
	
                    "  if (note.className == 'noteshown') {\n" +
                    " note.className = 'notehidden';\n" +
		            "person.className = 'person';\n" +
                    "} \n" +
	                "else {\n" +
                    "   note.className = 'noteshown';\n" +
		            "  person.className = 'personhighlight';\n" +
                    "}\n" +
                    "}\n" +

                    "function showhideall(hide) {\n" +
                    	
                    "var allTags=document.getElementsByTagName('tr');\n" +

                    "for (i=0; i<allTags.length; i++) {\n" +

                    "if(hide=='hide_all'){\n" +

                    "if (allTags[i].className=='noteshown') {\n" +
                    "	allTags[i].className='notehidden';\n" +
                    "	}\n" +

                    "if (allTags[i].className=='personhighlight') {\n" +
                    "	allTags[i].className='person';\n" +
                    "	}	\n" +
                    "}\n" +

                    "if(hide=='show_all'){\n" +

                    "if (allTags[i].className=='notehidden') {\n" +
                    "	allTags[i].className='noteshown';\n" +
                    "	allTags[i-1].className='personhighlight'; 	\n" +
                    "	}\n" +

                    "}\n" +

                    "}\n" +
                    "}\n" +

            "</script>";
        }

        /// <summary>
        /// Write the Footer information
        /// </summary>
        private static string Footer()
        {
            //write the software version and the date and time to the file
            Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            string versionlabel = string.Format(CultureInfo.CurrentCulture, "{0}.{1}.{2}", version.Major, version.Minor, version.Build);
            string date = DateTime.Now.ToString();
            return "</table><br/><p><i>" + Microsoft.FamilyShowLib.Properties.Resources.GeneratedByFamilyShow + " " + versionlabel + " " + Microsoft.FamilyShowLib.Properties.Resources.On + " " + date + "</i></p></body></html>";
        }

        #endregion

        #region helper methods

        /// <summary>
        /// Handles the privacy of the current person name in the report summary
        /// </summary>
        private static string CurrentPersonString(Person p, bool privacy)
        {
            string CurrentPerson = string.Empty;

            if (p.IsLiving == true && privacy == true && p.Restriction != Restriction.Private) //quick privacy option
                CurrentPerson = Microsoft.FamilyShowLib.Properties.Resources.CurrentPerson + " <b>" + Microsoft.FamilyShowLib.Properties.Resources.Living + " " + p.LastName + "</b><br/>";
            else if (p.Restriction == Restriction.Private) //a private record should not be exported
                CurrentPerson = Microsoft.FamilyShowLib.Properties.Resources.CurrentPerson + " <b>" + Microsoft.FamilyShowLib.Properties.Resources.PrivateRecord + "</b><br/>";
            else
            CurrentPerson = Microsoft.FamilyShowLib.Properties.Resources.CurrentPerson + " <b>" + p.FullName + "</b><br/>";

            return CurrentPerson;
        }

        /// <summary>
        /// Handles the export logic for a descendent generation
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
        /// Handles the export logic for an ancestor generation
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

        /// <summary>
        /// Get a date in dd/mm/yyyy format from a full DateTime?
        /// </summary>
        private static string dateformat(DateTime? dates)
         {
            string date = string.Empty;
                    if (dates != null)  //don't try if date is null!
                    {
                        int month = dates.Value.Month;
                        int day = dates.Value.Day;
                        int year = dates.Value.Year;
                        date = day + "/" + month + "/" + year;
                    }
            return date;
         }

        #endregion

    }
}
