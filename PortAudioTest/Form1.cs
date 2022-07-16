/*
This is free and unencumbered software released into the public domain.

Anyone is free to copy, modify, publish, use, compile, sell, or
distribute this software, either in source code form or as a compiled
binary, for any purpose, commercial or non-commercial, and by any
means.

In jurisdictions that recognize copyright laws, the author or authors
of this software dedicate any and all copyright interest in the
software to the public domain. We make this dedication for the benefit
of the public at large and to the detriment of our heirs and
successors. We intend this dedication to be an overt act of
relinquishment in perpetuity of all present and future rights to this
software under copyright law.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PortAudioTest
{
    public partial class Form1 : Form
    {
        IntPtr stream;

        private static PortAudio.PaStreamCallback callback = new PortAudio.PaStreamCallback(PortAudioCallbackNonInterleaved);

        const float sampleRate = 44100F;
        float frequency = 1000.0F;

        GCHandle thisPtr;

        double phaseIncrement;
        double phase;

        bool stopFlag;
        int deviceNumber;

        public static PortAudio.PaStreamCallbackResult PortAudioCallbackNonInterleaved(IntPtr input, IntPtr output, UInt32 frameCount, IntPtr timeInfo, UInt32 statusFlags, IntPtr userData)
        {
            GCHandle h = GCHandle.FromIntPtr(userData);
            Form1 thisPtr = h.Target as Form1;

            PortAudio.PaStreamCallbackResult result = thisPtr.stopFlag ? PortAudio.PaStreamCallbackResult.paComplete : PortAudio.PaStreamCallbackResult.paContinue;

            float[] dataI = new float[frameCount];
            float[] dataQ = new float[frameCount];

            for (int i = 0; i < frameCount; i++)
            {
                thisPtr.phase += thisPtr.phaseIncrement;
                thisPtr.phase %= 2.0 * Math.PI;

                dataI[i] = (float) (0.15 * Math.Cos(thisPtr.phase));
                dataQ[i] = (float) (0.15 * Math.Sin(thisPtr.phase));
            }

            // Get output buffers
            IntPtr[] outputArray = new IntPtr[2];
            Marshal.Copy(output, outputArray, 0, 2);

            // Copy data to the output buffers
            Marshal.Copy(dataI, 0, outputArray[0], (int) frameCount);
            Marshal.Copy(dataQ, 0, outputArray[1], (int) frameCount);

            return result;
        }

        public Form1()
        {
            InitializeComponent();

            phaseIncrement = (float) (2.0 * Math.PI * frequency / sampleRate); 

            this.thisPtr = GCHandle.Alloc(this);

            this.trackBarFrequency.Value = (int) frequency;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            PortAudio.NativeMethods.Pa_Initialize();

            int hostApiCount = PortAudio.NativeMethods.Pa_GetHostApiCount();
            for (int hostApi = 0; hostApi < hostApiCount; hostApi++)
            {
                PortAudio.PaHostApiInfo hostApiInfo = PortAudio.ManagedWrappers.Pa_GetHostApiInfo(hostApi);
                if (hostApiInfo.deviceCount != 0)
                {
                    this.comboBoxHostApi.Items.Add(hostApiInfo);
                }
            }

            this.comboBoxHostApi.SelectedIndex = 0;
        }

        private void comboBoxHostApi_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.comboBoxHostApiDevices.Items.Clear();

            PortAudio.PaHostApiInfo hostApiInfo = (PortAudio.PaHostApiInfo) this.comboBoxHostApi.SelectedItem;

            int hostApiIndex = PortAudio.NativeMethods.Pa_HostApiTypeIdToHostApiIndex(hostApiInfo.type);

            for (int hostDevice = 0; hostDevice < hostApiInfo.deviceCount; hostDevice++)
            {
                int device = PortAudio.NativeMethods.Pa_HostApiDeviceIndexToDeviceIndex(hostApiIndex, hostDevice);

                PortAudio.PaDeviceInfo deviceInfo = PortAudio.ManagedWrappers.Pa_GetDeviceInfo(device);

                this.comboBoxHostApiDevices.Items.Add(deviceInfo);
            }

            this.comboBoxHostApiDevices.SelectedIndex = 0;

            SetDeviceNumber();

            this.textBox2.Text = hostApiInfo.Dump();
        }

        private void comboBoxHostApiDevices_SelectedIndexChanged(object sender, EventArgs e)
        {
            PortAudio.PaDeviceInfo deviceInfo = (PortAudio.PaDeviceInfo) this.comboBoxHostApiDevices.SelectedItem;

            SetDeviceNumber();

            this.textBox1.Text = deviceInfo.Dump();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            PortAudio.NativeMethods.Pa_Terminate();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "Start")
            {
                this.Errror.Text = string.Empty;

                PortAudio.PaStreamParameters outputParameters = new PortAudio.PaStreamParameters();
                outputParameters.channelCount = 2;
                outputParameters.device = deviceNumber;
                outputParameters.hostApiSpecificStreamInfo = IntPtr.Zero;
                outputParameters.sampleFormat = PortAudio.PaSampleFormat.paFloat32 | PortAudio.PaSampleFormat.paNonInterleaved;
                outputParameters.suggestedLatency = 0;

                int result = PortAudio.NativeMethods.Pa_OpenStream(out this.stream, null, outputParameters, sampleRate, 2048, 0, callback, GCHandle.ToIntPtr(this.thisPtr));
                if (result != 0)
                {
                    this.Errror.Text = PortAudio.NativeMethods.Pa_GetErrorText(result);
                    return;
                }

                result = PortAudio.NativeMethods.Pa_StartStream(stream);
                if (result != 0)
                {
                    this.Errror.Text = PortAudio.NativeMethods.Pa_GetErrorText(result);
                    PortAudio.NativeMethods.Pa_CloseStream(stream);
                    stream = IntPtr.Zero;
                    return;
                }

                button1.Text = "Stop";
                this.stopFlag = false;
            }
            else
            {
                button1.Text = "Start";
                this.stopFlag = true;

                PortAudio.NativeMethods.Pa_StopStream(stream);
                PortAudio.NativeMethods.Pa_CloseStream(stream);
                stream = IntPtr.Zero;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.stream != IntPtr.Zero)
            {
                this.stopFlag = true;
                PortAudio.NativeMethods.Pa_StopStream(stream);
                PortAudio.NativeMethods.Pa_CloseStream(this.stream);
            }

            this.thisPtr.Free();

            PortAudio.NativeMethods.Pa_Terminate();
        }

        private void SetDeviceNumber()
        {
            int hostApi = this.comboBoxHostApi.SelectedIndex;
            int hostApiDeviceNumber = this.comboBoxHostApiDevices.SelectedIndex;
            
            this.deviceNumber = PortAudio.NativeMethods.Pa_HostApiDeviceIndexToDeviceIndex(hostApi, hostApiDeviceNumber);
            this.textBox3.Text = deviceNumber.ToString();
        }

        private void trackBarFrequency_Scroll(object sender, EventArgs e)
        {
            frequency = this.trackBarFrequency.Value;
            phaseIncrement = (float) (2.0 * Math.PI * frequency / sampleRate); 
        }
    }
}
