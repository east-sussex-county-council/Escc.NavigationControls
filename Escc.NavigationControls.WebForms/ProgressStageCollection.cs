using System.Collections;
using System.Collections.Generic;

namespace Escc.NavigationControls.WebForms
{

	/// <summary>
	/// A collection of stages in a process
	/// </summary>
    public class ProgressStageCollection : CollectionBase, IEnumerable<ProgressStage>
   {
      /// <summary>
      /// Gets the enumerator.
      /// </summary>
      /// <returns></returns>
        public new IEnumerator<ProgressStage> GetEnumerator()
      {
          foreach (ProgressStage stage in List)
         {
             yield return stage;
         }
      }

        /// <summary>
		/// Add a stage to the collection
		/// </summary>
		/// <param name="item">The <see cref="ProgressStage" /> to add</param>
		/// <returns>The index at which the stage was added</returns>
		public int Add(ProgressStage item)
		{
			return List.Add(item);
		}
		
		/// <summary>
		/// Insert a stage into the collection at the specified index
		/// </summary>
		/// <param name="index">The index at which to insert the stage</param>
		/// <param name="item">The <see cref="ProgressStage" /> to insert</param>
		public void Insert(int index, ProgressStage item)
		{
			List.Insert(index, item);
		}

		/// <summary>
		/// Remove a stage from the collection
		/// </summary>
		/// <param name="item">The <see cref="ProgressStage" /> to remove</param>
		public void Remove(ProgressStage item)
		{
			List.Remove(item);
		} 
		
		/// <summary>
		/// Checks whether a stage is already in the collection
		/// </summary>
		/// <param name="item">The <see cref="ProgressStage" /> to look for</param>
		/// <returns>True if found in the collection; False otherwise</returns>
		public bool Contains(ProgressStage item)
		{
			return List.Contains(item);
		}
		
		/// <summary>
		/// Gets the numeric index of a stage's position in the collection
		/// </summary>
		/// <param name="item">The <see cref="ProgressStage" /> to look for</param>
		/// <returns>The index of the specified <see cref="ProgressStage" /> in the collection</returns>
		public int IndexOf(ProgressStage item)
		{
			return List.IndexOf(item);
		}
		
		/// <summary>
		/// Copies the contents of the collection to an array
		/// </summary>
		/// <param name="array">The one-dimensional array to copy to</param>
		/// <param name="index">The zero-based index in array at which copying begins</param>
		public void CopyTo(ProgressStage[] array, int index)
		{
			List.CopyTo(array, index);
		}
		
		/// <summary>
		/// Gets or sets the <see cref="ProgressStage"/> at the specified index.
		/// </summary>
		/// <value>The <see cref="ProgressStage" /> at the specified index</value>
		public ProgressStage this[int index]
		{
			get { return (ProgressStage)List[index]; }
			set { List[index] = value; }
		}

	}
}
