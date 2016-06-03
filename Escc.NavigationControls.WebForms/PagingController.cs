using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.Web;
using System.Web.UI;

namespace Escc.NavigationControls.WebForms
{
    /// <summary>
    /// Facilitates paging result sets
    /// </summary>
    /// <remarks>
    /// <para>Best practice is to eliminate results from other pages at the data source. If you can't do that you can use the 
    /// <see cref="PagingController.TrimRows(IList)"/> to eliminate irrelevant results from your data before binding it. See <see cref="PagingController.TrimRows(IList)"/> for more details.</para>
    /// </remarks>
    /// <example>
    /// <para>You need to set the <see cref="PagingBarControl.PagingControllerId"/> attribute of each <see cref="PagingBarControl"/> on the page. All other attributes are optional.</para>
    /// <code>
    /// &lt;%@ Register TagPrefix=&quot;NavigationControls&quot; Namespace=&quot;Escc.NavigationControls.WebForms&quot; Assembly=&quot;Escc.NavigationControls.WebForms&quot; %&gt;
    /// &lt;!DOCTYPE html PUBLIC &quot;-//W3C//DTD XHTML 1.0 Strict//EN&quot; &quot;http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd&quot;&gt;
    /// &lt;html xmlns=&quot;http://www.w3.org/1999/xhtml&quot; dir=&quot;ltr&quot; xml:lang=&quot;en&quot;&gt;
    /// 	&lt;body&gt;
    ///
    ///			&lt;NavigationControls:PagingController id=&quot;pagingController&quot; runat=&quot;server&quot; PageSize=&quot;15&quot; ResultsTextSingular=&quot;result&quot; ResultsTextPlural=&quot;results&quot; /&gt;
    ///		
    ///			&lt;NavigationControls:PagingBarControl id=&quot;pagingTop&quot; runat=&quot;server&quot; PagingControllerId=&quot;pagingController&quot; /&gt;
    ///	
    ///			&lt;asp:Repeater id=&quot;myRepeater&quot; runat=&quot;server&quot;&gt;
    ///				&lt;ItemTemplate&gt;
    ///					// your template here
    ///				&lt;/ItemTemplate&gt;
    ///			&lt;/asp:Repeater&gt;
    ///		
    ///			&lt;NavigationControls:PagingBarControl id=&quot;pagingBottom&quot; runat=&quot;server&quot; PagingControllerId=&quot;pagingController&quot; /&gt;
    ///
    ///		&lt;/body&gt;
    /// &lt;/html&gt;
    /// </code>
    /// <para>You also need to set the <see cref="TotalResults"/> property of the <see cref="PagingController"/>, so that it knows how many results it is paging.</para>
    /// <code>
    /// this.pagingController.TotalResults = myDataSource.TotalResults;
    /// </code>
    /// <para>You can set the default page size in <c>web.config</c> using the following syntax. Leave out the <c>&lt;location /&gt;</c> 
    /// element to have it apply to all directories below the location of the <c>web.config</c> file.</para>
    /// <code>
    ///	&lt;configuration&gt;
    ///
    ///		&lt;configSections&gt;
    ///			&lt;sectionGroup name=&quot;Escc.NavigationControls.WebForms&quot;&gt;
    ///				&lt;section name=&quot;Paging&quot; type=&quot;System.Configuration.NameValueSectionHandler, System, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089&quot; /&gt;
    ///			&lt;/sectionGroup&gt;
    ///		&lt;/configSections&gt;
    ///
    ///		&lt;location path=&quot;yourapplication&quot;&gt;
    ///			&lt;Escc.NavigationControls.WebForms&gt;
    ///
    ///				&lt;Paging&gt;
    ///					&lt;add key=&quot;PageSize&quot; value=&quot;20&quot; /&gt;
    ///				&lt;/Paging&gt;
    ///
    ///			&lt;/Escc.NavigationControls.WebForms&gt;
    ///		&lt;/location&gt;
    ///	&lt;/configuration&gt;
    ///	</code>	
    /// <seealso cref="PagingBarControl"/>
    /// </example>
    [DefaultProperty("PageSize"),
    ToolboxData("<{0}:PagingController runat=\"server\"></{0}:PagingController>")]
    public class PagingController : Control
    {

        #region Private fields

