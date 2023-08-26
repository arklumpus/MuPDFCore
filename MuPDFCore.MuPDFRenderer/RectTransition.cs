/*
    MuPDFCore.MuPDFRenderer - A control to display documents in Avalonia using MuPDFCore.
    Copyright (C) 2020-2023  Giorgio Bianchini, University of Bristol

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as
    published by the Free Software Foundation, version 3.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>
*/

namespace Avalonia.Animation
{
    /// <summary>
    /// Transition class that handles <see cref="AvaloniaProperty"/> with <see cref="Rect"/> types.
    /// </summary>
    public class RectTransition : InterpolatingTransitionBase<Rect>
    {
        /// <inheritdoc/>
        protected override Rect Interpolate(double f, Rect oldValue, Rect newValue)
        {
            return new Rect((newValue.X - oldValue.X) * f + oldValue.X,
                         (newValue.Y - oldValue.Y) * f + oldValue.Y,
                         (newValue.Width - oldValue.Width) * f + oldValue.Width,
                         (newValue.Height - oldValue.Height) * f + oldValue.Height);
        }
    }
}
