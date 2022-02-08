using Microsoft.AspNetCore.SignalR;
using System.Threading.Channels;

namespace SignalRServer.Hubs
{
    public class UploadHub : Hub
    {
        private readonly ILogger<UploadHub> Logger;
        
        public UploadHub(ILogger<UploadHub> _logger)
        {
            Logger = _logger;
        }

        public List<String> GetFilesOnServer()
        {
            String[] allFiles = Directory.GetFiles(GetSourceDirectoryPath());
            List<String> fileList = new List<String>(allFiles);
            return fileList;
        }

        public ChannelReader<object> DownloadFileAsByteArray(string filePath, bool throwException, CancellationToken cancellationToken)
        {
            var channel = Channel.CreateUnbounded<object>();
            _ = WriteToChannelAsByteArray(channel.Writer, filePath, throwException, cancellationToken);
            return channel.Reader;
        }

        public ChannelReader<object> DownloadFileAsString(string filePath, bool throwException, CancellationToken cancellationToken)
        {
            var channel = Channel.CreateUnbounded<object>();
            _ = WriteToChannelAsString(channel.Writer, filePath, throwException, cancellationToken);
            return channel.Reader;
        }

        private async Task WriteToChannelAsByteArray(ChannelWriter<object> writer, string filePath, bool throwException, CancellationToken cancellationToken)
        {
            Exception localException = null;
            int numOfChunks = 0;
            int chunkSizeInKb = 60;
            try
            {
                int chunkSize = (int)(chunkSizeInKb * 1024);
                using (FileStream fs = File.OpenRead(filePath))
                {
                    long chunkLength = fs.Length > chunkSize ? chunkSize : fs.Length;

                    byte[] b = new byte[chunkLength];
                    while (fs.Read(b, 0, b.Length) > 0)
                    {
                        /* this delay has been added to throttle the rate at which items are written to the channel. 
                      If not throttled, all chunks are written to the channel rapidly before cancellationToken is received */
                        await Task.Delay(10);
                        if (cancellationToken.IsCancellationRequested)
                        {
                            Logger.LogInformation("Downstream: streaming cancelled");
                            break;
                        }
                        Logger.LogInformation($"chunk sent: {Convert.ToBase64String(b)}");
                        await writer.WriteAsync(b, cancellationToken);
                        numOfChunks++;
                    }
                }
                Logger.LogInformation($"Downstream: Total {numOfChunks} chunks written to channel");
            }
            catch (Exception ex)
            {
                localException = ex;
                Logger.LogError("Error occurred while writing to channel, Exception: " + ex.Message);
            }
            finally
            {
                if (throwException && localException == null)
                {
                    /* Due to this issue: https://github.com/dotnet/aspnetcore/issues/33753, currently only OperationCanceledException
                     are caught by SignalR.*/
                    localException = new OperationCanceledException("Custom Exception thrown from service");
                }
                writer.TryComplete(localException);
            }
        }

        private async Task WriteToChannelAsString(ChannelWriter<object> writer, string filePath, bool throwException, CancellationToken cancellationToken)
        {
            Exception localException = null;
            int numOfChunks = 0;
            int chunkSizeInKb = 60;
            try
            {
                int chunkSize = (int)(chunkSizeInKb * 1024);
                using (FileStream fs = File.OpenRead(filePath))
                {
                    long chunkLength = fs.Length > chunkSize ? chunkSize : fs.Length;

                    byte[] b = new byte[chunkLength];
                    while (fs.Read(b, 0, b.Length) > 0)
                    {
                        /* this delay has been added to throttle the rate at which items are written to the channel. 
                      If not throttled, all chunks are written to the channel rapidly before cancellationToken is received */
                        await Task.Delay(10);
                        if (cancellationToken.IsCancellationRequested)
                        {
                            Logger.LogInformation("Downstream: streaming cancelled");
                            break;
                        }
                        Logger.LogInformation($"chunk sent: {Convert.ToBase64String(b)}");
                        await writer.WriteAsync(Convert.ToBase64String(b), cancellationToken);
                        numOfChunks++;
                    }
                }
                Logger.LogInformation($"Downstream: Total {numOfChunks} chunks written to channel");
            }
            catch (Exception ex)
            {
                localException = ex;
                Logger.LogError("Error occurred while writing to channel, Exception: " + ex.Message);
            }
            finally
            {
                if (throwException && localException == null)
                {
                    /* Due to this issue: https://github.com/dotnet/aspnetcore/issues/33753, currently only OperationCanceledException
                     are caught by SignalR.*/
                    localException = new OperationCanceledException("Custom Exception thrown from service");
                }
                writer.TryComplete(localException);
            }
        }

        private string GetSourceDirectoryPath()
        {
            // change this location
            return @"path/to/folder/containing/images";
        }
    }
}