        private int totalPages;
        private int totalResults;
        private int pageSize;
        private string resultsTextSingular;
        private string resultsTextPlural;
        private string pageName;
        private string queryString;
        private NameValueCollection pagingConfig;

        #endregion Private fields

        #region Constructors

        /// <summary>
        /// Facilitates paging result sets, and building accessible page navigation
        /// </summary>
        public PagingController()
        {
            this.Initialise();
        }

        /// <summary>
        /// Initialises this instance.
        /// </summary>
        private void Initialise()
        {
            this.pagingConfig = ConfigurationManager.GetSection("Escc.NavigationControls.WebForms/Paging") as NameValueCollection;
            if (this.pagingConfig == null) this.pagingConfig = ConfigurationManager.GetSection("EsccWebTeam.NavigationControls/Paging") as NameValueCollection;

            this.CurrentPage = 1;
            this.totalPages = 1;
            this.totalResults = 0;
            this.CalculateCurrentPage();
            this.pageName = this.Context.Request.Path.ToString();
            if (this.pagingConfig != null && this.pagingConfig["PageSize"] != null)
            {
                try
                {
                    this.PageSize = Int32.Parse(this.pagingConfig["PageSize"], CultureInfo.CurrentCulture);
                }
                catch (FormatException)
                {
                    this.PageSize = 10;
                }
            }
            else
            {
                this.PageSize = 10;
            }
            this.queryString = "?" + this.Context.Request.QueryString.ToString();
            this.resultsTextSingular = Properties.Resources.PagingSingular;
            this.resultsTextPlural = Properties.Resources.PagingPlural;
        }

        #endregion Constructors

        #region Public properties

        /// <summary>
        /// Gets or sets the filename of the web page to which paging links point
        /// </summary>
        public string PageName
        {
            get { return this.pageName; }
            set { this.pageName = value; }
        }

        /// <summary>
        /// Gets or sets the querystring to be used - if not specified, uses the querystring from the current request
        /// </summary>
        public string QueryString
        {
            get { return this.queryString; }
            set { this.queryString = value; }
        }

        /// <summary>
        /// Gets or sets the integer number of the current page in the result set
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// Gets or sets the total number of pages in the result set
        /// </summary>
        public int TotalPages
        {
            get { return this.totalPages; }
            set { this.totalPages = value; }
        }

        /// <summary>
        /// Gets or sets the number of pages available.
        /// </summary>
        /// <value>The available pages.</value>
        public int AvailablePages { get; set; }

        /// <summary>
        /// Gets or sets the total number of results to be paged
        /// </summary>
        public int TotalResults
        {
            get { return this.totalResults; }
            set
            {
                this.totalResults = value;
                this.CalculateTotalPages();
                this.CalculateAvailablePages();
            }
        }

        /// <summary>
        /// Gets or sets whether the total number of results is approximate
        /// </summary>
        /// <value><c>true</c> if total is approximate; otherwise, <c>false</c>.</value>
        public bool TotalIsApproximate { get; set; }

        /// <summary>
        /// Gets or sets the maximum results available. Above this number a search can report that further results exist but not display them.
        /// </summary>
        /// <value>The maximum results available.</value>
        /// <remarks>Don't CalculateAvailablePages() at the point this property is set, because this property is likely to be set when the 
        /// control is created,and that ends up resetting the page to 1 rather than whatever page was requested.</remarks>
        public int MaximumResultsAvailable { get; set; }

        /// <summary>
        /// Gets the number of the first result on the current page
        /// </summary>
        public int FirstResultOnPage
        {
            get
            {
                return (this.CurrentPage * this.PageSize) - (this.PageSize - 1);
            }
        }

        /// <summary>
        /// Gets the number of the last result on the current page
        /// </summary>
        public int LastResultOnPage
        {
            get
            {
                int lastResult = (this.CurrentPage * this.PageSize);
                if (this.CurrentPage == this.TotalPages) lastResult = this.TotalResults;
                return lastResult;
            }
        }

        /// <summary>
        /// Gets the URL of the previous page of results, or null if this is the first page
        /// </summary>
        public Uri PreviousPageUrl
        {
            get
            {
                if (this.CurrentPage > 1)
                {
                    return BuildPagingUrl("page=" + (this.CurrentPage - 1).ToString(CultureInfo.CurrentCulture));
                }
                else return null;
            }
        }

