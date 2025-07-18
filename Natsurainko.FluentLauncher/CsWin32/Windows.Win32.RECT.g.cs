// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// ------------------------------------------------------------------------------

#pragma warning disable CS1591,CS1573,CS0465,CS0649,CS8019,CS1570,CS1584,CS1658,CS0436,CS8981
using global::System;
using global::System.Diagnostics;
using global::System.Diagnostics.CodeAnalysis;
using global::System.Runtime.CompilerServices;
using global::System.Runtime.InteropServices;
using global::System.Runtime.Versioning;
using winmdroot = global::Windows.Win32;
namespace Windows.Win32
{
    namespace Foundation
    {
        /// <summary>The RECT structure defines a rectangle by the coordinates of its upper-left and lower-right corners.</summary>
        /// <remarks>The RECT structure is identical to the <a href="https://docs.microsoft.com/windows/desktop/api/windef/ns-windef-rectl">RECTL</a> structure.</remarks>
        [global::System.CodeDom.Compiler.GeneratedCode("Microsoft.Windows.CsWin32", "0.3.183+73e6125f79.RR")]
        public struct RECT
        {
            /// <summary>Specifies the <i>x</i>-coordinate of the upper-left corner of the rectangle.</summary>
            public int left;

            /// <summary>Specifies the <i>y</i>-coordinate of the upper-left corner of the rectangle.</summary>
            public int top;

            /// <summary>Specifies the <i>x</i>-coordinate of the lower-right corner of the rectangle.</summary>
            public int right;

            /// <summary>Specifies the <i>y</i>-coordinate of the lower-right corner of the rectangle.</summary>
            public int bottom;

            public RECT(global::System.Drawing.Rectangle value) :
                this(value.Left, value.Top, value.Right, value.Bottom)
            {
            }

            public RECT(global::System.Drawing.Point location, global::System.Drawing.Size size) :
                this(location.X, location.Y, unchecked(location.X + size.Width), unchecked(location.Y + size.Height))
            {
            }

            public RECT(int left, int top, int right, int bottom)
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }

            public static RECT FromXYWH(int x, int y, int width, int height) =>
                new RECT(x, y, unchecked(x + width), unchecked(y + height));

            public readonly int Width => unchecked(this.right - this.left);

            public readonly int Height => unchecked(this.bottom - this.top);

            public readonly bool IsEmpty => this.left == 0 && this.top == 0 && this.right == 0 && this.bottom == 0;

            public readonly int X => this.left;

            public readonly int Y => this.top;

            public readonly global::System.Drawing.Size Size => new global::System.Drawing.Size(this.Width, this.Height);

            public static implicit operator global::System.Drawing.Rectangle(RECT value) => new global::System.Drawing.Rectangle(value.left, value.top, value.Width, value.Height);

            public static implicit operator global::System.Drawing.RectangleF(RECT value) => new global::System.Drawing.RectangleF(value.left, value.top, value.Width, value.Height);

            public static implicit operator RECT(global::System.Drawing.Rectangle value) => new RECT(value);
        }
    }
}
