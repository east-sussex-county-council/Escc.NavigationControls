using System;

namespace Escc.NavigationControls.WebForms
{
    /// <summary>
    /// A stage on a <see cref="ProgressBarControl"/>.
    /// </summary>
    public struct ProgressStage
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ProgressStage"/> is current.
        /// </summary>
        /// <value><c>true</c> if current; otherwise, <c>false</c>.</value>
        public bool Current { get; set; }


        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ProgressStage"/> is completed.
        /// </summary>
        /// <value><c>true</c> if completed; otherwise, <c>false</c>.</value>
        public bool Completed { get; set; }


        /// <summary>
        /// Gets or sets the name of the stage.
        /// </summary>
        /// <value>The name of the stage.</value>
        public string StageName { get; set; }

        /// <summary>
        /// Gets or sets the URL of the page.
        /// </summary>
        /// <value>The stage URL.</value>
        public Uri StageUrl { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressStage"/> class.
        /// </summary>
        /// <param name="stageName">Name of the stage.</param>
        /// <param name="completed">if set to <c>true</c> stage is completed.</param>
        /// <param name="current">if set to <c>true</c> stage is current stage.</param>
        public ProgressStage(string stageName, bool completed, bool current)
            : this()
        {
            this.StageName = stageName;
            this.Completed = completed;
            this.Current = current;
        }


        /// <summary>
        /// Determines whether the specified <see cref="ProgressStage"/> has the same name as the current <see cref="ProgressStage"/>.
        /// </summary>
        /// <param name="obj">The <see cref="ProgressStage"/> to compare with the current <see cref="ProgressStage"/>.</param>
        /// <returns>
        /// 	<see langword="true"/> if the specified <see cref="T:System.Object"/> is equal to the
        /// current <see cref="T:System.Object"/>; otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="System.InvalidCastException">Thrown if the specified object is not a <see cref="ProgressStage"/></exception>
        public override bool Equals(object obj)
        {
            return (this.StageName == ((ProgressStage)obj).StageName);
        }

        /// <summary>
        /// Determines whether the two specified <see cref="ProgressStage"/> objects have the same name.
        /// </summary>
        /// <param name="firstStage">The first stage.</param>
        /// <param name="secondStage">The second stage.</param>
        /// <returns><see langword="true"/> if the specified <see cref="ProgressStage"/> objects have the same name; 
        /// otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(ProgressStage firstStage, ProgressStage secondStage)
        {
            return (firstStage.StageName == secondStage.StageName);
        }

        /// <summary>
        /// Determines whether the two specified <see cref="ProgressStage"/> objects have different names.
        /// </summary>
        /// <param name="firstStage">The first stage.</param>
        /// <param name="secondStage">The second stage.</param>
        /// <returns><see langword="true"/> if the specified <see cref="ProgressStage"/> objects have different names; 
        /// otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(ProgressStage firstStage, ProgressStage secondStage)
        {
            return !(firstStage.StageName == secondStage.StageName);
        }


        /// <summary>
        /// Serves as a hash function for a particular type, suitable
        /// for use in hashing algorithms and data structures like a hash table.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            return this.StageName.GetHashCode();
        }


    }
}
