using Models;
using Repository.Generics;
using System;
using System.Linq;

namespace Repository.ModelRepos
{
    public class RecordingRepository(SheetMusicMakerDBContext ctx) : Repository<Recording>(ctx)
    {
        public override Recording Read(int id)
        {
            return ctx.Recordings.FirstOrDefault(r => r.Id.Equals(id)) ?? throw new NullReferenceException("No recording found with this Id!");
        }

        public override void Update(Recording item)
        {
            var old = Read(item.Id);
            old.SampleRate = item.SampleRate;
            old.SamplesJson = item.SamplesJson;
        }
    }
}
