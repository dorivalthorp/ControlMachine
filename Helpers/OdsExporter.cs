using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using ControlMachine.Models;

namespace ControlMachine.Helpers
{
    public static class OdsExporter
    {
        public static void Export(string filePath, List<Producao> producoes)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            using (var fs = new FileStream(filePath, FileMode.Create))
            using (var archive = new ZipArchive(fs, ZipArchiveMode.Create))
            {
                
                var mimetypeEntry = archive.CreateEntry("mimetype", CompressionLevel.NoCompression);
                using (var writer = new StreamWriter(mimetypeEntry.Open(), Encoding.ASCII))
                {
                    writer.Write("application/vnd.oasis.opendocument.spreadsheet");
                }

                
                var manifestEntry = archive.CreateEntry("META-INF/manifest.xml");
                using (var writer = new StreamWriter(manifestEntry.Open(), Encoding.UTF8))
                {
                    writer.Write(@"<?xml version=""1.0"" encoding=""UTF-8""?>
<manifest:manifest xmlns:manifest=""urn:oasis:names:tc:opendocument:xmlns:manifest:1.0"" manifest:version=""1.2"">
  <manifest:file-entry manifest:full-path=""/"" manifest:media-type=""application/vnd.oasis.opendocument.spreadsheet""/>
  <manifest:file-entry manifest:full-path=""content.xml"" manifest:media-type=""text/xml""/>
</manifest:manifest>");
                }

                
                var contentEntry = archive.CreateEntry("content.xml");
                using (var writer = new StreamWriter(contentEntry.Open(), Encoding.UTF8))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(@"<?xml version=""1.0"" encoding=""UTF-8""?>
<office:document-content 
    xmlns:office=""urn:oasis:names:tc:opendocument:xmlns:office:1.0"" 
    xmlns:table=""urn:oasis:names:tc:opendocument:xmlns:table:1.0"" 
    xmlns:text=""urn:oasis:names:tc:opendocument:xmlns:text:1.0"" 
    office:version=""1.2"">
  <office:body>
    <office:spreadsheet>
      <table:table table:name=""Producoes"">");

                    
                    sb.Append("<table:table-row>");
                    string[] headers = { "ID", "Pedido", "Cliente", "Número", "Status", "Quantidade", "Data", "Usuário Responsável" };
                    foreach (var h in headers)
                    {
                        sb.Append($@"<table:table-cell office:value-type=""string""><text:p>{EscapeXml(h)}</text:p></table:table-cell>");
                    }
                    sb.Append("</table:table-row>");

                    
                    foreach (var p in producoes)
                    {
                        sb.Append("<table:table-row>");
                        sb.Append($@"<table:table-cell office:value-type=""string""><text:p>{p.Id}</text:p></table:table-cell>");
                        sb.Append($@"<table:table-cell office:value-type=""string""><text:p>{EscapeXml(p.Pedido)}</text:p></table:table-cell>");
                        sb.Append($@"<table:table-cell office:value-type=""string""><text:p>{EscapeXml(p.Cliente)}</text:p></table:table-cell>");
                        sb.Append($@"<table:table-cell office:value-type=""string""><text:p>{EscapeXml(p.NumeroProducao)}</text:p></table:table-cell>");
                        sb.Append($@"<table:table-cell office:value-type=""string""><text:p>{EscapeXml(p.Status)}</text:p></table:table-cell>");
                        sb.Append($@"<table:table-cell office:value-type=""string""><text:p>{p.Quantidade}</text:p></table:table-cell>");
                        sb.Append($@"<table:table-cell office:value-type=""string""><text:p>{p.DataProducao:yyyy-MM-dd HH:mm}</text:p></table:table-cell>");
                        sb.Append($@"<table:table-cell office:value-type=""string""><text:p>{EscapeXml(p.NomeUsuario)}</text:p></table:table-cell>");
                        sb.Append("</table:table-row>");
                    }

                    sb.Append(@"      </table:table>
    </office:spreadsheet>
  </office:body>
</office:document-content>");
                    writer.Write(sb.ToString());
                }
            }
        }

        private static string EscapeXml(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";
            return System.Security.SecurityElement.Escape(str);
        }
    }
}
