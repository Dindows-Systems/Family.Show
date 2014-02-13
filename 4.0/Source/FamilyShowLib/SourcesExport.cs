/*
 * Exports sources from the People collection to a Html based report.
 * 
 * The report is based on the same style as the Person report.
 * 
 * All the html containers are in the html document structures region and 
 * are written to the XHtml 1.0 Transitional Standard and CSS 2 standard.
 * For further information see the http://www.w3.org/
 * 
*/

using System;
using System.Globalization;
using System.IO;

namespace Microsoft.FamilyShowLib
{
    public class SourcesExport
    {
        #region fields

        private TextWriter tw;

        #endregion

        /// <summary>
        /// Export all the data from the Sources collection to the specified html file.
        /// </summary>
        public void ExportSources(string fileName, string familyFileName, SourceCollection source)
        {

            tw = new StreamWriter(fileName);
            //write the necessary html code for a html document
            tw.WriteLine(Header());
            tw.WriteLine(CSS());
            tw.WriteLine(CSSprinting(7));

            tw.WriteLine("</head><body>");
            tw.WriteLine("<h2>" + Microsoft.FamilyShowLib.Properties.Resources.FamilyShow + "</h2>");

            tw.WriteLine("<i>"+ Microsoft.FamilyShowLib.Properties.Resources.SummaryOfSourcesForFile +" " + familyFileName + "</i><br/><br/>");
            //Write the column headings
            tw.WriteLine(NormalSourceColumns());

            foreach (Source s in source)
                tw.WriteLine("<tr><td><a name=\"" + s.Id + "\"></a>" + s.Id + "</td><td>" + s.SourceName + "</td><td>" + s.SourceAuthor + "</td><td>" + s.SourcePublisher + "</td><td>" + s.SourceNote + "</td><td>" + s.SourceRepository + "</td></tr>");
            
            tw.WriteLine(Footer());
            tw.Close();
        }

        #region data output methods

        /// <summary>
        /// Writes the table headers for source export
        /// </summary>
        private static string NormalSourceColumns()
        {
            return "<table id=\"sourcetable\" border=\"1\" rules=\"all\" frame=\"box\">\n" +
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

        #endregion

        #region html document structure methods

        /// <summary>
        /// Write the header
        /// </summary>
        private static string Header()
        {
            return "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                    "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">\n" +
                    "<html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"en\" lang=\"en\">\n" +
                    "<head>\n" +
                    "<title>"+ Microsoft.FamilyShowLib.Properties.Resources.FamilyShow + "</title>";
        }

        /// <summary>
        /// Write the CSS information
        /// </summary>
        private static string CSS()
        {
            return "<style type=\"text/css\">\n" +

                    "body { background-color: white; font-family: Calibri, Arial, sans-serif; font-size: 12px; line-height: 1.2; padding: 1em; color: #2E2E2E; }\n" +

                    "table { border: 0.5px gray solid; width: 100%; empty-cells: show; }\n" +
                    "th, td { border: 0.5px gray solid; padding: 0.5em; vertical-align: top; }\n" +
                    "td { text-align: left; }\n" +
                    "th { background-color: #F0F8FF; }\n" +
                    "td a { color: navy; text-decoration: none; }\n" +
                    "td a:hover  { text-decoration: underline; }";
        }

        /// <summary>
        /// Write the CSS printing information
        /// </summary>
        private static string CSSprinting(int i)
        {
            string printstyle = "@media print {\n" +
                                "table { border-width: 0px; }\n" +
                                "tr { page-break-inside: avoid; }\n" +
                                "tr >";

            for (int j = 1; j <= i; j++)
            {
                if (i != j)
                    printstyle += "*+";
                else
                    printstyle += "*";
            }

            printstyle += "{display: none; }\n" +
                            "}\n" +
                            "</style>";

            return printstyle;
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

    }
}
