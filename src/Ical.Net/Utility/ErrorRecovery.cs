using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Ical.Net.Utility
{
    public static class ErrorRecovery
    {
        // Accept a string pICSText and return pICSText with:
        // every line beginning with a space or a tab removed and appended to the previous line, and
        // return the resulting pICSText with the preceding operations included.
        internal static string UnWrapText(string pICSText)
        {
            // handle lines with leading spaces
            var pos = pICSText.IndexOf("\r\n ");
            while (pos != -1)
            {
                pICSText = pICSText.Substring(0, pos) + pICSText.Substring(pos + 3);
                pos = pICSText.IndexOf("\r\n ");
            }

            // handle lines with leading tabs
            pos = pICSText.IndexOf("\r\n\t");
            while (pos != -1)
            {
                pICSText = pICSText.Substring(0, pos) + pICSText.Substring(pos + 3);
                pos = pICSText.IndexOf("\r\n\t");
            }
            return pICSText;
        }

        /// <summary>
        /// Accept string pICSText, and break every line > 75 chars into multiple lines.
        /// Each new line begins with a space.
        /// Return the resulting string
        /// </summary>
        /// <param name="pICSText">string</param>
        /// <returns>string</returns>
        internal static string WrapText( string pICSText)
        {
            var result = "";
            var lines = pICSText.Split(new[] { "\r\n" }, StringSplitOptions.None);
            foreach (var line in lines)
            {
                // if line.Length > 75, break it into multiple lines ending in \r\n
                // each new line begins with a space
                int pos = 0;
                while ( (pos + 75) < line.Length)
                {
                    result += (line.Substring(pos, 75) + "\r\n ");
                    pos += 75;
                }
                result += line.Substring(pos, line.Length - pos) + "\r\n";
            }

            return result;
        }

        // Accept a list of 'MarkerValues' strings, and string pICSText and:
        // In every line beginning with a 'MarkerValues' and a colon replace every \r\\n with a \r\n.
        // Return the resulting pICSText with the preceding operation applied.
        internal static string RemoveEscapedNewlines(List<string> pListMarkers, string pICSText)
        {
            string result = pICSText;

            // regular expression to replace 'TEMP' with 'TEMP2' globally in a string
            var carriageReturnEscapedNewlines = new Regex(@"\r\\n", RegexOptions.None);

            // use carriageReturnEscapedNewlines regex to replace every match with a local Newline in result string
            result = carriageReturnEscapedNewlines.Replace(result, Environment.NewLine);

            return result;
        }

        // Accept a string pICSText and return pICSText with:
        //  first UnWrapText in pICSText
        //  second create a list of 'SUMMARY' and 'DESCRIPTION' strings
        //  third call RemoveEscapedNewlines with the above list and pICSText
        //  fourth WrapText in pICSText
        //  return the resulting pICSText with the preceding operations included.
        internal static string RemovedEscapedNewlinesFromSUMMARYandDESCRIPTION(string pICSText)
        {
            var unwrappedText = UnWrapText(pICSText);
            var listMarkers = new List<string> { "SUMMARY", "DESCRIPTION" };
            var removedEscapedNewlines = RemoveEscapedNewlines(listMarkers, unwrappedText);
            var wrappedText = WrapText(removedEscapedNewlines);
            return wrappedText;
        }

        // Remove leading space or tab from beginning of pICSText and return resulting pICSText.
        internal static string RemoveLeadingSpaceOrTab(string pICSText)
        {
            while (pICSText.StartsWith(" ") || pICSText.StartsWith("\t"))
                pICSText = pICSText.Substring(1);
            return pICSText;
        }


        // Knit together the above functions to create a single function that fixes spatie/icalendar-generator errors

        /// <summary>
        /// Accept a string pICSText and return pICSText with:
        /// first apply RemoveLeadingSpaceOrTab to pICSText
        /// second apply RemovedEscapedNewlinesFromSUMMARYandDESCRIPTION to pICSText
        /// return the resulting pICSText with the preceding operations included.
        /// </summary>
        /// <param name="pICSText">string</param>
        /// <returns>string</returns>
        internal static string Fix_spatieSLASHicalendarDASHgenerator(string pICSText)
        {
            var removedLeadingSpaceOrTab = RemoveLeadingSpaceOrTab(pICSText);
            var removedEscapedNewlinesFromSUMMARYandDESCRIPTION = RemovedEscapedNewlinesFromSUMMARYandDESCRIPTION(removedLeadingSpaceOrTab);
            return removedEscapedNewlinesFromSUMMARYandDESCRIPTION;
        }

        // Extract the text to the right of 'PRODID:' line in the pICSText string and return it.
        internal static string ExtractPRODID(string pICSText)
        {
            var pos = pICSText.IndexOf("PRODID:");
            if (pos == -1)
                return "";
            var result = pICSText.Substring(pos + 7);
            pos = result.IndexOf("\r\n");
            if (pos == -1)
                return "";
            result = result.Substring(0, pos);
            return result;
        }

        // if pICSText contains "spatie/icalendar-generator" then return "spatie/icalendar-generator"
        // else return null
        public static string HasErrorsPRODID(string pICSText)
        {
            var prodid = ExtractPRODID(pICSText);
            if (prodid.Contains("spatie/icalendar-generator"))
                return prodid;
            return null;
        }

        // use a switch statement to determine if pICSTEext contains a "spatie/icalendar-generator"
        // then run Fix_spatieSLASHicalendarDASHgenerator
        // else return pICSText.
        public static string FixErrorsPRODID(string pICSText)
        {
            string prodid = ExtractPRODID(pICSText);
            switch (prodid)
            {
                case "spatie/icalendar-generator":
                    return Fix_spatieSLASHicalendarDASHgenerator(pICSText);
                default:
                    return pICSText;
            }
        }
    }
}
