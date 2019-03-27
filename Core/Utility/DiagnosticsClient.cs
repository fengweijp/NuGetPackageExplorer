﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.WindowsServer;
using NuGetPe.Utility;

namespace NuGetPe
{
    public static class DiagnosticsClient
    {
        private static bool _initialized;

        static TelemetryClient  _client;



        public static void Initialize(string apiKey)
        {
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                TelemetryConfiguration.Active.InstrumentationKey = apiKey;
                TelemetryConfiguration.Active.TelemetryChannel.DeveloperMode = Debugger.IsAttached;
                TelemetryConfiguration.Active.TelemetryInitializers.Add(new VersionTelemetry());
                TelemetryConfiguration.Active.TelemetryInitializers.Add(new SessionTelemetry());

                _initialized = true;

                _client = new TelemetryClient();
            }
        }

        public static void OnExit()
        {
            if (!_initialized) return;

            _client.Flush();
            // Allow time for flushing:
            System.Threading.Thread.Sleep(1000);
        }

        public static void TrackEvent(string evt, IDictionary<string, string>? properties = null, IDictionary<string, double>? metrics = null)
        {
            if (!_initialized) return;
            _client.TrackEvent(evt, properties, metrics);
        }

        public static void TrackEvent([CallerMemberName] string? evt = null)
        {
            if (!_initialized) return;
            _client.TrackEvent(evt);
        }

        public static void TrackTrace(string evt)
        {
            if (!_initialized) return;
            _client.TrackTrace(evt);
        }

        public static void Notify(Exception exception)
        {
            if (!_initialized) return;

            _client.TrackException(exception);
        }

        public static void TrackPageView(string pageName)
        {
            if (!_initialized) return;

            _client.TrackPageView(pageName);
        }
    }
}
