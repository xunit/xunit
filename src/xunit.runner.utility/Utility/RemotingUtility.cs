namespace Xunit
{
    /// <summary>
    /// Internal helper class for remoting.
    /// </summary>
    public static class RemotingUtility
    {
        /// <summary>
        /// Unregisters any remoting channels.
        /// </summary>
        /// <remarks>
        /// If there are any registered remoting channels, then MarshalByRefObjects
        /// don't work. Based on bug #9749, it's clear that MSTest (at least through
        /// Visual Studio 2010) registers remoting channels when it runs but doesn't
        /// clean them up when it's done. Right now, the only way to reliably surface
        /// this issue is through MSBuild (as per the bug repro), so for the moment
        /// this work-around code is limited to the MSBuild runner.
        /// </remarks>
        public static void CleanUpRegisteredChannels()
        {
#if NETFRAMEWORK
            foreach (var channel in System.Runtime.Remoting.Channels.ChannelServices.RegisteredChannels)
                System.Runtime.Remoting.Channels.ChannelServices.UnregisterChannel(channel);
#endif
        }
    }
}
