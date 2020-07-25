/*
    MuPDFCore.MuPDFRenderer - A control to display documents in Avalonia using MuPDFCore.
    Copyright (C) 2020  Giorgio Bianchini

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>
*/

namespace MuPDFCore.MuPDFRenderer
{
    /// <summary>
    /// Contains temporary hacks to work around bugs in the Avalonia library.
    /// </summary>
    internal static class AvaloniaBugFixes
    {
        /// <summary>
        /// Get the the appropriate pixel format for the current platform. See also https://github.com/AvaloniaUI/Avalonia/issues/4354 .
        /// </summary>
        /// <returns><see cref="PixelFormats.BGRA"/> on Linux, <see cref="PixelFormats.RGBA"/> on other platforms.</returns>
        public static PixelFormats GetPlatformRGBA()
        {
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
            {
                return PixelFormats.BGRA;
            }
            else
            {
                return PixelFormats.RGBA;
            }
        }
    }
}
