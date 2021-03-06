﻿//-----------------------------------------------------------------------
// <copyright company="CoApp Project">
//     ResourceLib Original Code from http://resourcelib.codeplex.com
//     Original Copyright (c) 2008-2009 Vestris Inc.
//     Changes Copyright (c) 2011 Garrett Serack . All rights reserved.
// </copyright>
// <license>
// MIT License
// You may freely use and distribute this software under the terms of the following license agreement.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of 
// the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO 
// THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, 
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE
// </license>
//-----------------------------------------------------------------------

namespace CoApp.Developer.Toolkit.ResourceLib {
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using CoApp.Toolkit.Win32;

    /// <summary>
    ///   A container for the DIALOGTEMPLATEEX structure.
    /// </summary>
    public class DialogExTemplate : DialogTemplateBase {
        private CoApp.Toolkit.Win32.DialogExTemplate _header;

        /// <summary>
        ///   Indicates the character set to use.
        /// </summary>
        public byte CharacterSet { get; set; }

        /// <summary>
        ///   X-coordinate, in dialog box units, of the upper-left corner of the dialog box.
        /// </summary>
        public override Int16 x {
            get { return _header.x; }
            set { _header.x = value; }
        }

        /// <summary>
        ///   Y-coordinate, in dialog box units, of the upper-left corner of the dialog box.
        /// </summary>
        public override Int16 y {
            get { return _header.y; }
            set { _header.y = value; }
        }

        /// <summary>
        ///   Width, in dialog box units, of the dialog box.
        /// </summary>
        public override Int16 cx {
            get { return _header.cx; }
            set { _header.cx = value; }
        }

        /// <summary>
        ///   Height, in dialog box units, of the dialog box.
        /// </summary>
        public override Int16 cy {
            get { return _header.cy; }
            set { _header.cy = value; }
        }

        /// <summary>
        ///   Dialog style.
        /// </summary>
        public override UInt32 Style {
            get { return _header.style; }
            set { _header.style = value; }
        }

        /// <summary>
        ///   Extended dialog style.
        /// </summary>
        public override UInt32 ExtendedStyle {
            get { return _header.exStyle; }
            set { _header.exStyle = value; }
        }

        /// <summary>
        ///   Weight of the font.
        /// </summary>
        public UInt16 Weight { get; set; }

        /// <summary>
        ///   Indicates whether the font is italic.
        /// </summary>
        public bool Italic { get; set; }

        /// <summary>
        ///   Number of dialog items.
        /// </summary>
        public override UInt16 ControlCount {
            get { return _header.cDlgItems; }
        }

        /// <summary>
        ///   Read the dialog resource.
        /// </summary>
        /// <param name = "lpRes">Pointer to the beginning of the dialog structure.</param>
        internal override IntPtr Read(IntPtr lpRes) {
            _header = (CoApp.Toolkit.Win32.DialogExTemplate) Marshal.PtrToStructure(lpRes, typeof (CoApp.Toolkit.Win32.DialogExTemplate));

            lpRes = base.Read(new IntPtr(lpRes.ToInt32() + 26)); // Marshal.SizeOf(_header)

            if ((Style & (uint) DialogStyles.DS_SETFONT) > 0 || (Style & (uint) DialogStyles.DS_SHELLFONT) > 0) {
                // weight
                Weight = (UInt16) Marshal.ReadInt16(lpRes);
                lpRes = new IntPtr(lpRes.ToInt32() + 2);
                // italic
                Italic = (Marshal.ReadByte(lpRes) > 0);
                lpRes = new IntPtr(lpRes.ToInt32() + 1);
                // character set
                CharacterSet = Marshal.ReadByte(lpRes);
                lpRes = new IntPtr(lpRes.ToInt32() + 1);
                // typeface
                TypeFace = Marshal.PtrToStringUni(lpRes);
                lpRes = new IntPtr(lpRes.ToInt32() + (TypeFace.Length + 1)*Marshal.SystemDefaultCharSize);
            }

            return ReadControls(lpRes);
        }

        internal override IntPtr AddControl(IntPtr lpRes) {
            var control = new DialogExTemplateControl();
            Controls.Add(control);
            return control.Read(lpRes);
        }

        /// <summary>
        ///   Write dialog control to a binary stream.
        /// </summary>
        /// <param name = "w">Binary stream.</param>
        public override void Write(BinaryWriter w) {
            w.Write(_header.dlgVer);
            w.Write(_header.signature);
            w.Write(_header.helpID);
            w.Write(_header.exStyle);
            w.Write(_header.style);
            w.Write((UInt16) Controls.Count);
            w.Write(_header.x);
            w.Write(_header.y);
            w.Write(_header.cx);
            w.Write(_header.cy);

            base.Write(w);

            if ((Style & (uint) DialogStyles.DS_SETFONT) > 0 || (Style & (uint) DialogStyles.DS_SHELLFONT) > 0) {
                w.Write(Weight);
                w.Write((byte) (Italic ? 1 : 0));
                w.Write(CharacterSet);
                w.Write(Encoding.Unicode.GetBytes(TypeFace));
                w.Write((UInt16) 0);
            }

            WriteControls(w);
        }

        /// <summary>
        ///   String representation of the dialog.
        /// </summary>
        /// <returns>String in the DIALOGEX [dialog] format.</returns>
        public override string ToString() {
            return string.Format("DIALOGEX {0}", base.ToString());
        }
    }
}