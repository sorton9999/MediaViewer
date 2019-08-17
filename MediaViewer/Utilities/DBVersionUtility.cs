using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DataAccessLib;

namespace MediaViewer.Utilities
{
    /// <summary>
    /// Utility class to provide DB version identification
    /// </summary>
    public class DBVersionUtility
    {
        /// <summary>
        /// Masks used to isolate the different versions
        /// </summary>
        public const uint MajorVersionMask = 0x0F00;
        public const uint MinorVersionMask = 0x00F0;
        public const uint PointVersionMask = 0x000F;

        /// <summary>
        /// Get the isolated major version from the input DB version
        /// </summary>
        /// <param name="dbVersion">The DB version</param>
        /// <returns>The isolated major version</returns>
        public static uint GetMajorVersion(uint dbVersion)
        {
            return (dbVersion & MajorVersionMask);
        }

        /// <summary>
        /// Get the isolated minor version from the input DB version
        /// </summary>
        /// <param name="dbVersion">The DB version</param>
        /// <returns>The isolated minor version</returns>
        public static uint GetMinorVersion(uint dbVersion)
        {
            return (dbVersion & MinorVersionMask);
        }

        /// <summary>
        /// Get the isolated point version from the input DB version
        /// </summary>
        /// <param name="dbVersion">The DB version</param>
        /// <returns>The isolated point version</returns>
        public static uint GetPointVersion(uint dbVersion)
        {
            return (dbVersion & PointVersionMask);
        }

        /// <summary>
        /// Isolates the incoming DB version to the major version and
        /// returns if they match.
        /// </summary>
        /// <param name="dbVersion">The DB version</param>
        /// <param name="majorVersion">The isolated major version</param>
        /// <returns>Whether or not they match</returns>
        public static bool IsMajorVersion(uint dbVersion, uint majorVersion)
        {
            return (GetMajorVersion(dbVersion) == majorVersion);
        }

        /// <summary>
        /// Isolates the incoming DB version to the minor version and
        /// returns if they match.
        /// </summary>
        /// <param name="dbVersion">The DB version</param>
        /// <param name="minorVersion">The isolated minor version</param>
        /// <returns>Whether or not they match</returns>
        public static bool IsMinorVersion(uint dbVersion, uint minorVersion)
        {
            return (GetMinorVersion(dbVersion) == minorVersion);
        }

        /// <summary>
        /// Isolates the incoming DB version to the point version and
        /// returns if they match.
        /// </summary>
        /// <param name="dbVersion">The DB version</param>
        /// <param name="pointVersion">The isolated point version</param>
        /// <returns>Whether or not they match</returns>
        public static bool IsPointVersion(uint dbVersion, uint pointVersion)
        {
            return (GetPointVersion(dbVersion) == pointVersion);
        }

        /// <summary>
        /// Determines if the incoming DB version matches that stored in the DB
        /// </summary>
        /// <param name="dbVersion">The DB version</param>
        /// <returns>Whether or not they match</returns>
        public static bool IsDbVersion(uint dbVersion)
        {
            uint dbVersionFromDB = DataAccessLib.DataModel.DBVersion.GetDbVersion();
            return (dbVersionFromDB == dbVersion);
        }

    }
}
