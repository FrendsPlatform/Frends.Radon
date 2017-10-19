using System;

namespace Frends.Radon
{
    public interface IReportSender : IDisposable
    {
        void SendReport(string logDataHtml, string subject);
    }
}