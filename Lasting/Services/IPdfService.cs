namespace Lasting.Services
{
    public interface IPdfService
    {
        byte[] GeneratePdfFromHtml(string htmlContent);
    }
}
