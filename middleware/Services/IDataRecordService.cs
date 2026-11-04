namespace middleware.Services;
using middleware.Models;
public interface IDataRecordService
{
    Task<DataRecord> CreateDRAsync(DataRecord record);

    Task<DataRecord?> DeleteDRAsync(
        int channelId,
        string resourceName
    );
}