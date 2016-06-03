using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Escc.NavigationControls.WebForms
{
    /// <summary>
    /// A link to a file which includes file size and format information
    /// </summary>
    public class FileLinkControl : WebControl
    {
        private int fileSize;
        private bool linkTextHasTrailingSpace;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileLinkControl"/> class.
        /// </summary>
        public FileLinkControl()
            : base(HtmlTextWriterTag.A)
        {
            // Set properties which preserve original behaviour
            this.UseDefaultClasses = true;
            this.RecognisedFileTypesOnly = true;
        }

        /// <summary>
        /// Gets or sets the URL of the file to link to
        /// </summary>
        /// <value>The URL of the file.</value>
        public Uri NavigateUrl
        {
            get
            {
                if (!String.IsNullOrEmpty(this.Attributes["href"])) return new Uri(this.Attributes["href"], UriKind.RelativeOrAbsolute);
                else return null;
            }
            set
            {
                this.Attributes["href"] = value.ToString();
            }
        }

        /// <summary>
        /// Gets or sets the inner text of the link.
        /// </summary>
        /// <value>The inner text of the link.</value>
        public string InnerText { get; set; }

        /// <summary>
        /// Gets or sets the size of the file in bytes. This will be calculated automatically if you do not set it.
        /// </summary>
        /// <value>The size of the file.</value>
        public int FileSize
        {
            get
            {
                // If we already have a stored file size, return it
                if (this.fileSize > 0) return this.fileSize;

                // Otherwise recalculate it, 
                this.CalculateFileSize();
                return this.fileSize;
            }
            set
            {
                this.fileSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the file extension. This will be calculated automatically if you do not set it.
        /// </summary>
        /// <value>The file extension.</value>
        public string FileExtension { get; set; }

        /// <summary>
        /// Gets or sets whether to show format and size details inside the link, rather than after.
        /// </summary>
        /// <value>
        /// 	<c>true</c> to show details inside link; otherwise, <c>false</c>.
        /// </value>
        public bool ShowDetailsInsideLink { get; set; }

        /// <summary>
        /// Gets or sets whether to use the default classes applied by the original version of this control.
        /// </summary>
        /// <value><c>true</c> to use the default classes; otherwise, <c>false</c>.</value>
        public bool UseDefaultClasses { get; set; }

        /// <summary>
        /// Gets or sets whether to display a file type only if it's registered in <c>web.config</c>
        /// </summary>
        /// <value>
        /// 	<c>true</c> to display recognised file types only; otherwise, <c>false</c>.
        /// </value>
        public bool RecognisedFileTypesOnly { get; set; }

        /// <summary>
        /// Calculates the size of the file on disk.
        /// </summary>
        /// <returns></returns>
        private void CalculateFileSize()
        {
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Converts a file extension to a plain English file type name.
        /// </summary>
        /// <param name="extension">The extension.</param>
        /// <param name="recognisedFileTypesOnly">if <c>true</c>, do not display a file type if it's not registered in <c>web.config</c></param>
        /// <returns>File type name, or empty string if file type not known</returns> 
        /// <remarks>
        /// <para>This requires the preferred file type names for various extensions to be set up in <c>web.config</c>:</para>
        /// <example><code>
        ///  &lt;configuration&gt;
        ///     &lt;configSections&gt;
        ///         &lt;sectionGroup name=&quot;Escc.NavigationControls.WebForms&quot;&gt;
        ///             &lt;section name=&quot;FileTypeNames&quot; type=&quot;System.Configuration.NameValueSectionHandler, System, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089&quot; requirePermission=&quot;false&quot; /&gt;
        ///         &lt;/sectionGroup&gt;
        ///     &lt;/configSections&gt;
        ///  
        ///     &lt;Escc.NavigationControls.WebForms&gt;
        ///        &lt;FileTypeNames&gt;
        ///          &lt;add key=&quot;pdf&quot; value=&quot;Adobe PDF&quot; /&gt;
        ///          &lt;add key=&quot;doc&quot; value=&quot;Word&quot; /&gt;
        ///          &lt;add key=&quot;dot&quot; value=&quot;Word&quot; /&gt;
        ///          &lt;add key=&quot;rtf&quot; value=&quot;RTF&quot; /&gt;
        ///          &lt;add key=&quot;docx&quot; value=&quot;Word&quot; /&gt;
        ///          &lt;add key=&quot;xls&quot; value=&quot;Excel&quot; /&gt;
        ///          &lt;add key=&quot;xlt&quot; value=&quot;Excel&quot; /&gt;
        ///          &lt;add key=&quot;xlsx&quot; value=&quot;Excel&quot; /&gt;
        ///          &lt;add key=&quot;csv&quot; value=&quot;CSV&quot; /&gt;
        ///          &lt;add key=&quot;ppt&quot; value=&quot;Powerpoint&quot; /&gt;
        ///          &lt;add key=&quot;pps&quot; value=&quot;Powerpoint&quot; /&gt;
        ///          &lt;add key=&quot;pptx&quot; value=&quot;Powerpoint&quot; /&gt;
        ///          &lt;add key=&quot;mp3&quot; value=&quot;MP3&quot; /&gt;
        ///          &lt;add key=&quot;mov&quot; value=&quot;QuickTime&quot; /&gt;
        ///          &lt;add key=&quot;wmv&quot; value=&quot;Windows Media&quot; /&gt;
        ///          &lt;add key=&quot;wma&quot; value=&quot;Windows Media&quot; /&gt;
        ///          &lt;add key=&quot;exe&quot; value=&quot;Computer program&quot; /&gt;
        ///          &lt;add key=&quot;zip&quot; value=&quot;ZIP of several files&quot; /&gt;
        ///        &lt;/FileTypeNames&gt;
        ///     &lt;/Escc.NavigationControls.WebForms&gt;
        ///  &lt;/configuration&gt;
        /// </code></example>
        /// </remarks>
        public static string ConvertExtensionToFileType(string extension, bool recognisedFileTypesOnly)
        {
            var recognisedFileType = ConvertExtensionToFileType(extension);
            if (String.IsNullOrEmpty(recognisedFileType) && !recognisedFileTypesOnly)
            {
                recognisedFileType = extension.ToUpper(CultureInfo.CurrentCulture);
            }
            return recognisedFileType;
        }

        /// <summary>
        /// Converts a file extension to a plain English file type name, if registered.
        /// </summary>
        /// <param name="extension">The extension.</param>
        /// <remarks>See <see cref="ConvertExtensionToFileType(string,bool)"/>.</remarks>
        public static string ConvertExtensionToFileType(string extension)
        {
            var fileTypeNames = ConfigurationManager.GetSection("Escc.NavigationControls.WebForms/FileTypeNames") as NameValueCollection;
            if (fileTypeNames == null) fileTypeNames = ConfigurationManager.GetSection("EsccWebTeam.NavigationControls/FileTypeNames") as NameValueCollection;
            if (fileTypeNames != null && fileTypeNames[extension] != null)
            {
                return fileTypeNames[extension];
            }
            else
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            // Ensure there's a URL before processing it
            if (this.NavigateUrl == null)
            {
                this.Visible = false;
                return;
            }
            else
            {
                this.Visible = true;
            }

            // First, check that the link is to a document download
            if (String.IsNullOrEmpty(FileExtension)) FileExtension = Path.GetExtension(this.NavigateUrl.ToString()); // Set extension from URL if not set already
            if (String.IsNullOrEmpty(FileExtension)) return;
            FileExtension = FileExtension.ToLowerInvariant().Substring(1); // Remove the "."

            // Is it a file type we want to display?
            var fileType = ConvertExtensionToFileType(FileExtension, RecognisedFileTypesOnly);
            if (String.IsNullOrEmpty(fileType)) return;

            // If it is, add a "document" class to the link
            if (this.UseDefaultClasses)
            {
                if (Attributes["class"] != null)
                {
                    List<string> currentClasses = new List<string>(Attributes["class"].Split(' '));
                    while (currentClasses.Contains("document")) currentClasses.Remove("document"); // If already there remove it, so we don't get "document document document document..."
                    currentClasses.Add("document");
                    Attributes["class"] = String.Join(" ", currentClasses.ToArray());
                }
                else this.Attributes["class"] = "document";
            }

            // Split the link text into the first word and any other words
            Match m = Regex.Match(this.InnerText.ToString(), "^(?<firstword>[^\\W]+)(?<otherwords>.*?)$", RegexOptions.IgnoreCase);

            // Add a span around the first word of the link text. Important to get it around the first word
            // due to CSS issues when a link wraps in certain browsers
            StringBuilder innerHtml = new StringBuilder("<span class=\"");
            if (this.UseDefaultClasses) innerHtml.Append("download ");
            innerHtml.Append(FileExtension)
                    .Append("\">")
                    .Append(m.Groups["firstword"].Value.Trim())
                    .Append("</span>")
                    .Append(m.Groups["otherwords"].Value.TrimEnd());
            this.Controls.Add(new LiteralControl(innerHtml.ToString()));

            // if space was inside end of link, make a note and deal with it later
            linkTextHasTrailingSpace = m.Groups["otherwords"].Value.Length > m.Groups["otherwords"].Value.TrimEnd().Length;
        }

        /// <summary>
        /// Renders the HTML closing tag of the control into the specified writer. This method is used primarily by control developers.
        /// </summary>
        /// <param name="writer">A <see cref="T:System.Web.UI.HtmlTextWriter"/> that represents the output stream to render HTML content on the client.</param>
        public override void RenderEndTag(HtmlTextWriter writer)
        {
            if (!ShowDetailsInsideLink) base.RenderEndTag(writer);

            if (this.Visible)
            {
                // Attempt to find out format and file size of document, and append that information
                StringBuilder linkSuffix = new StringBuilder();
                string type = ConvertExtensionToFileType(FileExtension, this.RecognisedFileTypesOnly);
                string openHtml = " <span class=\"downloadDetail\">(";
                bool hasFileInfo = (type.Length > 0);
                if (hasFileInfo)
                {
                    linkSuffix.Append(openHtml).Append(type);
                }

                // Append file size
                if (FileSize > 0)
                {
                    // Convert to friendlier units and add to HTML
                    int k = FileSize / 1024;
                    if (k <= 999)
                    {
                        if (k == 0) k = 1; // if file less than 1K, show 1K rather than 0K
                        if (hasFileInfo)
                        {
                            linkSuffix.Append(", ");
                        }
                        else
                        {
                            linkSuffix.Append(openHtml);
                            hasFileInfo = true;
                        }
                        linkSuffix.Append(k.ToString(CultureInfo.CurrentCulture)).Append("KB");
                    }
                    else
                    {
                        float mb = (float)k / (float)1024;
                        if (hasFileInfo)
                        {
                            linkSuffix.Append(", ");
                        }
                        else
                        {
                            linkSuffix.Append(openHtml);
                            hasFileInfo = true;
                        }
                        linkSuffix.Append(mb.ToString("###.#", CultureInfo.CurrentCulture)).Append("MB");
                    }
                }

                if (hasFileInfo) linkSuffix.Append(")</span>");

                // if space was inside end of link, move it after the download details
                if (linkTextHasTrailingSpace) linkSuffix.Append(" ");

                writer.Write(linkSuffix.ToString());
            }

            if (ShowDetailsInsideLink) base.RenderEndTag(writer);
        }

        /// <summary>
        /// Returns the HTML element which would be rendered by this control
        /// </summary>
        /// <returns>
        /// An HTML element
        /// </returns>
        public override string ToString()
        {
            EnsureChildControls();

            var sb = new StringBuilder();
            using (var writer = new HtmlTextWriter(new StringWriter(sb)))
            {
                this.RenderControl(writer);
            }
            return sb.ToString();
        }
    }
}
