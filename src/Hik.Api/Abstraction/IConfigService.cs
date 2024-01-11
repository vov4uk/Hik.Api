using Hik.Api.Data;
using System;

namespace Hik.Api.Abstraction
{
    /// <summary>
    /// 
    /// </summary>
    public interface IConfigService
    {
        /// <summary>
        /// Gets the time.
        /// </summary>
        /// <param name="ipChannel">The ip channel.</param>
        /// <returns></returns>
        DateTime GetTime(int ipChannel = 1);
        /// <summary>
        /// Sets the time.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="ipChannel">The ip channel.</param>
        void SetTime(DateTime dateTime, int ipChannel = -1);
        /// <summary>
        /// Gets the device configuration.
        /// </summary>
        /// <returns></returns>
        DeviceConfig GetDeviceConfig();
        /// <summary>
        /// Gets the network configuration.
        /// </summary>
        /// <returns></returns>
        NetworkConfig GetNetworkConfig();

        /// <summary>
        /// Gets the HDD status.
        /// </summary>
        /// <param name="ipChannel">The ip channel.</param>
        /// <returns></returns>
        HdInfo GetHddStatus(int ipChannel = -1);
    }
}
