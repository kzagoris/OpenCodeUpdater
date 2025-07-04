using OneOf;
using OpenCodeUpdater.Errors;
using upgrade_opencode.Services;

namespace OpenCodeUpdater.Services;

public class FileDownloadService(HttpClient httpClient, IConsoleOutputService consoleOutput)
{

    public async Task<OneOf<byte[], HttpError>> DownloadFileWithProgressAsync(string url)
    {
        try
        {
            using var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            var contentLength = response.Content.Headers.ContentLength ?? 0;
            using var stream = await response.Content.ReadAsStreamAsync();
            using var ms = new MemoryStream();

            using var progressReporter = consoleOutput.CreateProgressReporter("Downloading OpenCode", contentLength);

            var buffer = new byte[8192];
            long totalRead = 0;
            int read;

            while ((read = await stream.ReadAsync(buffer)) > 0)
            {
                ms.Write(buffer, 0, read);
                totalRead += read;
                progressReporter.UpdateProgress(totalRead, contentLength > 0 ? contentLength : totalRead);
            }

            progressReporter.Complete();
            return ms.ToArray();
        }
        catch (HttpRequestException ex)
        {
            return new HttpError($"Failed to download file: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            return new HttpError("Download timed out", ex);
        }
        catch (Exception ex)
        {
            return new HttpError($"Unexpected error during download: {ex.Message}", ex);
        }
    }

    public async Task<OneOf<Success, FileError>> SaveFileAsync(string filePath, byte[] fileData)
    {
        try
        {
            await File.WriteAllBytesAsync(filePath, fileData);
            consoleOutput.WriteSuccess($"File saved to: {filePath}");
            return new Success();
        }
        catch (Exception ex)
        {
            return new FileError($"Failed to save file: {ex.Message}", ex);
        }
    }
}