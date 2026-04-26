using NeoSolve.ImageSharp.AVIF;
using SixLabors.ImageSharp;

namespace CigarHelper.Api.Storage;

/// <summary>
/// Регистрация AVIF (NeoSolve: libavif avifenc/avifdec в выходной каталоге) в глобальной конфигурации ImageSharp.
/// </summary>
internal static class AvifImageSharpBootstrap
{
    private static readonly object Gate = new();
    private static bool _configured;

    public static void EnsureConfigured()
    {
        if (_configured)
            return;
        lock (Gate)
        {
            if (_configured)
                return;
            Configuration.Default.Configure(new AVIFConfigurationModule());
            _configured = true;
        }
    }
}
