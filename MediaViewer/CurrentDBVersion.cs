using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLib
{
    /// <summary>
    /// Every time the DB changes, this class must change to match the current
    /// allowable version of the DB to work.
    /// 
    /// The DB version is held in the DBVersion table in the MediaDB database.
    /// DB versions are always added and never taken away.  This allows a way
    /// to follow the DB changes and the ability to go back to earlier
    /// versions.
    /// 
    /// TODO: (08/16/2019) This way currently requires a recompile.  Investigate a way to
    /// provide versioning without recompile.
    /// </summary>
    public static class CurrentDBVersion
    {
        /// <summary>
        /// The current DB major version
        /// </summary>
        public static uint CurrentMajorVersion = 0x0300;
        /// <summary>
        /// The current DB minor version
        /// </summary>
        public static uint CurrentMinorVersion = 0;
        /// <summary>
        /// The current DB point version
        /// </summary>
        public static uint CurrentPointVersion = 0;
    }
}
