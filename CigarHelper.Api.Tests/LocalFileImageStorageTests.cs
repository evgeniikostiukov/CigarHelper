using CigarHelper.Api.Storage;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CigarHelper.Api.Tests;

/// <summary>Тесты LocalFileImageStorage: запись, чтение, удаление файлов.</summary>
public class LocalFileImageStorageTests : IDisposable
{
    private readonly string _tempDir;
    private readonly LocalFileImageStorage _storage;

    public LocalFileImageStorageTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"cigar_img_tests_{Guid.NewGuid():N}");
        _storage = new LocalFileImageStorage(_tempDir, NullLogger<LocalFileImageStorage>.Instance);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    [Fact]
    public void Constructor_CreatesDirectory()
    {
        Assert.True(Directory.Exists(_tempDir));
    }

    [Fact]
    public void StoresExternally_IsTrue()
    {
        Assert.True(_storage.StoresExternally);
    }

    [Fact]
    public async Task SaveAsync_ReturnsNonNullKey()
    {
        var data = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 };

        var key = await _storage.SaveAsync(data, "test.jpg");

        Assert.NotNull(key);
        Assert.NotEmpty(key);
    }

    [Fact]
    public async Task SaveAsync_CreatesFile()
    {
        var data = new byte[] { 1, 2, 3, 4, 5 };

        var key = await _storage.SaveAsync(data, "test.jpg");

        var fullPath = Path.Combine(_tempDir, key!);
        Assert.True(File.Exists(fullPath));
    }

    [Fact]
    public async Task ReadAsync_ReturnsOriginalData()
    {
        var data = new byte[] { 10, 20, 30, 40, 50 };
        var key = await _storage.SaveAsync(data, "sample.jpg");

        var read = await _storage.ReadAsync(key!);

        Assert.NotNull(read);
        Assert.Equal(data, read);
    }

    [Fact]
    public async Task ReadAsync_MissingFile_ReturnsNull()
    {
        var result = await _storage.ReadAsync("nonexistent_file.jpg");

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_RemovesFile()
    {
        var data = new byte[] { 9, 8, 7 };
        var key = await _storage.SaveAsync(data, "to_delete.jpg");

        await _storage.DeleteAsync(key!);

        var fullPath = Path.Combine(_tempDir, key!);
        Assert.False(File.Exists(fullPath));
    }

    [Fact]
    public async Task DeleteAsync_NonExistentFile_DoesNotThrow()
    {
        await _storage.DeleteAsync("nonexistent.jpg");
    }

    [Fact]
    public async Task SaveAsync_SanitizesIllegalCharsInFileName()
    {
        var data = new byte[] { 1, 2, 3 };

        var key = await _storage.SaveAsync(data, "file<>|name.jpg");

        Assert.NotNull(key);
        var fullPath = Path.Combine(_tempDir, key!);
        Assert.True(File.Exists(fullPath));
    }

    [Fact]
    public async Task SaveAsync_MultipleFiles_UniqueKeys()
    {
        var data = new byte[] { 1, 2, 3 };

        var key1 = await _storage.SaveAsync(data, "img.jpg");
        var key2 = await _storage.SaveAsync(data, "img.jpg");

        Assert.NotEqual(key1, key2);
    }

    [Fact]
    public async Task PutAtKeyAsync_NestedPath_WritesAndDescribe()
    {
        var data = new byte[] { 1, 2, 3, 4 };
        const string key = "import/ab/deadbeef.jpg";

        await _storage.PutAtKeyAsync(data, key, "image/jpeg");

        var described = await _storage.TryDescribeAsync(key);
        Assert.NotNull(described);
        Assert.Equal(data.LongLength, described!.Size);

        Assert.True(await _storage.ExistsAsync(key));
        var read = await _storage.ReadAsync(key);
        Assert.Equal(data, read);
    }

    [Fact]
    public async Task TryDescribeAsync_Missing_ReturnsNull()
    {
        var described = await _storage.TryDescribeAsync("import/none.jpg");
        Assert.Null(described);
    }
}
