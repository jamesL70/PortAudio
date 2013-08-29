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
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace PortAudio
{
    #region Enum's
    public enum PaHostApiTypeId
    {
        paInDevelopment = 0,
        paDirectSound = 1,
        paMME = 2,
        paASIO = 3,
        paSoundManager = 4,
        paCoreAudio = 5,
        paOSS = 7,
        paALSA = 8,
        paAL = 9,
        paBeOS = 10,
        paWDMKS = 11,
        paJACK = 12,
        paWASAPI = 13,
        paAudioScienceHPI = 14
    }

    public enum PaSampleFormat
    {
        paFloat32 = 0x00000001,
        paInt32 = 0x00000002,
        paInt24 = 0x00000004,
        paInt16 = 0x00000008,
        paInt8 = 0x00000010,
        paUInt8 = 0x00000020,
        paCustomFormat = 0x00010000,
        paNonInterleaved = ~0x7fffffff
    }

    public enum PaStreamCallbackResult
    {
        paContinue = 0,
        paComplete = 1,
        paAbort = 2
    }

    #endregion

    #region Struct's
    [StructLayout(LayoutKind.Sequential)]
    public struct PaStream { }

    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Ansi)]
    public class PaHostApiInfo
    {
        public int structVersion;
        public PaHostApiTypeId type;

        public string name;

        public int deviceCount;
        public int defaultInputDevice;
        public int defaultOutputDevice;

        public override string ToString()
        {
            return name;
        }

        public String Dump()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("structVersion:       " + structVersion + "\r\n");
            sb.Append("type:                " + type + "\r\n");
            sb.Append("name:                " + name + "\r\n");
            sb.Append("deviceCount:         " + deviceCount + "\r\n");
            sb.Append("defaultInputDevice:  " + defaultInputDevice + "\r\n");
            sb.Append("defaultOutputDevice: " + defaultOutputDevice + "\r\n");

            Trace.WriteLine(sb.ToString());

            return sb.ToString();
        }
    };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class PaDeviceInfo
    {
        public int structVersion;
        
        public string name;
        public int hostApi;

        public int maxInputChannels;
        public int maxOutputChannels;

        public double defaultLowInputLatency;
        public double defaultLowOutputLatency;

        public double defaultHighInputLatency;
        public double defaultHighOutputLatency;

        public double defaultSampleRate;

        public override string ToString()
        {
            return name;
        }

        public String Dump()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("structVersion:            " + structVersion + "\r\n");
            sb.Append("name:                     " + name + "\r\n");
            sb.Append("hostApi:                  " + hostApi + "\r\n");
            sb.Append("maxInputChannels:         " + maxInputChannels + "\r\n");
            sb.Append("maxOutputChannels:        " + maxOutputChannels + "\r\n");
            sb.Append("defaultLowInputLatency:   " + defaultLowInputLatency + "\r\n");
            sb.Append("defaultLowOutputLatency:  " + defaultLowOutputLatency + "\r\n");
            sb.Append("defaultHighInputLatency:  " + defaultHighInputLatency + "\r\n");
            sb.Append("defaultHighOutputLatency: " + defaultHighOutputLatency + "\r\n");
            sb.Append("defaultSampleRate:        " + defaultSampleRate + "\r\n");

            Trace.WriteLine(sb.ToString());

            return sb.ToString();
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class PaHostErrorInfo
    {
        PaHostApiTypeId hostApiType;
        int errorCode;

        string errorText;

        public override string ToString()
        {
            return errorText;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class PaStreamParameters
    {
        public int device;
        public int channelCount;
        public PaSampleFormat sampleFormat;
        public double suggestedLatency;
        public IntPtr hostApiSpecificStreamInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class PaStreamCallbackTimeInfo
    {
        public double inputBufferAdcTime;
        public double currentTime;
        public double outputBufferDacTime;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class PaStreamInfo
    {
        public int structVersion;
        public double inputLatency;
        public double outputLatency;
        public double sampleRate;
    }
    #endregion

    #region Delegates
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate PaStreamCallbackResult PaStreamCallback([In] IntPtr input, [Out] IntPtr output, UInt32 frameCount,
                                                            IntPtr timeInfo, UInt32 statusFlags, IntPtr userData);
    #endregion

    #region Managed Methods
    unsafe public static class ManagedWrappers
    {
        // A valid host API index ranging from 0 to (Pa_GetHostApiCount()-1)
        public static PaHostApiInfo Pa_GetHostApiInfo(int hostApi)
        {
            IntPtr ptr = NativeMethods.Pa_GetHostApiInfo(hostApi);
            PaHostApiInfo info = (PaHostApiInfo) Marshal.PtrToStructure(ptr, typeof(PaHostApiInfo));
            return info;
        }

        // A valid device index in the range 0 to (Pa_GetDeviceCount()-1)
        public static PaDeviceInfo Pa_GetDeviceInfo(int device)
        {
            IntPtr ptr = NativeMethods.Pa_GetDeviceInfo(device);
            PaDeviceInfo info = (PaDeviceInfo) Marshal.PtrToStructure(ptr, typeof(PaDeviceInfo));
            return info;
        }

        public static PaDeviceInfo Pa_GetDeviceInfo(int hostApi, int hostApiDeviceIndex)
        {
            int deviceIndex = NativeMethods.Pa_HostApiDeviceIndexToDeviceIndex(hostApi, hostApiDeviceIndex);
            PaDeviceInfo info = Pa_GetDeviceInfo(deviceIndex);
            return info;
        }

        public static PaHostErrorInfo Pa_GetLastHostErrorInfo()
        {
            IntPtr ptr = NativeMethods.Pa_GetLastHostErrorInfo();
            PaHostErrorInfo info = (PaHostErrorInfo) Marshal.PtrToStructure(ptr, typeof(PaHostErrorInfo));
            return info;
        }

        public static PaStreamInfo Pa_GetStreamInfo(PaStream* stream)
        {
            IntPtr ptr = NativeMethods.Pa_GetStreamInfo(stream);
            PaStreamInfo info = (PaStreamInfo) Marshal.PtrToStructure(ptr, typeof(PaStreamInfo));
            return info;
        }
    }
    #endregion

    #region Imported Methods
    unsafe public static class NativeMethods
    {
#if X86
        const string dllName = "portaudio_x86.dll";
#else
        const string dllName = "portaudio_x64.dll";
#endif

        [DllImport(dllName)]
        public static extern int Pa_GetVersion();

        [DllImport(dllName)]
        public static extern string Pa_GetVersionText();

        [DllImport(dllName)]
        public static extern string Pa_GetErrorText(int errorCode);

        [DllImport(dllName)]
        public static extern int Pa_Initialize();

        [DllImport(dllName)]
        public static extern int Pa_Terminate();

        [DllImport(dllName)]
        public static extern int Pa_GetHostApiCount();

        [DllImport(dllName)]
        public static extern int Pa_GetDefaultHostApi();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hostApi">A valid host API index ranging from 0 to (Pa_GetHostApiCount()-1)</param>
        /// <returns></returns>
        [DllImport(dllName)]
        public static extern IntPtr Pa_GetHostApiInfo(int hostApi);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">A unique host API identifier belonging to the PaHostApiTypeId enumeration.</param>
        /// <returns></returns>
        [DllImport(dllName)]
        public static extern int Pa_HostApiTypeIdToHostApiIndex(PaHostApiTypeId type);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hostApi">A valid host API index ranging from 0 to (Pa_GetHostApiCount()-1)</param>
        /// <param name="hostApiDeviceIndex">A valid per-host device index in the range 0 to (Pa_GetHostApiInfo(hostApi)->deviceCount-1)</param>
        /// <returns></returns>
        [DllImport(dllName)]
        public static extern int Pa_HostApiDeviceIndexToDeviceIndex(int hostApi, int hostApiDeviceIndex);

        [DllImport(dllName)]
        public static extern IntPtr Pa_GetLastHostErrorInfo();

        [DllImport(dllName)]
        public static extern int Pa_GetDeviceCount();

        [DllImport(dllName)]
        public static extern int Pa_GetDefaultInputDevice();

        [DllImport(dllName)]
        public static extern int Pa_GetDefaultOutputDevice();

        [DllImport(dllName)]
        public static extern IntPtr Pa_GetDeviceInfo(int device);

        [DllImport(dllName)]
        public static extern int Pa_IsFormatSupported(PaStreamParameters inputParameters, PaStreamParameters outputParameters, Double sampleRate);

        [DllImport(dllName)]
        public static extern int Pa_OpenStream(out IntPtr stream, PaStreamParameters inputParameters, PaStreamParameters outputParameters,
                                                Double sampleRate, UInt32 framesPerBuffer, UInt32 streamFlags,
                                                PaStreamCallback streamCallback, IntPtr userData);

        [DllImport(dllName)]
        public static extern int Pa_OpenDefaultStream([Out] IntPtr stream, int numInputChannels, int numOutputChannels,
                                                       UInt32 sampleFormat, double sampleRate, UInt32 framesPerBuffer,
                                                       PaStreamCallback streamCallback, IntPtr userData);

        [DllImport(dllName)]
        public static extern int Pa_CloseStream(IntPtr stream);

        [DllImport(dllName)]
        public static extern void PaStreamFinishedCallback(IntPtr userData);

        [DllImport(dllName)]
        public static extern int Pa_StartStream(IntPtr stream);

        [DllImport(dllName)]
        public static extern int Pa_StopStream(IntPtr stream);

        [DllImport(dllName)]
        public static extern int Pa_AbortStream(IntPtr stream);

        [DllImport(dllName)]
        public static extern int Pa_IsStreamStopped(IntPtr stream);

        [DllImport(dllName)]
        public static extern int Pa_IsStreamActive(IntPtr stream);

        [DllImport(dllName)]
        public static extern IntPtr Pa_GetStreamInfo(PaStream* stream);

        [DllImport(dllName)]
        public static extern double Pa_GetStreamTime(IntPtr stream);

        [DllImport(dllName)]
        public static extern double Pa_GetStreamCouLoad(IntPtr stream);

        [DllImport(dllName)]
        public static extern int Pa_ReadStream(IntPtr stream, [Out] Byte[] buffer, UInt32 frames);

        [DllImport(dllName)]
        public static extern int Pa_WriteStream(IntPtr stream, Byte[] buffer, UInt32 frames);

        [DllImport(dllName)]
        public static extern int Pa_GetStreamReadAvailable(IntPtr stream);

        [DllImport(dllName)]
        public static extern int Pa_GetStreamWriteAvailable(IntPtr stream);

        [DllImport(dllName)]
        public static extern int Pa_GetSampleSize(UInt32 format);

        [DllImport(dllName)]
        public static extern void Pa_Sleep(UInt32 msec);

        [DllImport(dllName)]
        public static extern int PaAsio_GetAvailableLatencyValues(int device, [Out] UInt32 minLatency, [Out] UInt32 maxLatency,
                                                                  [Out] UInt32 preferredLatency, [Out] UInt32 granularity);

        [DllImport(dllName)]
        public static extern int PaAsio_ShowControlPanel(int device, IntPtr systemSpecific);

        [DllImport(dllName)]
        public static extern void PaUtil_InitializeX86PlainConverters();

        [DllImport(dllName)]
        public static extern int PaAsio_GetInputChannelName(int device, int channelIndex, [Out] string channelName);

        [DllImport(dllName)]
        public static extern int PaAsio_GetOutputChannelName(int device, int channelIndex, [Out] string channelName);
    }
    #endregion

    public class PortAudioException : Exception
    {
        private Int32 portAudioError;

        public Int32 PaErrorCode
        {
            get { return this.PaErrorCode; }
        }

        public PortAudioException(int portAudioError) : base(PortAudio.NativeMethods.Pa_GetErrorText(portAudioError))
        {
            this.portAudioError = portAudioError;
        }
    }
}