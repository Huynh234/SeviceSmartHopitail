using Microsoft.EntityFrameworkCore;
using SeviceSmartHopitail.Datas;
using QuestPDF.Fluent;

namespace SeviceSmartHopitail.Services.Health
{
    public class ReportService
    {
        private readonly AppDbContext _db;
        public ReportService(AppDbContext db)
        {
            _db = db;
        }

        public DateTime ConvertSTD(string day)
        {
            return DateTime.Parse(day, null, System.Globalization.DateTimeStyles.RoundtripKind);
        }

        public async Task<List<object>> DataDetail(int proId, string start, string end = "")
        {
            DateTime startDate, endDate;

            if (!string.IsNullOrEmpty(end))
            {
                endDate = ConvertSTD(end);
                startDate = ConvertSTD(start);
            }
            else
            {
                endDate = DateTime.Now;
                startDate = endDate.AddDays(-int.Parse(start));
            }

            var bp = await _db.BloodPressureRecords
                .Where(x =>x.UserProfileId == proId && x.RecordedAt >= startDate && x.RecordedAt <= endDate)
                .ToListAsync();
            var hr = await _db.HeartRateRecords
                .Where(x => x.UserProfileId == proId && x.RecordedAt >= startDate && x.RecordedAt <= endDate)
                .ToListAsync();
            var bs = await _db.BloodSugarRecords
                .Where(x => x.UserProfileId == proId && x.RecordedAt >= startDate && x.RecordedAt <= endDate)
                .ToListAsync();
            var sl = await _db.SleepRecords
                .Where(x => x.UserProfileId == proId && x.RecordedAt >= startDate && x.RecordedAt <= endDate)
                .ToListAsync();

            var dailyData = (from date in Enumerable.Range(0, (endDate - startDate).Days + 1)
                             let day = startDate.AddDays(date)
                             let bpDay = bp.Where(x => x.RecordedAt.Date == day.Date)
                             let hrDay = hr.Where(x => x.RecordedAt.Date == day.Date)
                             let bsDay = bs.Where(x => x.RecordedAt.Date == day.Date)
                             let slDay = sl.Where(x => x.RecordedAt.Date == day.Date)
                             select new
                             {
                                 Date = day.ToString("dd/MM/yyyy"),
                                 AvgSystolic = bpDay.Any() ? bpDay.Average(x => x.Systolic) : (double?)null,
                                 AvgDiastolic = bpDay.Any() ? bpDay.Average(x => x.Diastolic) : (double?)null,
                                 AvgHeartRate = hrDay.Any() ? hrDay.Average(x => x.HeartRate) : (double?)null,
                                 AvgBloodSugar = bsDay.Any() ? (double?)bsDay.Average(x => x.BloodSugar) : null,
                                 AvgSleep = slDay.Any() ? (double?)slDay.Average(x => (double)x.HoursSleep) : null,
                                 Note = string.Join("; ",
                                     bpDay.Where(x => !string.IsNullOrEmpty(x.Note)).Select(x => x.Note)
                                     .Concat(hrDay.Where(x => !string.IsNullOrEmpty(x.Note)).Select(x => x.Note))
                                     .Concat(bsDay.Where(x => !string.IsNullOrEmpty(x.Note)).Select(x => x.Note))
                                     .Concat(slDay.Where(x => !string.IsNullOrEmpty(x.Note)).Select(x => x.Note))
                                 )
                             }).ToList();

            // Tính trung bình chung an toàn
            var avgRow = new
            {
                Date = "Trung bình chung",
                AvgSystolic = dailyData.Any(x => x.AvgSystolic.HasValue) ? dailyData.Where(x => x.AvgSystolic.HasValue).Average(x => x.AvgSystolic) ?? 0 : 0,
                AvgDiastolic = dailyData.Any(x => x.AvgDiastolic.HasValue) ? dailyData.Where(x => x.AvgDiastolic.HasValue).Average(x => x.AvgDiastolic) ?? 0 : 0,
                AvgHeartRate = dailyData.Any(x => x.AvgHeartRate.HasValue) ? dailyData.Where(x => x.AvgHeartRate.HasValue).Average(x => x.AvgHeartRate) ?? 0 : 0,
                AvgBloodSugar = dailyData.Any(x => x.AvgBloodSugar.HasValue) ? dailyData.Where(x => x.AvgBloodSugar.HasValue).Average(x => x.AvgBloodSugar) ?? 0 : 0,
                AvgSleep = dailyData.Any(x => x.AvgSleep.HasValue) ? dailyData.Where(x => x.AvgSleep.HasValue).Average(x => x.AvgSleep) ?? 0 : 0,
                Note = "",
            };
            //string = Coment = "Trong 7 ngày qua, các chỉ số sức khỏe của bạn nhìn chung đang ổn định. Huyết áp trung bình" +da+" mmHg, nhịp tim 72 BPM, đường huyết 95 mg/dL và giấc ngủ trung bình 7.6 giờ/đêm. Tiếp tục duy trì lối sống lành mạnh và theo dõi thường xuyên."
            
            var result = dailyData.Select(x => new
            {
                x.Date,
                BloodPressure = x.AvgSystolic.HasValue ? $"{x.AvgSystolic:0}/{x.AvgDiastolic:0} mmHg" : "Không có dữ liệu",
                HeartRate = x.AvgHeartRate.HasValue ? $"{x.AvgHeartRate:0} bpm" : "Không có dữ liệu",
                BloodSugar = x.AvgBloodSugar.HasValue ? $"{x.AvgBloodSugar:0.##} mg/dL" : "Không có dữ liệu",
                Sleep = x.AvgSleep.HasValue ? $"{x.AvgSleep:0.##} giờ" : "Không có dữ liệu",
                x.Note
            }).Cast<object>().ToList();

            result.Add(new
            {
                Date = avgRow.Date,
                BloodPressure = $"{avgRow.AvgSystolic:0}/{avgRow.AvgDiastolic:0} mmHg",
                HeartRate = $"{avgRow.AvgHeartRate:0} bpm",
                BloodSugar = $"{avgRow.AvgBloodSugar:0.##} mg/dL",
                Sleep = $"{avgRow.AvgSleep:0.##} giờ",
                Note = ""
            });

            return result;
        }
        public byte[] ExportToPdf(List<object> data)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);
                    page.Header()
                        .AlignCenter()
                        .Text("Báo cáo sức khỏe")
                        .FontSize(20)
                        .Bold();

                    page.Content()
                        .Table(table =>
                        {
                            // Header
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(100);
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Ngày").Bold();
                                header.Cell().Text("Huyết áp").Bold();
                                header.Cell().Text("Nhịp tim").Bold();
                                header.Cell().Text("Đường huyết").Bold();
                                header.Cell().Text("Giấc ngủ").Bold();
                                header.Cell().Text("Ghi chú").Bold();
                            });

                            // Body
                            foreach (var item in data.Cast<dynamic>()) // Explicitly cast to dynamic
                            {
                                table.Cell().Text((string)item.Date); // Explicitly cast dynamic properties
                                table.Cell().Text((string)item.BloodPressure);
                                table.Cell().Text((string)item.HeartRate);
                                table.Cell().Text((string)item.BloodSugar);
                                table.Cell().Text((string)item.Sleep);
                                table.Cell().Text((string)(item.Note ?? ""));
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(txt =>
                        {
                            txt.Span("Trang ");
                            txt.CurrentPageNumber();
                            txt.Span(" / ");
                            txt.TotalPages();
                        });
                });
            });

            return document.GeneratePdf();
        }

        //public async
    }
}

