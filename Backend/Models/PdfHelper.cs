using System.Net.Http;
using System.Threading.Tasks;
using UglyToad.PdfPig;
using System.IO;

public static class PdfHelper
{
    private static readonly HttpClient _httpClient = new HttpClient();

    public static async Task<string> ExtractTextFromUrlAsync(string url)
    {
        var pdfBytes = await _httpClient.GetByteArrayAsync(url);

        using (var ms = new MemoryStream(pdfBytes))
        {
            using (var pdf = PdfDocument.Open(ms))
            {
                var text = "";
                foreach (var page in pdf.GetPages())
                {
                    text += page.Text + "\n";
                }
                return text;
            }
        }
    }
}
