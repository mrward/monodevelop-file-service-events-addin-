﻿//
// FileServiceEventsPad.cs
//
// Author:
//       Matt Ward <matt.ward@microsoft.com>
//
// Copyright (c) 2018 Microsoft
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using Gtk;
using MonoDevelop.Components;
using MonoDevelop.Components.Docking;
using MonoDevelop.Core;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Ide.Gui.Components;

namespace MonoDevelop.FileServiceEvents
{
	class FileServiceEventsPad : PadContent
	{
		static FileServiceEventsPad instance;
		static readonly LogView logView = new LogView ();
		bool enabled;

		public FileServiceEventsPad ()
		{
			instance = this;
		}

		public static FileServiceEventsPad Instance {
			get { return instance; }
		}

		protected override void Initialize (IPadWindow window)
		{
			DockItemToolbar toolbar = window.GetToolbar (DockPositionType.Right);

			var enableButton = new CheckButton ();
			enableButton.TooltipText = GettextCatalog.GetString ("Enable file events monitoring");
			enableButton.Clicked += EnableButtonClicked;
			toolbar.Add (enableButton);

			var clearButton = new Button (new ImageView (Ide.Gui.Stock.Broom, IconSize.Menu));
			clearButton.Clicked += ButtonClearClicked;
			clearButton.TooltipText = GettextCatalog.GetString ("Clear");
			toolbar.Add (clearButton);

			toolbar.ShowAll ();
			logView.ShowAll ();
		}

		public override Control Control {
			get { return logView; }
		}

		public static LogView LogView {
			get { return logView; }
		}

		void EnableButtonClicked (object sender, EventArgs e)
		{
			enabled = !enabled;

			OnEnabledChanged ();
		}

		void OnEnabledChanged ()
		{
			if (enabled) {
				FileService.FileCreated += FileCreated;
				FileService.FileRemoved += FileRemoved;
				FileService.FileRenamed += FileRenamed;
				FileService.FileChanged += FileChanged;
			} else {
				FileService.FileCreated -= FileCreated;
				FileService.FileRemoved -= FileRemoved;
				FileService.FileRenamed -= FileRenamed;
				FileService.FileChanged -= FileChanged;
			}
		}

		void FileChanged (object sender, FileEventArgs e)
		{
			foreach (FileEventInfo info in e) {
				WriteText ("FileChanged: " + info.FileName);
			}
		}

		void FileRenamed (object sender, FileCopyEventArgs e)
		{
			foreach (FileCopyEventInfo info in e) {
				WriteText (string.Format ("FileRenamed: {0} -> {1}", info.SourceFile, info.TargetFile));
			}
		}

		void FileRemoved (object sender, FileEventArgs e)
		{
			foreach (FileEventInfo info in e) {
				WriteText ("FileRemoved: " + info.FileName);
			}
		}

		void FileCreated (object sender, FileEventArgs e)
		{
			foreach (FileEventInfo info in e) {
				WriteText ("FileCreated: " + info.FileName);
			}
		}

		void ButtonClearClicked (object sender, EventArgs e)
		{
			logView.Clear ();
		}

		static void WriteText (string message)
		{
			string fullMessage = string.Format ("{0}: {1}{2}", DateTime.Now.ToString ("u"), message, Environment.NewLine);
			Runtime.RunInMainThread (() => {
				logView.WriteText (null, fullMessage);
			});
		}
	}
}
