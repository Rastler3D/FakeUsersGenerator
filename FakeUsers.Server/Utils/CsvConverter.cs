using CsvHelper;
using FakeUsers.Server.Models;
using System.Globalization;

namespace FakeUsers.Server.Utils
{
    public static class CsvConverter
    {
        public static byte[] ConvertToCsv(IEnumerable<User> data)
        {
            using var memoryStream = new MemoryStream();
            using var writer = new StreamWriter(memoryStream);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            csv.WriteRecords(data);
            writer.Flush();
            return memoryStream.ToArray();
        }
    }
}
