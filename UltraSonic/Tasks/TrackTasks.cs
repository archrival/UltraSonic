using System.Threading.Tasks;
using Subsonic.Common;

namespace UltraSonic
{
    public partial class MainWindow
    {
        private void UpdateTrackListingGrid(Task<Directory> task)
        {
            switch (task.Status)
            {
                case TaskStatus.RanToCompletion:
                    UpdateTrackListingGrid(task.Result.Child);
                    break;
            }
        }
    }
}
