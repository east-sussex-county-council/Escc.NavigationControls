using System;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Escc.NavigationControls.WebForms
{
    /// <summary>
    /// An accessible paged navigation bar
    /// </summary>
    /// <remarks>
    /// See <see cref="PagingController"/> for an example of how to use this control.
    /// <seealso cref="PagingController"/>
    /// </remarks>
    [DefaultProperty("PagingControllerId"),
        ToolboxData("<{0}:PagingBarControl runat=\"server\"></{0}:PagingBarControl>")]
    [ParseChildren(ChildrenAsProperties = true)]
    public class PagingBarControl : System.Web.UI.WebControls.WebControl
    {
        #region Public properties

        /// <summary>
        /// Gets or sets the the paging controller, which controls the current page, number of pages, page size etc.
        /// </summary>
        /// <value>The controller.</value>
        /// <remarks>If the <see cref="PagingControllerId"/> property is given a value before <see cref="CreateChildControls"/> executes,
        /// the <see cref="PagingControllerId"/> will be used to update this property.</remarks>
        public PagingController PagingController
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the id of the paging controller, which controls the current page, number of pages, page size etc.
        /// </summary>
        /// <value>The controller id.</value>
        public string PagingControllerId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the layout of the paging bar using controls in a template
        /// </summary>
        [TemplateContainer(typeof(XhtmlContainer))]
        [PersistenceMode(PersistenceMode.InnerDefaultProperty)] // This makes it validate in Visual Studio 
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)] // This makes it validate in Visual Studio 
        public ITemplate PagingTemplate { get; set; }

        /// <summary>
        /// Gets or sets the layout of the no results view using controls in a template
        /// </summary>
        [TemplateContainer(typeof(XhtmlContainer))]
        [PersistenceMode(PersistenceMode.InnerDefaultProperty)] // This makes it validate in Visual Studio
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)] // This makes it validate in Visual Studio 
        public ITemplate NoResultsTemplate { get; set; }

        #endregion Public properties

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PagingBarControl"/> class.
        /// </summary>
        public PagingBarControl()
            : base(HtmlTextWriterTag.Div)
        {
            this.CssClass = "roundedBox infoBar";
        }
        #endregion Constructors

        #region CreateChildControls

        /// <summary>
        /// Notifies server controls that use composition-based implementation to create any child
        /// controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            // Get the paging controller
            if (this.PagingControllerId != null && this.PagingControllerId.Length > 0)
            {
                this.PagingController = this.Parent.FindControl(this.PagingControllerId) as PagingController;

                // this isn't normal behaviour but is needed 
                // for backwards compatibility
                if (this.PagingController == null)
                {
                    this.PagingController = this.Page.FindControl(this.PagingControllerId) as PagingController;
                }
            }

            // did we find a paging controller?
            if (this.PagingController == null)
            {
                string badControllerId = PagingControllerId == null ? "<undefined>" : PagingControllerId;

                throw new Exception(
                    string.Format(CultureInfo.CurrentCulture, "No paging controller with specified id '{0}'", badControllerId)
                );
            }

            // Only display if there are some search results
            if (this.PagingController.TotalResults > 0)
            {
                this.Visible = true;

                if (this.PagingTemplate != null)
                {
                    // Base the display on the template
                    using (var container = new XhtmlContainer())
                    {
                        this.PagingTemplate.InstantiateIn(container);

                        // If range control is found, inject "{first-on-page} to {last-on-page} of {total}", or just "{total}" if it's only one page 
                        var range = container.FindControl("range") as Literal;
                        if (range != null) range.Text = HttpUtility.HtmlEncode(this.PagingController.CurrentRange).Replace("-", "&#8211;");

                        // If results control is found, inject the results singular or plural text
                        var results = container.FindControl("results") as Literal;
                        if (results != null) results.Text = HttpUtility.HtmlEncode(this.PagingController.ResultsTextCurrent);

                        // If pages control is found, inject the list of pages
                        var pages = container.FindControl("pages") as Literal;
                        if (pages != null) pages.Text = this.BuildPagesXhtml();

                        this.Controls.Add(container);
                    }
                }
                else
                {
                    // Otherwise create a containing box
                    using (HtmlGenericControl inner1 = new HtmlGenericControl("div"))
                    {
                        using (HtmlGenericControl inner2 = new HtmlGenericControl("div"))
                        {
                            using (HtmlGenericControl inner3 = new HtmlGenericControl("div"))
                            {

                                this.Controls.Add(inner1);
                                inner1.Controls.Add(inner2);
                                inner2.Controls.Add(inner3);

                                // Add "page x of y"
                                using (HtmlGenericControl resultsInContext = new HtmlGenericControl("div"))
                                {
                                    resultsInContext.Attributes["class"] = "pagingResultsInContext";
                                    resultsInContext.InnerHtml = this.BuildResultsInContextXhtml();
                                    inner3.Controls.Add(resultsInContext);
                                }

                                // Add links to other pages
                                using (HtmlGenericControl pages = new HtmlGenericControl("div"))
                                {
                                    pages.Attributes["class"] = "pagingPages";
                                    pages.InnerHtml = this.BuildPagesXhtml();
                                    if (pages.InnerHtml.Length > 0) inner3.Controls.Add(pages);
                                }
                            }
                        }
                    }
                }

            }
            else
            {
                if (this.NoResultsTemplate != null)
                {
                    using (var templateContainer = new XhtmlContainer())
                    {
                        this.NoResultsTemplate.InstantiateIn(templateContainer);
                        this.Controls.Add(templateContainer);
                    }
                }
                else
                {
                    this.Visible = false;
                }
            }
        }

        /// <summary>
        /// Renders the HTML opening tag of the control to the specified writer. This method is used primarily by control developers.
        /// </summary>
        /// <param name="writer">A <see cref="T:System.Web.UI.HtmlTextWriter"/> that represents the output stream to render HTML content on the client.</param>
        public override void RenderBeginTag(HtmlTextWriter writer)
        {
            // Only render the default HTML if a paging template is not supplied. The template gives more control.
            if (this.PagingController.TotalResults > 0 && this.PagingTemplate == null)
            {
                base.RenderBeginTag(writer);
            }
        }

        /// <summary>
        /// Renders the HTML closing tag of the control into the specified writer. This method is used primarily by control developers.
        /// </summary>
        /// <param name="writer">A <see cref="T:System.Web.UI.HtmlTextWriter"/> that represents the output stream to render HTML content on the client.</param>
        public override void RenderEndTag(HtmlTextWriter writer)
        {
            // Only render the default HTML if a paging template is not supplied. The template gives more control.
            if (this.PagingController.TotalResults > 0 && this.PagingTemplate == null)
            {
                base.RenderEndTag(writer);
            }
        }

        /// <summary>
        /// Container control for PagingTemplate
        /// </summary>
        private class XhtmlContainer : PlaceHolder, INamingContainer { }

        #endregion CreateChildControls


        #region Build XHTML for default display

        /// <summary>
        ///  Gets a results string in the format "Items x-y of z"
        /// </summary>
        /// <returns></returns>
        private string BuildResultsInContextXhtml()
        {
            int firstResult = (this.PagingController.CurrentPage * this.PagingController.PageSize) - (this.PagingController.PageSize - 1);
            int lastResult = (this.PagingController.CurrentPage * this.PagingController.PageSize);
            if (this.PagingController.CurrentPage == this.PagingController.TotalPages) lastResult = this.PagingController.TotalResults;

            string text;

            if (this.PagingController.TotalResults == 0)
            {
                text = "0 " + this.PagingController.ResultsTextPlural;
            }
            else if (this.PagingController.TotalResults == 1)
            {
                text = this.PagingController.ResultsTextSingular.Substring(0, 1).ToUpper(CultureInfo.CurrentCulture) + this.PagingController.ResultsTextSingular.Substring(1) + " <span class=\"pagingCurrentResults\">" + firstResult + "</span> of " + this.PagingController.TotalResults;
            }
            else if (firstResult == this.PagingController.TotalResults || firstResult == lastResult)
            {
                text = this.PagingController.ResultsTextSingular.Substring(0, 1).ToUpper(CultureInfo.CurrentCulture) + this.PagingController.ResultsTextSingular.Substring(1) + " <span class=\"pagingCurrentResults\">" + firstResult.ToString(CultureInfo.CurrentCulture) + "</span> of " + this.PagingController.TotalResults.ToString(CultureInfo.CurrentCulture) + " " + this.PagingController.ResultsTextPlural;
            }
            else
            {
                text = "<span class=\"pagingCurrentResults\">" + firstResult.ToString(CultureInfo.CurrentCulture) + "&#8211;" + lastResult.ToString(CultureInfo.CurrentCulture) + "</span> of " + this.PagingController.TotalResults.ToString(CultureInfo.CurrentCulture) + " " + this.PagingController.ResultsTextPlural;
            }

            return text;
        }

        /// <summary>
        /// Build up the links to other pages, including Next and Previous links
        /// </summary>
        /// <returns></returns>
        private string BuildPagesXhtml()
        {
            int totalPages = Convert.ToInt32(this.PagingController.TotalPages);
            int lower1;
            int upper1 = 0;
            int lower2 = this.PagingController.CurrentPage;
            int upper2;
            int lowerSpread = this.PagingController.CurrentPage - 1;
            int upperSpread = totalPages - this.PagingController.CurrentPage;
            bool showUpperEllipses = false;
            bool showLowerEllipses = false;
            StringBuilder navLinks = new StringBuilder();

            var query = HttpUtility.ParseQueryString(this.PagingController.QueryString);
            query.Remove("page");
            var pageUrlReadyForParameter = HttpUtility.HtmlEncode(this.PagingController.PageName + "?" + query + (query.Count > 0 ? "&" : String.Empty));

            string linkTemplate = "<a href=\"" + pageUrlReadyForParameter + "page={0}\">{0}</a>";
            const string linkEllipses = "&#8230;";
            string linkPrev = "<a href=\"" + pageUrlReadyForParameter + "page={0}\">&lt; Prev</a> ";
            string linkNext = " <a href=\"" + pageUrlReadyForParameter + "page={0}\">Next &gt;</a>";

            // don't bother with page navigation if there's only one page
            if (totalPages > 1)
            {
                // render prev link
                if (this.PagingController.CurrentPage > 1)
                {
                    navLinks.Append(String.Format(CultureInfo.CurrentCulture, linkPrev, (this.PagingController.CurrentPage - 1).ToString(CultureInfo.CurrentCulture)));
                }

                // calculate ellipses
                // calculate lower
                if ((this.PagingController.CurrentPage - 1) > 1)
                {
                    upper1 = this.PagingController.CurrentPage - 1;
                }

                if (lowerSpread < 3)
                {
                    lower1 = upper1;
                }
                else
                {
                    if (lowerSpread > 5)
                    {
                        lower1 = this.PagingController.CurrentPage - 4;
                        showLowerEllipses = true;
                    }
                    else
                    {
                        lower1 = this.PagingController.CurrentPage - (lowerSpread - 1);
                    }
                }
                // calculate upper
                if ((this.PagingController.CurrentPage + 1) < totalPages)
                {
                    lower2 = this.PagingController.CurrentPage + 1;
                }
                if (upperSpread < 3)
                {
                    upper2 = lower2;
                }
                else
                {
                    if (upperSpread > 5)
                    {
                        upper2 = this.PagingController.CurrentPage + 4;
                        showUpperEllipses = true;
                    }
                    else
                    {
                        upper2 = this.PagingController.CurrentPage + (upperSpread - 1);
                    }
                }

                // render Page 1 link
                if (this.PagingController.CurrentPage == 1)
                {
                    navLinks.Append("<em>1</em> ");
                }
                else
                {
                    navLinks.Append(String.Format(CultureInfo.CurrentCulture, linkTemplate, "1"));
                    navLinks.Append(" ");
                }

                // render Lower Ellipses
                if (lower1 > 0)
                {
                    if (showLowerEllipses)
                    {
                        navLinks.Append(String.Format(CultureInfo.CurrentCulture, linkEllipses, (lower1 - 1).ToString(CultureInfo.CurrentCulture)));
                        navLinks.Append(" ");
                    }

                    for (int i = lower1; i <= upper1; i++)
                    {
                        navLinks.Append(String.Format(CultureInfo.CurrentCulture, linkTemplate, i.ToString(CultureInfo.CurrentCulture)));
                        navLinks.Append(" ");
                    }
                }

                // render Current Page link
                if ((this.PagingController.CurrentPage > 1) && (this.PagingController.CurrentPage != totalPages))
                {
                    navLinks.Append("<em>");
                    navLinks.Append(this.PagingController.CurrentPage.ToString(CultureInfo.CurrentCulture));
                    navLinks.Append("</em> ");
                }


                // render Upper Ellipses
                if ((lower2 > 0) && (upper2 != this.PagingController.CurrentPage))
                {
                    for (int i = lower2; i <= upper2; i++)
                    {
                        if (i > this.PagingController.AvailablePages) break;

                        navLinks.Append(String.Format(CultureInfo.CurrentCulture, linkTemplate, i.ToString(CultureInfo.CurrentCulture)));
                        navLinks.Append(" ");
                    }
                    if (showUpperEllipses)
                    {
                        if ((upper2 + 1) <= this.PagingController.AvailablePages)
                        {
                            navLinks.Append(String.Format(CultureInfo.CurrentCulture, linkEllipses, (upper2 + 1).ToString(CultureInfo.CurrentCulture)));
                            navLinks.Append(" ");
                        }
                    }
                }

                // If there are more results found than can be displayed, just list the next few pages. 
                // Otherwise list the final page.
                if (this.PagingController.MaximumResultsAvailable == 0 || this.PagingController.MaximumResultsAvailable >= this.PagingController.TotalResults)
                {
                    // render Last Page link
                    if (this.PagingController.CurrentPage == totalPages)
                    {
                        navLinks.Append("<em>");
                        navLinks.Append(totalPages.ToString(CultureInfo.CurrentCulture));
                        navLinks.Append("</em>");
                    }
                    else
                    {
                        navLinks.Append(String.Format(CultureInfo.CurrentCulture, linkTemplate, totalPages.ToString(CultureInfo.CurrentCulture)));
                    }
                }

                // render next link
                if (this.PagingController.CurrentPage < this.PagingController.AvailablePages)
                {
                    navLinks.Append(String.Format(CultureInfo.CurrentCulture, linkNext, (this.PagingController.CurrentPage + 1).ToString(CultureInfo.CurrentCulture)));
                }
            }

            return (navLinks.Length > 0) ? navLinks.ToString() : String.Empty;

        }

        #endregion // Build XHTML for default display

    }
}
