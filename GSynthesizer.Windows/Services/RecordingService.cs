using KSynthesizer.Filters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace GSynthesizer.Windows.Services
{
    class RecordingService
    {
        private RecordFilter Filter { get; set; }

        private string TempPath { get; set; }

        public void Start(RecordFilter filter)
        {
            var recorder = filter;
            if (recorder != null)
            {
                var path = Path.GetTempFileName();
                TempPath = path;

                var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
                recorder.StartRecording(fs, true, false);
                Filter = filter;
            }
        }

        public async Task StopAsync(string outputPath)
        {
            var recorder = Filter;
            if (recorder != null)
            {
                recorder.StopRecording();
                using (var sourceStream = new FileStream(TempPath, FileMode.Open, FileAccess.Read))
                using (var outStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                {
                    await sourceStream.CopyToAsync(outStream);
                }
            }
        }

        public void Cancel()
        {
            var recorder = Filter;
            if (recorder != null)
            {
                recorder.StopRecording();
                File.Delete(TempPath);
            }
        }
    }
}
