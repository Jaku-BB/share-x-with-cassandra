using Cassandra;
using Cassandra.Mapping;
using Cassandra.Mapping.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using server.Configuration;

namespace server.Services;

public class FileStorageService
{
    private readonly Cassandra.ISession _session;
    private readonly IMapper _mapper;
    private const string KeyspaceName = "share_x";
    private const string TableName = "files";

    [Table("files")]
    private class FileData
    {
        [PartitionKey]
        [Column("file_id")]
        public string FileId { get; set; } = string.Empty;

        [Column("original_file_name")]
        public string OriginalFileName { get; set; } = string.Empty;

        [Column("content")]
        public byte[] Content { get; set; } = [];
    }

    public FileStorageService(IOptions<CassandraSettings> settings)
    {
        var builder = Cluster.Builder();
        
        foreach (var contactPoint in settings.Value.ContactPoints)
        {
            var parts = contactPoint.Split(':');
            var host = parts[0];
            var port = parts.Length > 1 ? int.Parse(parts[1]) : 9042;
            
            builder.AddContactPoint(host).WithPort(port);
        }

        if (!string.IsNullOrEmpty(settings.Value.Username))
        {
            builder.WithCredentials(settings.Value.Username, settings.Value.Password);
        }

        var cluster = builder.Build();
        _session = cluster.Connect();
        
        _session.Execute($@"
            CREATE KEYSPACE IF NOT EXISTS {KeyspaceName}
            WITH REPLICATION = {{ 
                'class' : 'SimpleStrategy', 
                'replication_factor' : 1 
            }}");

        _session.ChangeKeyspace(KeyspaceName);

        _session.Execute($@"
            CREATE TABLE IF NOT EXISTS {TableName} (
                file_id text PRIMARY KEY,
                original_file_name text,
                content blob
            )");

        _mapper = new Mapper(_session);
    }

    public async Task<string> StoreFile(IFormFile file)
    {
        var fileId = Guid.NewGuid().ToString();

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        
        var fileData = new FileData
        {
            FileId = fileId,
            OriginalFileName = file.FileName,
            Content = memoryStream.ToArray()
        };

        await _mapper.InsertAsync(fileData);
        return fileId;
    }

    public async Task<string> GetFilePath(string fileId)
    {
        var fileData = await _mapper.FirstOrDefaultAsync<FileData>(
            "WHERE file_id = ?",
            fileId);
        
        return fileData == null ? string.Empty : fileId;
    }

    public async Task<string> GetFileName(string fileId)
    {
        var fileData = await _mapper.FirstOrDefaultAsync<FileData>(
            "WHERE file_id = ?",
            fileId);
        
        return fileData?.OriginalFileName ?? string.Empty;
    }

    public async Task<byte[]?> GetFileContent(string fileId)
    {
        var fileData = await _mapper.FirstOrDefaultAsync<FileData>(
            "WHERE file_id = ?",
            fileId);
        
        return fileData?.Content;
    }
}