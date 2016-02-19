using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace EsccWebTeam.NavigationControls
{
    /// <summary>
    /// An indicator of the current stage in a multi-stage process
    /// </summary>
    /// <remarks>
    /// <para>The stages of the progress bar are read from the <c>EsccWebTeam.NavigationControls/ProgressBar</c> section in <c>web.config</c>. 
    /// The keys become the stage labels. The values are not used by default.</para>
    /// <para>If you want the user to be able to click the progress bar to go back to previous stages, set the <see cref="LinkCompletedStages"/>
    /// property to <c>true</c>. You can make this the default using the <c>EsccWebTeam.NavigationControls/ProgressBarSettings</c> section in <c>web.config</c>.
    /// If you enable this option, the URLs to link to must be entered as the values corresponding to the stage labels.</para>
    /// <para>If you need to pass parameters around between the pages, list the names of the parameters, spearated by semi-colons, in the <c>ParametersToPreserve</c>
    /// property of the <c>EsccWebTeam.NavigationControls/ProgressBarSettings</c> section in <c>web.config</c>. The value of the first parameter listed here
    /// can be represented as <c>{0}</c> in page URLs, the second parameter as <c>{1}</c> and so on. These will be populated with the value from the current
    /// request at runtime. Note that ampersands in URLs should be double-encoded as <c>&amp;amp;amp;</c> to ensure they appear correctly in the final HTML.</para>
    /// <example><code>
    /// &lt;configuration&gt;
    ///   &lt;configSections&gt;
    ///     &lt;sectionGroup name=&quot;EsccWebTeam.NavigationControls&quot;&gt;
    ///       &lt;section name=&quot;ProgressBar&quot; type=&quot;System.Configuration.NameValueSectionHandler, System, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089&quot; /&gt;
    ///       &lt;section name=&quot;ProgressBarSettings&quot; type=&quot;System.Configuration.NameValueSectionHandler, System, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089&quot; /&gt;
    ///     &lt;/sectionGroup&gt;
    ///   &lt;/configSections&gt;
    /// 
    ///   &lt;EsccWebTeam.NavigationControls&gt;
    ///     &lt;ProgressBar&gt;
    ///       &lt;add key=&quot;Stage 1&quot; value=&quot;/example-application/stage1.aspx?example={0}&amp;amp;amp;id={1}&quot;/&gt;
    ///       &lt;add key=&quot;Stage 2&quot; value=&quot;/example-application/stage2.aspx?example={0}&amp;amp;amp;id={1}&quot;/&gt;
    ///       &lt;add key=&quot;Stage 3&quot; value=&quot;/example-application/stage3.aspx?example={0}&amp;amp;amp;id={1}&quot;/&gt;
    ///     &lt;/ProgressBar&gt;
    ///     &lt;ProgressBarSettings&gt;
    ///       &lt;add key=&quot;LinkCompletedStages&quot; value=&quot;true&quot; /&gt;
    ///       &lt;add key=&quot;LinkFutureStages&quot; value=&quot;false&quot; /&gt;
    ///       &lt;add key=&quot;ParametersToPreserve&quot; value=&quot;example;id&quot; /&gt;
    ///     &lt;/ProgressBarSettings&gt;
    ///   &lt;/EsccWebTeam.NavigationControls&gt;
    /// &lt;/configuration&gt;
    /// </code></example>
    /// </remarks>
    [DefaultProperty("CurrentStage"),
    ToolboxData("<{0}:ProgressBarControl runat=\"server\"></{0}:ProgressBarControl>")]
    public class ProgressBarControl : System.Web.UI.WebControls.WebControl
    {
        private ProgressStageCollection stages;
        private List<string> parametersToPreserve = new List<string>();

        /// <summary>
        /// Gets or sets the current stage.
        /// </summary>
        /// <value>The current stage.</value>
        /// <remarks>Designed to work when stages specified declaratively, in config and ASPX. Won't work with programmatic stage-setting.</remarks>
        public int CurrentStage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to display previous stages as links, allowing backward navigation.
        /// </summary>
        /// <value><c>true</c> to link previous stages; otherwise, <c>false</c>.</value>
        public bool LinkCompletedStages { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to display future stages as links, allowing forward navigation.
        /// </summary>
        /// <value><c>true</c> to link future stages; otherwise, <c>false</c>.</value>
        public bool LinkFutureStages { get; set; }

        /// <summary>
        /// Gets or sets the stages.
        /// </summary>
        /// <value>The stages.</value>
        public ProgressStageCollection Stages
        {
            get
            {
                return this.stages;
            }
        }

        /// <summary>
        /// Gets the parameters to preserve across requests when linking to stages.
        /// </summary>
        /// <value>The parameters to preserve.</value>
        public IList<string> ParametersToPreserve { get { return this.parametersToPreserve; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressBarControl"/> class.
        /// </summary>
        public ProgressBarControl()
            : base()
        {
            this.stages = new ProgressStageCollection();
            this.CurrentStage = 1;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this.GetStagesFromConfig();
            this.GetSettingsFromConfig();
        }


        /// <summary>
        /// Gets the stages from web.config.
        /// </summary>
        private void GetStagesFromConfig()
        {
            // Get config: it's optional
            NameValueCollection config = ConfigurationManager.GetSection("EsccWebTeam.NavigationControls/" + ConfigSectionName) as NameValueCollection;
            if (config != null)
            {
                // Add a stage for each entry in config
                // Text is in keys, URLs are in values
                for (int i = 0; i < config.AllKeys.Length; i++)
                {
                    if (config[config.AllKeys[i]] != null)
                    {
                        bool completed = (i < this.CurrentStage - 1);
                        bool current = (i == this.CurrentStage - 1);
                        ProgressStage stage = new ProgressStage(config.AllKeys[i], completed, current);
                        stage.StageUrl = new Uri(config[config.AllKeys[i]], UriKind.RelativeOrAbsolute);
                        this.stages.Add(stage);
                    }
                    else break;
                }
            }
        }

        /// <summary>
        /// Gets or sets the name of the configuration section to read the progress bar from ('ProgressBar' by default).
        /// </summary>
        /// <value>
        /// The name of the configuration section.
        /// </value>
        public string ConfigSectionName { get; set; } = "ProgressBar";

        /// <summary>
        /// Gets settings from web.config controlling the behaviour.
        /// </summary>
        private void GetSettingsFromConfig()
        {
            // Get config: it's optional
            NameValueCollection config = ConfigurationManager.GetSection("EsccWebTeam.NavigationControls/" + ConfigSectionName + "Settings") as NameValueCollection;
            if (config != null)
            {
                if (config["LinkCompletedStages"] != null) LinkCompletedStages = Convert.ToBoolean(config["LinkCompletedStages"], CultureInfo.CurrentCulture);
                if (config["LinkFutureStages"] != null) LinkFutureStages = Convert.ToBoolean(config["LinkFutureStages"], CultureInfo.CurrentCulture);
                if (config["ParametersToPreserve"] != null)
                {
                    string[] parameters = config["ParametersToPreserve"].Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                    this.parametersToPreserve.AddRange(parameters);
                }
            }
        }

        #region Control over rendering

        /// <summary>
        /// Notifies server controls that use composition-based implementation to create any child
        /// controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            if (this.stages.Count > 0)
            {
                using (HtmlGenericControl list = new HtmlGenericControl("ol"))
                {
                    list.Attributes["class"] = "progressBar";
                    this.Controls.Add(list);

                    // Get the values of any query string parameters which are due to be preserved
                    int len = this.parametersToPreserve.Count;
                    string[] queryStringParameters = new string[len];
                    for (int i = 0; i < len; i++)
                    {
                        queryStringParameters[i] = Context.Request.QueryString[this.parametersToPreserve[i]];
                    }


                    foreach (ProgressStage st in this.stages)
                    {
                        using (HtmlGenericControl li = new HtmlGenericControl("li"))
                        {
                            if (st.Current)
                            {
                                li.Attributes["class"] = "current";
                                using (HtmlGenericControl em = new HtmlGenericControl("em"))
                                {
                                    em.InnerText = st.StageName;
                                    li.Controls.Add(em);
                                }
                            }
                            else if (st.Completed)
                            {
                                li.Attributes["class"] = "completed";
                                if (LinkCompletedStages)
                                {
                                    using (HtmlAnchor stageLink = new HtmlAnchor())
                                    {
                                        // Get the link from config, including the values of any parameters which are to be preserved across requests
                                        stageLink.HRef = BuildStageUrl(st.StageUrl, queryStringParameters);

                                        stageLink.InnerText = st.StageName;
                                        li.Controls.Add(stageLink);
                                    }
                                }
                                else
                                {
                                    li.InnerText = st.StageName;
                                }
                            }
                            else
                            {
                                if (LinkFutureStages)
                                {
                                    using (HtmlAnchor stageLink = new HtmlAnchor())
                                    {
                                        // Get the link from config, including the values of any parameters which are to be preserved across requests
                                        stageLink.HRef = BuildStageUrl(st.StageUrl, queryStringParameters);

                                        stageLink.InnerText = st.StageName;
                                        li.Controls.Add(stageLink);
                                    }
                                }
                                else
                                {
                                    li.InnerText = st.StageName;
                                }
                            }
                            list.Controls.Add(li);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Builds the stage URL when linking to other stages.
        /// </summary>
        /// <param name="stageUrl">The stage URL.</param>
        /// <param name="queryStringParameters">The query string parameters.</param>
        /// <returns></returns>
        private string BuildStageUrl(Uri stageUrl, string[] queryStringParameters)
        {
            string link = String.Format(CultureInfo.CurrentCulture, stageUrl.ToString(), queryStringParameters);

            // To hide params which are perhaps meant to be optional, remove any params with no value from the query string
            int pos = link.IndexOf('?');
            if (pos > 0)
            {
                string query = link.Substring(pos);
                foreach (string name in this.parametersToPreserve)
                {
                    if (String.IsNullOrEmpty(Context.Request.QueryString[name])) query = RemoveParameter(query, name);
                }
                if (query.EndsWith("?", false, CultureInfo.InvariantCulture)) query = query.Substring(0, query.Length - 1);
                if (query.EndsWith("&amp;", false, CultureInfo.InvariantCulture)) query = query.Substring(0, query.Length - 5);
                link = link.Substring(0, pos) + query;
            }
            return link;
        }

        /// <summary>
        /// Remove any existing parameter with a specified key from a given query string
        /// </summary>
        /// <param name="query">Existing query string</param>
        /// <param name="parameterName">Parameter key</param>
        /// <returns>Modified query string, beginning with "?"</returns>
        [Obsolete("Need to update code to call EsccWebTeam.Data.Web.Iri.RemoveParameterFromQueryString")]
        private static string RemoveParameter(string query, string parameterName)
        {
            if (query == null) throw new ArgumentNullException("query");

            // if supplied querystring starts with ?, remove it
            if (query.StartsWith("?")) query = query.Substring(1);

            // split supplied querystring into sections
            query = query.Replace("&amp;", "&");
            string[] qsBits = query.Split('&');

            //rebuild query string without its parameter= value
            StringBuilder newQS = new StringBuilder();

            for (int i = 0; i < qsBits.Length; i++)
            {
                string[] paramBits = qsBits[i].Split('=');

                if ((paramBits[0] != parameterName) && (paramBits.Length > 1))
                {
                    if (newQS.Length > 0) newQS.Append("&amp;");
                    newQS.Append(paramBits[0]).Append("=").Append(paramBits[1]);
                }
            }

            // get querystring ready for new parameter
            if (newQS.Length > 0) newQS.Append("&amp;");
            else newQS.Append("?");

            string completeQS = newQS.ToString();
            if (!completeQS.StartsWith("?")) completeQS = "?" + completeQS;

            return completeQS;
        }

        /// <summary> 
        /// Stop WebControl from outputting a span element
        /// </summary>
        /// <param name="writer"> The HTML writer to write out to </param>
        public override void RenderBeginTag(HtmlTextWriter writer)
        {
            // don't render it

        }

        /// <summary> 
        /// Stop WebControl from outputting a span element
        /// </summary>
        /// <param name="writer"> The HTML writer to write out to </param>
        public override void RenderEndTag(HtmlTextWriter writer)
        {
            // don't render it
        }

        #endregion Control over rendering

    }
}
