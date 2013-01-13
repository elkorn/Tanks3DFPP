
using System;
using Tanks3DFPP.Utilities;

namespace Tanks3DFPP.Terrain.Interfaces
{
    public class AsyncLoadingElement
    {
        public event EventHandler<ProgressEventArgs> Progress;

        public event EventHandler Ready;

        public static int TotalProgress { get; protected set; }

        protected void FireProgress(object sender, ProgressEventArgs e)
        {
            Progress.Invoke(sender, e);
        }

        protected void FireReady(object sender)
        {
            if(Ready != null)
                Ready.Invoke(sender, null);
        }
    }
}
