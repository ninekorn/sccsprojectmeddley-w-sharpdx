// Copyright (c) 2010-2013 SharpDX - Alexandre Mutel
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
using System.Diagnostics;
using System.Windows.Forms;

using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using System.Windows.Threading;

namespace SC_Console_APP
{
    /// <summary>
    /// SharpDX MiniCube Direct3D 11 Sample
    /// </summary>
    internal static class Program
    {
        public static Dispatcher MainDispatch;

        //[STAThread]
        private static void Main()
        {
            MainDispatch = Dispatcher.CurrentDispatcher;

             SC_DirectX directXEntry = new SC_DirectX();

            //Console.WriteLine("test");

        }
        /*private delegate void DispatchHandler();
        void DescriptionCallback(IAsyncResult ar)
        {
            string description = (ar.AsyncState as IEchoService).EndDescription(ar);
            Dispatcher dispatcher = this.Dispatcher;
            DispatchHandler handler = new DispatchHandler(UpdateDescription);
            Object[] args = { description };
            this.Dispatcher.BeginInvoke(handler, args);
        }*/
    }
    /*private delegate void InvokeDelegate();
    private void tbAux_SelectionChanged(object sender, EventArgs e)
    {
        this.BeginInvoke(new InvokeDelegate(HandleSelection));
    }
    private void HandleSelection()
    {
        textBox.Text = tbAux.Text;
    }*/

  

}