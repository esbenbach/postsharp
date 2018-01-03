// <copyright file="CursorType.cs">
// </copyright>
// <author></author>
// <email></email>
// <date></date>
// <summary></summary>
namespace Aspects.Forms
{
    /// <summary>
    /// An enumeration of cursor types.
    /// </summary>
    /// <remarks>
    /// This is used in WaitCursorAspect because you can't use the System.Windows.Forms.Cursor type in attribute properties.
    /// We also take the opportunity to give the cursors more meaningful names.
    /// </remarks>
    public enum CursorType
    {
        /// <summary>
        /// The default cursor.
        /// </summary>
        /// <remarks>
        /// This type maps to Cursors.Default, which is usually a regular mouse pointer.
        /// </remarks>
        Default = 0,

        /// <summary>
        /// A regular "busy" cursor, which indicates that the view is busy with a foreground operation and that no action can be expected
        /// to work until the cursor changes back to the default.
        /// </summary>
        /// <remarks>
        /// This type maps to Cursors.WaitCursor, which has no mouse pointer and usually consists of something like an hourglass/spinner.
        /// </remarks>
        Busy,

        /// <summary>
        /// A cursor that indicates that the view is "partially busy", which is appropriate if an important background operation is in progress,
        /// but it's OK for the user to continue working in the meantime.
        /// </summary>
        /// <remarks>
        /// This type maps to the very poorly named Cursors.AppStarting, which usually has a mouse pointer and a small hourglass/spinner next to it.
        /// </remarks>
        PartiallyBusy
    }
}