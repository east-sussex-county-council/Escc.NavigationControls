#region Using Directives
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
#endregion

namespace EsccWebTeam.NavigationControls
{
    /// <summary>
    /// An A-Z list of links for alphabetical navigation
    /// </summary>
    public class AZNavigation : WebControl, INamingContainer
    {
        #region Private fields
        private string linkTitle = "Show all items";
        private string disabledTitle = "There are no items";
        private string selectedChar = String.Empty;
        private string mergeChars = String.Empty;
        private NameValueCollection qsParams = new NameValueCollection();

        #endregion


        #region Constructors
        /// <summary>
        /// Create a new AZNavigation control, which is rendered as a <div></div> element 
        /// </summary>
        public AZNavigation()
            : base(HtmlTextWriterTag.Div)
        {
            SetupDefaults();
        }

        /// <summary>
        /// Create a new AZNavigation control, which is rendered as a <div></div> element 
        /// </summary>
        /// <param name="azNavigation">Redundant param required to use the control declaratively</param>
        public AZNavigation(string azNavigation)
            : base(HtmlTextWriterTag.Div)
        {
            SetupDefaults();
        }

        private void SetupDefaults()
        {
            this.TargetFile = "index.aspx";
            this.UrlParameter = "index";
            this.ItemSeparator = String.Empty;
        }
        #endregion


        #region Public properties
        /// <summary>
        /// Gets or sets characters to merge into groups
        /// </summary>
        public string MergeChars
        {
            get
            {
                return this.mergeChars;
            }
            set
            {
                this.mergeChars = value;
            }
        }

        /// <summary>
        /// Gets or sets characters to show as disabled
        /// </summary>
        public string DisableChars { get; set; }

        /// <summary>
        /// Gets or sets charaters to skip
        /// </summary>
        /// <value>The characters to skip.</value>
        public string SkipChars { get; set; }

        /// <summary>
        /// Gets or sets characters which support individual styling.
        /// </summary>
        /// <value>The style chars.</value>
        public string StyleChars { get; set; }

        /// <summary>
        /// The filename of the page each link will point to
        /// </summary>
        public string TargetFile { get; set; }

        /// <summary>
        /// The string to place between each letter
        /// </summary>
        public string ItemSeparator { get; set; }

        /// <summary>
        /// The querystring parameter used to pass the letter, eg ?index=a to list items beginning with A
        /// </summary>
        public string UrlParameter { get; set; }

        /// <summary>
        /// Pop-up title which will appear on each link - suffixed by "beginning with [letter]"
        /// </summary>
        public string LinkTitle
        {
            get { return this.linkTitle; }
            set
            {
                if (value != null)
                {
                    value = value.Trim();
                    if (value.Length > 0) value = value.Substring(0, 1).ToUpper(CultureInfo.CurrentCulture) + value.Substring(1);
                }
                this.linkTitle = value;
            }
        }

        /// <summary>
        /// Pop-up title which will appear on each disabled letter - suffixed by "beginning with [letter]"
        /// </summary>
        public string DisabledTitle
        {
            get { return this.disabledTitle; }
            set
            {
                if (value != null)
                {
                    value = value.Trim();
                    if (value.Length > 0) value = value.Substring(0, 1).ToUpper(CultureInfo.CurrentCulture) + value.Substring(1);
                }
                this.disabledTitle = value;
            }
        }

        /// <summary>
        /// Property to store whether to begin alphabet with a 0-9 option
        /// </summary>
        public bool Numbers { get; set; }

        /// <summary>
        /// Property to store the currently selected character
        /// </summary>
        public string SelectedChar
        {
            get { return this.selectedChar; }
            set
            {
                if (!String.IsNullOrEmpty(value)) value = value.Substring(0, 1).ToLower(CultureInfo.CurrentCulture);
                this.selectedChar = value;
            }
        }

