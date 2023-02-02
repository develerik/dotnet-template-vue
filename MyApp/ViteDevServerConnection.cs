namespace MyApp;

using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.SpaServices;

public static class ViteDevServerConnection {
  // port the Vite dev server is listening on
  private static int Port => 5173;

  // Vite dev server endpoint uri
  private static Uri DevServerEndpoint { get; } = new Uri($"http://localhost:{Port}");

  /// <summary>
  ///   Use Vite dev server.
  /// </summary>
  /// <param name="spaBuilder">An instance of <see cref="ISpaBuilder"/>.</param>
  /// <exception cref="TimeoutException">Vite build timed out.</exception>
  public static void UseViteDevServer(this ISpaBuilder spaBuilder) {
    spaBuilder.UseProxyToSpaDevelopmentServer(ProxyRequest);
  }

  private static async Task<Uri> ProxyRequest() {
    IEnumerable<int> tcpEndpoints = IPGlobalProperties.GetIPGlobalProperties()
      .GetActiveTcpListeners()
      .Select(e => e.Port);

    if (tcpEndpoints.Contains(Port)) {
      return DevServerEndpoint;
    }

    bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    var processInfo = new ProcessStartInfo {
      FileName = isWindows ? "cmd" : "yarn",
      Arguments = $"{(isWindows ? "/c yarn " : string.Empty)} serve",
      WorkingDirectory = "ClientApp",
      RedirectStandardError = true,
      RedirectStandardInput = true,
      RedirectStandardOutput = true,
      UseShellExecute = false,
    };

    var process = Process.Start(processInfo);
    TaskCompletionSource<int> tcs = new TaskCompletionSource<int>();

    _ = Task.Run(() => {
      try {
        while (process?.StandardOutput.ReadLine() is { } line) {
          if (!tcs.Task.IsCompleted && line.Contains("ready in")) {
            tcs.SetResult(1);
          }
        }
      } catch (EndOfStreamException ex) {
        tcs.SetException(new InvalidOperationException("'yarn serve' failed.", ex));
      }
    });

    var timeout = Task.Delay(TimeSpan.FromSeconds(60));
    if (await Task.WhenAny(timeout, tcs.Task) == timeout) {
      throw new TimeoutException();
    }

    return DevServerEndpoint;
  }
}
