using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using SeviceSmartHopitail.Datas;
using ScottPlot;
using ScottPlot.TickGenerators;
using System.Drawing;
using Color = System.Drawing.Color;
using SkiaSharp;

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
            if (string.IsNullOrWhiteSpace(day) || day == "\"\"" || day.Equals("null", StringComparison.OrdinalIgnoreCase))
            {
                // Nếu không có ngày thì trả về hiện tại
                return DateTime.Now;
            }

            // Nếu day là số 7 hoặc 30 → cộng thêm số ngày đó
            if (day == "7" || day == "30")
            {
                return DateTime.Now.AddDays(-(int.Parse(day)));
            }

            // Nếu không, thử parse theo định dạng ISO hoặc tự động
            if (DateTime.TryParse(day, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime parsed))
            {
                return parsed;
            }

            // Nếu không parse được, fallback về hiện tại
            return DateTime.Now;
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
                .Where(x => x.UserProfileId == proId && x.RecordedAt >= startDate && x.RecordedAt <= endDate)
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
                BloodPressure = x.AvgSystolic.HasValue ? $"{x.AvgSystolic:0}/{x.AvgDiastolic:0} mmHg" : "",
                HeartRate = x.AvgHeartRate.HasValue ? $"{x.AvgHeartRate:0} bpm" : "",
                BloodSugar = x.AvgBloodSugar.HasValue ? $"{x.AvgBloodSugar:0.##} mg/dL" : "",
                Sleep = x.AvgSleep.HasValue ? $"{x.AvgSleep:0.##} giờ" : "",
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
                            // Column definition
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(100);
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            // Header
                            table.Header(header =>
                            {
                                header.Cell().Border(1).Padding(5).Text("Ngày").Bold();
                                header.Cell().Border(1).Padding(5).Text("Huyết áp").Bold();
                                header.Cell().Border(1).Padding(5).Text("Nhịp tim").Bold();
                                header.Cell().Border(1).Padding(5).Text("Đường huyết").Bold();
                                header.Cell().Border(1).Padding(5).Text("Giấc ngủ").Bold();
                                header.Cell().Border(1).Padding(5).Text("Ghi chú").Bold();
                            });

                            // Body
                            foreach (var item in data.Cast<dynamic>())
                            {
                                table.Cell().Border(1).Padding(5).Text((string)item.Date);
                                table.Cell().Border(1).Padding(5).Text((string)item.BloodPressure ?? "---");
                                table.Cell().Border(1).Padding(5).Text((string)item.HeartRate ?? "---");
                                table.Cell().Border(1).Padding(5).Text((string)item.BloodSugar ?? "---");
                                table.Cell().Border(1).Padding(5).Text((string)item.Sleep ?? "---");
                                table.Cell().Border(1).Padding(5).Text((string)(item.Note ?? "---"));
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

        public byte[] DrawChart(object chartData, string name)
        {
            if (chartData == null)
                throw new Exception("ChartData is null");

            var plt = new Plot();

            var labels = (chartData.GetType().GetProperty("labels")?.GetValue(chartData) as IEnumerable<string>)?.ToList()
                         ?? throw new Exception("labels not found");

            var datasets = (chartData.GetType().GetProperty("datasets")?.GetValue(chartData) as IEnumerable<object>)
                           ?? throw new Exception("datasets not found");

            double[] xs = Enumerable.Range(0, labels.Count).Select(i => (double)i).ToArray();
            bool hasSecondary = false;

            foreach (var ds in datasets)
            {
                var dsType = ds.GetType();
                string label = dsType.GetProperty("label")?.GetValue(ds)?.ToString() ?? "";
                string hex = dsType.GetProperty("borderColor")?.GetValue(ds)?.ToString() ?? "#000000";
                var color = ScottPlot.Color.FromHex(hex);
                string yAxisID = dsType.GetProperty("yAxisID")?.GetValue(ds)?.ToString() ?? "y";

                var raw = dsType.GetProperty("data")?.GetValue(ds);
                double[] ys;
                if (raw is IEnumerable<object> objList)
                {
                    // data dạng List<object> (rất phổ biến khi deserialize)
                    ys = objList.Select(v => Convert.ToDouble(v)).ToArray();
                }
                else
                {
                    ys = (raw as IEnumerable<int>)?.Select(Convert.ToDouble).ToArray()
                       ?? (raw as IEnumerable<long>)?.Select(Convert.ToDouble).ToArray()
                       ?? (raw as IEnumerable<float>)?.Select(Convert.ToDouble).ToArray()
                       ?? (raw as IEnumerable<double>)?.ToArray()
                       ?? (raw as IEnumerable<decimal>)?.Select(Convert.ToDouble).ToArray()   // <-- THÊM DÒNG NÀY
                       ?? throw new Exception("Invalid dataset.data: unsupported type " + raw?.GetType());
                }

                bool useSecondary = yAxisID == "y1";
                if (useSecondary) hasSecondary = true;

                var sc = plt.Add.Scatter(xs, ys);
                sc.Label = label;
                sc.Color = color;
                sc.MarkerSize = 5;

                if (useSecondary)
                    sc.Axes.YAxis = plt.Axes.Right;
            }

            // Set tick for X-axis with category labels
            plt.Axes.Bottom.SetTicks(xs, labels.ToArray());

            // Rotate X-axis labels
            plt.Axes.Bottom.TickLabelStyle.Rotation = -45;

            // Set axis titles
            plt.Axes.Left.Label.Text = "Primary Y";
            if (hasSecondary)
                plt.Axes.Right.Label.Text = "Secondary Y";
            plt.Axes.Bottom.Label.Text = "Dates";

            // Set plot title
            plt.Title(name);

            // Show legend
            plt.Add.Legend();
            // Render to bitmap in memory
            var img = plt.GetImage(1200, 600);
            return img.GetImageBytes(ScottPlot.ImageFormat.Png);

        }
    }
}

