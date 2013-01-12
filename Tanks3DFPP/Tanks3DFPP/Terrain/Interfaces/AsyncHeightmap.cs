
using System;
using Tanks3DFPP.Utilities;

namespace Tanks3DFPP.Terrain.Interfaces
{
    public class AsyncHeightMap
    {
        public static event EventHandler<ProgressEventArgs> Progressing;

        public static event EventHandler Finished;

        public static int TotalProgress { get; protected set; }

        protected void FireProgressing(object sender, ProgressEventArgs e)
        {
            Progressing.Invoke(sender, e);
        }

        protected void FireFinished(object sender)
        {
            if(Finished != null)
                Finished.Invoke(sender, null);
        }
    }
}