        private Uri BuildPagingUrl(string pageParameter)
        {
            var query = HttpUtility.ParseQueryString(this.QueryString);
            query.Remove("page");
            return new Uri(HttpContext.Current.Request.Url, this.PageName + "?" + query + (query.Count > 0 ? "&" : String.Empty));
        }

        /// <summary>
        /// Gets the URL of the next page of results, or null if this is the final page
        /// </summary>
        public Uri NextPageUrl
        {
            get
            {
                if (this.CurrentPage < this.TotalPages)
                {
                    return BuildPagingUrl("page=" + (this.CurrentPage + 1).ToString(CultureInfo.CurrentCulture));
                }
                else return null;

            }
        }

        /// <summary>
        /// Gets or sets the number or results to appear on each page. Calls to this method are overridden if a "pagesize" parameter is specified in the querysting of the current request.
        /// </summary>
        public int PageSize
        {
            get { return this.pageSize; }
            set
            {
                // allow query string override
                if ((this.Context.Request.QueryString["pagesize"] != null) && (this.Context.Request.QueryString["pagesize"].ToString().Length > 0))
                {
                    this.pageSize = Convert.ToInt32(this.Context.Request.QueryString["pagesize"].ToString(), CultureInfo.CurrentCulture);
                }
                else
                {
                    this.pageSize = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the text to appear in the page navigation bar when a single result is returned. Defaults to "item".
        /// </summary>
        public string ResultsTextSingular
        {
            get { return this.resultsTextSingular; }
            set { this.resultsTextSingular = value; }
        }

        /// <summary>
        /// Gets or sets the text to appear in the page navigation bar when multiple results are returned. Defaults to "items".
        /// </summary>
        public string ResultsTextPlural
        {
            get { return this.resultsTextPlural; }
            set { this.resultsTextPlural = value; }
        }

        /// <summary>
        ///  Gets <seealso cref="ResultsTextSingular"/> or <seealso cref="ResultsTextPlural"/> depending on the total found
        /// </summary>
        /// <returns></returns>
        public string ResultsTextCurrent
        {
            get
            {
                return (this.TotalResults == 1) ? this.ResultsTextSingular : this.ResultsTextPlural;
            }
        }

        #endregion Public properties

        #region Limit page to boundaries
        /// <summary>
        /// This method can be called to limit a page value to within the available page range
        /// at its lower end.
        /// </summary>
        private void LimitPageToLowerBoundary()
        {
            if (this.CurrentPage <= 0)
            {
                // Page 1 is the default landing place for any page choice at zero or below
                this.CurrentPage = 1;
            }
        }

        /// <summary>
        /// This method can be called to limit a page value to within the available page range
        /// at its higher end.
        /// </summary>
        private void LimitPageToUpperBoundary()
        {
            if (this.totalPages <= 0)
            {
                // If the total pages are too low then we are forced to set a sensible default
                this.CurrentPage = 1;
            }
            else if (this.CurrentPage > this.totalPages)
            {
                // The page choice is too high so reset to the maximum page possible
                this.CurrentPage = this.totalPages;
            }
            else if (this.AvailablePages > 0 && this.CurrentPage > this.AvailablePages)
            {
                // The page choice is too high so reset to the maximum page possible
                this.CurrentPage = this.AvailablePages;
            }
        }
        #endregion

        #region Work out the numbers

        /// <summary>
        /// Discovers which page should be the current page - expects the "page" parameter to be specified in the querystring.
        /// </summary>
        private void CalculateCurrentPage()
        {
            if (this.Context.Request.HttpMethod == "POST")
            {
                // Set current page to 1 on postback
                this.CurrentPage = 1;
            }
            else if (this.Context.Request.QueryString["page"] != null)
            {
                try
                {
                    this.CurrentPage = Convert.ToInt32(this.Context.Request.QueryString["page"], CultureInfo.CurrentCulture);
                    // Ensure the current page is not a negative or zero value at this point
                    // (however do not limit the page at the upper end here because we do not know the
                    // total number of pages yet).
                    this.LimitPageToLowerBoundary();
                }
                catch (FormatException)
                {
                    this.CurrentPage = this.totalPages;
                }
                catch (OverflowException)
                {
                    this.CurrentPage = this.totalPages;
                }
            }
            else
            {
                this.CurrentPage = 1;
            }
        }

        /// <summary>
        /// Discovers how many pages there should be, based on the total number of results and the page size.
        /// </summary>
        private void CalculateTotalPages()
        {
            int leftover = (this.totalResults % this.pageSize);
            double pages;

            if (leftover > 0)
            {
                pages = (((this.totalResults - leftover) / this.pageSize) + 1);
            }
            else
            {
                pages = this.totalResults / this.pageSize;
            }
            this.totalPages = Convert.ToInt32(System.Math.Ceiling(pages));

            // Ensure the current page is now not too high 
            this.LimitPageToUpperBoundary();
        }

        /// <summary>
        /// Discovers how many pages that could be displayed if the search returned its maximum number of results
        /// </summary>
        private void CalculateAvailablePages()
        {
            if (this.MaximumResultsAvailable == 0 || (this.TotalResults > 0 && this.TotalResults <= this.MaximumResultsAvailable))
            {
                this.AvailablePages = this.TotalPages;
            }
            else
            {
                int leftover = (this.MaximumResultsAvailable % this.pageSize);
                double pages;

                if (leftover > 0)
                {
                    pages = (((this.MaximumResultsAvailable - leftover) / this.pageSize) + 1);
                }
                else
                {
                    pages = this.MaximumResultsAvailable / this.pageSize;
                }
                this.AvailablePages = Convert.ToInt32(System.Math.Ceiling(pages));
            }


            // Ensure the current page is now not too high 
            this.LimitPageToUpperBoundary();
        }

        /// <summary>
        ///  Gets a results string in the format "{first-on-page} to {last-on-page} of {total}", or just "{total}" if it's only one page 
        /// </summary>
        /// <returns></returns>
        public string CurrentRange
        {
            get
            {
                int firstResult = (this.CurrentPage * this.PageSize) - (this.PageSize - 1);
                int lastResult = (this.CurrentPage * this.PageSize);
                if (this.CurrentPage == this.TotalPages) lastResult = this.TotalResults;

                string text;

                if (this.TotalResults <= this.PageSize)
                {
                    // only one page of results
                    text = this.TotalResults.ToString(CultureInfo.CurrentCulture);
                }
                else if (firstResult == this.TotalResults || firstResult == lastResult)
                {
                    // only one result on the page
                    text = firstResult.ToString(CultureInfo.CurrentCulture) + " of " + this.TotalResults.ToString(CultureInfo.CurrentCulture);
                }
                else if (this.CurrentPage == this.TotalPages)
                {
                    // last page
                    text = firstResult.ToString(CultureInfo.CurrentCulture) + "-" + lastResult.ToString(CultureInfo.CurrentCulture) + " of " + this.TotalResults.ToString(CultureInfo.CurrentCulture);
                }
                else
                {
                    // multiple pages and this isn't the last one
                    var of = this.TotalIsApproximate ? " of about " : " of ";
                    text = firstResult.ToString(CultureInfo.CurrentCulture) + "-" + lastResult.ToString(CultureInfo.CurrentCulture) + of + this.TotalResults.ToString(CultureInfo.CurrentCulture);
                }

                return text;
            }
        }

        #endregion Work out the numbers

        #region TrimRows

        /// <summary>
        /// If paging has not been handled by the database, use this method to delete results which are not needed for the current page.
        /// </summary>
        /// <param name="listSource">A collection of items to be paged</param>
        /// <example>
        /// <para>Best practice is to eliminate results from other pages at the data source. For example, if you are viewing the first 10 of 10,000 results, 
        /// you should avoid retrieving the other 9,990 results. Sometimes this isn't possible. For example, you may not be able to determine how many rows
        /// returned by your data source make up a single result.</para>
        /// <para>In these cases you can use <see cref="PagingController.TrimRows(IList)"/> to eliminate the 
        /// irrelevant results from your data before binding it.</para>
        /// <code>
        /// this.pagingController.TrimRows(myCollection);
        /// </code>
        /// <code>
        /// this.pagingController.TrimRows(myDataView);
        /// </code>
        /// <para>Example of best practice: paging data in a SQL Server stored procedure, avoiding the need to call <see cref="PagingController.TrimRows(IList)"/>:</para>
        /// <code>
        ///	CREATE PROCEDURE [dbo].[usp_Paging_Example]
        ///		@pageSize int,
        ///		@pageNumber int,
        ///		@otherParameter varchar(100)
        ///	AS
        ///	BEGIN
        ///		-- SET NOCOUNT ON added to prevent extra result sets from
        ///		-- interfering with SELECT statements.
        ///		SET NOCOUNT ON;
        ///
        ///		-- Create a temp table which will have no gaps in the primary key sequence
        ///		DECLARE @tempTable TABLE 
        ///		(
        ///			Id int IDENTITY PRIMARY KEY,
        ///			YourDataId int
        ///		)
        ///
        ///		-- Get all the relevant results
        ///		INSERT INTO @tempTable 
        ///		(
        ///			YourDataIdD
        ///		)
        ///		SELECT 
        ///			YourDataId
        ///		FROM
        ///			YourTable
        ///		WHERE 
        ///			YourDataField LIKE @otherParameter
        ///		ORDER BY
        ///		    YourSortField
        ///
        ///		-- Calculate the first and last id of the range of results we need
        ///		DECLARE @fromId int
        ///		DECLARE @toId int
        ///
        ///		SET @fromId = ((@pageNumber - 1) * @pageSize) + 1
        ///		SET @toId = @pageNumber * @pageSize
        ///
        ///		-- Select the results
        ///		SELECT 
        ///			YourDataId, YourDataField
        ///		FROM
        ///		    YourTable
        ///		WHERE
        ///		    YourDataId IN (SELECT YourDataId FROM @tempTable WHERE Id &gt;= @fromId AND Id &lt;= @toId)
        ///		ORDER BY
        ///		    YourSortField
        ///
        ///	END
        /// </code>
        /// </example>
        /// <exception cref="System.ArgumentNullException" />
        public void TrimRows(IListSource listSource)
        {
            if (listSource == null) throw new ArgumentNullException("listSource");
            this.TrimRows(listSource.GetList());
        }

        /// <summary>
        /// If paging has not been handled by the database, use this method to delete results which are not needed for the current page.
        /// </summary>
        /// <param name="items">A collection of items to be paged</param>
        /// <exception cref="System.ArgumentNullException" />
        public void TrimRows(IList items)
        {
            if (items == null) throw new ArgumentNullException("items");

            // update totalResults if not set
            if (this.totalResults == 0 && items.Count > 0) this.TotalResults = items.Count;

            // work out which items items we need for this page
            int firstResultRow = ((this.CurrentPage * this.pageSize) - (this.pageSize - 1)) - 1;
            int lastResultRow = (this.CurrentPage * this.pageSize) - 1;
            if (this.CurrentPage == this.totalPages) lastResultRow = this.totalResults - 1;

            // loop through and delete the ones we don't need
            // loop backwards because otherwise deleting rows messes up the count using i
            for (int i = items.Count - 1; i >= 0; i--)
            {
                if ((i < firstResultRow) || (i > lastResultRow)) items.RemoveAt(i); // no delete supported
            }
        }

        /// <summary>
        /// If paging has not been handled by the database, use this method to delete results which are not needed for the current page.
        /// </summary>
        /// <param name="items">A collection of items to be paged</param>
        /// <exception cref="System.ArgumentNullException" />
        public void TrimRows<T>(IList<T> items)
        {
            if (items == null) throw new ArgumentNullException("items");

            // update totalResults if not set
            if (this.totalResults == 0 && items.Count > 0) this.TotalResults = items.Count;

            // work out which items items we need for this page
            int firstResultRow = ((this.CurrentPage * this.pageSize) - (this.pageSize - 1)) - 1;
            int lastResultRow = (this.CurrentPage * this.pageSize) - 1;
            if (this.CurrentPage == this.totalPages) lastResultRow = this.totalResults - 1;

            // loop through and delete the ones we don't need
            // loop backwards because otherwise deleting rows messes up the count using i
            for (int i = items.Count - 1; i >= 0; i--)
            {
                if ((i < firstResultRow) || (i > lastResultRow)) items.RemoveAt(i); // no delete supported
            }
        }


        #endregion TrimRows



    }
}
