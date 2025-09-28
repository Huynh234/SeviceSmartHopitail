using System;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SeviceSmartHopitail.Datas;
using SeviceSmartHopitail.Models;

namespace SeviceSmartHopitail.Services
{
    public class PdfExtractor
    {
        /// <summary>
        /// Extract dữ liệu từ 3 file txt: 
        /// - codes.txt
        /// - code-description pairs.txt
        /// - descriptions.txt
        /// và lưu vào 3 bảng: IcdCodes, TextChunks, IndexTerms
        /// </summary>
        public static void Extract(string codesPath, string pairsPath, string descriptionsPath, IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.Migrate();

            var codes = File.ReadAllLines(codesPath).Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
            var pairs = File.ReadAllLines(pairsPath).Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
            var descriptions = File.ReadAllLines(descriptionsPath).Where(l => !string.IsNullOrWhiteSpace(l)).ToList();

            // 1. Thêm IcdCode từ code-description pairs
            foreach (var line in pairs)
            {
                var parts = line.Split('\t', 2); // tách theo tab
                if (parts.Length < 2) continue;

                var code = parts[0].Trim();
                var title = parts[1].Trim();

                var icd = db.IcdCodes.FirstOrDefault(c => c.Code == code);
                if (icd == null)
                {
                    icd = new IcdCode
                    {
                        Code = code,
                        Title = title,
                        SourceVolume = "TXT"
                    };
                    db.IcdCodes.Add(icd);
                    db.SaveChanges();
                }

                // 2. Thêm IndexTerm (dùng chính title)
                if (!db.IndexTerms.Any(t => t.Term == title && t.IcdCodeId == icd.Id))
                {
                    db.IndexTerms.Add(new IndexTerm
                    {
                        Term = title,
                        IcdCodeId = icd.Id
                    });
                    db.SaveChanges();
                }
            }

            // 3. Ghép codes + descriptions để tạo TextChunk
            for (int i = 0; i < Math.Min(codes.Count, descriptions.Count); i++)
            {
                var code = codes[i].Trim();
                var desc = descriptions[i].Trim();

                var icd = db.IcdCodes.FirstOrDefault(c => c.Code == code);
                if (icd != null && !string.IsNullOrEmpty(desc))
                {
                    // Chia nhỏ description thành nhiều chunks (500 ký tự)
                    const int chunkSize = 500;
                    for (int j = 0; j < desc.Length; j += chunkSize)
                    {
                        var chunkText = desc.Substring(j, Math.Min(chunkSize, desc.Length - j));

                        if (!db.TextChunks.Any(tc => tc.IcdCodeId == icd.Id && tc.Text == chunkText))
                        {
                            db.TextChunks.Add(new TextChunk
                            {
                                IcdCodeId = icd.Id,
                                Text = chunkText,
                                Embedding = null,
                                EmbeddingCreatedAt = null
                            });
                            db.SaveChanges();
                        }
                    }
                }
            }
        }
    }
}
