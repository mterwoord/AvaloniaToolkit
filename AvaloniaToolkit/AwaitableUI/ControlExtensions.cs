using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace AvaloniaToolkit.AwaitableUI
{
    public static class ControlExtensions
    {
        /// <summary>
        /// Waits for the element to load (construct and add to the main object tree).
        /// </summary>
        public static async Task WaitForLoadedAsync(this Control frameworkElement)
        {
            if (frameworkElement.GetVisualRoot() != null)
                return;

            await EventAsync.FromEvent<VisualTreeAttachmentEventArgs>(
                eh => frameworkElement.AttachedToVisualTree += eh,
                eh => frameworkElement.AttachedToVisualTree -= eh);
        }
    }
}