        /// <summary>
        /// Gets or sets the header template which appears before the A-Z links
        /// </summary>
        /// <value>The header template.</value>
        [TemplateContainer(typeof(XhtmlContainer))]
        [PersistenceMode(PersistenceMode.InnerDefaultProperty)] // This makes it validate in Visual Studio 
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)] // This makes it validate in Visual Studio 
        public ITemplate HeaderTemplate { get; set; }

        /// <summary>
        /// Gets or sets the footer template which appears after the A-Z links
        /// </summary>
        /// <value>The footer template.</value>
        [TemplateContainer(typeof(XhtmlContainer))]
        [PersistenceMode(PersistenceMode.InnerDefaultProperty)] // This makes it validate in Visual Studio 
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)] // This makes it validate in Visual Studio 
        public ITemplate FooterTemplate { get; set; }

        /// <summary>
        /// Container for header and footer templates
        /// </summary>
        private class XhtmlContainer : PlaceHolder, INamingContainer { }
        #endregion


        /// <summary>
        /// Add a parameter and value to the querystring for each link
        /// </summary>
        /// <param name="parameterName">The name of the parameter to add</param>
        /// <param name="parameterValue">The value of the parameter</param>
        public void AddQueryStringParameter(string parameterName, string parameterValue)
        {
            this.qsParams.Add(parameterName, parameterValue);
        }

        /// <summary>
        /// Remove a parameter and value from the querystring of each link
        /// </summary>
        /// <param name="parameterName">The name of the parameter to remove</param>
        public void RemoveQueryStringParameter(string parameterName)
        {
            this.qsParams.Remove(parameterName);
        }

        /// <summary>
        /// Build the navigation using the supplied properties when called upon the build the control
        /// </summary>
        protected override void CreateChildControls()
        {
            // append standard class name
            this.CssClass = (this.CssClass + " alphabet").Trim();

            // Add header template
            if (HeaderTemplate != null)
            {
                var header = new XhtmlContainer();
                HeaderTemplate.InstantiateIn(header);
                this.Controls.Add(header);
            }

            // only add numerical link if option set
            if (this.Numbers)
            {
                var link = new HtmlAnchor();
                link.HRef = BuildLetterPageUrl("0");
                link.Title = this.LinkTitle.Trim() + " beginning with 0-9";
                link.InnerText = "0&#151;9";
                this.Controls.Add(link);
            }

            // get list of chars to skip completely
            int skipCount = String.IsNullOrEmpty(this.SkipChars) ? 0 : this.SkipChars.Length;
            var charsToSkip = new List<string>(skipCount);
            for (short i = 0; i < skipCount; i++)
            {
                charsToSkip.Add(this.SkipChars.Substring(i, 1).ToLowerInvariant());
            }

            // get list of characters to group together
            List<string> charsToMerge = new List<string>(this.mergeChars.Length);
            for (short i = 0; i < this.mergeChars.Length; i++)
            {
                var character = this.mergeChars.Substring(i, 1).ToLowerInvariant();
                if (!charsToSkip.Contains(character)) charsToMerge.Add(character);
            }

            // get list of chars to show as disabled
            int disableCount = String.IsNullOrEmpty(this.DisableChars) ? 0 : this.DisableChars.Length;
            List<string> charsToDisable = new List<string>(disableCount);
            for (short i = 0; i < disableCount; i++)
            {
                var character = this.DisableChars.Substring(i, 1).ToLowerInvariant();
                if (!charsToSkip.Contains(character)) charsToDisable.Add(character);
            }

            // get list of chars to add classes to
            int styleCount = String.IsNullOrEmpty(this.StyleChars) ? 0 : this.StyleChars.Length;
            List<string> charsToStyle = new List<string>(styleCount);
            for (short i = 0; i < styleCount; i++)
            {
                var character = this.StyleChars.Substring(i, 1).ToLowerInvariant();
                if (!charsToSkip.Contains(character)) charsToStyle.Add(character);
            }


            // Add a link for each letter of the Latin alphabet
            List<string> alphabet = new List<string>(new string[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" });
            foreach (var character in charsToSkip) alphabet.Remove(character);

            for (int i = 0; i < alphabet.Count; i++)
            {
                // If we've just added an item and are about to add another, add a divider
                if (i > 0 && this.ItemSeparator.Length > 0)
                {
                    this.Controls.Add(new LiteralControl(this.ItemSeparator));
                }

                // Group character with subsequent characters if specified
                string letter = alphabet[i];
                int charPos;
                string nextChar;
                string groups = this.mergeChars.ToLower();

                while ((i + 1) < alphabet.Count && charsToMerge.Contains(alphabet[i]) && charsToMerge.Contains(alphabet[i + 1]))
                {
                    i++;
                    letter = String.Format(CultureInfo.CurrentCulture, "{0}{1}", letter, alphabet[i]);

                    // use a semi-colon to break groups
                    charPos = groups.IndexOf(alphabet[i]) + 1;
                    if (groups.Length > (charPos))
                    {
                        nextChar = groups.Substring(charPos, 1);
                        if (nextChar == ";") break;
                    }
                }

                // If any one of the current group of characters is the selected character
                // display it without a link
                string upperLetter = letter.ToUpper(CultureInfo.CurrentCulture);
                if (this.selectedChar.Length > 0 && letter.IndexOf(this.selectedChar) > -1)
                {
                    HtmlGenericControl span = new HtmlGenericControl("em");
                    if (charsToStyle.Contains(letter)) span.Attributes["class"] = letter;
                    span.InnerText = upperLetter;
                    this.Controls.Add(span);
                }
                else
                {
                    // Grammar - turn "ABC" into "A, B or C"
                    string begin = upperLetter;
                    if (begin.Length > 1)
                    {
                        string[] beginBits = new string[begin.Length];
                        for (short j = 0; j < begin.Length; j++)
                        {
                            beginBits[j] = begin.Substring(j, 1);
                            if (j > 0 && j < (begin.Length - 1))
                            {
                                beginBits[j] = ", " + beginBits[j];
                            }
                            else if (j == (begin.Length - 1))
                            {
                                beginBits[j] = " or " + beginBits[j];
                            }
                        }
                        begin = String.Join("", beginBits);

                    }

                    // Is this character or range of characters disabled?
                    bool disabled = true;
                    for (short j = 0; j < letter.Length; j++)
                    {
                        string thisLetter = letter.Substring(j, 1);
                        if (!charsToDisable.Contains(thisLetter))
                        {
                            disabled = false;
                            break;
                        }
                    }

                    if (disabled)
                    {
                        // Display character(s) as deleted
                        HtmlGenericControl del = new HtmlGenericControl("del");
                        if (charsToStyle.Contains(letter)) del.Attributes["class"] = letter;
                        del.Attributes["title"] = this.disabledTitle + " beginning with " + begin;
                        del.InnerText = upperLetter;
                        this.Controls.Add(del);
                    }
                    else
                    {
                        // Display character(s) as a link
                        HtmlAnchor link = new HtmlAnchor();
                        if (charsToStyle.Contains(letter)) link.Attributes["class"] = letter;
                        link.HRef = BuildLetterPageUrl(letter);
                        link.Title = this.linkTitle + " beginning with " + begin;
                        link.InnerText = upperLetter;
                        this.Controls.Add(link);
                    }
                }
            }


            // Add footer template
            if (FooterTemplate != null)
            {
                XhtmlContainer footer = new XhtmlContainer();
                FooterTemplate.InstantiateIn(footer);
                this.Controls.Add(footer);
            }
        }

        /// <summary>
        /// Builds the URL to link to from a letter
        /// </summary>
        /// <param name="letter"></param>
        /// <returns></returns>
        private string BuildLetterPageUrl(string letter)
        {
            var url = String.Format(CultureInfo.InvariantCulture, this.TargetFile, letter);
            if (!String.IsNullOrEmpty(this.UrlParameter))
            {
                url += ("?" + this.UrlParameter + "=" + letter);
            }
            else if (this.qsParams.Count > 0)
            {
                url += "?";
            };
            foreach (string key in this.qsParams.Keys)
            {
                url += "&amp;" + key + "=" + HttpUtility.UrlEncode(qsParams[key]);
            }

            return url;
        }
    }
}
